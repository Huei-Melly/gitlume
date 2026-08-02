using System.Drawing;
using System.Drawing.Drawing2D;

namespace GitLume.UI;

/// <summary>
/// 简洁科技感主题：深色底、细边框、单一蓝色点缀，不花哨。
/// </summary>
public static class Theme
{
    // ---- 背景 ----
    public static readonly Color Bg = Color.FromArgb(15, 18, 23);        // 窗体背景
    public static readonly Color Panel = Color.FromArgb(21, 25, 32);     // 面板背景
    public static readonly Color Input = Color.FromArgb(12, 15, 19);     // 输入框背景

    // ---- 边框 ----
    public static readonly Color Border = Color.FromArgb(45, 52, 64);        // 普通边框
    public static readonly Color BorderFocus = Color.FromArgb(59, 130, 246); // 聚焦边框

    // ---- 强调色 ----
    public static readonly Color Accent = Color.FromArgb(59, 130, 246);       // 主蓝
    public static readonly Color AccentHover = Color.FromArgb(96, 155, 255);  // 主蓝悬停
    public static readonly Color Danger = Color.FromArgb(248, 113, 113);
    public static readonly Color Warn = Color.FromArgb(251, 191, 36);
    public static readonly Color Success = Color.FromArgb(52, 211, 153);

    // ---- 文字 ----
    public static readonly Color Text = Color.FromArgb(230, 237, 243);
    public static readonly Color TextDim = Color.FromArgb(139, 148, 161);

    // ---- 字体 ----
    public static Font FontTitle(float s = 15f) => new("Microsoft YaHei UI", s, FontStyle.Bold, GraphicsUnit.Point);
    public static Font FontLabel(float s = 9.5f) => new("Microsoft YaHei UI", s, FontStyle.Regular, GraphicsUnit.Point);
    public static Font FontBold(float s = 9.5f) => new("Microsoft YaHei UI", s, FontStyle.Bold, GraphicsUnit.Point);
    public static Font FontMono(float s = 9.5f) => new("Consolas", s, FontStyle.Regular, GraphicsUnit.Point);

    /// <summary>圆角矩形路径。</summary>
    public static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        float d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
