using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitLume.UI;

/// <summary>
/// Git 身份验证对话框（用户名 + 密码，可选择加密后保存）。
/// 会明确显示"正在为哪个仓库"输入凭据。
/// </summary>
public sealed class CredentialDialog : Form
{
    private readonly SciField _user;
    private readonly SciField _pass;
    private readonly CheckBox _remember;

    public string UserName => _user.Text.Trim();
    public string Password => _pass.Text;
    public bool Remember => _remember.Checked;

    public CredentialDialog(string remoteName, string remoteUrl, string defaultUser, bool savedPasswordExists)
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.Bg;
        Font = Theme.FontLabel(9.5f);
        Text = $"身份验证 · {remoteName}";

        var title = new Label
        {
            Text = "需要身份验证",
            ForeColor = Theme.Text,
            Font = Theme.FontTitle(13f),
            Location = new Point(20, 12),
            AutoSize = true,
        };

        var msgFont = Theme.FontLabel(9f);
        var msgText = $"远程仓库「{remoteName}」需要登录：\n{remoteUrl}";
        if (savedPasswordExists)
            msgText += "\n已保存过该仓库的凭据：密码留空=继续使用已保存的密码，填写=更新密码。";
        var msgSize = TextRenderer.MeasureText(msgText, msgFont, new Size(386, 0), TextFormatFlags.WordBreak);
        var msg = new Label
        {
            Text = msgText,
            ForeColor = Theme.TextDim,
            Font = msgFont,
            Bounds = new Rectangle(20, 42, 390, msgSize.Height),
            AutoSize = false,
        };

        int userY = 42 + msgSize.Height + 8;
        _user = new SciField("用户名") { Location = new Point(20, userY), Width = 390, Text = defaultUser };
        _pass = new SciField("密码") { Location = new Point(20, userY + 50), Width = 390 };
        _pass.SetPassword();

        _remember = new CheckBox
        {
            Text = "记住密码（简单加密后保存到磁盘，下次自动使用）",
            ForeColor = Theme.TextDim,
            Font = Theme.FontLabel(9f),
            Checked = true,
            Location = new Point(20, userY + 104),
            AutoSize = true,
        };

        int btnY = userY + 146;
        var ok = new SciButton { Text = "确  定", Accent = true, Location = new Point(214, btnY), Size = new Size(92, 36) };
        var cancel = new SciButton { Text = "取  消", Location = new Point(318, btnY), Size = new Size(92, 36) };

        ok.Click += (_, _) =>
        {
            if (string.IsNullOrEmpty(UserName))
            {
                MessageBox.Show(this, "请输入用户名。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(Password) && !savedPasswordExists)
            {
                MessageBox.Show(this, "请输入密码。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
        };
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        AcceptButton = ok;
        CancelButton = cancel;

        ClientSize = new Size(430, btnY + 52);
        Controls.AddRange(new Control[] { title, msg, _user, _pass, _remember, ok, cancel });
        Shown += (_, _) => _user.FocusBox();
    }
}
