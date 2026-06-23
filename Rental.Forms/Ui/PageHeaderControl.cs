namespace Rental.Forms.Ui;

internal class PageHeaderControl : Panel, IThemedControl
{
    private readonly Label _title = new() { AutoSize = true, Font = UiTheme.TitleFont };
    private readonly Label _subtitle = new() { AutoSize = true, Font = UiTheme.BodyFont, Margin = new Padding(0, 6, 0, 0) };
    private readonly RoundedPanel _searchPanel = new() { Height = 40, Width = 280, Radius = 20 };
    private readonly TextBox _searchBox = new() { PlaceholderText = "Поиск...", Dock = DockStyle.Fill };
    private readonly RoundedButton _actionButton = new() { Text = "Добавить", Width = 120, Visible = false };

    public event EventHandler? SearchTextChanged;
    public event EventHandler? ActionClicked;

    public TextBox SearchBox => _searchBox;
    public RoundedButton ActionButton => _actionButton;

    public PageHeaderControl()
    {
        Dock = DockStyle.Top;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BackColor = Color.Transparent;
        Padding = new Padding(0, 0, 0, 12);

        _title.Tag = "primary";
        _subtitle.Tag = "secondary";

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var titlePanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0),
            BackColor = Color.Transparent
        };
        titlePanel.Controls.Add(_title);
        titlePanel.Controls.Add(_subtitle);

        _searchPanel.Padding = new Padding(14, 8, 14, 8);
        _searchPanel.Margin = new Padding(8, 8, 8, 0);
        UiTheme.StyleTextBox(_searchBox);
        _searchPanel.Controls.Add(_searchBox);
        _searchBox.TextChanged += (_, _) => SearchTextChanged?.Invoke(this, EventArgs.Empty);

        _actionButton.Margin = new Padding(8, 8, 0, 0);
        _actionButton.Click += (_, _) => ActionClicked?.Invoke(this, EventArgs.Empty);

        layout.Controls.Add(titlePanel, 0, 0);
        layout.Controls.Add(_searchPanel, 1, 0);
        layout.Controls.Add(_actionButton, 2, 0);

        Controls.Add(layout);
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        _title.ForeColor = UiTheme.TextPrimary;
        _subtitle.ForeColor = UiTheme.TextSecondary;
        UiTheme.StyleTextBox(_searchBox);
        _searchPanel.ApplyTheme();
        _actionButton.ApplyTheme();
    }

    public void SetTitle(string title, string subtitle = "")
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        _subtitle.Visible = !string.IsNullOrWhiteSpace(subtitle);
    }

    public void ConfigureAction(string text, bool visible = true)
    {
        _actionButton.Text = text;
        _actionButton.Visible = visible;
        if (visible)
            _actionButton.Width = Math.Max(100, TextRenderer.MeasureText(text, _actionButton.Font).Width + 32);
    }

    public void HideSearch()
    {
        _searchPanel.Visible = false;
    }
}
