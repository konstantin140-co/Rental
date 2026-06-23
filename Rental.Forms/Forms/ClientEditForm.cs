using Rental.Data.Context;
using Rental.Data.Models;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class ClientEditForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly int _id;
    private readonly TextBox _txtLastName = new() { Width = 280 };
    private readonly TextBox _txtFirstName = new() { Width = 280 };
    private readonly TextBox _txtPassport = new() { Width = 280 };
    private readonly TextBox _txtPhone = new() { Width = 280 };

    public ClientEditForm(int id)
    {
        _id = id;
        Text = id == 0 ? "Добавление клиента" : "Редактирование клиента";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(440, 300);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont;

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(int row, string label, TextBox textBox)
        {
            table.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = UiTheme.TextSecondary, Font = UiTheme.BodyFont }, 0, row);
            table.Controls.Add(UiTheme.WrapInput(textBox), 1, row);
        }

        AddRow(0, "Фамилия:", _txtLastName);
        AddRow(1, "Имя:", _txtFirstName);
        AddRow(2, "Паспорт:", _txtPassport);
        AddRow(3, "Телефон:", _txtPhone);

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
        FormClosed += (_, _) => _db.Dispose();

        if (_id != 0)
        {
            var item = _db.Clients.Find(_id);
            if (item != null)
            {
                _txtLastName.Text = item.LastName;
                _txtFirstName.Text = item.FirstName;
                _txtPassport.Text = item.Passport;
                _txtPhone.Text = item.Phone;
            }
        }
    }

    private void Save()
    {
        FormValidation.ResetBackColor(_txtLastName, _txtFirstName, _txtPassport, _txtPhone);
        var valid = FormValidation.RequireText(_txtLastName)
                    & FormValidation.RequireText(_txtFirstName)
                    & FormValidation.RequireText(_txtPassport)
                    & FormValidation.RequireText(_txtPhone);
        if (!valid)
        {
            MessageBox.Show("Заполните обязательные поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Client item = _id == 0
                ? new Client()
                : _db.Clients.Find(_id) ?? throw new InvalidOperationException("Запись не найдена");

            item.LastName = _txtLastName.Text.Trim();
            item.FirstName = _txtFirstName.Text.Trim();
            item.Passport = _txtPassport.Text.Trim();
            item.Phone = _txtPhone.Text.Trim();

            if (_id == 0) _db.Clients.Add(item);
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
