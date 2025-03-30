using ca.whittaker.Maui.Controls.Demo.ViewModels;


namespace ca.whittaker.Maui.Controls.Demo.Views;

public partial class SampleCheckBoxPage : ContentPage
{

    public SampleCheckBoxPage(LoginLogoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
        CheckBox_IsPublic.Undo();
        CheckBox_IsPublic.Unfocus();
    }
}
