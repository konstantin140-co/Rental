using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Helpers;
using Rental.Data.Models;

namespace Rental.Forms.Forms;

public class ReturnRentalForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly ComboBox _cmbRental = new() { Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblCost = new() { AutoSize = true };
    private readonly Label _lblFine = new() { AutoSize = true };
    private readonly Label _lblTotal = new() { AutoSize = true };
    private readonly DateTimePicker _dtpReturn = new() { Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };

    public ReturnRentalForm()
    {
        Text = "Возврат инвентаря";
        Width = 520;
        Height = 320;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        _dtpReturn.Value = DateTime.Now;
        _cmbRental.SelectedIndexChanged += (_, _) => UpdatePreview();
        _dtpReturn.ValueChanged += (_, _) => UpdatePreview();

        _lblTotal.Font = new Font(Font, FontStyle.Bold);

        BuildLayout();
        LoadRentals();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void BuildLayout()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2 };
        AddRow(table, 0, "Активная аренда:", _cmbRental);
        AddRow(table, 1, "Дата возврата:", _dtpReturn);
        AddRow(table, 2, "Стоимость аренды:", _lblCost);
        AddRow(table, 3, "Штраф:", _lblFine);
        AddRow(table, 4, "Итого к оплате:", _lblTotal);

        var btnReturn = new Button { Text = "Принять возврат", Width = 140 };
        var btnClose = new Button { Text = "Закрыть", DialogResult = DialogResult.Cancel, Width = 100 };
        btnReturn.Click += (_, _) => ProcessReturn();

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 48, Padding = new Padding(8) };
        buttons.Controls.Add(btnClose);
        buttons.Controls.Add(btnReturn);

        Controls.Add(table);
        Controls.Add(buttons);
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void LoadRentals()
    {
        var rentals = _db.RentalRecords
            .Include(r => r.Inventory)
            .Include(r => r.Client)
            .Where(r => r.Status == RentalRecordStatuses.Active || r.Status == RentalRecordStatuses.Overdue)
            .OrderBy(r => r.PlannedReturnDate)
            .ToList();

        _cmbRental.DisplayMember = "DisplayText";
        _cmbRental.ValueMember = "Id";
        _cmbRental.DataSource = rentals.Select(r => new RentalListItem(r)).ToList();

        if (rentals.Count == 0)
            MessageBox.Show("Нет активных аренд для возврата.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_cmbRental.SelectedItem is not RentalListItem item)
        {
            _lblCost.Text = _lblFine.Text = _lblTotal.Text = "—";
            return;
        }

        var rental = item.Record;
        var cost = RentalCalculator.CalculateRentalCost(
            rental.IssueDate,
            _dtpReturn.Value,
            rental.Inventory.PricePerHour,
            rental.Inventory.PricePerDay,
            rental.Tariff);

        var fine = RentalCalculator.CalculateFine(rental.PlannedReturnDate, _dtpReturn.Value, cost);
        var total = RentalCalculator.CalculateReturnTotal(cost, fine);

        _lblCost.Text = $"{cost:N2} руб.";
        _lblFine.Text = $"{fine:N2} руб.";
        _lblTotal.Text = $"{total:N2} руб.";
    }

    private void ProcessReturn()
    {
        try
        {
            if (_cmbRental.SelectedItem is not RentalListItem item)
                throw new ArgumentException("Выберите аренду");

            var rental = _db.RentalRecords
                .Include(r => r.Inventory)
                .FirstOrDefault(r => r.Id == item.Record.Id)
                ?? throw new ArgumentException("Аренда не найдена");

            if (rental.Status == RentalRecordStatuses.Completed)
                throw new InvalidOperationException("Аренда уже завершена");

            var returnDate = _dtpReturn.Value;
            if (returnDate < rental.IssueDate)
                throw new ArgumentException("Дата возврата не может быть раньше даты выдачи");

            var cost = RentalCalculator.CalculateRentalCost(
                rental.IssueDate,
                returnDate,
                rental.Inventory.PricePerHour,
                rental.Inventory.PricePerDay,
                rental.Tariff);

            rental.Fine = RentalCalculator.CalculateFine(rental.PlannedReturnDate, returnDate, cost);
            rental.TotalAmount = cost;
            rental.ActualReturnDate = returnDate;
            rental.Status = RentalRecordStatuses.Completed;
            rental.Inventory.Status = InventoryStatuses.Available;

            _db.SaveChanges();

            MessageBox.Show(
                $"Возврат оформлен.\nСтоимость: {cost:N2} руб.\nШтраф: {rental.Fine:N2} руб.\nИтого: {RentalCalculator.CalculateReturnTotal(cost, rental.Fine):N2} руб.",
                "Успешно",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadRentals();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Недопустимая операция", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Системная ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class RentalListItem
    {
        public RentalRecord Record { get; }
        public int Id => Record.Id;
        public string DisplayText => $"№{Record.Id}: {Record.Inventory.Name} — {Record.Client.LastName} {Record.Client.FirstName} (до {Record.PlannedReturnDate:dd.MM.yyyy HH:mm})";

        public RentalListItem(RentalRecord record) => Record = record;
    }
}
