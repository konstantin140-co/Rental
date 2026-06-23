using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rental.Forms.Forms;

internal static class DataGridViewSortHelper
{
    private static readonly ConditionalWeakTable<DataGridView, SortState> SortStates = new();

    public static void Attach(DataGridView grid)
    {
        grid.ColumnHeaderMouseClick += (_, e) => SortByColumn(grid, e.ColumnIndex);
    }

    private static void SortByColumn(DataGridView grid, int columnIndex)
    {
        if (columnIndex < 0)
            return;

        var column = grid.Columns[columnIndex];
        var propertyName = string.IsNullOrWhiteSpace(column.DataPropertyName)
            ? column.Name
            : column.DataPropertyName;

        if (string.IsNullOrWhiteSpace(propertyName))
            return;

        if (grid.DataSource is not IList source || source.Count == 0)
            return;

        var property = source[0]!.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property == null)
            return;

        var state = SortStates.GetOrCreateValue(grid);
        var ascending = state.PropertyName == propertyName
            ? !state.Ascending
            : true;

        state.PropertyName = propertyName;
        state.Ascending = ascending;

        var items = source.Cast<object>().ToList();
        items.Sort((left, right) =>
        {
            var comparison = CompareValues(property.GetValue(left), property.GetValue(right));
            return ascending ? comparison : -comparison;
        });

        var keyColumn = FindColumn(grid, "Id");
        object? selectedKey = null;
        if (keyColumn != null && grid.CurrentRow != null)
            selectedKey = grid.CurrentRow.Cells[keyColumn.Index].Value;

        grid.DataSource = items;
        ApplySortGlyphs(grid, columnIndex, ascending);

        if (selectedKey != null && keyColumn != null)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (Equals(row.Cells[keyColumn.Index].Value, selectedKey))
                {
                    row.Selected = true;
                    grid.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }
    }

    private static void ApplySortGlyphs(DataGridView grid, int sortedColumnIndex, bool ascending)
    {
        foreach (DataGridViewColumn col in grid.Columns)
            col.HeaderCell.SortGlyphDirection = SortOrder.None;

        grid.Columns[sortedColumnIndex].HeaderCell.SortGlyphDirection =
            ascending ? SortOrder.Ascending : SortOrder.Descending;
    }

    private static DataGridViewColumn? FindColumn(DataGridView grid, string propertyName)
    {
        foreach (DataGridViewColumn col in grid.Columns)
        {
            if (col.Name == propertyName || col.DataPropertyName == propertyName)
                return col;
        }

        return null;
    }

    private static int CompareValues(object? left, object? right)
    {
        if (left == null && right == null)
            return 0;
        if (left == null)
            return -1;
        if (right == null)
            return 1;

        return left is IComparable comparable
            ? comparable.CompareTo(right)
            : string.Compare(left.ToString(), right.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }

    private sealed class SortState
    {
        public string? PropertyName { get; set; }
        public bool Ascending { get; set; } = true;
    }
}
