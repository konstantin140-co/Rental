namespace Rental.Forms.Ui;

internal class FilterTabsControl : Panel, IThemedControl
{
    private readonly FlowLayoutPanel _flow = new()
    {
        Dock = DockStyle.Fill,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(0, 4, 0, 4)
    };

    private string? _selected;
    private string _activeTabText = "Все";

    public event EventHandler<string?>? FilterChanged;

    public FilterTabsControl()
    {
        Height = 44;
        Dock = DockStyle.Top;
        BackColor = Color.Transparent;
        Controls.Add(_flow);
    }

    public void SetTabs(params string[] tabs)
    {
        _flow.Controls.Clear();
        foreach (var tab in tabs)
        {
            var btn = new Button
            {
                Text = tab,
                Tag = tab == "Все" ? null : tab,
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(14, 6, 14, 6),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = UiTheme.BodyFont
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (_, _) => SelectTab(btn.Tag as string, btn.Text);
            _flow.Controls.Add(btn);
        }

        if (_flow.Controls.Count > 0)
            SelectTab(null, "Все");
    }

    public void ApplyTheme()
    {
        BackColor = Color.Transparent;
        _flow.BackColor = Color.Transparent;
        foreach (Control control in _flow.Controls)
        {
            if (control is not Button btn) continue;
            var isActive = btn.Text == _activeTabText;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = isActive ? UiTheme.SidebarActive : Color.Transparent;
            btn.ForeColor = isActive ? UiTheme.Accent : UiTheme.TextSecondary;
            btn.Font = isActive ? new Font(UiTheme.BodyFont, FontStyle.Bold) : UiTheme.BodyFont;
        }
    }

    private void SelectTab(string? filterValue, string displayText)
    {
        _selected = filterValue;
        _activeTabText = displayText;
        foreach (Control control in _flow.Controls)
        {
            if (control is not Button btn) continue;
            var isActive = btn.Text == displayText;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = isActive ? UiTheme.SidebarActive : Color.Transparent;
            btn.ForeColor = isActive ? UiTheme.Accent : UiTheme.TextSecondary;
            btn.Font = isActive ? new Font(UiTheme.BodyFont, FontStyle.Bold) : UiTheme.BodyFont;
        }

        FilterChanged?.Invoke(this, _selected);
    }

    public string? SelectedFilter => _selected;
}
