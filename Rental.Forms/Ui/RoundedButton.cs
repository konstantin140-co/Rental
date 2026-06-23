using System.Drawing.Drawing2D;

namespace Rental.Forms.Ui;

internal class RoundedButton : Control, IThemedControl
{
    private bool _hover;
    private bool _pressed;

    public int Radius { get; set; } = 18;

    public RoundedButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(120, 36);
        Cursor = Cursors.Hand;
        TabStop = true;
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        ForeColor = Color.White;
        Invalidate();
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
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundRectHelper.CreatePath(rect, Radius);

        var color = _pressed ? UiTheme.AccentHover
            : _hover ? UiTheme.AccentHover
            : UiTheme.Accent;

        using (var brush = new SolidBrush(color))
            e.Graphics.FillPath(brush, path);

        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
