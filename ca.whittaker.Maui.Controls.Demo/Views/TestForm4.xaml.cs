using ca.whittaker.Maui.Controls.Demo.ViewModels;


namespace ca.whittaker.Maui.Controls.Demo.Views;

public partial class TestForm4 : ContentPage
{

    public TestForm4(TestForm1ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
        //CheckBox_IsPublic.FieldUndo();
        CheckBox_IsPublic.Unfocus();
    }
}
