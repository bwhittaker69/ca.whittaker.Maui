using ca.whittaker.Maui.Controls.Demo.ViewModels;


namespace ca.whittaker.Maui.Controls.Demo.Views;

public partial class TestForm1 : ContentPage
{

    public TestForm1(TestForm1ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

}
