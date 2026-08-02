using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitLume.Core;

/// <summary>拉取结果的分类（用于智能决定后续动作）。</summary>
internal enum PullOutcome
{
    Ok,
    /// <summary>云端仓库为空或分支不存在，跳过拉取直接推送。</summary>
    NoRemoteRef,
    Failed,
}

/// <summary>
/// Git 高层业务服务：
/// - 仓库状态检测 / 初始化；
/// - 全局配置写入（user.name / user.email）；
/// - 首次提交与更新提交的差异化命令序列；
/// - 多仓库同时推送；
/// - 认证失败自动弹出凭据对话框并重试（凭据仅保存在内存）。
/// </summary>
public sealed class GitService
{
    private readonly GitProcessRunner _runner = new();
    private string _userName = "";
    private string _userEmail = "";

    /// <summary>本次会话按远程 URL 记忆的凭据（仅内存，启动时从设置解密载入）。</summary>
    private readonly Dictionary<string, GitSessionCredentials> _sessionCreds = new();

    /// <summary>某个远程需要登录时触发；返回用户填写的凭据，取消则返回 null。</summary>
    public Func<RemoteInfo, Task<CredentialEntry?>>? AuthRequired { get; set; }

    /// <summary>某远程认证通过后触发（凭据有效才允许保存，避免记住错误密码）。</summary>
    public event Action<RemoteInfo, CredentialEntry>? CredentialsAccepted;

    public event Action<GitOutputLine>? OutputReceived;
    public event Action<string>? StatusChanged;

    public GitService()
    {
        _runner.OutputReceived += line => OutputReceived?.Invoke(line);
    }

    /// <summary>设置身份（同时作用于每条命令的 -c 参数与全局配置写入）。</summary>
    public void SetIdentity(string name, string email)
    {
        _userName = name;
        _userEmail = email;
    }

    // ==================== 基础执行 ====================

    private async Task<GitResult> RunAsync(string folder, bool addIdentity, params string[] args)
    {
        var cmd = new List<string>();
        if (addIdentity)
        {
            if (!string.IsNullOrWhiteSpace(_userName))
            {
                cmd.Add("-c");
                cmd.Add($"user.name={_userName}");
            }
            if (!string.IsNullOrWhiteSpace(_userEmail))
            {
                cmd.Add("-c");
                cmd.Add($"user.email={_userEmail}");
            }
        }
        cmd.AddRange(args);

        var result = await _runner.RunAsync(folder, cmd.ToArray());
        result.AuthFailed = IsAuthError(result.Output);
        return result;
    }

    private static bool IsAuthError(string output)
    {
        if (string.IsNullOrEmpty(output)) return false;
        string[] markers =
        {
            "could not read Username", "could not read Password",
            "Authentication failed", "invalid username or password",
            "HTTP Basic: Access denied", "failed to authenticate",
            "incorrect username or password", "access denied",
            "error: 401", "remote: Authentication",
        };
        return markers.Any(m => output.IndexOf(m, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>规范化远程 URL 作为凭据存储的键（小写、去尾部斜杠与 .git）。</summary>
    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        var u = url.Trim();
        try
        {
            var uri = new Uri(u);
            u = uri.Scheme + "://" + uri.Host + uri.AbsolutePath;
        }
        catch
        {
            // 非标准 URL 直接按原样处理
        }
        u = u.TrimEnd('/');
        if (u.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            u = u[..^4];
        return u.ToLowerInvariant();
    }

    /// <summary>取某个远程的本次会话凭据（无则 null）。</summary>
    private GitSessionCredentials? GetSessionCreds(RemoteInfo remote)
    {
        return _sessionCreds.TryGetValue(NormalizeUrl(remote.Url), out var c) && c.HasCredentials ? c : null;
    }

    /// <summary>记录/更新某个远程的会话凭据（登录成功或用户重新填写后调用）。</summary>
    public void SetSessionCredentials(RemoteInfo remote, string userName, string password)
    {
        _sessionCreds[NormalizeUrl(remote.Url)] = new GitSessionCredentials { UserName = userName, Password = password };
    }

    /// <summary>启动时把已保存（加密）的凭据解密载入会话，推送时自动使用、不再弹窗。</summary>
    public void LoadSavedCredentials(IEnumerable<KeyValuePair<string, StoredCredential>> saved)
    {
        _sessionCreds.Clear();
        foreach (var (key, sc) in saved)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(sc?.Password)) continue;
            var password = SecureCodec.Decrypt(sc.Password);
            if (string.IsNullOrEmpty(password)) continue;
            _sessionCreds[key] = new GitSessionCredentials { UserName = sc.Username, Password = password };
        }
    }

    /// <summary>清空本次会话所有远程凭据。</summary>
    public void ClearCredentials() => _sessionCreds.Clear();

    /// <summary>应用退出时调用：终止可能仍在执行的 git 进程，确保资源立即释放、不留孤儿进程。</summary>
    public void Shutdown() => _runner.KillCurrent();

    // ==================== 仓库检测与初始化 ====================

    public async Task<RepoStatus> DetectRepoStatusAsync(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            return RepoStatus.Unknown;

        var inside = await RunAsync(folder, false, "rev-parse", "--is-inside-work-tree");
        if (!inside.Success || !inside.Output.Contains("true"))
            return RepoStatus.NotGitRepo;

        var head = await RunAsync(folder, false, "rev-parse", "--verify", "HEAD");
        return head.Success ? RepoStatus.HasCommits : RepoStatus.EmptyRepo;
    }

    public async Task<GitResult> InitRepoAsync(string folder)
    {
        StatusChanged?.Invoke("正在初始化仓库...");
        var init = await RunAsync(folder, false, "init");
        if (init.Success)
        {
            // 与常用工作流保持一致，默认分支命名为 master
            await RunAsync(folder, false, "branch", "-M", "master");
            StatusChanged?.Invoke("仓库初始化完成（默认分支 master）。");
        }
        return init;
    }

    /// <summary>写入全局 user.name / user.email（一台电脑只需设置一次）。</summary>
    public async Task<GitResult> EnsureGlobalConfigAsync(string name, string email)
    {
        StatusChanged?.Invoke("写入 Git 全局配置...");
        var r1 = await RunAsync("", false, "config", "--global", "user.name", name);
        var r2 = await RunAsync("", false, "config", "--global", "user.email", email);
        if (r1.Success && r2.Success)
        {
            StatusChanged?.Invoke("全局配置已写入 ~/.gitconfig。");
            return r1;
        }
        return new GitResult { ExitCode = 1, Output = r1.Output + r2.Output };
    }

    // ==================== 分支与远程 ====================

    /// <summary>获取当前分支名（新仓库无提交时回退 master）。</summary>
    public async Task<string> GetBranchAsync(string folder)
    {
        var r = await RunAsync(folder, false, "branch", "--show-current");
        var branch = r.Output?.Trim();
        return string.IsNullOrEmpty(branch) ? "master" : branch;
    }

    /// <summary>读取仓库已配置的远程（名称 + URL 去重）。</summary>
    public async Task<List<RemoteInfo>> GetRemotesAsync(string folder)
    {
        var result = new List<RemoteInfo>();
        var r = await RunAsync(folder, false, "remote", "-v");
        if (!r.Success) return result;

        var seen = new HashSet<string>();
        foreach (var line in r.Output.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            var name = parts[0];
            var url = parts[1];
            if (seen.Add(name))
                result.Add(new RemoteInfo { Name = name, Url = url });
        }
        return result;
    }

    /// <summary>确保远程仓库已注册（不存在则 add，URL 变化则 set-url）。</summary>
    private async Task EnsureRemoteAsync(string folder, RemoteInfo remote)
    {
        var existing = (await GetRemotesAsync(folder)).FirstOrDefault(x => x.Name == remote.Name);
        if (existing == null)
        {
            await RunAsync(folder, false, "remote", "add", remote.Name, remote.Url);
        }
        else if (!string.Equals(existing.Url, remote.Url, StringComparison.OrdinalIgnoreCase))
        {
            await RunAsync(folder, false, "remote", "set-url", remote.Name, remote.Url);
        }
    }

    /// <summary>
    /// 在临时带凭据的远程 URL 作用域内执行操作。
    /// 完成后立即恢复干净的 URL，避免凭据被持久化到 .git/config。
    /// </summary>
    private async Task<GitResult> RunInCredentialScopeAsync(string folder, RemoteInfo remote, Func<Task<GitResult>> action)
    {
        var creds = GetSessionCreds(remote);
        if (creds != null)
        {
            await RunAsync(folder, false, "remote", "set-url", remote.Name, WithCreds(remote.Url, creds));
        }

        try
        {
            return await action();
        }
        finally
        {
            if (creds != null)
            {
                await RunAsync(folder, false, "remote", "set-url", remote.Name, remote.Url);
            }
        }
    }

    /// <summary>把该仓库自己的用户名密码嵌入 URL（仅用于内存中的临时认证，不会落盘）。</summary>
    private string WithCreds(string url, GitSessionCredentials creds)
    {
        try
        {
            var builder = new UriBuilder(new Uri(url));
            builder.UserName = Uri.EscapeDataString(creds.UserName);
            builder.Password = Uri.EscapeDataString(creds.Password);
            return builder.Uri.ToString().TrimEnd('/');
        }
        catch
        {
            return url;
        }
    }

    // ==================== 拉取 / 推送（带认证重试） ====================

    private async Task<PullOutcome> PullWithAuthAsync(string folder, RemoteInfo remote, string branch, bool autoResolveConflicts = false)
    {
        var authTried = false;
        CredentialEntry? entered = null;
        while (true)
        {
            // 首次提交时加 -X theirs 自动解决冲突（如远程已有 README 与本地不同），
            // 用远程版本覆盖本地冲突文件，避免手动解决合并冲突
            var pullArgs = autoResolveConflicts
                ? new[] { "pull", "-X", "theirs", remote.Name, branch, "--allow-unrelated-histories" }
                : new[] { "pull", remote.Name, branch, "--allow-unrelated-histories" };

            var result = await RunInCredentialScopeAsync(folder, remote,
                () => RunAsync(folder, false, pullArgs));

            if (result.Success)
            {
                // 本次会话中弹窗填过凭据且勾选"记住" → 认证已通过，通知保存
                if (authTried && entered is { Remember: true })
                    CredentialsAccepted?.Invoke(remote, entered);
                StatusChanged?.Invoke($"已从 {remote.Name} 拉取最新代码。");
                return PullOutcome.Ok;
            }

            if (result.Output.Contains("Couldn't find remote ref", StringComparison.OrdinalIgnoreCase)
                || result.Output.Contains("couldn't find remote ref", StringComparison.OrdinalIgnoreCase)
                || result.Output.Contains("no matching remote name", StringComparison.OrdinalIgnoreCase))
            {
                StatusChanged?.Invoke("云端仓库为空，跳过拉取，直接推送。");
                return PullOutcome.NoRemoteRef;
            }

            if (result.AuthFailed)
            {
                if (authTried)
                {
                    Error($"身份验证失败，请检查该仓库的用户名密码：\n{result.Output.Trim()}");
                    return PullOutcome.Failed;
                }
                var entry = AuthRequired == null ? null : await AuthRequired(remote);
                if (entry != null)
                {
                    SetSessionCredentials(remote, entry.UserName, entry.Password);
                    authTried = true;
                    entered = entry;
                    continue;
                }
                return PullOutcome.Failed;
            }

            if (result.Output.Contains("CONFLICT", StringComparison.OrdinalIgnoreCase)
                || result.Output.Contains("merge conflict", StringComparison.OrdinalIgnoreCase))
            {
                if (autoResolveConflicts)
                {
                    Error($"自动合并冲突失败（已尝试用远程版本覆盖，仍有冲突）：\n{result.Output.Trim()}");
                }
                else
                {
                    Error($"拉取失败（存在冲突，请先在本地手动解决冲突后再提交推送）：\n{result.Output.Trim()}");
                }
                return PullOutcome.Failed;
            }

            Error($"拉取失败：\n{result.Output.Trim()}");
            return PullOutcome.Failed;
        }
    }

    private async Task<bool> PushWithAuthAsync(string folder, RemoteInfo remote, string branch, bool setUpstream)
    {
        var authTried = false;
        CredentialEntry? entered = null;
        while (true)
        {
            var result = await RunInCredentialScopeAsync(folder, remote, () =>
                RunAsync(folder, false, setUpstream
                    ? new[] { "push", "-u", remote.Name, branch }
                    : new[] { "push", remote.Name, branch }));

            if (result.Success)
            {
                // 本次会话中弹窗填过凭据且勾选"记住" → 认证已通过，通知保存
                if (authTried && entered is { Remember: true })
                    CredentialsAccepted?.Invoke(remote, entered);
                Success($"✔ 已推送到 {remote.Name}（{remote.Url}）");
                return true;
            }

            if (result.AuthFailed)
            {
                if (authTried)
                {
                    Error($"身份验证失败，请检查该仓库的用户名密码：\n{result.Output.Trim()}");
                    return false;
                }
                var entry = AuthRequired == null ? null : await AuthRequired(remote);
                if (entry != null)
                {
                    SetSessionCredentials(remote, entry.UserName, entry.Password);
                    authTried = true;
                    continue;
                }
                return false;
            }

            Error($"推送到 {remote.Name} 失败：\n{result.Output.Trim()}");
            return false;
        }
    }

    // ==================== 高层业务操作 ====================

    private async Task<GitResult> StageAndCommitAsync(string folder, string message)
    {
        StatusChanged?.Invoke("暂存所有更改...");
        LogCommand("git add .");
        var add = await RunAsync(folder, true, "add", ".");
        if (!add.Success)
        {
            Error($"git add 失败：{add.Output.Trim()}");
            return add;
        }

        StatusChanged?.Invoke("创建本地提交...");
        LogCommand($"git commit -m \"{message}\"");
        var commit = await RunAsync(folder, true, "commit", "-m", message);
        if (!commit.Success)
        {
            if (commit.Output.Contains("nothing to commit", StringComparison.OrdinalIgnoreCase))
            {
                Warn("没有需要提交的更改（nothing to commit），继续执行后续步骤。");
                return new GitResult { ExitCode = 0 };
            }
            Error($"git commit 失败：\n{commit.Output.Trim()}");
        }
        return commit;
    }

    /// <summary>
    /// 智能提交并推送：
    /// - 首次提交（仓库刚初始化）：add -> commit -> 绑定远程 -> pull 合并云端历史 -> push -u；
    /// - 更新提交：add -> commit -> push 到所有远程。
    /// </summary>
    public async Task<bool> CommitAndPushAsync(string folder, string message, bool isFirstCommit, IReadOnlyList<RemoteInfo> remotes)
    {
        var commit = await StageAndCommitAsync(folder, message);
        if (!commit.Success) return false;

        if (remotes.Count == 0)
        {
            Warn("未配置远程仓库，已完成本地提交（可在下方添加远程仓库后推送）。");
            return true;
        }

        foreach (var remote in remotes)
            await EnsureRemoteAsync(folder, remote);

        var branch = await GetBranchAsync(folder);

        // 推送前自动拉取云端最新内容：
        // 首次提交合并云端历史；之后增量提交也先拉取，避免推送被拒，全程无需手动操作
        StatusChanged?.Invoke(isFirstCommit ? "首次提交模式：合并云端历史..." : "自动拉取云端最新内容...");
        var pull = await PullWithAuthAsync(folder, remotes[0], branch, autoResolveConflicts: isFirstCommit);
        if (pull == PullOutcome.Failed) return false;

        foreach (var remote in remotes)
        {
            if (!await PushWithAuthAsync(folder, remote, branch, setUpstream: isFirstCommit))
                return false;
        }

        Success("✔ 全部操作完成。");
        return true;
    }

    /// <summary>仅本地提交（不推送）。</summary>
    public async Task<bool> CommitOnlyAsync(string folder, string message)
    {
        var commit = await StageAndCommitAsync(folder, message);
        if (commit.Success) Success("✔ 本地提交完成。");
        return commit.Success;
    }

    /// <summary>向所有已配置远程仓库推送当前分支。</summary>
    public async Task<bool> PushAllAsync(string folder, IReadOnlyList<RemoteInfo> remotes)
    {
        if (remotes.Count == 0)
        {
            Warn("未配置任何远程仓库。");
            return false;
        }

        foreach (var remote in remotes)
            await EnsureRemoteAsync(folder, remote);

        var branch = await GetBranchAsync(folder);
        foreach (var remote in remotes)
        {
            if (!await PushWithAuthAsync(folder, remote, branch, setUpstream: false))
                return false;
        }
        Success("✔ 已推送到全部远程仓库。");
        return true;
    }

    /// <summary>从第一个远程拉取更新到当前分支。</summary>
    public async Task<bool> PullAllAsync(string folder, IReadOnlyList<RemoteInfo> remotes)
    {
        if (remotes.Count == 0)
        {
            Warn("未配置任何远程仓库。");
            return false;
        }
        foreach (var remote in remotes)
            await EnsureRemoteAsync(folder, remote);

        var branch = await GetBranchAsync(folder);
        var outcome = await PullWithAuthAsync(folder, remotes[0], branch);
        if (outcome == PullOutcome.Failed) return false;
        Success("✔ 拉取完成。");
        return true;
    }

    // ==================== 日志（按级别着色） ====================

    public void LogCommand(string command) =>
        OutputReceived?.Invoke(new GitOutputLine { Kind = OutputKind.Command, Text = command });

    private void Warn(string msg) =>
        OutputReceived?.Invoke(new GitOutputLine { Kind = OutputKind.Warn, Text = msg });

    private void Error(string msg) =>
        OutputReceived?.Invoke(new GitOutputLine { Kind = OutputKind.Error, Text = msg });

    private void Success(string msg) =>
        OutputReceived?.Invoke(new GitOutputLine { Kind = OutputKind.Success, Text = msg });
}
