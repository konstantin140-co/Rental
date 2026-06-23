using System.Drawing.Drawing2D;

namespace Rental.Forms.Ui;

internal class RoundedPanel : Panel, IThemedControl
{
    public int Radius { get; set; } = UiTheme.CornerRadius;
    public Color BorderColor { get; set; } = UiTheme.Border;
    public int BorderWidth { get; set; } = 1;
    public Color FillColor { get; set; } = UiTheme.CardBackground;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
    }

    public void ApplyTheme()
    {
        FillColor = UiTheme.CardBackground;
        BorderColor = UiTheme.Border;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (Parent != null)
            e.Graphics.Clear(Parent.BackColor);
        else
            base.OnPaintBackground(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundRectHelper.CreatePath(rect, Radius);
        using var brush = new SolidBrush(FillColor);
        e.Graphics.FillPath(brush, path);

        if (BorderWidth > 0)
        {
            using var pen = new Pen(BorderColor, BorderWidth);
            e.Graphics.DrawPath(pen, path);
        }
    }
}
