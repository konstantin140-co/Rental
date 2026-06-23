using Microsoft.EntityFrameworkCore;
using Rental.Data.Context;
using Rental.Forms.Ui;

namespace Rental.Forms.Forms;

public class CategoryReportForm : ShellForm
{
    private readonly AppDbContext _db = new();
    private readonly DateTimePicker _dtpFrom = new() { Width = 140, Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _dtpTo = new() { Width = 140, Format = DateTimePickerFormat.Short };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
    private readonly Label _lblTotal = new() { AutoSize = true, ForeColor = UiTheme.TextSecondary };

    public CategoryReportForm()
    {
        SetActiveNavKey("categories");
        NavigationService.NavigateTo(this, "categories");
        UiTheme.StyleDataGrid(_grid);

        _dtpFrom.Value = DateTime.Today.AddMonths(-3);
        _dtpTo.Value = DateTime.Today;

        var header = new PageHeaderControl();
        header.SetTitle("Популярные категории");
        header.ConfigureAction("", false);
        header.HideSearch();
        header.SearchBox.Parent!.Visible = false;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Категория", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RentalCount", HeaderText = "Кол-во аренд", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Revenue", HeaderText = "Выручка", Width = 120 });

        var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44 };
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

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 32 };
        footer.Controls.Add(_lblTotal);

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
        var data = _db.RentalRecords.Include(r => r.Inventory)
            .Where(r => r.IssueDate >= from && r.IssueDate < to)
            .AsEnumerable()
            .GroupBy(r => r.Inventory.Category)
            .Select(g => new { Category = g.Key, RentalCount = g.Count(), Revenue = g.Sum(x => x.TotalAmount + x.Fine) })
            .OrderByDescending(x => x.RentalCount).ToList();

        _grid.DataSource = data;
        _lblTotal.Text = $"Всего категорий: {data.Count}, аренд: {data.Sum(x => x.RentalCount)}";
    }
}
