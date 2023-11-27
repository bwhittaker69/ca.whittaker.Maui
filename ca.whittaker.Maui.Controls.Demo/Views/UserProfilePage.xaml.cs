using ca.whittaker.Maui.Controls.Forms;
using Microsoft.Extensions.Logging;

namespace ca.whittaker.MobileApp2.Views;

public partial class LoginPage : ContentPage
{

    public LoginPage(ViewModels.LoginPageViewModel viewModel, ILogger<Form> _logger)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((ViewModels.LoginPageViewModel)this.BindingContext).Initialize();
    }
   
}
