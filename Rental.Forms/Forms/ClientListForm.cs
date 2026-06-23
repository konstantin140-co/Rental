using Microsoft.EntityFrameworkCore;
using Rental.Data.Context;
using Rental.Data.Models;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class ClientListForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly PageHeaderControl _header = new();
    private readonly CardGridPanel _cardGrid = new();
    private readonly Label _lblCount = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary, Font = UiTheme.SmallFont, Tag = "secondary" };
    private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 300 };

    public ClientListForm()
    {
        SetActiveNavKey("clients");
        NavigationService.NavigateTo(this, "clients");

        _header.SetTitle("Клиенты", "База клиентов пункта проката");
        _header.ConfigureAction("Добавить");

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 28 };
        footer.Controls.Add(_lblCount);

        ContentPanel.Controls.Add(_cardGrid);
        ContentPanel.Controls.Add(footer);
        ContentPanel.Controls.Add(_header);

        _header.ActionClicked += (_, _) => EditRecord(0);
        _header.SearchTextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
        _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); LoadData(_header.SearchBox.Text); };

        Load += (_, _) => LoadData();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadData(string filter = "")
    {
        var query = _db.Clients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim();
            query = query.Where(c => c.LastName.Contains(f) || c.FirstName.Contains(f) || c.Phone.Contains(f) || c.Passport.Contains(f));
        }

        var items = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
        _cardGrid.ClearCards();

        foreach (var item in items)
        {
            var card = new EntityCardControl();
            card.Bind(
                item.Id,
                item.FullName,
                "Клиент",
                item.LastName,
                $"Телефон: {item.Phone}\nПаспорт: {item.Passport}");
            card.PrimaryClicked += (_, id) => EditRecord(id);
            card.SecondaryClicked += (_, id) => DeleteRecord(id);
            _cardGrid.AddCard(card);
        }

        _lblCount.Text = $"Записей: {items.Count}";
    }

    private void EditRecord(int id)
    {
        using var form = new ClientEditForm(id);
        if (form.ShowDialog() == DialogResult.OK)
            LoadData(_header.SearchBox.Text);
    }

    private void DeleteRecord(int id)
    {
        try
        {
            var item = _db.Clients.Find(id);
            if (item == null) return;

            if (MessageBox.Show($"Удалить «{item.FullName}»? Это действие нельзя отменить.", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _db.Clients.Remove(item);
            _db.SaveChanges();
            LoadData(_header.SearchBox.Text);
        }
        catch (DbUpdateException)
        {
            MessageBox.Show("Нельзя удалить запись: есть связанные аренды.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
