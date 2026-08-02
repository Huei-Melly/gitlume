using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitLume.UI;

/// <summary>
/// 主窗体设计器文件：所有控件的位置、大小、锚定在此维护，
/// 可在 VS2022 设计器视图中直接拖拽编辑。
/// </summary>
partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    /// <summary>释放资源。</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    // ---- 顶部标题 ----
    private Label _lblTitle = null!;
    private Label _lblSub = null!;
    private Label _lblVersion = null!;

    // ---- 面板 ----
    private SciPanel _panelGlobal = null!;
    private SciPanel _panelLocal = null!;
    private SciPanel _panelRemotes = null!;
    private SciPanel _panelCommit = null!;
    private SciPanel _panelConsole = null!;

    // ---- 全局配置 ----
    private SciField _txtName = null!;
    private SciField _txtEmail = null!;
    private Label _lblConfigHint = null!;

    // ---- 本地仓库 ----
    private SciField _txtFolder = null!;
    private SciButton _btnBrowse = null!;
    private SciButton _btnDetect = null!;
    private Label _statusLabel = null!;
    private Label _guidance = null!;

    // ---- 远程仓库 ----
    private ListView _remoteList = null!;
    private SciButton _btnAddRemote = null!;
    private SciButton _btnEditRemote = null!;
    private SciButton _btnDeleteRemote = null!;
    private SciButton _btnLoadRemotes = null!;

    // ---- 提交与推送 ----
    private SciField _txtMessage = null!;
    private SciButton _btnSmartPush = null!;
    private Label _lblCommitHint = null!;

    // ---- 执行日志 ----
    private RichTextBox _console = null!;
    private SciButton _btnClearLog = null!;

    // ---- 状态栏 ----
    private SciProgressBar _progress = null!;
    private Label _statusText = null!;
    private Label _lblStatusRight = null!;

    private void InitializeComponent()
    {
        _lblTitle = new Label();
        _lblSub = new Label();
        _lblVersion = new Label();
        _panelGlobal = new SciPanel();
        _txtName = new SciField();
        _txtEmail = new SciField();
        _lblConfigHint = new Label();
        _panelLocal = new SciPanel();
        _txtFolder = new SciField();
        _btnBrowse = new SciButton();
        _btnDetect = new SciButton();
        _statusLabel = new Label();
        _guidance = new Label();
        _panelRemotes = new SciPanel();
        _remoteList = new ListView();
        _btnAddRemote = new SciButton();
        _btnEditRemote = new SciButton();
        _btnDeleteRemote = new SciButton();
        _btnLoadRemotes = new SciButton();
        _panelCommit = new SciPanel();
        _txtMessage = new SciField();
        _btnSmartPush = new SciButton();
        _lblCommitHint = new Label();
        _panelConsole = new SciPanel();
        _console = new RichTextBox();
        _btnClearLog = new SciButton();
        _progress = new SciProgressBar();
        _statusText = new Label();
        _lblStatusRight = new Label();
        _panelGlobal.SuspendLayout();
        _panelLocal.SuspendLayout();
        _panelRemotes.SuspendLayout();
        _panelCommit.SuspendLayout();
        _panelConsole.SuspendLayout();
        SuspendLayout();

        // ==================== 顶部标题 ====================
        _lblTitle.AutoSize = true;
        _lblTitle.Font = Theme.FontTitle(15f);
        _lblTitle.ForeColor = Theme.Text;
        _lblTitle.Location = new Point(24, 10);
        _lblTitle.Text = "GitLume";

        _lblSub.AutoSize = true;
        _lblSub.Font = Theme.FontLabel(9f);
        _lblSub.ForeColor = Theme.TextDim;
        _lblSub.Location = new Point(100, 15);
        _lblSub.Text = "Git 桌面客户端 · 选择目录 → 填备注 → 提交推送";

        _lblVersion.AutoSize = true;
        _lblVersion.Font = Theme.FontLabel(8.5f);
        _lblVersion.ForeColor = Theme.TextDim;
        _lblVersion.Location = new Point(412, 15);
        _lblVersion.Text = "v1.3 · 简洁版";

        // ==================== 全局配置面板 ====================
        _panelGlobal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelGlobal.Location = new Point(0, 54);
        _panelGlobal.Size = new Size(960, 82);
        _panelGlobal.Title = "全局配置";

        _txtName.LabelText = "用户名 user.name";
        _txtName.Location = new Point(16, 24);
        _txtName.Size = new Size(210, 50);

        _txtEmail.LabelText = "邮箱 user.email";
        _txtEmail.Location = new Point(240, 24);
        _txtEmail.Size = new Size(300, 50);

        _lblConfigHint.Font = Theme.FontLabel(8.5f);
        _lblConfigHint.ForeColor = Theme.TextDim;
        _lblConfigHint.Location = new Point(556, 40);
        _lblConfigHint.Size = new Size(300, 15);
        _lblConfigHint.Text = "自动保存：离开输入框即写入全局配置";

        _panelGlobal.Controls.Add(_txtName);
        _panelGlobal.Controls.Add(_txtEmail);
        _panelGlobal.Controls.Add(_lblConfigHint);

        // ==================== 本地仓库面板 ====================
        _panelLocal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelLocal.Location = new Point(0, 136);
        _panelLocal.Size = new Size(960, 104);
        _panelLocal.Title = "本地仓库";

        _txtFolder.LabelText = "项目文件夹路径";
        _txtFolder.Location = new Point(16, 24);
        _txtFolder.Size = new Size(420, 50);

        _btnBrowse.Location = new Point(444, 33);
        _btnBrowse.Size = new Size(80, 32);
        _btnBrowse.Text = "选择...";

        _btnDetect.Location = new Point(532, 33);
        _btnDetect.Size = new Size(84, 32);
        _btnDetect.Text = "重新检测";

        _statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _statusLabel.Font = Theme.FontBold(10f);
        _statusLabel.ForeColor = Theme.TextDim;
        _statusLabel.Location = new Point(774, 40);
        _statusLabel.Size = new Size(170, 16);
        _statusLabel.Text = "○ 尚未选择文件夹";

        _guidance.AutoSize = true;
        _guidance.Font = Theme.FontLabel(8.5f);
        _guidance.ForeColor = Theme.TextDim;
        _guidance.Location = new Point(16, 84);
        _guidance.Text = "提示：选择你的项目文件夹，我会自动判断是否初始化过，你只需点下方「提交并推送」。";

        _panelLocal.Controls.Add(_txtFolder);
        _panelLocal.Controls.Add(_btnBrowse);
        _panelLocal.Controls.Add(_btnDetect);
        _panelLocal.Controls.Add(_statusLabel);
        _panelLocal.Controls.Add(_guidance);

        // ==================== 远程仓库面板 ====================
        _panelRemotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelRemotes.Location = new Point(0, 240);
        _panelRemotes.Size = new Size(960, 210);
        _panelRemotes.Title = "远程仓库（可添加多个，推送时会同时上传到所有仓库）";

        _remoteList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
        _remoteList.BackColor = Theme.Input;
        _remoteList.BorderStyle = BorderStyle.None;
        _remoteList.Font = Theme.FontLabel(9f);
        _remoteList.ForeColor = Theme.Text;
        _remoteList.FullRowSelect = true;
        _remoteList.GridLines = false;
        _remoteList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _remoteList.Location = new Point(16, 26);
        _remoteList.Size = new Size(762, 172);
        _remoteList.View = View.Details;
        _remoteList.Columns.Add("名称", 100);
        _remoteList.Columns.Add("URL", 600);

        _btnAddRemote.Accent = true;
        _btnAddRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnAddRemote.Location = new Point(794, 26);
        _btnAddRemote.Size = new Size(150, 32);
        _btnAddRemote.Text = "+ 添加";

        _btnEditRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnEditRemote.Location = new Point(794, 68);
        _btnEditRemote.Size = new Size(150, 32);
        _btnEditRemote.Text = "编辑";

        _btnDeleteRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnDeleteRemote.Danger = true;
        _btnDeleteRemote.Location = new Point(794, 110);
        _btnDeleteRemote.Size = new Size(150, 32);
        _btnDeleteRemote.Text = "删除";

        _btnLoadRemotes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnLoadRemotes.Location = new Point(794, 152);
        _btnLoadRemotes.Size = new Size(150, 32);
        _btnLoadRemotes.Text = "从仓库读取";

        _panelRemotes.Controls.Add(_remoteList);
        _panelRemotes.Controls.Add(_btnAddRemote);
        _panelRemotes.Controls.Add(_btnEditRemote);
        _panelRemotes.Controls.Add(_btnDeleteRemote);
        _panelRemotes.Controls.Add(_btnLoadRemotes);

        // ==================== 提交与推送面板 ====================
        _panelCommit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelCommit.Location = new Point(0, 450);
        _panelCommit.Size = new Size(960, 104);
        _panelCommit.Title = "提交与推送";

        _btnSmartPush.Accent = true;
        _btnSmartPush.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnSmartPush.Location = new Point(744, 33);
        _btnSmartPush.Size = new Size(200, 36);
        _btnSmartPush.Text = "提交并推送";

        _txtMessage.LabelText = "本次修改的备注（如：更新了登录功能）";
        _txtMessage.Location = new Point(16, 24);
        _txtMessage.Size = new Size(720, 50);

        _lblCommitHint.AutoSize = true;
        _lblCommitHint.Font = Theme.FontLabel(8.5f);
        _lblCommitHint.ForeColor = Theme.TextDim;
        _lblCommitHint.Location = new Point(16, 84);
        _lblCommitHint.Text = "自动模式：未初始化会自动 git init（只需一次）；提交后自动拉取云端最新内容再推送，全程一个按键。";

        _panelCommit.Controls.Add(_txtMessage);
        _panelCommit.Controls.Add(_btnSmartPush);
        _panelCommit.Controls.Add(_lblCommitHint);

        // ==================== 执行日志面板 ====================
        _panelConsole.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _panelConsole.Location = new Point(0, 554);
        _panelConsole.Size = new Size(960, 170);
        _panelConsole.Title = "执行日志";

        _console.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _console.BackColor = Color.FromArgb(10, 12, 16);
        _console.BorderStyle = BorderStyle.None;
        _console.DetectUrls = false;
        _console.Font = Theme.FontMono(9.5f);
        _console.ForeColor = Theme.Text;
        _console.Location = new Point(16, 36);
        _console.ReadOnly = true;
        _console.Size = new Size(928, 122);

        _btnClearLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnClearLog.Location = new Point(856, 6);
        _btnClearLog.Size = new Size(88, 24);
        _btnClearLog.Text = "清空日志";

        _panelConsole.Controls.Add(_console);
        _panelConsole.Controls.Add(_btnClearLog);

        // ==================== 状态栏 ====================
        _progress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _progress.Location = new Point(10, 737);
        _progress.Size = new Size(160, 5);

        _statusText.AutoSize = true;
        _statusText.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _statusText.Font = Theme.FontLabel(9f);
        _statusText.ForeColor = Theme.TextDim;
        _statusText.Location = new Point(180, 731);
        _statusText.Text = "就绪";

        _lblStatusRight.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _lblStatusRight.Font = Theme.FontLabel(8.5f);
        _lblStatusRight.ForeColor = Theme.TextDim;
        _lblStatusRight.Location = new Point(844, 733);
        _lblStatusRight.Size = new Size(100, 15);
        _lblStatusRight.Text = "GITLUME v1.3";

        // ==================== 窗体 ====================
        BackColor = Theme.Bg;
        ClientSize = new Size(960, 760);
        Controls.Add(_lblTitle);
        Controls.Add(_lblSub);
        Controls.Add(_lblVersion);
        Controls.Add(_panelGlobal);
        Controls.Add(_panelLocal);
        Controls.Add(_panelRemotes);
        Controls.Add(_panelCommit);
        Controls.Add(_panelConsole);
        Controls.Add(_progress);
        Controls.Add(_statusText);
        Controls.Add(_lblStatusRight);
        DoubleBuffered = true;
        Font = Theme.FontLabel(9.5f);
        MinimumSize = new Size(860, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "GitLume · Git 客户端";

        _panelGlobal.ResumeLayout(false);
        _panelLocal.ResumeLayout(false);
        _panelRemotes.ResumeLayout(false);
        _panelCommit.ResumeLayout(false);
        _panelConsole.ResumeLayout(false);
        ResumeLayout(false);
    }
}
