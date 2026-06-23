using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Helpers;
using Rental.Data.Models;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class ReturnRentalForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly ComboBox _cmbRental = new() { Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblCost = new() { AutoSize = true, ForeColor = UiTheme.TextPrimary };
    private readonly Label _lblFine = new() { AutoSize = true, ForeColor = UiTheme.TextPrimary };
    private readonly Label _lblTotal = new() { AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = UiTheme.Accent };
    private readonly DateTimePicker _dtpReturn = new() { Width = 220, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };

    public ReturnRentalForm()
    {
        SetActiveNavKey("return");
        NavigationService.NavigateTo(this, "return");

        var header = new PageHeaderControl();
        header.SetTitle("Возврат инвентаря", "Расчёт стоимости и штрафа");
        header.ConfigureAction("", false);
        header.HideSearch();

        _dtpReturn.Value = DateTime.Now;
        _cmbRental.SelectedIndexChanged += (_, _) => UpdatePreview();
        _dtpReturn.ValueChanged += (_, _) => UpdatePreview();

        var card = new RoundedPanel { Dock = DockStyle.Top, Height = 260, Padding = new Padding(24) };
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        AddRow(table, 0, "Активная аренда:", _cmbRental);
        AddRow(table, 1, "Дата возврата:", _dtpReturn);
        AddRow(table, 2, "Стоимость:", _lblCost);
        AddRow(table, 3, "Штраф:", _lblFine);
        AddRow(table, 4, "Итого:", _lblTotal);
        card.Controls.Add(table);

        var btnReturn = new Button { Text = "Принять возврат", Width = 160 };
        UiTheme.StylePrimaryButton(btnReturn);
        btnReturn.Click += (_, _) => ProcessReturn();

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48 };
        actions.Controls.Add(btnReturn);

        ContentPanel.Controls.Add(actions);
        ContentPanel.Controls.Add(card);
        ContentPanel.Controls.Add(header);

        Load += (_, _) => LoadRentals();
        FormClosed += (_, _) => _db.Dispose();
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        table.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = UiTheme.TextSecondary }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void LoadRentals()
    {
        var rentals = _db.RentalRecords
            .Include(r => r.Inventory).Include(r => r.Client)
            .Where(r => r.Status == RentalRecordStatuses.Active || r.Status == RentalRecordStatuses.Overdue)
            .OrderBy(r => r.PlannedReturnDate).ToList();

        _cmbRental.DataSource = rentals.Select(r => new RentalListItem(r)).ToList();
        _cmbRental.DisplayMember = "DisplayText";
        if (rentals.Count == 0)
            MessageBox.Show("Нет активных аренд.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_cmbRental.SelectedItem is not RentalListItem item)
        {
            _lblCost.Text = _lblFine.Text = _lblTotal.Text = "—";
            return;
        }
        var r = item.Record;
        var cost = RentalCalculator.CalculateRentalCost(r.IssueDate, _dtpReturn.Value, r.Inventory.PricePerHour, r.Inventory.PricePerDay, r.Tariff);
        var fine = RentalCalculator.CalculateFine(r.PlannedReturnDate, _dtpReturn.Value, cost);
        _lblCost.Text = $"{cost:N2} ₽";
        _lblFine.Text = $"{fine:N2} ₽";
        _lblTotal.Text = $"{RentalCalculator.CalculateReturnTotal(cost, fine):N2} ₽";
    }

    private void ProcessReturn()
    {
        try
        {
            if (_cmbRental.SelectedItem is not RentalListItem item)
                throw new ArgumentException("Выберите аренду");

            var rental = _db.RentalRecords.Include(r => r.Inventory).First(r => r.Id == item.Record.Id);
            var returnDate = _dtpReturn.Value;
            var cost = RentalCalculator.CalculateRentalCost(rental.IssueDate, returnDate, rental.Inventory.PricePerHour, rental.Inventory.PricePerDay, rental.Tariff);

            rental.Fine = RentalCalculator.CalculateFine(rental.PlannedReturnDate, returnDate, cost);
            rental.TotalAmount = cost;
            rental.ActualReturnDate = returnDate;
            rental.Status = RentalRecordStatuses.Completed;
            rental.Inventory.Status = InventoryStatuses.Available;
            _db.SaveChanges();

            MessageBox.Show("Возврат оформлен.", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadRentals();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private sealed class RentalListItem(RentalRecord record)
    {
        public RentalRecord Record { get; } = record;
        public string DisplayText => $"№{Record.Id}: {Record.Inventory.Name} — {Record.Client.FullName}";
    }
}
