using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Demo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Navigating += OnShellNavigating;
            Navigated += OnShellNavigated;
        }

        private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            Debug.WriteLine($"Navigating from {e.Current?.Location} to {e.Target?.Location}.");
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            Debug.WriteLine("Navigation completed.");
        }
    }
}
