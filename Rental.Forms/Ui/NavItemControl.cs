using System.Drawing.Drawing2D;

namespace Rental.Forms.Ui;

internal class NavItemControl : Control, IThemedControl
{
    private bool _hover;
    private bool _active;

    public string NavKey { get; }

    public event EventHandler? ItemClicked;

    public NavItemControl(string text, string navKey)
    {
        NavKey = navKey;
        Text = text;
        Height = 38;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ApplyTheme();
    }

    public void SetActive(bool active)
    {
        _active = active;
        Font = _active ? new Font(UiTheme.BodyFont, FontStyle.Bold) : UiTheme.BodyFont;
        Invalidate();
    }

    public void ApplyTheme()
    {
        Font = _active ? new Font(UiTheme.BodyFont, FontStyle.Bold) : UiTheme.BodyFont;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(GetSidebarColor());
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        ItemClicked?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(GetSidebarColor());

        var rect = new Rectangle(4, 3, Width - 8, Height - 6);

        if (_active || _hover)
        {
            using var path = RoundRectHelper.CreatePath(rect, 10);
            var (top, bottom) = _active ? UiTheme.SidebarActiveGradient : UiTheme.SidebarHoverGradient;
            using var brush = new LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical);
            e.Graphics.FillPath(brush, path);
        }

        var textColor = _active ? UiTheme.Accent : UiTheme.TextPrimary;
        var textRect = new Rectangle(16, 0, Width - 20, Height);
        TextRenderer.DrawText(e.Graphics, Text, Font, textRect, textColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
    }

    private static Color GetSidebarColor()
    {
        return UiTheme.SidebarBackground;
    }
}
