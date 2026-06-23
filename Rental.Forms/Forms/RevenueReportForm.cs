using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class RevenueReportForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly DateTimePicker _dtpFrom = new() { Width = 140, Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _dtpTo = new() { Width = 140, Format = DateTimePickerFormat.Short };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
    private readonly Label _lblTotal = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary };
    private readonly Label _lblRevenue = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary };
    private readonly Label _lblFines = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary };

    public RevenueReportForm()
    {
        SetActiveNavKey("revenue");
        NavigationService.NavigateTo(this, "revenue");
        UiTheme.StyleDataGrid(_grid);

        _dtpFrom.Value = DateTime.Today.AddDays(-30);
        _dtpTo.Value = DateTime.Today;

        var header = new PageHeaderControl();
        header.SetTitle("Выручка за период");
        header.ConfigureAction("", false);
        header.HideSearch();
        header.SearchBox.Parent!.Visible = false;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryName", HeaderText = "Инвентарь", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClientName", HeaderText = "Клиент", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReturnDate", HeaderText = "Возврат", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Сумма", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Fine", HeaderText = "Штраф", Width = 80 });

        var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 4, 0, 8) };
        filter.Controls.Add(new Label { Text = "С:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filter.Controls.Add(_dtpFrom);
        filter.Controls.Add(new Label { Text = "По:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
        filter.Controls.Add(_dtpTo);
        var btnApply = new Button { Text = "Применить", Width = 110 };
        UiTheme.StylePrimaryButton(btnApply);
        btnApply.Click += (_, _) => LoadReport();
        filter.Controls.Add(btnApply);

        var card = new RoundedPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        card.Controls.Add(_grid);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36 };
        footer.Controls.AddRange([_lblTotal, _lblRevenue, _lblFines]);

        ContentPanel.Controls.Add(card);
        ContentPanel.Controls.Add(footer);
        ContentPanel.Controls.Add(filter);
        ContentPanel.Controls.Add(header);

        DataGridViewSortHelper.Attach(_grid);
        Load += (_, _) => LoadReport();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadReport()
    {
        var from = _dtpFrom.Value.Date;
        var to = _dtpTo.Value.Date.AddDays(1);
        var rentals = _db.RentalRecords.Include(r => r.Inventory).Include(r => r.Client)
            .Where(r => r.Status == RentalRecordStatuses.Completed && r.ActualReturnDate >= from && r.ActualReturnDate < to)
            .OrderByDescending(r => r.ActualReturnDate).ToList();

        _grid.DataSource = rentals.Select(r => new { r.Id, InventoryName = r.Inventory.Name, ClientName = r.Client.FullName, ReturnDate = r.ActualReturnDate!.Value, r.TotalAmount, r.Fine }).ToList();
        _lblTotal.Text = $"Всего: {rentals.Count}";
        _lblRevenue.Text = $"  Выручка: {rentals.Sum(r => r.TotalAmount):N2} ₽";
        _lblFines.Text = $"  Штрафы: {rentals.Sum(r => r.Fine):N2} ₽";
    }
}
