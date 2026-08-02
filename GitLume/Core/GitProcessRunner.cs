using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace GitLume.Core;

/// <summary>输出行级别，用于 UI 着色。</summary>
public enum OutputKind
{
    Info,
    Error,
    Command,
    Success,
    Warn,
}

/// <summary>Git 输出的一行。</summary>
public sealed class GitOutputLine
{
    public OutputKind Kind { get; set; } = OutputKind.Info;
    public bool IsError { get; set; }
    public string Text { get; set; } = "";
}

/// <summary>
/// 封装 git 进程的执行：
/// - 无窗口、完全重定向输入输出，避免黑窗口闪现；
/// - 设置 GIT_EDITOR=true / GIT_TERMINAL_PROMPT=0，禁止弹出 VIM 编辑器与交互式提示；
/// - 异步读取输出，不会因输出量大使进程死锁。
/// </summary>
public sealed class GitProcessRunner
{
    /// <summary>每行输出（stdout / stderr 统一回调）。</summary>
    public event Action<GitOutputLine>? OutputReceived;

    private readonly Dictionary<string, string> _env = new()
    {
        // 合并提交时自动采用默认信息，绝不打开 VIM
        ["GIT_EDITOR"] = "true",
        ["core.editor"] = "true",
        // 禁止 git 等待终端交互输入（如输入用户名密码），避免程序假死
        ["GIT_TERMINAL_PROMPT"] = "0",
        // 阻止 Git Credential Manager (GCM) 弹出白色 GUI 登录框；
        // 不用系统默认的凭据助手，由我们的自定义弹窗统一处理认证
        ["GCM_INTERACTIVE"] = "never",
        // 固定为英文输出，便于程序稳定识别状态
        ["LC_ALL"] = "C",
        ["LANG"] = "C",
    };

    public void SetEnv(string key, string value) => _env[key] = value;

    /// <summary>设置 HTTP 代理（用于 GitHub 等需要代理访问的平台）。</summary>
    public void SetProxy(string host, int port)
    {
        var proxy = $"http://{host}:{port}";
        _env["HTTP_PROXY"] = proxy;
        _env["HTTPS_PROXY"] = proxy;
    }

    /// <summary>清除 HTTP 代理设置。</summary>
    public void ClearProxy()
    {
        _env.Remove("HTTP_PROXY");
        _env.Remove("HTTPS_PROXY");
    }

    /// <summary>当前正在执行的 git 进程（应用退出时用于强制终止，避免留下孤儿进程）。</summary>
    private Process? _current;

    /// <summary>终止正在执行的 git 进程（若存在）。应用退出时调用，确保资源立即释放。</summary>
    public void KillCurrent()
    {
        var p = _current;
        if (p != null && !p.HasExited)
        {
            try { p.Kill(true); } catch { /* 进程已结束则忽略 */ }
        }
    }

    /// <summary>Git 命令默认超时时间（秒），超过此时间未完成则强制终止，避免网络卡死永久挂起。</summary>
    private const int DefaultTimeoutSeconds = 120;

    /// <summary>在指定工作目录执行一条 git 命令，返回退出码与输出。</summary>
    public async Task<GitResult> RunAsync(string workingDir, params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDir)
                ? AppContext.BaseDirectory
                : workingDir,
        };

        foreach (var kv in _env)
            psi.Environment[kv.Key] = kv.Value;

        foreach (var a in args)
            psi.ArgumentList.Add(a);

        var proc = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        proc.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            stdout.AppendLine(e.Data);
            OutputReceived?.Invoke(new GitOutputLine { Kind = OutputKind.Info, Text = e.Data });
        };
        proc.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            stderr.AppendLine(e.Data);
            var data = e.Data;
            // Git 把推送/拉取的进度信息也输出到 stderr（如 "From "、"To "、"remote:"、
            // " * branch"、"Already up to date" 等），这些不是错误，不应显示为红色。
            // 只有真正的错误（fatal:/error: 等）才标记为 Error。
            var kind = IsActualError(data) ? OutputKind.Error : OutputKind.Info;
            OutputReceived?.Invoke(new GitOutputLine { Kind = kind, Text = data });
        };

        static bool IsActualError(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;
            if (line.Contains("fatal:", StringComparison.OrdinalIgnoreCase)) return true;
            if (line.Contains("error:", StringComparison.OrdinalIgnoreCase)) return true;
            if (line.StartsWith("warning:", StringComparison.OrdinalIgnoreCase)) return true;
            if (line.StartsWith("hint:", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        try
        {
            if (!proc.Start())
                return new GitResult { ExitCode = -1, Output = "无法启动 git 进程。" };
        }
        catch (Exception ex)
        {
            return new GitResult
            {
                ExitCode = -1,
                Output = $"未找到 git 命令：{ex.Message}\n请先安装 Git for Windows 并加入系统 PATH。",
            };
        }

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        proc.EnableRaisingEvents = true;
        proc.Exited += (_, _) => tcs.TrySetResult(proc.ExitCode);
        if (proc.HasExited)
            tcs.TrySetResult(proc.ExitCode);

        _current = proc;
        try
        {
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                try { proc.Kill(true); } catch { /* 强制终止超时进程 */ }
                return new GitResult
                {
                    ExitCode = -1,
                    Output = $"git 命令执行超时（{DefaultTimeoutSeconds} 秒），已强制终止。\n命令：git {string.Join(" ", args)}",
                };
            }

            int exitCode = await tcs.Task;
            return new GitResult
            {
                ExitCode = exitCode,
                Output = stdout.ToString() + stderr.ToString(),
            };
        }
        finally
        {
            if (ReferenceEquals(_current, proc)) _current = null;
            // 确保异步输出读取事件全部完成后再释放（进程已退出，立即返回，不阻塞）
            proc.WaitForExit();
            proc.WaitForExit();
            proc.Dispose(); // 释放进程句柄，避免每条命令都残留一个 Process 对象
        }
    }
}
