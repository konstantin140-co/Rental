using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;

namespace Rental.Forms.Forms;

public class RevenueReportForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly DateTimePicker _dtpFrom = new() { Width = 160, Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _dtpTo = new() { Width = 160, Format = DateTimePickerFormat.Short };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
    private readonly Label _lblTotal = new() { AutoSize = true, Padding = new Padding(8) };
    private readonly Label _lblRevenue = new() { AutoSize = true, Padding = new Padding(8) };
    private readonly Label _lblFines = new() { AutoSize = true, Padding = new Padding(8) };

    public RevenueReportForm()
    {
        Text = "Отчёт: выручка за период";
        Width = 1000;
        Height = 550;
        StartPosition = FormStartPosition.CenterScreen;

        _dtpFrom.Value = DateTime.Today.AddDays(-30);
        _dtpTo.Value = DateTime.Today;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryName", HeaderText = "Инвентарь", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClientName", HeaderText = "Клиент", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReturnDate", HeaderText = "Возврат", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Сумма", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Fine", HeaderText = "Штраф", Width = 80 });

        var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(8) };
        filterPanel.Controls.Add(new Label { Text = "С:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filterPanel.Controls.Add(_dtpFrom);
        filterPanel.Controls.Add(new Label { Text = "По:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
        filterPanel.Controls.Add(_dtpTo);
        var btnApply = new Button { Text = "Применить", Width = 110 };
        btnApply.Click += (_, _) => LoadReport();
        filterPanel.Controls.Add(btnApply);

        var summaryPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8) };
        summaryPanel.Controls.AddRange([_lblTotal, _lblRevenue, _lblFines]);

        Controls.Add(_grid);
        Controls.Add(summaryPanel);
        Controls.Add(filterPanel);

        DataGridViewSortHelper.Attach(_grid);

        Load += (_, _) => LoadReport();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadReport()
    {
        var from = _dtpFrom.Value.Date;
        var to = _dtpTo.Value.Date.AddDays(1);

        var rentals = _db.RentalRecords
            .Include(r => r.Inventory)
            .Include(r => r.Client)
            .Where(r => r.Status == RentalRecordStatuses.Completed &&
                        r.ActualReturnDate >= from &&
                        r.ActualReturnDate < to)
            .OrderByDescending(r => r.ActualReturnDate)
            .ToList();

        _grid.DataSource = rentals.Select(r => new
        {
            r.Id,
            InventoryName = r.Inventory.Name,
            ClientName = r.Client.FullName,
            ReturnDate = r.ActualReturnDate!.Value,
            r.TotalAmount,
            r.Fine
        }).ToList();

        _lblTotal.Text = $"Всего аренд: {rentals.Count}";
        _lblRevenue.Text = $"Выручка: {rentals.Sum(r => r.TotalAmount):N2} руб.";
        _lblFines.Text = $"Штрафы: {rentals.Sum(r => r.Fine):N2} руб.";
    }
}
