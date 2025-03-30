using ca.whittaker.Maui.Controls.Demo.ViewModels;


namespace ca.whittaker.Maui.Controls.Demo.Views;

public partial class LoginLogoutPage : ContentPage
{

    public LoginLogoutPage(LoginLogoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

}
