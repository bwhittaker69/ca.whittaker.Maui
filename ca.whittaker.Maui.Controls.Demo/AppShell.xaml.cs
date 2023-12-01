using ca.whittaker.Maui.Controls.Demo.Views;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using ShellItem = Microsoft.Maui.Controls.ShellItem;

namespace ca.whittaker.Maui.Controls.Demo;

public partial class AppShell : Shell
{
    private readonly Dictionary<string, ShellItem> shellItems = new();
    public AppShell()
    {
        InitializeComponent();
    }

}
