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
        // 
        // _lblTitle
        // 
        _lblTitle.AutoSize = true;
        _lblTitle.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);
        _lblTitle.ForeColor = Color.FromArgb(230, 237, 243);
        _lblTitle.Location = new Point(24, 10);
        _lblTitle.Name = "_lblTitle";
        _lblTitle.Size = new Size(97, 27);
        _lblTitle.TabIndex = 0;
        _lblTitle.Text = "GitLume";
        // 
        // _lblSub
        // 
        _lblSub.AutoSize = true;
        _lblSub.Font = new Font("Microsoft YaHei UI", 9F);
        _lblSub.ForeColor = Color.FromArgb(139, 148, 161);
        _lblSub.Location = new Point(100, 15);
        _lblSub.Name = "_lblSub";
        _lblSub.Size = new Size(271, 17);
        _lblSub.TabIndex = 1;
        _lblSub.Text = "Git 桌面客户端 · 选择目录 → 填备注 → 提交推送";
        // 
        // _lblVersion
        // 
        _lblVersion.AutoSize = true;
        _lblVersion.Font = new Font("Microsoft YaHei UI", 8.5F);
        _lblVersion.ForeColor = Color.FromArgb(139, 148, 161);
        _lblVersion.Location = new Point(412, 15);
        _lblVersion.Name = "_lblVersion";
        _lblVersion.Size = new Size(78, 17);
        _lblVersion.TabIndex = 2;
        _lblVersion.Text = "v1.3 · 简洁版";
        // 
        // _panelGlobal
        // 
        _panelGlobal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelGlobal.BackColor = Color.FromArgb(21, 25, 32);
        _panelGlobal.Controls.Add(_txtName);
        _panelGlobal.Controls.Add(_txtEmail);
        _panelGlobal.Controls.Add(_lblConfigHint);
        _panelGlobal.Location = new Point(0, 54);
        _panelGlobal.Name = "_panelGlobal";
        _panelGlobal.Padding = new Padding(14, 30, 14, 10);
        _panelGlobal.Size = new Size(844, 82);
        _panelGlobal.TabIndex = 3;
        _panelGlobal.Title = "全局配置";
        // 
        // _txtName
        // 
        _txtName.BackColor = Color.FromArgb(21, 25, 32);
        _txtName.LabelText = "用户名 user.name";
        _txtName.Location = new Point(16, 24);
        _txtName.Name = "_txtName";
        _txtName.Size = new Size(210, 50);
        _txtName.TabIndex = 0;
        // 
        // _txtEmail
        // 
        _txtEmail.BackColor = Color.FromArgb(21, 25, 32);
        _txtEmail.LabelText = "邮箱 user.email";
        _txtEmail.Location = new Point(240, 24);
        _txtEmail.Name = "_txtEmail";
        _txtEmail.Size = new Size(300, 50);
        _txtEmail.TabIndex = 1;
        // 
        // _lblConfigHint
        // 
        _lblConfigHint.Font = new Font("Microsoft YaHei UI", 8.5F);
        _lblConfigHint.ForeColor = Color.FromArgb(139, 148, 161);
        _lblConfigHint.Location = new Point(561, 48);
        _lblConfigHint.Name = "_lblConfigHint";
        _lblConfigHint.Size = new Size(300, 15);
        _lblConfigHint.TabIndex = 2;
        _lblConfigHint.Text = "自动保存：离开输入框即写入全局配置";
        // 
        // _panelLocal
        // 
        _panelLocal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelLocal.BackColor = Color.FromArgb(21, 25, 32);
        _panelLocal.Controls.Add(_txtFolder);
        _panelLocal.Controls.Add(_btnBrowse);
        _panelLocal.Controls.Add(_btnDetect);
        _panelLocal.Controls.Add(_statusLabel);
        _panelLocal.Controls.Add(_guidance);
        _panelLocal.Location = new Point(0, 136);
        _panelLocal.Name = "_panelLocal";
        _panelLocal.Padding = new Padding(14, 30, 14, 10);
        _panelLocal.Size = new Size(844, 104);
        _panelLocal.TabIndex = 4;
        _panelLocal.Title = "本地仓库";
        // 
        // _txtFolder
        // 
        _txtFolder.BackColor = Color.FromArgb(21, 25, 32);
        _txtFolder.LabelText = "项目文件夹路径";
        _txtFolder.Location = new Point(16, 24);
        _txtFolder.Name = "_txtFolder";
        _txtFolder.Size = new Size(420, 50);
        _txtFolder.TabIndex = 0;
        // 
        // _btnBrowse
        // 
        _btnBrowse.Accent = false;
        _btnBrowse.Danger = false;
        _btnBrowse.FlatStyle = FlatStyle.Flat;
        _btnBrowse.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnBrowse.ForeColor = Color.FromArgb(230, 237, 243);
        _btnBrowse.Location = new Point(460, 42);
        _btnBrowse.Name = "_btnBrowse";
        _btnBrowse.Size = new Size(80, 32);
        _btnBrowse.TabIndex = 1;
        _btnBrowse.Text = "选择...";
        // 
        // _btnDetect
        // 
        _btnDetect.Accent = false;
        _btnDetect.Danger = false;
        _btnDetect.FlatStyle = FlatStyle.Flat;
        _btnDetect.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnDetect.ForeColor = Color.FromArgb(230, 237, 243);
        _btnDetect.Location = new Point(561, 42);
        _btnDetect.Name = "_btnDetect";
        _btnDetect.Size = new Size(84, 32);
        _btnDetect.TabIndex = 2;
        _btnDetect.Text = "重新检测";
        // 
        // _statusLabel
        // 
        _statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _statusLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        _statusLabel.ForeColor = Color.FromArgb(139, 148, 161);
        _statusLabel.Location = new Point(662, 49);
        _statusLabel.Name = "_statusLabel";
        _statusLabel.Size = new Size(170, 16);
        _statusLabel.TabIndex = 3;
        _statusLabel.Text = "○ 尚未选择文件夹";
        _statusLabel.Click += _statusLabel_Click;
        // 
        // _guidance
        // 
        _guidance.AutoSize = true;
        _guidance.Font = new Font("Microsoft YaHei UI", 8.5F);
        _guidance.ForeColor = Color.FromArgb(139, 148, 161);
        _guidance.Location = new Point(16, 84);
        _guidance.Name = "_guidance";
        _guidance.Size = new Size(488, 17);
        _guidance.TabIndex = 4;
        _guidance.Text = "提示：选择你的项目文件夹，我会自动判断是否初始化过，你只需点下方「提交并推送」。";
        // 
        // _panelRemotes
        // 
        _panelRemotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelRemotes.BackColor = Color.FromArgb(21, 25, 32);
        _panelRemotes.Controls.Add(_remoteList);
        _panelRemotes.Controls.Add(_btnAddRemote);
        _panelRemotes.Controls.Add(_btnEditRemote);
        _panelRemotes.Controls.Add(_btnDeleteRemote);
        _panelRemotes.Controls.Add(_btnLoadRemotes);
        _panelRemotes.Location = new Point(0, 240);
        _panelRemotes.Name = "_panelRemotes";
        _panelRemotes.Padding = new Padding(14, 30, 14, 10);
        _panelRemotes.Size = new Size(844, 210);
        _panelRemotes.TabIndex = 5;
        _panelRemotes.Title = "远程仓库（可添加多个，推送时会同时上传到所有仓库）";
        // 
        // _remoteList
        // 
        _remoteList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        _remoteList.BackColor = Color.FromArgb(12, 15, 19);
        _remoteList.BorderStyle = BorderStyle.None;
        _remoteList.Font = new Font("Microsoft YaHei UI", 9F);
        _remoteList.ForeColor = Color.FromArgb(230, 237, 243);
        _remoteList.FullRowSelect = true;
        _remoteList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _remoteList.Location = new Point(16, 26);
        _remoteList.Name = "_remoteList";
        _remoteList.Size = new Size(629, 172);
        _remoteList.TabIndex = 0;
        _remoteList.UseCompatibleStateImageBehavior = false;
        _remoteList.View = View.Details;
        _remoteList.Columns.Add("名称", 150);
        _remoteList.Columns.Add("URL", 450);
        // 
        // _btnAddRemote
        // 
        _btnAddRemote.Accent = true;
        _btnAddRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnAddRemote.Danger = false;
        _btnAddRemote.FlatStyle = FlatStyle.Flat;
        _btnAddRemote.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnAddRemote.ForeColor = Color.FromArgb(230, 237, 243);
        _btnAddRemote.Location = new Point(678, 26);
        _btnAddRemote.Name = "_btnAddRemote";
        _btnAddRemote.Size = new Size(150, 32);
        _btnAddRemote.TabIndex = 1;
        _btnAddRemote.Text = "+ 添加";
        // 
        // _btnEditRemote
        // 
        _btnEditRemote.Accent = false;
        _btnEditRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnEditRemote.Danger = false;
        _btnEditRemote.FlatStyle = FlatStyle.Flat;
        _btnEditRemote.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnEditRemote.ForeColor = Color.FromArgb(230, 237, 243);
        _btnEditRemote.Location = new Point(678, 68);
        _btnEditRemote.Name = "_btnEditRemote";
        _btnEditRemote.Size = new Size(150, 32);
        _btnEditRemote.TabIndex = 2;
        _btnEditRemote.Text = "编辑";
        // 
        // _btnDeleteRemote
        // 
        _btnDeleteRemote.Accent = false;
        _btnDeleteRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnDeleteRemote.Danger = true;
        _btnDeleteRemote.FlatStyle = FlatStyle.Flat;
        _btnDeleteRemote.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnDeleteRemote.ForeColor = Color.FromArgb(230, 237, 243);
        _btnDeleteRemote.Location = new Point(678, 110);
        _btnDeleteRemote.Name = "_btnDeleteRemote";
        _btnDeleteRemote.Size = new Size(150, 32);
        _btnDeleteRemote.TabIndex = 3;
        _btnDeleteRemote.Text = "删除";
        // 
        // _btnLoadRemotes
        // 
        _btnLoadRemotes.Accent = false;
        _btnLoadRemotes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnLoadRemotes.Danger = false;
        _btnLoadRemotes.FlatStyle = FlatStyle.Flat;
        _btnLoadRemotes.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnLoadRemotes.ForeColor = Color.FromArgb(230, 237, 243);
        _btnLoadRemotes.Location = new Point(678, 152);
        _btnLoadRemotes.Name = "_btnLoadRemotes";
        _btnLoadRemotes.Size = new Size(150, 32);
        _btnLoadRemotes.TabIndex = 4;
        _btnLoadRemotes.Text = "从仓库读取";
        // 
        // _panelCommit
        // 
        _panelCommit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _panelCommit.BackColor = Color.FromArgb(21, 25, 32);
        _panelCommit.Controls.Add(_txtMessage);
        _panelCommit.Controls.Add(_btnSmartPush);
        _panelCommit.Controls.Add(_lblCommitHint);
        _panelCommit.Location = new Point(0, 450);
        _panelCommit.Name = "_panelCommit";
        _panelCommit.Padding = new Padding(14, 30, 14, 10);
        _panelCommit.Size = new Size(844, 104);
        _panelCommit.TabIndex = 6;
        _panelCommit.Title = "提交与推送";
        // 
        // _txtMessage
        // 
        _txtMessage.BackColor = Color.FromArgb(21, 25, 32);
        _txtMessage.LabelText = "本次修改的备注（如：更新了登录功能）";
        _txtMessage.Location = new Point(16, 24);
        _txtMessage.Name = "_txtMessage";
        _txtMessage.Size = new Size(629, 50);
        _txtMessage.TabIndex = 0;
        // 
        // _btnSmartPush
        // 
        _btnSmartPush.Accent = true;
        _btnSmartPush.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnSmartPush.Danger = false;
        _btnSmartPush.FlatStyle = FlatStyle.Flat;
        _btnSmartPush.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnSmartPush.ForeColor = Color.FromArgb(230, 237, 243);
        _btnSmartPush.Location = new Point(678, 38);
        _btnSmartPush.Name = "_btnSmartPush";
        _btnSmartPush.Size = new Size(148, 36);
        _btnSmartPush.TabIndex = 1;
        _btnSmartPush.Text = "提交并推送";
        // 
        // _lblCommitHint
        // 
        _lblCommitHint.AutoSize = true;
        _lblCommitHint.Font = new Font("Microsoft YaHei UI", 8.5F);
        _lblCommitHint.ForeColor = Color.FromArgb(139, 148, 161);
        _lblCommitHint.Location = new Point(16, 84);
        _lblCommitHint.Name = "_lblCommitHint";
        _lblCommitHint.Size = new Size(564, 17);
        _lblCommitHint.TabIndex = 2;
        _lblCommitHint.Text = "自动模式：未初始化会自动 git init（只需一次）；提交后自动拉取云端最新内容再推送，全程一个按键。";
        // 
        // _panelConsole
        // 
        _panelConsole.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _panelConsole.BackColor = Color.FromArgb(21, 25, 32);
        _panelConsole.Controls.Add(_console);
        _panelConsole.Controls.Add(_btnClearLog);
        _panelConsole.Location = new Point(0, 554);
        _panelConsole.Name = "_panelConsole";
        _panelConsole.Padding = new Padding(14, 30, 14, 10);
        _panelConsole.Size = new Size(844, 170);
        _panelConsole.TabIndex = 7;
        _panelConsole.Title = "执行日志";
        // 
        // _console
        // 
        _console.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _console.BackColor = Color.FromArgb(10, 12, 16);
        _console.BorderStyle = BorderStyle.None;
        _console.DetectUrls = false;
        _console.Font = new Font("Consolas", 9.5F);
        _console.ForeColor = Color.FromArgb(230, 237, 243);
        _console.Location = new Point(16, 36);
        _console.Name = "_console";
        _console.ReadOnly = true;
        _console.Size = new Size(812, 122);
        _console.TabIndex = 0;
        _console.Text = "";
        // 
        // _btnClearLog
        // 
        _btnClearLog.Accent = false;
        _btnClearLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnClearLog.Danger = false;
        _btnClearLog.FlatStyle = FlatStyle.Flat;
        _btnClearLog.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnClearLog.ForeColor = Color.FromArgb(230, 237, 243);
        _btnClearLog.Location = new Point(740, 6);
        _btnClearLog.Name = "_btnClearLog";
        _btnClearLog.Size = new Size(88, 24);
        _btnClearLog.TabIndex = 1;
        _btnClearLog.Text = "清空日志";
        // 
        // _progress
        // 
        _progress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _progress.Location = new Point(10, 737);
        _progress.Name = "_progress";
        _progress.Size = new Size(160, 5);
        _progress.TabIndex = 8;
        // 
        // _statusText
        // 
        _statusText.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _statusText.AutoSize = true;
        _statusText.Font = new Font("Microsoft YaHei UI", 9F);
        _statusText.ForeColor = Color.FromArgb(139, 148, 161);
        _statusText.Location = new Point(180, 731);
        _statusText.Name = "_statusText";
        _statusText.Size = new Size(32, 17);
        _statusText.TabIndex = 9;
        _statusText.Text = "就绪";
        // 
        // _lblStatusRight
        // 
        _lblStatusRight.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _lblStatusRight.Font = new Font("Microsoft YaHei UI", 8.5F);
        _lblStatusRight.ForeColor = Color.FromArgb(139, 148, 161);
        _lblStatusRight.Location = new Point(728, 733);
        _lblStatusRight.Name = "_lblStatusRight";
        _lblStatusRight.Size = new Size(100, 15);
        _lblStatusRight.TabIndex = 10;
        _lblStatusRight.Text = "GITLUME v1.3";
        // 
        // MainForm
        // 
        BackColor = Color.FromArgb(15, 18, 23);
        ClientSize = new Size(844, 760);
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
        Font = new Font("Microsoft YaHei UI", 9.5F);
        MinimumSize = new Size(860, 700);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "GitLume · Git 客户端";
        _panelGlobal.ResumeLayout(false);
        _panelLocal.ResumeLayout(false);
        _panelLocal.PerformLayout();
        _panelRemotes.ResumeLayout(false);
        _panelCommit.ResumeLayout(false);
        _panelCommit.PerformLayout();
        _panelConsole.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
