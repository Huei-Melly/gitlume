using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GitLume.UI;

/// <summary>简洁面板：深色底 + 细边框 + 小标题。</summary>
public sealed class SciPanel : Panel
{
    /// <summary>面板标题（设计器属性面板中可编辑、可序列化，保存布局时不会丢失）。</summary>
    public string Title { get; set; } = "";

    // 静态缓存字体：OnPaint 每次重绘若 new Font 会造成 GDI 对象持续泄漏
    private static readonly Font _titleFont = Theme.FontBold(9.5f);

    public SciPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        // BackColor 与填充色一致：按钮等子控件按 Parent.BackColor 清背景时才不会出现色块
        BackColor = Theme.Panel;
        Padding = new Padding(14, 30, 14, 10);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var rect = new RectangleF(0, 0, Width - 1, Height - 1);
        using (var path = Theme.RoundedRect(rect, 4))
        using (var brush = new SolidBrush(Theme.Panel))
            g.FillPath(brush, path);
        using (var path = Theme.RoundedRect(rect, 4))
        using (var pen = new Pen(Theme.Border, 1f))
            g.DrawPath(pen, path);

        if (!string.IsNullOrEmpty(Title))
        {
            using var titleBrush = new SolidBrush(Theme.Accent);
            g.DrawString(Title, _titleFont, titleBrush, new RectangleF(14, 6, Width - 28, 18));
        }
    }
}

/// <summary>简洁按钮：扁平底色 + 细边框，主操作用蓝色填充。</summary>
public sealed class SciButton : Button
{
    /// <summary>主按钮：蓝色填充（设计器属性面板可勾选）。</summary>
    public bool Accent { get; set; }

    /// <summary>危险操作：红色系（设计器属性面板可勾选）。</summary>
    public bool Danger { get; set; }

    private bool _hover;

    public SciButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        Height = 32;
        ForeColor = Theme.Text;
        Font = Theme.FontLabel(9.5f);
        TabStop = true;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.Bg);

        Color bg, border, text;

        if (Accent)
        {
            bg = _hover ? Theme.AccentHover : Theme.Accent;
            border = Theme.AccentHover;
            text = Color.White;
        }
        else if (Danger)
        {
            bg = _hover ? Color.FromArgb(66, 30, 34) : Color.FromArgb(52, 22, 26);
            border = _hover ? Theme.Danger : Color.FromArgb(110, 44, 48);
            text = Theme.Danger;
        }
        else
        {
            bg = _hover ? Color.FromArgb(30, 37, 48) : Color.FromArgb(24, 30, 40);
            border = _hover ? Theme.BorderFocus : Theme.Border;
            text = _hover ? Color.White : Theme.Text;
        }

        if (!Enabled)
        {
            bg = Color.FromArgb(21, 26, 33);
            border = Color.FromArgb(36, 42, 52);
            text = Color.FromArgb(88, 98, 112);
        }

        var rect = new RectangleF(1, 1, Width - 3, Height - 3);
        using (var path = Theme.RoundedRect(rect, 4))
        using (var brush = new SolidBrush(bg))
            g.FillPath(brush, path);
        using (var path = Theme.RoundedRect(rect, 4))
        using (var pen = new Pen(border, 1f))
            g.DrawPath(pen, path);

        TextRenderer.DrawText(g, Text, Font, new Rectangle(0, 0, Width, Height), text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

/// <summary>简洁输入框：小标签 + 深色圆角输入区 + 聚焦高亮边框。</summary>
public sealed class SciField : UserControl
{
    private readonly TextBox _box;
    // 静态缓存字体：OnPaint 每次重绘若 new Font 会造成 GDI 对象持续泄漏
    private static readonly Font _labelFont = Theme.FontLabel(8.5f);

    private string _labelText = "";

    /// <summary>输入框上方的说明文字（设计器属性面板中可编辑）。</summary>
    public string LabelText
    {
        get => _labelText;
        set
        {
            _labelText = value ?? "";
            Invalidate();
        }
    }

    /// <summary>输入内容（转发到内部文本框，设计器可序列化）。</summary>
    public new string Text
    {
        get => _box.Text;
        set => _box.Text = value;
    }

    public new event EventHandler? TextChanged;

    /// <summary>设计器无参构造。</summary>
    public SciField() : this("") { }

    public SciField(string label)
    {
        _labelText = label;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        BackColor = Theme.Panel;

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = Theme.Input,
            ForeColor = Theme.Text,
            Font = Theme.FontLabel(10f),
            Location = new Point(14, 22),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
        };
        _box.GotFocus += (_, _) => Invalidate();
        _box.LostFocus += (_, _) => Invalidate();
        _box.TextChanged += (_, e) => TextChanged?.Invoke(this, e);
        Controls.Add(_box);

        // 必须在 _box 创建后再设置高度，否则 OnResize 时 _box 尚为空
        Height = 50;
        UpdateBoxWidth();
    }

    public void SetPassword() => _box.UseSystemPasswordChar = true;

    public void FocusBox()
    {
        _box.Focus();
        _box.SelectAll();
    }

    private void UpdateBoxWidth() => _box.Width = Math.Max(10, Width - 28);

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateBoxWidth();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        // 标签区 4~19，输入框区 22~49，中间留 3px，避免标签与输入框粘连
        using var labelBrush = new SolidBrush(Theme.TextDim);
        g.DrawString(LabelText, _labelFont, labelBrush, 12, 4);

        var boxRect = new RectangleF(0, 22, Width - 1, Height - 23);
        using (var path = Theme.RoundedRect(boxRect, 4))
        using (var brush = new SolidBrush(Theme.Input))
            g.FillPath(brush, path);
        using (var path = Theme.RoundedRect(boxRect, 4))
        using (var pen = new Pen(_box.Focused ? Theme.BorderFocus : Theme.Border, 1f))
            g.DrawPath(pen, path);
    }
}

/// <summary>简洁进度条：细条 + 缓慢流动的蓝色光带。</summary>
public sealed class SciProgressBar : Control
{
    private readonly System.Windows.Forms.Timer _timer;
    private float _offset;

    public SciProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        Height = 5;
        _timer = new System.Windows.Forms.Timer { Interval = 40 };
        _timer.Tick += (_, _) =>
        {
            _offset += 2f;
            if (_offset > Width + 60) _offset = -Width * 0.3f;
            Invalidate();
        };
    }

    public void StartFlow() => _timer.Start();
    public void StopFlow() => _timer.Stop();

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.Bg);

        using (var pen = new Pen(Theme.Border, 1f))
            g.DrawLine(pen, 0, Height / 2f, Width, Height / 2f);

        var w = Math.Max(50f, Width * 0.3f);
        using var brush = new SolidBrush(Theme.Accent);
        using var path = Theme.RoundedRect(new RectangleF(_offset, 0, w, Height), Height / 2f);
        g.FillPath(brush, path);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _timer.Stop();
        base.OnHandleDestroyed(e);
    }
}
