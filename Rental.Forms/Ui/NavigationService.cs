namespace Rental.Forms.Ui;

internal static class NavigationService
{
    private static Form? _currentShell;
    private static readonly List<ShellForm> _openShells = [];

    public static void NavigateTo(Form target, string navKey)
    {
        if (_currentShell != null && _currentShell != target && !_currentShell.IsDisposed)
            _currentShell.Hide();

        _currentShell = target;
        if (target is ShellForm shell)
        {
            shell.SetActiveNavKey(navKey);
            if (!_openShells.Contains(shell))
                _openShells.Add(shell);
            shell.ApplyTheme();
        }

        target.Show();
        target.BringToFront();
    }

    public static void Unregister(Form form)
    {
        if (_currentShell == form)
            _currentShell = null;
        if (form is ShellForm shell)
            _openShells.Remove(shell);
    }

    public static void RefreshAllThemes()
    {
        foreach (var shell in _openShells.Where(s => !s.IsDisposed).ToList())
            shell.ApplyTheme();
    }
}
