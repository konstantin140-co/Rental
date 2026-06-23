using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Models;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class IssueRentalForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly ComboBox _cmbInventory = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbClient = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbTariff = new() { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _dtpPlannedReturn = new() { Width = 220, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };
    private readonly Label _lblDeposit = new() { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = UiTheme.TextPrimary };

    public IssueRentalForm()
    {
        SetActiveNavKey("issue");
        NavigationService.NavigateTo(this, "issue");

        var header = new PageHeaderControl();
        header.SetTitle("Выдача в аренду", "Оформление новой аренды");
        header.ConfigureAction("", false);
        header.HideSearch();

        _cmbTariff.Items.AddRange(TariffTypes.All);
        _cmbTariff.SelectedIndex = 0;
        _dtpPlannedReturn.Value = DateTime.Now.AddHours(2);
        _cmbInventory.SelectedIndexChanged += (_, _) => UpdateDeposit();

        var card = new RoundedPanel { Dock = DockStyle.Top, Height = 280, Padding = new Padding(24) };
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        AddRow(table, 0, "Инвентарь:", _cmbInventory);
        AddRow(table, 1, "Клиент:", _cmbClient);
        AddRow(table, 2, "Тариф:", _cmbTariff);
        AddRow(table, 3, "Плановый возврат:", _dtpPlannedReturn);
        AddRow(table, 4, "Залог:", _lblDeposit);
        card.Controls.Add(table);

        var btnIssue = new Button { Text = "Выдать в аренду", Width = 160 };
        UiTheme.StylePrimaryButton(btnIssue);
        btnIssue.Click += (_, _) => Issue();

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 8, 0, 0) };
        actions.Controls.Add(btnIssue);

        ContentPanel.Controls.Add(actions);
        ContentPanel.Controls.Add(card);
        ContentPanel.Controls.Add(header);

        Load += (_, _) => LoadCombos();
        FormClosed += (_, _) => _db.Dispose();
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        table.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = UiTheme.TextSecondary, Font = UiTheme.BodyFont }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void LoadCombos()
    {
        var inventories = _db.Inventories.Where(i => i.Status == InventoryStatuses.Available).OrderBy(i => i.Name).ToList();
        if (inventories.Count == 0)
            MessageBox.Show("Нет свободного инвентаря для выдачи.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
        _lblDeposit.Text = _cmbInventory.SelectedItem is Inventory item ? $"{item.Deposit:N2} ₽" : "—";
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

            var record = new RentalRecord
            {
                InventoryId = inventory.Id,
                ClientId = client.Id,
                IssueDate = DateTime.Now,
                PlannedReturnDate = _dtpPlannedReturn.Value,
                Tariff = _cmbTariff.SelectedItem?.ToString() ?? TariffTypes.Hour,
                Status = RentalRecordStatuses.Active,
                DepositAmount = inventory.Deposit
            };
            inventory.Status = InventoryStatuses.Rented;
            _db.RentalRecords.Add(record);
            _db.SaveChanges();

            MessageBox.Show($"Инвентарь «{inventory.Name}» выдан клиенту {client.FullName}.", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadCombos();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
