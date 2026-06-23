namespace Rental.Forms.Ui;

internal class SidebarControl : Panel, IThemedControl
{
    private readonly Dictionary<string, NavItemControl> _navItems = new();
    private readonly Label _logo = new();
    private string? _activeKey;

    public event EventHandler<string>? NavigationRequested;

    public SidebarControl()
    {
        Dock = DockStyle.Left;
        Width = UiTheme.SidebarWidth;
        Padding = new Padding(12, 16, 12, 16);

        _logo.Text = "Пункт проката";
        _logo.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _logo.Dock = DockStyle.Top;
        _logo.Height = 44;
        _logo.TextAlign = ContentAlignment.MiddleLeft;
        _logo.Tag = "accent";

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Width = UiTheme.SidebarWidth - 24,
            BackColor = Color.Transparent
        };

        AddSection(menu, "Справочники");
        AddNavItem(menu, "Главная", "home");
        AddNavItem(menu, "Инвентарь", "inventory");
        AddNavItem(menu, "Клиенты", "clients");
        AddNavItem(menu, "Аренды", "rentals");

        AddSection(menu, "Операции");
        AddNavItem(menu, "Выдача в аренду", "issue");
        AddNavItem(menu, "Возврат", "return");
        AddNavItem(menu, "Просроченные", "overdue");

        AddSection(menu, "Отчёты");
        AddNavItem(menu, "Выручка", "revenue");
        AddNavItem(menu, "Категории", "categories");

        scroll.Controls.Add(menu);
        Controls.Add(scroll);
        Controls.Add(_logo);
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        BackColor = UiTheme.SidebarBackground;
        _logo.ForeColor = UiTheme.Accent;
        _logo.BackColor = UiTheme.SidebarBackground;
        RefreshChildThemes(Controls);
        foreach (var item in _navItems.Values)
            item.ApplyTheme();
        Invalidate();
    }

    private static void RefreshChildThemes(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            if (control is Panel panel)
                panel.BackColor = UiTheme.SidebarBackground;
            if (control is Label label && label.Tag as string == "secondary")
                label.ForeColor = UiTheme.TextSecondary;
            if (control.HasChildren)
                RefreshChildThemes(control.Controls);
        }
    }

    private static void AddSection(FlowLayoutPanel parent, string text)
    {
        parent.Controls.Add(new Label
        {
            Text = text.ToUpper(),
            Font = UiTheme.SectionFont,
            ForeColor = UiTheme.TextSecondary,
            AutoSize = true,
            Margin = new Padding(8, 16, 0, 6),
            Width = parent.Width - 16,
            Tag = "secondary"
        });
    }

    private void AddNavItem(FlowLayoutPanel parent, string text, string key)
    {
        var item = new NavItemControl(text, key)
        {
            Width = parent.Width - 8,
            Margin = new Padding(0, 0, 0, 2)
        };
        item.ItemClicked += (_, _) =>
        {
            SetActiveNavKey(key);
            NavigationRequested?.Invoke(this, key);
        };
        _navItems[key] = item;
        parent.Controls.Add(item);
    }

    public void SetActiveNavKey(string key)
    {
        _activeKey = key;
        foreach (var (navKey, item) in _navItems)
            item.SetActive(navKey == key);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(UiTheme.Border);
        e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
    }
}
