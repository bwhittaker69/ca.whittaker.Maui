using ca.whittaker.Maui.Controls.Demo.ViewModels;


namespace ca.whittaker.Maui.Controls.Demo.Views;

public partial class TestForm3 : ContentPage
{

    public TestForm3(TestForm1ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
        //TextBox_Name.FieldUndo();
        TextBox_Name.Unfocus();
    }
}
