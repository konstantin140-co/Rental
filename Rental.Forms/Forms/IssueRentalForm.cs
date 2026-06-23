using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Models;

namespace Rental.Forms.Forms;

public class IssueRentalForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly ComboBox _cmbInventory = new() { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbClient = new() { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbTariff = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _dtpPlannedReturn = new() { Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };
    private readonly Label _lblDeposit = new() { AutoSize = true };

    public IssueRentalForm()
    {
        Text = "Выдача в аренду";
        Width = 500;
        Height = 320;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        _cmbTariff.Items.AddRange(TariffTypes.All);
        _cmbTariff.SelectedIndex = 0;
        _dtpPlannedReturn.Value = DateTime.Now.AddHours(2);
        _cmbInventory.SelectedIndexChanged += (_, _) => UpdateDeposit();

        _lblDeposit.Font = new Font(Font, FontStyle.Bold);
        BuildLayout();
        LoadCombos();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void BuildLayout()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2 };
        AddRow(table, 0, "Инвентарь:", _cmbInventory);
        AddRow(table, 1, "Клиент:", _cmbClient);
        AddRow(table, 2, "Тариф:", _cmbTariff);
        AddRow(table, 3, "Плановый возврат:", _dtpPlannedReturn);
        AddRow(table, 4, "Залог:", _lblDeposit);

        var btnIssue = new Button { Text = "Выдать", Width = 100 };
        var btnClose = new Button { Text = "Закрыть", DialogResult = DialogResult.Cancel, Width = 100 };
        btnIssue.Click += (_, _) => Issue();

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 48, Padding = new Padding(8) };
        buttons.Controls.Add(btnClose);
        buttons.Controls.Add(btnIssue);

        Controls.Add(table);
        Controls.Add(buttons);
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void LoadCombos()
    {
        var inventories = _db.Inventories
            .Where(i => i.Status == InventoryStatuses.Available)
            .OrderBy(i => i.Name)
            .ToList();

        if (inventories.Count == 0)
        {
            MessageBox.Show("Нет свободного инвентаря для выдачи.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        _cmbInventory.DisplayMember = "Name";
        _cmbInventory.ValueMember = "Id";
        _cmbInventory.DataSource = inventories;

        var clients = _db.Clients.OrderBy(c => c.LastName).ToList();
        _cmbClient.DisplayMember = "FullName";
        _cmbClient.ValueMember = "Id";
        _cmbClient.DataSource = clients;

        UpdateDeposit();
    }

    private void UpdateDeposit()
    {
        if (_cmbInventory.SelectedItem is Inventory item)
            _lblDeposit.Text = $"{item.Deposit:N2} руб.";
        else
            _lblDeposit.Text = "—";
    }

    private void Issue()
    {
        try
        {
            if (_cmbInventory.SelectedItem is not Inventory inventory)
                throw new ArgumentException("Выберите инвентарь");
            if (_cmbClient.SelectedItem is not Client client)
                throw new ArgumentException("Выберите клиента");
            if (_dtpPlannedReturn.Value <= DateTime.Now)
                throw new ArgumentException("Плановая дата возврата должна быть в будущем");
            if (inventory.Status != InventoryStatuses.Available)
                throw new InvalidOperationException($"Инвентарь «{inventory.Name}» недоступен (статус: {inventory.Status})");

            var tariff = _cmbTariff.SelectedItem?.ToString() ?? TariffTypes.Hour;
            var record = new RentalRecord
            {
                InventoryId = inventory.Id,
                ClientId = client.Id,
                IssueDate = DateTime.Now,
                PlannedReturnDate = _dtpPlannedReturn.Value,
                Tariff = tariff,
                Status = RentalRecordStatuses.Active,
                DepositAmount = inventory.Deposit,
                TotalAmount = 0,
                Fine = 0
            };

            inventory.Status = InventoryStatuses.Rented;
            _db.RentalRecords.Add(record);
            _db.SaveChanges();

            MessageBox.Show(
                $"Инвентарь «{inventory.Name}» выдан клиенту {client.FullName}.\nЗалог: {inventory.Deposit:N2} руб.\nПлановый возврат: {record.PlannedReturnDate:dd.MM.yyyy HH:mm}",
                "Успешно",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadCombos();
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
}
