using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Models;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class InventoryListForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly PageHeaderControl _header = new();
    private readonly FilterTabsControl _filterTabs = new();
    private readonly CardGridPanel _cardGrid = new();
    private readonly Label _lblCount = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary, Font = UiTheme.SmallFont, Tag = "secondary" };
    private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 300 };
    private string? _statusFilter;

    public InventoryListForm()
    {
        SetActiveNavKey("inventory");
        NavigationService.NavigateTo(this, "inventory");

        _header.SetTitle("Инвентарь", "Управление оборудованием пункта проката");
        _header.ConfigureAction("Добавить");
        _filterTabs.SetTabs("Все", InventoryStatuses.Available, InventoryStatuses.Rented, InventoryStatuses.Repair);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 28 };
        footer.Controls.Add(_lblCount);

        ContentPanel.Controls.Add(_cardGrid);
        ContentPanel.Controls.Add(footer);
        ContentPanel.Controls.Add(_filterTabs);
        ContentPanel.Controls.Add(_header);

        _header.ActionClicked += (_, _) => EditRecord(0);
        _header.SearchTextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
        _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); LoadData(_header.SearchBox.Text); };
        _filterTabs.FilterChanged += (_, status) => { _statusFilter = status; LoadData(_header.SearchBox.Text); };

        Load += (_, _) => LoadData();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadData(string filter = "")
    {
        var query = _db.Inventories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim();
            query = query.Where(i => i.Name.Contains(f) || i.Category.Contains(f) || i.InventoryNumber.Contains(f));
        }

        if (!string.IsNullOrWhiteSpace(_statusFilter))
            query = query.Where(i => i.Status == _statusFilter);

        var items = query.OrderBy(i => i.Name).ToList();
        _cardGrid.ClearCards();

        foreach (var item in items)
        {
            var card = new EntityCardControl();
            var letter = string.IsNullOrEmpty(item.Category) ? "?" : item.Category[0].ToString();
            card.Bind(
                item.Id,
                item.Name,
                item.Status,
                letter,
                $"Категория: {item.Category}\nИнв. №: {item.InventoryNumber}\nЦена/час: {item.PricePerHour:N0} ₽  |  /сутки: {item.PricePerDay:N0} ₽\nЗалог: {item.Deposit:N0} ₽");
            card.PrimaryClicked += (_, id) => EditRecord(id);
            card.SecondaryClicked += (_, id) => DeleteRecord(id);
            _cardGrid.AddCard(card);
        }

        _lblCount.Text = $"Записей: {items.Count}";
    }

    private void EditRecord(int id)
    {
        using var form = new InventoryEditForm(id);
        if (form.ShowDialog() == DialogResult.OK)
            LoadData(_header.SearchBox.Text);
    }

    private void DeleteRecord(int id)
    {
        try
        {
            var item = _db.Inventories.Find(id);
            if (item == null) return;

            if (MessageBox.Show($"Удалить «{item.Name}»? Это действие нельзя отменить.", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _db.Inventories.Remove(item);
            _db.SaveChanges();
            LoadData(_header.SearchBox.Text);
        }
        catch (DbUpdateException)
        {
            MessageBox.Show("Нельзя удалить запись: есть связанные аренды.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
