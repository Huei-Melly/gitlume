using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitLume.Core;

namespace GitLume.UI;

/// <summary>
/// 主窗体：全局配置 → 本地仓库（自动判断是否初始化过）→ 远程仓库管理 → 一键提交推送 → 执行日志。
/// 控件布局在 MainForm.Designer.cs（可在 VS2022 设计器中拖拽编辑），本文件只保留业务逻辑。
/// </summary>
public partial class MainForm : Form
{
    private readonly GitSettings _settings = ConfigStore.Load();
    private readonly GitService _service = new();
    private RepoStatus _repoStatus = RepoStatus.Unknown;
    private bool _busy;

    // 日志字体静态缓存：每行日志若 new Font 会造成 GDI 对象持续泄漏
    private static readonly Font _logMonoFont = Theme.FontMono(9.5f);
    private static readonly Font _logLabelFont = Theme.FontLabel(9f);
    private int _logLines;

    // 配置防抖保存：停止输入 500ms 后才写盘，避免每敲一个字符就写一次磁盘
    private readonly System.Windows.Forms.Timer _saveDebounce = new() { Interval = 500 };

    public MainForm()
    {
        InitializeComponent();
        WireEvents();

        // 配置防抖：timer 触发时执行一次保存
        _saveDebounce.Tick += (_, _) => { _saveDebounce.Stop(); SaveSettings(); };

        // 初始按当前窗口宽度校准可伸缩控件（列表宽度 / 备注输入框宽度）
        UpdateRemoteListWidth();
        UpdateCommitMessageWidth();

        LoadSettingsToUi();
        LoadSavedCredentials();
        // 启动时若已填过用户名/邮箱，自动补写一次全局 git 配置（替代原「保存」按钮）
        _ = AutoSaveGlobalConfigAsync();
    }

    /// <summary>列表宽度 = 按钮左缘 - 32px，确保任何窗口宽度下列表和按钮都不重叠。</summary>
    private void UpdateRemoteListWidth()
    {
        _remoteList.Width = _btnAddRemote.Left - 32;
        if (_remoteList.Columns.Count >= 2)
            _remoteList.Columns[1].Width = Math.Max(200, _remoteList.Width - _remoteList.Columns[0].Width - 4);
    }

    /// <summary>备注输入框宽度 = 「提交并推送」左缘 - 24px，任何窗口宽度下都不与按钮重叠。</summary>
    private void UpdateCommitMessageWidth()
    {
        _txtMessage.Width = Math.Max(200, _btnSmartPush.Left - 24);
    }

    // ==================== 事件与业务 ====================

    private void WireEvents()
    {
        // 全局配置自动保存：输入时存本地，离开输入框时写入全局 git 配置
        _txtName.TextChanged += (_, _) => AutoSaveLocalConfig();
        _txtEmail.TextChanged += (_, _) => AutoSaveLocalConfig();
        _txtName.Leave += async (_, _) => await AutoSaveGlobalConfigAsync();
        _txtEmail.Leave += async (_, _) => await AutoSaveGlobalConfigAsync();

        _btnBrowse.Click += OnBrowse;
        _btnDetect.Click += async (_, _) => await DetectStatusAsync();
        _btnAddRemote.Click += OnAddRemote;
        _btnEditRemote.Click += OnEditRemote;
        _btnDeleteRemote.Click += OnDeleteRemote;
        _btnLoadRemotes.Click += async (_, _) => await OnLoadRemotes();
        _btnSmartPush.Click += async (_, _) => await OnSmartPush();
        _btnClearLog.Click += (_, _) => { _console.Clear(); _console.ScrollToCaret(); _logLines = 0; };
        _remoteList.DoubleClick += (_, _) => OnEditRemote(null, EventArgs.Empty);

        // 窗口缩放时同步校准列表宽度与备注输入框宽度
        _panelRemotes.Resize += (_, _) => UpdateRemoteListWidth();
        _panelCommit.Resize += (_, _) => UpdateCommitMessageWidth();

        FormClosing += OnFormClosing;

        _service.SetIdentity(_settings.UserName, _settings.UserEmail);
        _service.OutputReceived += line => AppendConsoleLine(line);
        _service.StatusChanged += msg => SafeUi(() => _statusText.Text = msg);
        _service.AuthRequired = OnAuthRequired;
        _service.CredentialsAccepted += OnCredentialsAccepted;

        // 代理按钮：点击切换开/关状态，并应用配置
        _btnProxyToggle.Click += (_, _) =>
        {
            bool isOn = _btnProxyToggle.Text == "开启";
            _btnProxyToggle.Text = isOn ? "关闭" : "开启";
            _btnProxyToggle.Accent = false;
            _btnProxyToggle.Danger = !isOn;
            _btnProxyToggle.Invalidate();
            UpdateProxyAppearance();
            ApplyProxyConfig();
            LogProxyStatus();
            _saveDebounce.Stop();
            _saveDebounce.Start();
        };
        _txtProxyPort.TextChanged += (_, _) => { if (_btnProxyToggle.Text == "开启") ApplyProxyConfig(); _saveDebounce.Stop(); _saveDebounce.Start(); };
    }

    /// <summary>启动时把已保存（加密）的凭据解密载入会话：每个远程各用各的账号密码，推送时自动使用、不再弹窗。</summary>
    private void LoadSavedCredentials()
    {
        // 旧版"单一凭据"迁移：若历史配置里只有一个远程，把旧凭据绑定到它
        if (_settings.CredentialsByUrl.Count == 0
            && !string.IsNullOrEmpty(_settings.CredentialPassword)
            && _settings.Remotes.Count == 1)
        {
            var legacyKey = GitService.NormalizeUrl(_settings.Remotes[0].Url);
            _settings.CredentialsByUrl[legacyKey] = new StoredCredential
            {
                Username = _settings.CredentialUsername,
                Password = _settings.CredentialPassword,
            };
            SaveSettings();
        }

        _service.LoadSavedCredentials(_settings.CredentialsByUrl);
        if (_settings.CredentialsByUrl.Count > 0)
            AppendConsoleLine(new GitOutputLine { Kind = OutputKind.Info, Text = $"已加载 {_settings.CredentialsByUrl.Count} 个远程仓库的保存凭据（推送时自动使用，无需再输密码）。" });
    }

    /// <summary>
    /// 某个远程需要登录时弹窗；窗口会明确显示当前是哪个仓库（名称 + URL）。
    /// 此回调可能在 git 后台线程触发，必须编组到 UI 线程再弹窗，否则跨线程操作会抛异常。
    /// 注意：凭据不在弹窗时保存，等认证真正成功后（见 OnCredentialsAccepted）才落盘，避免记住错误密码。
    /// </summary>
    private Task<CredentialEntry?> OnAuthRequired(RemoteInfo remote)
    {
        var tcs = new TaskCompletionSource<CredentialEntry?>(TaskCreationOptions.RunContinuationsAsynchronously);
        SafeUi(() =>
        {
            try
            {
                var key = GitService.NormalizeUrl(remote.Url);
                _settings.CredentialsByUrl.TryGetValue(key, out var saved);
                bool savedExists = saved != null && !string.IsNullOrEmpty(saved.Password);

                AppendConsoleLine(new GitOutputLine
                {
                    Kind = OutputKind.Warn,
                    Text = $"远程仓库「{remote.Name}」（{remote.Url}）需要登录，请在弹出的窗口中输入该仓库的用户名和密码。",
                });

                using var dlg = new CredentialDialog(remote.Name, remote.Url, saved?.Username ?? "", savedExists);
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    tcs.SetResult(null);
                    return;
                }

                var entry = new CredentialEntry { UserName = dlg.UserName, Password = dlg.Password, Remember = dlg.Remember };
                if (string.IsNullOrEmpty(entry.Password) && savedExists)
                    entry.Password = SecureCodec.Decrypt(saved!.Password);
                tcs.SetResult(entry);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    /// <summary>认证真正通过后保存凭据（加密落盘），保证存的一定是能用的密码。可能在后台线程触发。</summary>
    private void OnCredentialsAccepted(RemoteInfo remote, CredentialEntry entry)
    {
        SafeUi(() =>
        {
            var key = GitService.NormalizeUrl(remote.Url);
            _settings.CredentialsByUrl[key] = new StoredCredential
            {
                Username = entry.UserName,
                Password = SecureCodec.Encrypt(entry.Password),
            };
            SaveSettings();
            AppendConsoleLine(new GitOutputLine
            {
                Kind = OutputKind.Success,
                Text = $"✔ 远程仓库「{remote.Name}」的登录凭据已验证并加密保存（下次自动使用，无需再输）。",
            });
        });
    }

    private void LoadSettingsToUi()
    {
        // 只恢复必要配置：用户名、邮箱、远程仓库列表、代理设置；不恢复上次的文件夹路径
        _txtName.Text = _settings.UserName;
        _txtEmail.Text = _settings.UserEmail;
        _txtProxyPort.Text = _settings.ProxyPort.ToString();
        bool proxyOn = _settings.ProxyEnabled;
        _btnProxyToggle.Text = proxyOn ? "开启" : "关闭";
        _btnProxyToggle.Accent = false;
        _btnProxyToggle.Danger = proxyOn;
        _btnProxyToggle.Invalidate();
        UpdateProxyAppearance();
        RefreshRemoteList(_settings.Remotes);
        SetStatusChip(RepoStatus.Unknown);
        ApplyProxyConfig();
        LogProxyStatus();

        // 如果 config.json 中邮箱为空，从 git 全局配置读取并自动填充
        if (string.IsNullOrWhiteSpace(_settings.UserEmail))
            _ = LoadGlobalEmailAsync();
    }

    private async Task LoadGlobalEmailAsync()
    {
        var email = await _service.GetGlobalConfigAsync("user.email");
        if (!string.IsNullOrEmpty(email) && string.IsNullOrWhiteSpace(_txtEmail.Text))
        {
            _txtEmail.Text = email;
            _settings.UserEmail = email;
            _saveDebounce.Stop();
            _saveDebounce.Start();
        }
    }

    private void SaveSettings()
    {
        SyncRemotesFromList();
        _settings.UserName = _txtName.Text.Trim();
        _settings.UserEmail = _txtEmail.Text.Trim();
        _settings.ProxyEnabled = _btnProxyToggle.Text == "开启";
        if (int.TryParse(_txtProxyPort.Text.Trim(), out var port) && port > 0 && port < 65536)
            _settings.ProxyPort = port;
        ConfigStore.Save(_settings);
    }

    // ---------- 全局配置（自动保存） ----------

    /// <summary>用户名/邮箱变化时实时写入本地配置，提交推送等操作自动使用，无需手动保存。</summary>
    private void AutoSaveLocalConfig()
    {
        _settings.UserName = _txtName.Text.Trim();
        _settings.UserEmail = _txtEmail.Text.Trim();
        _service.SetIdentity(_settings.UserName, _settings.UserEmail);
        // 防抖保存：停止输入 500ms 后自动写盘
        _saveDebounce.Stop();
        _saveDebounce.Start();
    }

    /// <summary>离开输入框时自动写入全局 git 配置（~/.gitconfig），以后所有项目不用再输。</summary>
    private async Task AutoSaveGlobalConfigAsync()
    {
        var name = _txtName.Text.Trim();
        var email = _txtEmail.Text.Trim();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email)) return;
        try
        {
            var r = await _service.EnsureGlobalConfigAsync(name, email);
            if (r.Success)
                AppendConsoleLine(new GitOutputLine { Kind = OutputKind.Success, Text = "✔ 全局配置已自动保存（以后所有项目不用再输）" });
        }
        catch
        {
            // 后台写入全局配置失败时忽略，不影响主流程
        }
    }

    // ---------- 本地仓库 ----------

    private void OnBrowse(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "选择你的项目文件夹",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(_txtFolder.Text) ? _txtFolder.Text : AppContext.BaseDirectory,
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _txtFolder.Text = dlg.SelectedPath;
            _ = DetectStatusAsync();
        }
    }

    private async Task DetectStatusAsync()
    {
        var folder = _txtFolder.Text.Trim();
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            SetStatusChip(RepoStatus.Unknown);
            UpdateStatusBar("请选择一个有效的项目文件夹。");
            return;
        }

        SetBusy(true, "正在检测仓库状态...");
        try
        {
            _repoStatus = await _service.DetectRepoStatusAsync(folder);
            SetStatusChip(_repoStatus);

            var remotes = await _service.GetRemotesAsync(folder);
            if (remotes.Count > 0)
            {
                RefreshRemoteList(remotes);
                SaveSettings();
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetStatusChip(RepoStatus status)
    {
        _repoStatus = status;
        (string text, Color color) = status switch
        {
            RepoStatus.NotGitRepo => ("○ 未初始化", Theme.Warn),
            RepoStatus.EmptyRepo => ("● 已初始化 · 还没提交过", Theme.Warn),
            RepoStatus.HasCommits => ("● 已有提交历史", Theme.Success),
            _ => ("○ 尚未选择文件夹", Theme.TextDim),
        };
        _statusLabel.Text = text;
        _statusLabel.ForeColor = color;
        // 文本变化后按新宽度重新贴右缘，保持右边距 16，防止超出面板
        _statusLabel.Size = TextRenderer.MeasureText(text, _statusLabel.Font);
        if (_statusLabel.Parent != null)
            _statusLabel.Location = new Point(_statusLabel.Parent.Width - _statusLabel.Width - 16, 40);

        _guidance.Text = status switch
        {
            RepoStatus.NotGitRepo => "这个文件夹还没有初始化。直接点下方「提交并推送」，我会自动帮你初始化（只需这一次），然后首次上传。",
            RepoStatus.EmptyRepo => "已经初始化过了，还没提交过。点「提交并推送」即可首次上传（会自动合并云端已有内容）。",
            RepoStatus.HasCommits => "正常仓库。改完代码后点「提交并推送」即可上传更新。",
            _ => "提示：选择你的项目文件夹，我会自动判断是否初始化过，你只需点下方「提交并推送」。",
        };
    }

    // ---------- 远程仓库管理 ----------

    private void SyncRemotesFromList()
    {
        var list = new List<RemoteInfo>();
        foreach (ListViewItem item in _remoteList.Items)
        {
            if (item.Tag is RemoteInfo r)
                list.Add(new RemoteInfo { Name = r.Name, Url = r.Url });
        }
        _settings.Remotes = list;
    }

    private void RefreshRemoteList(IEnumerable<RemoteInfo> remotes)
    {
        _remoteList.BeginUpdate();
        _remoteList.Items.Clear();
        foreach (var r in remotes)
        {
            var item = new ListViewItem(new[] { r.Name, r.Url }) { Tag = r };
            item.UseItemStyleForSubItems = false;
            item.SubItems[0].ForeColor = Theme.Accent;
            item.SubItems[1].ForeColor = Theme.Text;
            _remoteList.Items.Add(item);
        }
        _remoteList.EndUpdate();
    }

    private RemoteInfo? SelectedRemote()
    {
        if (_remoteList.SelectedItems.Count == 0) return null;
        return _remoteList.SelectedItems[0].Tag as RemoteInfo;
    }

    private void OnAddRemote(object? sender, EventArgs e)
    {
        var nextName = "origin";
        var names = _remoteList.Items.Cast<ListViewItem>().Select(i => ((RemoteInfo)i.Tag!).Name).ToHashSet();
        for (int n = 2; names.Contains(nextName); n++)
            nextName = n == 2 ? "origin2" : $"origin{n}";

        if (ShowRemoteDialog(null, nextName, out var remote))
        {
            _remoteList.Items.Add(CreateRemoteItem(remote));
            SaveSettings();
            AppendConsoleLine(new GitOutputLine { Kind = OutputKind.Info, Text = $"已添加远程仓库：{remote.Name} → {remote.Url}" });
        }
    }

    private void OnEditRemote(object? sender, EventArgs e)
    {
        var selected = SelectedRemote();
        if (selected == null)
        {
            ShowWarn("请先在列表中选择要编辑的远程仓库。");
            return;
        }

        if (ShowRemoteDialog(selected, selected.Name, out var remote))
        {
            var item = _remoteList.SelectedItems[0];
            item.Tag = remote;
            item.Text = remote.Name;
            item.SubItems[1].Text = remote.Url;
            SaveSettings();
        }
    }

    private void OnDeleteRemote(object? sender, EventArgs e)
    {
        var selected = SelectedRemote();
        if (selected == null)
        {
            ShowWarn("请先在列表中选择要删除的远程仓库。");
            return;
        }

        if (MessageBox.Show(this, $"确定删除远程仓库 {selected.Name}（{selected.Url}）吗？", "删除远程",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _remoteList.Items.Remove(_remoteList.SelectedItems[0]);
            SaveSettings();
        }
    }

    private async Task OnLoadRemotes()
    {
        var folder = _txtFolder.Text.Trim();
        if (!Directory.Exists(folder))
        {
            ShowWarn("请先选择项目文件夹。");
            return;
        }
        SetBusy(true, "正在读取仓库远程配置...");
        try
        {
            var remotes = await _service.GetRemotesAsync(folder);
            if (remotes.Count == 0)
            {
                ShowWarn("该仓库尚未配置远程（git remote 为空）。");
            }
            else
            {
                RefreshRemoteList(remotes);
                SaveSettings();
                AppendConsoleLine(new GitOutputLine { Kind = OutputKind.Success, Text = $"✔ 已读取 {remotes.Count} 个远程仓库" });
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private ListViewItem CreateRemoteItem(RemoteInfo r)
    {
        var item = new ListViewItem(new[] { r.Name, r.Url }) { Tag = r };
        item.UseItemStyleForSubItems = false;
        item.SubItems[0].ForeColor = Theme.Accent;
        item.SubItems[1].ForeColor = Theme.Text;
        return item;
    }

    private bool ShowRemoteDialog(RemoteInfo? existing, string defaultName, out RemoteInfo result)
    {
        var captured = new RemoteInfo();
        using var dlg = new Form
        {
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(520, 220),
            BackColor = Theme.Bg,
            Text = existing == null ? "添加远程仓库" : "编辑远程仓库",
        };

        var name = new SciField("远程名称（如 origin，一般填 origin 即可）") { Location = new Point(20, 22), Width = 480 };
        var url = new SciField("仓库 URL（https://gitee.com/你的账号/仓库名.git）") { Location = new Point(20, 76), Width = 480 };
        name.Text = defaultName;
        url.Text = existing?.Url ?? "";

        var ok = new SciButton { Text = "保  存", Accent = true, Location = new Point(286, 162), Size = new Size(96, 36) };
        var cancel = new SciButton { Text = "取  消", Location = new Point(396, 162), Size = new Size(96, 36) };

        ok.Click += (_, _) =>
        {
            var n = name.Text.Trim();
            var u = url.Text.Trim();
            if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(u))
            {
                MessageBox.Show(dlg, "请填写远程名称和 URL。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (existing == null || !string.Equals(existing.Name, n, StringComparison.OrdinalIgnoreCase))
            {
                bool duplicate = _remoteList.Items.Cast<ListViewItem>()
                    .Any(i => string.Equals(((RemoteInfo)i.Tag!).Name, n, StringComparison.OrdinalIgnoreCase));
                if (duplicate)
                {
                    MessageBox.Show(dlg, "已存在同名远程仓库，请换一个名称。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            captured.Name = n;
            captured.Url = u;
            dlg.DialogResult = DialogResult.OK;
        };
        cancel.Click += (_, _) => dlg.DialogResult = DialogResult.Cancel;
        dlg.AcceptButton = ok;
        dlg.CancelButton = cancel;
        dlg.Controls.AddRange(new Control[] { name, url, ok, cancel });

        bool done = dlg.ShowDialog(this) == DialogResult.OK;
        result = captured;
        return done;
    }

    // ---------- 提交 / 推送 ----------

    private async Task OnSmartPush()
    {
        if (_busy)
        {
            ShowWarn("正在执行其他操作（如检测仓库），请稍候再试。");
            return;
        }
        var folder = _txtFolder.Text.Trim();
        if (!Directory.Exists(folder))
        {
            ShowWarn("请先选择你的项目文件夹。");
            _btnBrowse.PerformClick();
            return;
        }

        var status = _repoStatus;
        if (status is RepoStatus.Unknown or RepoStatus.NotGitRepo)
            status = await _service.DetectRepoStatusAsync(folder);

        // 自动判断：没初始化过就自动初始化（git init 只需要做一次）
        if (status == RepoStatus.NotGitRepo)
        {
            AppendConsoleLine(new GitOutputLine { Kind = OutputKind.Info, Text = "检测到该文件夹尚未初始化，自动执行 git init（只需这一次）..." });
            var init = await _service.InitRepoAsync(folder);
            if (!init.Success)
            {
                AppendConsoleLine(new GitOutputLine { Kind = OutputKind.Error, Text = init.Output });
                return;
            }
            status = RepoStatus.EmptyRepo;
        }

        var message = _txtMessage.Text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            ShowWarn("请先填写本次修改的备注。");
            _txtMessage.FocusBox();
            return;
        }
        if (string.IsNullOrWhiteSpace(_settings.UserName) || string.IsNullOrWhiteSpace(_settings.UserEmail))
        {
            ShowWarn("请先在顶部填写用户名和邮箱（自动保存，无需手动保存）。");
            _txtName.FocusBox();
            return;
        }

        SaveSettings();

        bool isFirst = status == RepoStatus.EmptyRepo;
        SetBusy(true, isFirst ? "首次提交：提交 → 合并云端 → 推送..." : "提交 → 拉取云端 → 推送...");
        try
        {
            bool ok = await _service.CommitAndPushAsync(folder, message, isFirst, _settings.Remotes);
            _repoStatus = await _service.DetectRepoStatusAsync(folder);
            SetStatusChip(_repoStatus);
            UpdateStatusBar(ok ? "✔ 操作完成" : "✖ 操作未完成，请查看日志");
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ---------- 状态 / 日志 / 忙碌 ----------

    private void SetBusy(bool busy, string? statusText = null)
    {
        _busy = busy;
        UpdateStatusBar(statusText ?? "");
        if (busy)
        {
            _progress.StartFlow();
            _progress.Visible = true;
        }
        else
        {
            _progress.StopFlow();
            _progress.Visible = false;
            if (_statusText.Text.StartsWith("正在") || _statusText.Text == "就绪")
                UpdateStatusBar("就绪");
        }

        bool enable = !busy;
        foreach (var c in new Control[] { _btnBrowse, _btnDetect, _btnAddRemote, _btnEditRemote, _btnDeleteRemote, _btnLoadRemotes, _btnSmartPush, _txtName, _txtEmail, _txtFolder, _txtMessage, _txtProxyPort, _btnProxyToggle })
            c.Enabled = enable;

        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void UpdateStatusBar(string text) => SafeUi(() => _statusText.Text = text ?? "");

    private void SafeUi(Action action)
    {
        if (IsDisposed) return;
        try
        {
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
        catch (ObjectDisposedException) { /* 窗体已释放，忽略 */ }
        catch (InvalidOperationException) { /* 句柄尚未创建，忽略 */ }
    }

    private void AppendConsoleLine(GitOutputLine line)
    {
        SafeUi(() =>
        {
            // 日志行数上限：超过后删除最旧的 200 行，防止长期运行导致界面越用越卡
            const int maxLines = 600;
            const int trimLines = 200;
            if (_logLines >= maxLines)
            {
                var firstChar = _console.GetFirstCharIndexFromLine(trimLines);
                if (firstChar > 0)
                {
                    _console.Select(0, firstChar);
                    _console.SelectedText = "";
                    _logLines -= trimLines;
                }
            }

            var (color, font, prefix) = line.Kind switch
            {
                OutputKind.Command => (Theme.Accent, _logMonoFont, "❯ "),
                OutputKind.Error => (Theme.Danger, _logLabelFont, "✖ "),
                OutputKind.Success => (Theme.Success, _logLabelFont, "✔ "),
                OutputKind.Warn => (Theme.Warn, _logLabelFont, "⚠ "),
                _ => (Color.FromArgb(200, 209, 220), _logLabelFont, ""),
            };
            _console.SelectionStart = _console.TextLength;
            _console.SelectionLength = 0;
            _console.SelectionColor = color;
            _console.SelectionFont = font;
            _console.AppendText(prefix + line.Text + "\n");
            _logLines++;
            _console.ScrollToCaret();
        });
    }

    private void ShowWarn(string msg) =>
        MessageBox.Show(this, msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    /// <summary>根据当前 UI 状态应用或清除代理设置。</summary>
    private void ApplyProxyConfig()
    {
        bool proxyOn = _btnProxyToggle.Text == "开启";
        if (proxyOn && int.TryParse(_txtProxyPort.Text.Trim(), out var port) && port > 0 && port < 65536)
        {
            _service.EnableProxy("127.0.0.1", port);
        }
        else
        {
            _service.DisableProxy();
        }
    }

    /// <summary>代理开启时全红醒目样式，关闭时恢复默认。</summary>
    private void UpdateProxyAppearance()
    {
        bool proxyOn = _btnProxyToggle.Text == "开启";
        if (proxyOn)
        {
            _lblProxyLabel.ForeColor = Theme.Danger;
            _txtProxyPort.BackColor = Color.FromArgb(50, 18, 20);
            _txtProxyPort.ForeColor = Theme.Danger;
        }
        else
        {
            _lblProxyLabel.ForeColor = Color.FromArgb(139, 148, 161);
            _txtProxyPort.BackColor = Color.FromArgb(12, 15, 19);
            _txtProxyPort.ForeColor = Color.FromArgb(230, 237, 243);
        }
    }

    /// <summary>在日志中用红色醒目提示当前代理端口状态。</summary>
    private void LogProxyStatus()
    {
        bool proxyOn = _btnProxyToggle.Text == "开启";
        var port = _txtProxyPort.Text.Trim();
        SafeUi(() =>
        {
            _console.SelectionStart = _console.TextLength;
            _console.SelectionLength = 0;
            _console.SelectionColor = proxyOn ? Theme.Danger : Color.FromArgb(139, 148, 161);
            _console.SelectionFont = _logLabelFont;
            var msg = proxyOn ? $"🔴 代理端口已开启（127.0.0.1:{port}）" : $"代理端口已关闭（127.0.0.1:{port}）";
            _console.AppendText(msg + "\n");
            _logLines++;
            _console.ScrollToCaret();
        });
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _progress.StopFlow();   // 停止进度条动画计时器，避免退出后仍在空转
        _service.Shutdown();    // 终止可能仍在执行的 git 进程，不留孤儿进程
        _service.ClearCredentials();
        _saveDebounce.Stop();   // 停止防抖，直接写盘
        _saveDebounce.Dispose();// 释放防抖计时器资源
        SaveSettings();         // 从 UI 控件读取最新值（代理、远程列表等）完整保存
    }

    // ---------- 背景 ----------

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        using var pen = new Pen(Theme.Border, 1f);
        g.DrawLine(pen, 0, 50, Width, 50);
    }
}
