namespace Rental.Forms.Forms;

internal static class FormValidation
{
    public static void ResetBackColor(params TextBox[] boxes)
    {
        foreach (var box in boxes)
            box.BackColor = SystemColors.Window;
    }

    public static bool RequireText(TextBox box)
    {
        if (!string.IsNullOrWhiteSpace(box.Text))
            return true;

        box.BackColor = Color.LightPink;
        return false;
    }
}
