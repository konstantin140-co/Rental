using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Helpers;

namespace Rental.Forms.Forms;

public class OverdueListForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly Label _lblCount = new() { AutoSize = true, Padding = new Padding(8) };

    public OverdueListForm()
    {
        Text = "Просроченные аренды";
        Width = 1000;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryName", HeaderText = "Инвентарь", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClientName", HeaderText = "Клиент", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IssueDate", HeaderText = "Выдача", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PlannedReturnDate", HeaderText = "План возврата", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OverdueDays", HeaderText = "Дней просрочки", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Deposit", HeaderText = "Залог", Width = 90 });

        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8) };
        var btnRefresh = new Button { Text = "Обновить", Width = 110 };
        btnRefresh.Click += (_, _) => LoadData();
        panel.Controls.Add(btnRefresh);
        panel.Controls.Add(_lblCount);

        Controls.Add(_grid);
        Controls.Add(panel);

        DataGridViewSortHelper.Attach(_grid);

        Load += (_, _) => LoadData();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadData()
    {
        var now = DateTime.Now;
        var rentals = _db.RentalRecords
            .Include(r => r.Inventory)
            .Include(r => r.Client)
            .Where(r => r.ActualReturnDate == null &&
                        (r.Status == RentalRecordStatuses.Overdue ||
                         (r.Status == RentalRecordStatuses.Active && r.PlannedReturnDate < now)))
            .OrderBy(r => r.PlannedReturnDate)
            .ToList();

        foreach (var rental in rentals.Where(r => r.Status == RentalRecordStatuses.Active && r.PlannedReturnDate < now))
            rental.Status = RentalRecordStatuses.Overdue;

        if (rentals.Any(r => r.Status == RentalRecordStatuses.Overdue))
            _db.SaveChanges();

        var items = rentals.Select(r => new
        {
            r.Id,
            InventoryName = r.Inventory.Name,
            ClientName = r.Client.FullName,
            IssueDate = r.IssueDate,
            PlannedReturnDate = r.PlannedReturnDate,
            OverdueDays = RentalCalculator.GetOverdueDays(r.PlannedReturnDate, now),
            Deposit = r.DepositAmount
        }).ToList();

        _grid.DataSource = items;
        _lblCount.Text = $"Просрочено: {items.Count}";
    }
}
