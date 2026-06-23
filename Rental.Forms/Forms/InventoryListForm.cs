using Microsoft.EntityFrameworkCore;
using Rental.Data.Context;
using Rental.Data.Models;

namespace Rental.Forms.Forms;

public class InventoryListForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly TextBox _txtSearch = new() { PlaceholderText = "Введите для поиска...", Dock = DockStyle.Top };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoGenerateColumns = false, AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(245, 245, 245) } };
    private readonly Button _btnAdd = new() { Text = "Добавить", Width = 110 };
    private readonly Button _btnEdit = new() { Text = "Редактировать", Width = 130, Enabled = false };
    private readonly Button _btnDelete = new() { Text = "Удалить", Width = 110, Enabled = false };
    private readonly Button _btnRefresh = new() { Text = "Обновить", Width = 110 };
    private readonly Label _lblCount = new() { AutoSize = true, Padding = new Padding(8) };
    private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 300 };

    public InventoryListForm()
    {
        Text = "Инвентарь";
        Width = 1000;
        Height = 550;
        StartPosition = FormStartPosition.CenterScreen;

        ConfigureGrid();
        BuildLayout();
        DataGridViewSortHelper.Attach(_grid);

        Load += (_, _) => LoadData();
        _txtSearch.TextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
        _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); LoadData(_txtSearch.Text); };
        _grid.SelectionChanged += (_, _) => UpdateButtons();
        _btnAdd.Click += (_, _) => EditRecord(0);
        _btnEdit.Click += (_, _) => EditSelected();
        _btnDelete.Click += (_, _) => DeleteSelected();
        _btnRefresh.Click += (_, _) => LoadData(_txtSearch.Text);
        FormClosed += (_, _) => _db.Dispose();
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InventoryNumber", DataPropertyName = "InventoryNumber", HeaderText = "Инв. номер", Width = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Название", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", DataPropertyName = "Category", HeaderText = "Категория", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PricePerHour", DataPropertyName = "PricePerHour", HeaderText = "Цена/час", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PricePerDay", DataPropertyName = "PricePerDay", HeaderText = "Цена/сутки", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Deposit", DataPropertyName = "Deposit", HeaderText = "Залог", Width = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Статус", Width = 100 });
    }

    private void BuildLayout()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(8) };
        panel.Controls.AddRange([_btnAdd, _btnEdit, _btnDelete, _btnRefresh, _lblCount]);
        Controls.Add(_grid);
        Controls.Add(panel);
        Controls.Add(_txtSearch);
    }

    private void LoadData(string filter = "")
    {
        var query = _db.Inventories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim();
            query = query.Where(i => i.Name.Contains(f) || i.Category.Contains(f) || i.InventoryNumber.Contains(f));
        }

        var items = query.OrderBy(i => i.Name).ToList();
        _grid.DataSource = items;
        _lblCount.Text = $"Записей: {items.Count}";
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var hasSelection = _grid.CurrentRow?.DataBoundItem is Inventory;
        _btnEdit.Enabled = hasSelection;
        _btnDelete.Enabled = hasSelection;
    }

    private int GetSelectedId()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Inventory item)
            throw new InvalidOperationException("Не выбрана строка");
        return item.Id;
    }

    private void EditSelected()
    {
        try
        {
            EditRecord(GetSelectedId());
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void EditRecord(int id)
    {
        using var form = new InventoryEditForm(id);
        if (form.ShowDialog() == DialogResult.OK)
            LoadData(_txtSearch.Text);
    }

    private void DeleteSelected()
    {
        try
        {
            var id = GetSelectedId();
            var item = _db.Inventories.Find(id);
            if (item == null) return;

            var result = MessageBox.Show(
                $"Удалить «{item.Name}»? Это действие нельзя отменить.",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            _db.Inventories.Remove(item);
            _db.SaveChanges();
            LoadData(_txtSearch.Text);
        }
        catch (DbUpdateException)
        {
            MessageBox.Show("Нельзя удалить запись: есть связанные аренды.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}
