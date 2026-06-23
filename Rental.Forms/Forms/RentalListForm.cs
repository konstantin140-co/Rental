using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class RentalListForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly PageHeaderControl _header = new();
    private readonly FilterTabsControl _filterTabs = new();
    private readonly CardGridPanel _cardGrid = new();
    private readonly Label _lblCount = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary, Font = UiTheme.SmallFont, Tag = "secondary" };
    private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 300 };
    private string? _statusFilter;

    public RentalListForm()
    {
        SetActiveNavKey("rentals");
        NavigationService.NavigateTo(this, "rentals");

        _header.SetTitle("Аренды", "История и текущие аренды");
        _header.ConfigureAction("Выдать");
        _filterTabs.SetTabs("Все", RentalRecordStatuses.Active, RentalRecordStatuses.Completed, RentalRecordStatuses.Overdue);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 28 };
        footer.Controls.Add(_lblCount);

        ContentPanel.Controls.Add(_cardGrid);
        ContentPanel.Controls.Add(footer);
        ContentPanel.Controls.Add(_filterTabs);
        ContentPanel.Controls.Add(_header);

        _header.ActionClicked += (_, _) => NavigationService.NavigateTo(new IssueRentalForm(), "issue");
        _header.SearchTextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
        _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); LoadData(_header.SearchBox.Text); };
        _filterTabs.FilterChanged += (_, status) => { _statusFilter = status; LoadData(_header.SearchBox.Text); };

        Load += (_, _) => LoadData();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadData(string filter = "")
    {
        var query = _db.RentalRecords
            .Include(r => r.Inventory)
            .Include(r => r.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim();
            query = query.Where(r =>
                r.Inventory.Name.Contains(f) ||
                r.Client.LastName.Contains(f) ||
                r.Client.FirstName.Contains(f) ||
                r.Status.Contains(f));
        }

        if (!string.IsNullOrWhiteSpace(_statusFilter))
            query = query.Where(r => r.Status == _statusFilter);

        var items = query.OrderByDescending(r => r.IssueDate).ToList();
        _cardGrid.ClearCards();

        foreach (var item in items)
        {
            var card = new EntityCardControl();
            card.Bind(
                item.Id,
                item.Inventory.Name,
                item.Status,
                item.Inventory.Category,
                $"Клиент: {item.Client.FullName}\nВыдача: {item.IssueDate:dd.MM.yyyy HH:mm}\nВозврат (план): {item.PlannedReturnDate:dd.MM.yyyy HH:mm}\nТариф: {item.Tariff}  |  Сумма: {item.TotalAmount:N0} ₽  |  Штраф: {item.Fine:N0} ₽");
            card.PrimaryClicked += (_, id) => EditRecord(id);
            card.SecondaryClicked += (_, id) => DeleteRecord(id);
            _cardGrid.AddCard(card);
        }

        _lblCount.Text = $"Записей: {items.Count}";
    }

    private void EditRecord(int id)
    {
        using var form = new RentalEditForm(id);
        if (form.ShowDialog() == DialogResult.OK)
            LoadData(_header.SearchBox.Text);
    }

    private void DeleteRecord(int id)
    {
        try
        {
            var item = _db.RentalRecords.Include(r => r.Inventory).FirstOrDefault(r => r.Id == id);
            if (item == null) return;

            if (MessageBox.Show($"Удалить аренду №{item.Id}? Это действие нельзя отменить.", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (item.Status == RentalRecordStatuses.Active)
                item.Inventory.Status = InventoryStatuses.Available;

            _db.RentalRecords.Remove(item);
            _db.SaveChanges();
            LoadData(_header.SearchBox.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при удалении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
