using ca.whittaker.Maui.Controls.Demo.ViewModels;


namespace ca.whittaker.Maui.Controls.Demo.Views;

public partial class SampleTextBox : ContentPage
{

    public SampleTextBox(LoginLogoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
        //TextBox_Name.FieldUndo();
        TextBox_Name.FieldUnfocus();
    }
}
