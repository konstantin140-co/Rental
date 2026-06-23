using Rental.Data.Constants;
using Rental.Data.Context;
using Rental.Data.Models;

namespace Rental.Forms.Forms;

public class InventoryEditForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly int _id;
    private readonly TextBox _txtName = new() { Width = 280 };
    private readonly TextBox _txtCategory = new() { Width = 280 };
    private readonly TextBox _txtNumber = new() { Width = 280 };
    private readonly NumericUpDown _nudHour = new() { Width = 120, Maximum = 999999, DecimalPlaces = 2 };
    private readonly NumericUpDown _nudDay = new() { Width = 120, Maximum = 999999, DecimalPlaces = 2 };
    private readonly NumericUpDown _nudDeposit = new() { Width = 120, Maximum = 999999, DecimalPlaces = 2 };
    private readonly ComboBox _cmbStatus = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

    public InventoryEditForm(int id)
    {
        _id = id;
        Text = id == 0 ? "Добавление инвентаря" : "Редактирование инвентаря";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, 360);

        _cmbStatus.Items.AddRange(InventoryStatuses.All);
        BuildLayout();
        LoadData();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void BuildLayout()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(12) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, 0, "Название:", _txtName);
        AddRow(table, 1, "Категория:", _txtCategory);
        AddRow(table, 2, "Инв. номер:", _txtNumber);
        AddRow(table, 3, "Цена/час:", _nudHour);
        AddRow(table, 4, "Цена/сутки:", _nudDay);
        AddRow(table, 5, "Залог:", _nudDeposit);
        AddRow(table, 6, "Статус:", _cmbStatus);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 45, Padding = new Padding(8) };
        var btnSave = new Button { Text = "Сохранить", DialogResult = DialogResult.None };
        var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
        btnSave.Click += (_, _) => Save();
        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnSave);

        Controls.Add(table);
        Controls.Add(buttons);
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void LoadData()
    {
        if (_id == 0)
        {
            _cmbStatus.SelectedItem = InventoryStatuses.Available;
            return;
        }

        var item = _db.Inventories.Find(_id);
        if (item == null) return;

        _txtName.Text = item.Name;
        _txtCategory.Text = item.Category;
        _txtNumber.Text = item.InventoryNumber;
        _nudHour.Value = item.PricePerHour;
        _nudDay.Value = item.PricePerDay;
        _nudDeposit.Value = item.Deposit;
        _cmbStatus.SelectedItem = item.Status;
    }

    private void Save()
    {
        FormValidation.ResetBackColor(_txtName, _txtCategory, _txtNumber);
        var valid = FormValidation.RequireText(_txtName)
                    & FormValidation.RequireText(_txtCategory)
                    & FormValidation.RequireText(_txtNumber);
        if (!valid)
        {
            MessageBox.Show("Заполните обязательные поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Inventory item;
            if (_id == 0)
            {
                item = new Inventory();
                _db.Inventories.Add(item);
            }
            else
            {
                item = _db.Inventories.Find(_id) ?? throw new InvalidOperationException("Запись не найдена");
            }

            item.Name = _txtName.Text.Trim();
            item.Category = _txtCategory.Text.Trim();
            item.InventoryNumber = _txtNumber.Text.Trim();
            item.PricePerHour = _nudHour.Value;
            item.PricePerDay = _nudDay.Value;
            item.Deposit = _nudDeposit.Value;
            item.Status = _cmbStatus.SelectedItem?.ToString() ?? InventoryStatuses.Available;

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
