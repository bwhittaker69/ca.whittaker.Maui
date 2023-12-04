using ca.whittaker.Maui.Controls.Demo.ViewModels;


namespace ca.whittaker.Maui.Controls.Demo.Views;

public partial class UserProfilePage : ContentPage
{

    public UserProfilePage(LoginLogoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
        UserProfileForm.CancelForm();
    }
}
