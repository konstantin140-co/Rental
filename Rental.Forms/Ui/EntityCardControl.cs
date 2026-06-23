namespace Rental.Forms.Ui;

internal class EntityCardControl : RoundedPanel, IThemedControl
{
    private readonly Label _avatar = new()
    {
        Size = new Size(44, 44),
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold),
        ForeColor = Color.White,
        BackColor = UiTheme.Accent
    };

    private readonly Label _title = new() { AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, MaximumSize = new Size(180, 0) };
    private readonly StatusBadgeControl _badge = new();
    private readonly Label _details = new()
    {
        AutoSize = false,
        Height = 72,
        Dock = DockStyle.Top,
        Font = UiTheme.SmallFont,
        ForeColor = UiTheme.TextSecondary
    };

    private readonly Button _primaryAction = new() { Text = "Редактировать", Width = 130 };
    private readonly Button _secondaryAction = new() { Text = "Удалить", Width = 100 };

    public int EntityId { get; private set; }

    public event EventHandler<int>? PrimaryClicked;
    public event EventHandler<int>? SecondaryClicked;

    public EntityCardControl()
    {
        Size = new Size(UiTheme.CardWidth, UiTheme.CardHeight);
        Margin = new Padding(0, 0, 16, 16);
        Padding = new Padding(16);

        var header = new Panel { Dock = DockStyle.Top, Height = 52 };
        _avatar.Location = new Point(0, 4);
        _title.Location = new Point(56, 4);
        _badge.Location = new Point(56, 28);
        header.Controls.AddRange([_avatar, _title, _badge]);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        UiTheme.StyleSecondaryButton(_primaryAction);
        UiTheme.StyleSecondaryButton(_secondaryAction);
        _primaryAction.Click += (_, _) => PrimaryClicked?.Invoke(this, EntityId);
        _secondaryAction.Click += (_, _) => SecondaryClicked?.Invoke(this, EntityId);
        actions.Controls.Add(_primaryAction);
        actions.Controls.Add(_secondaryAction);

        Controls.Add(actions);
        Controls.Add(_details);
        Controls.Add(header);
    }

    public new void ApplyTheme()
    {
        base.ApplyTheme();
        _title.ForeColor = UiTheme.TextPrimary;
        _details.ForeColor = UiTheme.TextSecondary;
        _avatar.BackColor = UiTheme.Accent;
        UiTheme.StyleSecondaryButton(_primaryAction);
        UiTheme.StyleSecondaryButton(_secondaryAction);
        _badge.SetStatus(_badge.Text);
    }

    public void Bind(int id, string title, string status, string avatarLetter, string details,
        string primaryText = "Редактировать", string secondaryText = "Удалить")
    {
        EntityId = id;
        _title.Text = title;
        _badge.SetStatus(status);
        _avatar.Text = string.IsNullOrEmpty(avatarLetter) ? "?" : avatarLetter[..1].ToUpper();
        _details.Text = details;
        _primaryAction.Text = primaryText;
        _secondaryAction.Text = secondaryText;
    }
}
