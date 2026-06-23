using Microsoft.EntityFrameworkCore;
using Rental.Data.Context;

namespace Rental.Forms.Forms;

public class RentalListForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly TextBox _txtSearch = new() { PlaceholderText = "Введите для поиска...", Dock = DockStyle.Top };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoGenerateColumns = false };
    private readonly Button _btnEdit = new() { Text = "Редактировать", Width = 130, Enabled = false };
    private readonly Button _btnDelete = new() { Text = "Удалить", Width = 110, Enabled = false };
    private readonly Button _btnRefresh = new() { Text = "Обновить", Width = 110 };
    private readonly Label _lblCount = new() { AutoSize = true, Padding = new Padding(8) };
    private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 300 };

    public RentalListForm()
    {
        Text = "Аренды";
        Width = 1100;
        Height = 550;
        StartPosition = FormStartPosition.CenterScreen;

        var dateFormat = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" };

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryName", HeaderText = "Инвентарь", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClientName", HeaderText = "Клиент", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IssueDate", HeaderText = "Выдача", Width = 130, DefaultCellStyle = dateFormat });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PlannedReturnDate", HeaderText = "План возврата", Width = 130, DefaultCellStyle = dateFormat });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Tariff", HeaderText = "Тариф", Width = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Сумма", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Fine", HeaderText = "Штраф", Width = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Статус", Width = 100 });

        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(8) };
        panel.Controls.AddRange([_btnEdit, _btnDelete, _btnRefresh, _lblCount]);
        Controls.Add(_grid);
        Controls.Add(panel);
        Controls.Add(_txtSearch);

        DataGridViewSortHelper.Attach(_grid);

        Load += (_, _) => LoadData();
        _txtSearch.TextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
        _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); LoadData(_txtSearch.Text); };
        _grid.SelectionChanged += (_, _) => _btnEdit.Enabled = _btnDelete.Enabled = _grid.CurrentRow != null;
        _btnEdit.Click += (_, _) => EditSelected();
        _btnDelete.Click += (_, _) => DeleteSelected();
        _btnRefresh.Click += (_, _) => LoadData(_txtSearch.Text);
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

        var items = query
            .Select(r => new RentalGridRow
            {
                Id = r.Id,
                InventoryName = r.Inventory.Name,
                ClientName = r.Client.LastName + " " + r.Client.FirstName,
                IssueDate = r.IssueDate,
                PlannedReturnDate = r.PlannedReturnDate,
                Tariff = r.Tariff,
                TotalAmount = r.TotalAmount,
                Fine = r.Fine,
                Status = r.Status
            })
            .ToList();

        _grid.DataSource = items;
        _lblCount.Text = $"Записей: {items.Count}";
    }

    private int GetSelectedId()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is not int id)
            throw new InvalidOperationException("Не выбрана строка");
        return id;
    }

    private void EditSelected()
    {
        try
        {
            using var form = new RentalEditForm(GetSelectedId());
            if (form.ShowDialog() == DialogResult.OK)
                LoadData(_txtSearch.Text);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void DeleteSelected()
    {
        try
        {
            var id = GetSelectedId();
            var item = _db.RentalRecords.Include(r => r.Inventory).FirstOrDefault(r => r.Id == id);
            if (item == null) return;

            if (MessageBox.Show($"Удалить аренду №{item.Id}? Это действие нельзя отменить.", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (item.Status == Rental.Data.Constants.RentalRecordStatuses.Active)
                item.Inventory.Status = Rental.Data.Constants.InventoryStatuses.Available;

            _db.RentalRecords.Remove(item);
            _db.SaveChanges();
            LoadData(_txtSearch.Text);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при удалении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class RentalGridRow
    {
        public int Id { get; init; }
        public string InventoryName { get; init; } = string.Empty;
        public string ClientName { get; init; } = string.Empty;
        public DateTime IssueDate { get; init; }
        public DateTime PlannedReturnDate { get; init; }
        public string Tariff { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public decimal Fine { get; init; }
        public string Status { get; init; } = string.Empty;
    }
}
