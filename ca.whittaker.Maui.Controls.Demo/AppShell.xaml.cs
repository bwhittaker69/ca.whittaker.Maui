using ca.whittaker.Maui.Controls.Demo.Views;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using ShellItem = Microsoft.Maui.Controls.ShellItem;

namespace ca.whittaker.Maui.Controls.Demo;

public partial class AppShell : Shell
{
    private Dictionary<string, ShellItem> shellItems = new();
    public AppShell()
    {
        InitializeComponent();
    }

    //protected void RegisterForRoute<T>() => Routing.RegisterRoute(typeof(T).Name, typeof(T));

    public void OnLogin()
    {
        LoggedInLayout.IsVisible = true;
        LoggedOutLayout.IsVisible = false;
        this.GoToAsync("//LoggedIn");
    }
    public void OnLogout()
    {
        LoggedInLayout.IsVisible = false;
        LoggedOutLayout.IsVisible = true;
        this.GoToAsync("//LoggedOut");
    }


}
