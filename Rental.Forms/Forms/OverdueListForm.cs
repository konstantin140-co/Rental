using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Helpers;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class OverdueListForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
    private readonly Label _lblCount = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary, Tag = "secondary" };

    public OverdueListForm()
    {
        SetActiveNavKey("overdue");
        NavigationService.NavigateTo(this, "overdue");
        UiTheme.StyleDataGrid(_grid);

        var header = new PageHeaderControl();
        header.SetTitle("Просроченные аренды");
        header.ConfigureAction("", false);
        header.HideSearch();
        header.SearchBox.Parent!.Visible = false;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryName", HeaderText = "Инвентарь", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClientName", HeaderText = "Клиент", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PlannedReturnDate", HeaderText = "План возврата", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OverdueDays", HeaderText = "Дней просрочки", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Deposit", HeaderText = "Залог", Width = 90 });

        var card = new RoundedPanel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        card.Controls.Add(_grid);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 32 };
        var btnRefresh = new Button { Text = "Обновить", Width = 110 };
        UiTheme.StyleSecondaryButton(btnRefresh);
        btnRefresh.Click += (_, _) => LoadData();
        footer.Controls.Add(btnRefresh);
        footer.Controls.Add(_lblCount);

        ContentPanel.Controls.Add(card);
        ContentPanel.Controls.Add(footer);
        ContentPanel.Controls.Add(header);

        DataGridViewSortHelper.Attach(_grid);
        Load += (_, _) => LoadData();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadData()
    {
        var now = DateTime.Now;
        var rentals = _db.RentalRecords.Include(r => r.Inventory).Include(r => r.Client)
            .Where(r => r.ActualReturnDate == null && (r.Status == RentalRecordStatuses.Overdue || (r.Status == RentalRecordStatuses.Active && r.PlannedReturnDate < now)))
            .OrderBy(r => r.PlannedReturnDate).ToList();

        foreach (var r in rentals.Where(r => r.Status == RentalRecordStatuses.Active && r.PlannedReturnDate < now))
            r.Status = RentalRecordStatuses.Overdue;
        if (rentals.Any(r => r.Status == RentalRecordStatuses.Overdue)) _db.SaveChanges();

        _grid.DataSource = rentals.Select(r => new
        {
            r.Id,
            InventoryName = r.Inventory.Name,
            ClientName = r.Client.FullName,
            r.PlannedReturnDate,
            OverdueDays = RentalCalculator.GetOverdueDays(r.PlannedReturnDate, now),
            Deposit = r.DepositAmount
        }).ToList();
        _lblCount.Text = $"  Просрочено: {rentals.Count}";
    }
}
