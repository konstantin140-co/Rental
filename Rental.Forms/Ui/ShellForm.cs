namespace Rental.Forms.Ui;

public class ShellForm : Form
{
    protected readonly Panel ContentPanel;
    private readonly SidebarControl _sidebar = new();
    private readonly ToolStripMenuItem _lightThemeItem;
    private readonly ToolStripMenuItem _darkThemeItem;

    protected ShellForm()
    {
        Text = "Пункт проката";
        MinimumSize = new Size(1100, 650);
        Size = new Size(1200, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont;

        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("Файл");
        fileMenu.DropDownItems.Add("Выход", null, (_, _) => Application.Exit());

        var viewMenu = new ToolStripMenuItem("Вид");
        _lightThemeItem = new ToolStripMenuItem("Светлая тема") { Checked = UiTheme.Current == AppTheme.Light };
        _darkThemeItem = new ToolStripMenuItem("Тёмная тема") { Checked = UiTheme.Current == AppTheme.Dark };
        _lightThemeItem.Click += (_, _) => SetTheme(AppTheme.Light);
        _darkThemeItem.Click += (_, _) => SetTheme(AppTheme.Dark);
        viewMenu.DropDownItems.AddRange([_lightThemeItem, _darkThemeItem]);

        menu.Items.AddRange([fileMenu, viewMenu]);
        UiTheme.StyleMenuStrip(menu);
        MainMenuStrip = menu;

        ContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(UiTheme.Padding),
            BackColor = UiTheme.Background,
            Tag = "theme-bg"
        };

        _sidebar.NavigationRequested += OnNavigationRequested;
        Controls.Add(ContentPanel);
        Controls.Add(_sidebar);
        Controls.Add(menu);

        UiTheme.Changed += OnThemeChanged;
        FormClosed += (_, _) =>
        {
            UiTheme.Changed -= OnThemeChanged;
            NavigationService.Unregister(this);
        };
    }

    private void SetTheme(AppTheme theme)
    {
        UiTheme.SetTheme(theme);
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        _lightThemeItem.Checked = UiTheme.Current == AppTheme.Light;
        _darkThemeItem.Checked = UiTheme.Current == AppTheme.Dark;
        NavigationService.RefreshAllThemes();
    }

    public void ApplyTheme()
    {
        BackColor = UiTheme.Background;
        ContentPanel.BackColor = UiTheme.Background;
        if (MainMenuStrip != null)
            UiTheme.StyleMenuStrip(MainMenuStrip);
        _sidebar.ApplyTheme();
        UiTheme.ApplyThemeToControl(ContentPanel);
        Invalidate(true);
    }

    public void SetActiveNavKey(string key) => _sidebar.SetActiveNavKey(key);

    protected virtual void OnNavigationRequested(object? sender, string key)
    {
        Form? target = key switch
        {
            "home" => new Forms.MainForm(),
            "inventory" => new Forms.InventoryListForm(),
            "clients" => new Forms.ClientListForm(),
            "rentals" => new Forms.RentalListForm(),
            "issue" => new Forms.IssueRentalForm(),
            "return" => new Forms.ReturnRentalForm(),
            "overdue" => new Forms.OverdueListForm(),
            "revenue" => new Forms.RevenueReportForm(),
            "categories" => new Forms.CategoryReportForm(),
            _ => null
        };

        if (target != null)
            NavigationService.NavigateTo(target, key);
    }
}
