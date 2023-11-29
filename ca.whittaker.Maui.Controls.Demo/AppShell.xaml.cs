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
        AddDynamicItems();
    }

    private void AddDynamicItems()
    {
        // Create ShellContent
        var loginPage = new ShellContent
        {
            Title = "Login",
            Route = nameof(LoginLogoutPage),
            ContentTemplate = new DataTemplate(typeof(LoginLogoutPage)),
            IsVisible = true
        };

        var profilePage = new ShellContent
        {
            Title = "Profile",
            Route = nameof(UserProfilePage),
            ContentTemplate = new DataTemplate(typeof(UserProfilePage)),
            IsVisible = false
        };

        // Create Tab
        var tab = new Tab { Title = "Demo" };
        tab.Items.Add(loginPage);
        tab.Items.Add(profilePage);

        // Create FlyoutItem and add the Tab to it
        var flyoutItem = new FlyoutItem { FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems };
        flyoutItem.Items.Add(tab);

        // Add the FlyoutItem to the Shell
        Items.Add(flyoutItem);
    }

    public void SetShellContentVisibility(string contentRoute, bool isVisible)
    {
        var shellContent = FindShellContent(contentRoute) as ShellContent;

        if (shellContent != null)
        {
            shellContent.IsVisible = isVisible;
        }
    }

    public ShellContent FindShellContent(string itemName)
    {
        foreach (var shellItem in Items)
        {
            // ShellItem can contain multiple ShellSections
            foreach (var shellSection in shellItem.Items)
            {
                foreach (var shellContent in shellSection.Items)
                {
                    if (shellContent.Route == itemName)
                    {
                        return shellContent;
                    }
                }
            }
        }

        return null;
    }


    public void OnLogin()
    {
        if (FindShellContent(nameof(UserProfilePage)) != null)
        {
            SetShellContentVisibility(nameof(UserProfilePage), true);
            this.GoToAsync($"//{nameof(UserProfilePage)}");
            return;
        }
    }
    public void OnLogout()
    {
        if (FindShellContent(nameof(UserProfilePage)) != null)
        {
            SetShellContentVisibility(nameof(UserProfilePage), false);
            this.GoToAsync($"//{nameof(LoginLogoutPage)}");
            return;
        }
    }


}
