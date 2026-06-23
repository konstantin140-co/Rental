namespace Rental.Forms.Forms;

public class MainForm : Form
{
    private readonly System.Windows.Forms.Timer _clockTimer = new() { Interval = 1000 };
    private readonly ToolStripStatusLabel _statusLabel = new("Готово");
    private readonly ToolStripStatusLabel _timeLabel = new();

    public MainForm()
    {
        InitializeComponent();
        BuildMenu();
        BuildStatusBar();
        _clockTimer.Tick += (_, _) => _timeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        _clockTimer.Start();
    }

    private void InitializeComponent()
    {
        Text = "Пункт проката";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;
        MainMenuStrip = new MenuStrip();
        Controls.Add(MainMenuStrip);
    }

    private void BuildMenu()
    {
        var directories = new ToolStripMenuItem("Справочники");
        directories.DropDownItems.Add(CreateMenuItem("Инвентарь", (_, _) => OpenForm<InventoryListForm>()));
        directories.DropDownItems.Add(CreateMenuItem("Клиенты", (_, _) => OpenForm<ClientListForm>()));
        directories.DropDownItems.Add(CreateMenuItem("Аренды", (_, _) => OpenForm<RentalListForm>()));

        var operations = new ToolStripMenuItem("Операции");
        operations.DropDownItems.Add(CreateMenuItem("Выдача в аренду", (_, _) => OpenForm<IssueRentalForm>()));
        operations.DropDownItems.Add(CreateMenuItem("Возврат", (_, _) => OpenForm<ReturnRentalForm>()));
        operations.DropDownItems.Add(CreateMenuItem("Просроченные аренды", (_, _) => OpenForm<OverdueListForm>()));

        var reports = new ToolStripMenuItem("Отчёты");
        reports.DropDownItems.Add(CreateMenuItem("Выручка за период", (_, _) => OpenForm<RevenueReportForm>()));
        reports.DropDownItems.Add(CreateMenuItem("Популярные категории", (_, _) => OpenForm<CategoryReportForm>()));

        MainMenuStrip!.Items.Add(directories);
        MainMenuStrip.Items.Add(operations);
        MainMenuStrip.Items.Add(reports);
    }

    private void BuildStatusBar()
    {
        var statusStrip = new StatusStrip();
        _timeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        statusStrip.Items.Add(_statusLabel);
        statusStrip.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusStrip.Items.Add(_timeLabel);
        Controls.Add(statusStrip);
    }

    private static ToolStripMenuItem CreateMenuItem(string text, EventHandler handler)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += handler;
        return item;
    }

    private static void OpenForm<T>() where T : Form, new()
    {
        var form = new T();
        form.Show();
    }
}
