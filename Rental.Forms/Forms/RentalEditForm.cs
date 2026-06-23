using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class RentalEditForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly int _id;
    private readonly ComboBox _cmbInventory = new() { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbClient = new() { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _dtpIssue = new() { Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };
    private readonly DateTimePicker _dtpPlanned = new() { Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };
    private readonly ComboBox _cmbTariff = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbStatus = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _nudTotal = new() { Width = 120, Maximum = 9999999, DecimalPlaces = 2 };
    private readonly NumericUpDown _nudFine = new() { Width = 120, Maximum = 9999999, DecimalPlaces = 2 };

    public RentalEditForm(int id)
    {
        _id = id;
        Text = "Редактирование аренды";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(460, 400);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont;

        _cmbTariff.Items.AddRange(TariffTypes.All);
        _cmbStatus.Items.AddRange(RentalRecordStatuses.All);

        LoadCombos();
        BuildLayout();
        LoadRecord();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadCombos()
    {
        var inventories = _db.Inventories.OrderBy(i => i.Name).ToList();
        _cmbInventory.DisplayMember = "Name";
        _cmbInventory.ValueMember = "Id";
        _cmbInventory.DataSource = inventories;

        var clients = _db.Clients.OrderBy(c => c.LastName).ToList();
        _cmbClient.DisplayMember = "FullName";
        _cmbClient.ValueMember = "Id";
        _cmbClient.DataSource = clients;
    }

    private void BuildLayout()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        AddRow(table, 0, "Инвентарь:", _cmbInventory);
        AddRow(table, 1, "Клиент:", _cmbClient);
        AddRow(table, 2, "Дата выдачи:", _dtpIssue);
        AddRow(table, 3, "План возврата:", _dtpPlanned);
        AddRow(table, 4, "Тариф:", _cmbTariff);
        AddRow(table, 5, "Статус:", _cmbStatus);
        AddRow(table, 6, "Сумма:", _nudTotal);
        AddRow(table, 7, "Штраф:", _nudFine);

        var btnSave = new Button { Text = "Сохранить" };
        var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
        UiTheme.StylePrimaryButton(btnSave);
        UiTheme.StyleSecondaryButton(btnCancel);
        btnSave.Click += (_, _) => Save();

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 45, Padding = new Padding(8) };
        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnSave);

        Controls.Add(table);
        Controls.Add(buttons);
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        table.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = UiTheme.TextSecondary, Font = UiTheme.BodyFont }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void LoadRecord()
    {
        var record = _db.RentalRecords.Find(_id);
        if (record == null) return;

        _cmbInventory.SelectedValue = record.InventoryId;
        _cmbClient.SelectedValue = record.ClientId;
        _dtpIssue.Value = record.IssueDate;
        _dtpPlanned.Value = record.PlannedReturnDate;
        _cmbTariff.SelectedItem = record.Tariff;
        _cmbStatus.SelectedItem = record.Status;
        _nudTotal.Value = record.TotalAmount;
        _nudFine.Value = record.Fine;
    }

    private void Save()
    {
        try
        {
            var record = _db.RentalRecords.Find(_id) ?? throw new InvalidOperationException("Запись не найдена");
            record.InventoryId = (int)(_cmbInventory.SelectedValue ?? 0);
            record.ClientId = (int)(_cmbClient.SelectedValue ?? 0);
            record.IssueDate = _dtpIssue.Value;
            record.PlannedReturnDate = _dtpPlanned.Value;
            record.Tariff = _cmbTariff.SelectedItem?.ToString() ?? TariffTypes.Hour;
            record.Status = _cmbStatus.SelectedItem?.ToString() ?? RentalRecordStatuses.Active;
            record.TotalAmount = _nudTotal.Value;
            record.Fine = _nudFine.Value;

            _db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
