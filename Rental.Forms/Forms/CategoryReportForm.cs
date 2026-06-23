using Microsoft.EntityFrameworkCore;
using Rental.Data.Constants;
using Rental.Data.Context;

namespace Rental.Forms.Forms;

public class CategoryReportForm : Form
{
    private readonly AppDbContext _db = new();
    private readonly DateTimePicker _dtpFrom = new() { Width = 160, Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _dtpTo = new() { Width = 160, Format = DateTimePickerFormat.Short };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
    private readonly Label _lblTotal = new() { AutoSize = true, Padding = new Padding(8) };

    public CategoryReportForm()
    {
        Text = "Отчёт: популярные категории";
        Width = 700;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;

        _dtpFrom.Value = DateTime.Today.AddMonths(-3);
        _dtpTo.Value = DateTime.Today;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Категория", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RentalCount", HeaderText = "Кол-во аренд", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Revenue", HeaderText = "Выручка", Width = 120 });

        var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(8) };
        filterPanel.Controls.Add(new Label { Text = "С:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filterPanel.Controls.Add(_dtpFrom);
        filterPanel.Controls.Add(new Label { Text = "По:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
        filterPanel.Controls.Add(_dtpTo);
        var btnApply = new Button { Text = "Применить", Width = 110 };
        btnApply.Click += (_, _) => LoadReport();
        filterPanel.Controls.Add(btnApply);

        var summaryPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8) };
        summaryPanel.Controls.Add(_lblTotal);

        Controls.Add(_grid);
        Controls.Add(summaryPanel);
        Controls.Add(filterPanel);

        DataGridViewSortHelper.Attach(_grid);

        Load += (_, _) => LoadReport();
        FormClosed += (_, _) => _db.Dispose();
    }

    private void LoadReport()
    {
        var from = _dtpFrom.Value.Date;
        var to = _dtpTo.Value.Date.AddDays(1);

        var data = _db.RentalRecords
            .Include(r => r.Inventory)
            .Where(r => r.IssueDate >= from && r.IssueDate < to)
            .AsEnumerable()
            .GroupBy(r => r.Inventory.Category)
            .Select(g => new
            {
                Category = g.Key,
                RentalCount = g.Count(),
                Revenue = g.Sum(x => x.TotalAmount + x.Fine)
            })
            .OrderByDescending(x => x.RentalCount)
            .ToList();

        _grid.DataSource = data;
        _lblTotal.Text = $"Всего категорий: {data.Count}, аренд: {data.Sum(x => x.RentalCount)}";
    }
}
