using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class MainForm : ShellForm
{
    public MainForm()
    {
        SetActiveNavKey("home");
        NavigationService.NavigateTo(this, "home");
        BuildWelcome();
    }

    private void BuildWelcome()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Tag = "theme-bg"
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var welcome = new Label
        {
            Text = "Добро пожаловать",
            Font = UiTheme.TitleFont,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8),
            Tag = "primary"
        };

        var subtitle = new Label
        {
            Text = "Выберите раздел в меню слева или быстрый переход ниже",
            Font = UiTheme.BodyFont,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 24),
            Tag = "secondary"
        };

        var cards = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        cards.Controls.Add(CreateQuickCard("Инвентарь", "Учёт оборудования", "inventory", "И"));
        cards.Controls.Add(CreateQuickCard("Клиенты", "База клиентов", "clients", "К"));
        cards.Controls.Add(CreateQuickCard("Аренды", "История аренд", "rentals", "А"));

        layout.Controls.Add(welcome, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(cards, 0, 2);

        ContentPanel.Controls.Add(layout);
    }

    private static RoundedPanel CreateQuickCard(string title, string desc, string navKey, string letter)
    {
        var card = new RoundedPanel { Size = new Size(280, 150), Margin = new Padding(0, 0, 16, 16), Cursor = Cursors.Hand };
        var avatar = new Label
        {
            Text = letter,
            Size = new Size(40, 40),
            Location = new Point(16, 16),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = UiTheme.Accent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
        var titleLbl = new Label
        {
            Text = title,
            Location = new Point(16, 68),
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Tag = "primary"
        };
        var descLbl = new Label
        {
            Text = desc,
            Location = new Point(16, 96),
            AutoSize = true,
            Font = UiTheme.SmallFont,
            MaximumSize = new Size(248, 0),
            Tag = "secondary"
        };

        void open(object? _, EventArgs __) => NavigationService.NavigateTo(navKey switch
        {
            "inventory" => new InventoryListForm(),
            "clients" => new ClientListForm(),
            _ => new RentalListForm()
        }, navKey);

        card.Click += open;
        avatar.Click += open;
        titleLbl.Click += open;
        descLbl.Click += open;
        card.Controls.AddRange([avatar, titleLbl, descLbl]);
        return card;
    }
}
