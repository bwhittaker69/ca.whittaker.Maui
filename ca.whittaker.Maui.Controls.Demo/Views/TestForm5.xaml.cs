using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Demo.Views
{
    public partial class TestForm5 : ContentPage
    {
        public TestForm5()
        {
            InitializeComponent();
            Debug.WriteLine("[Page] TestForm5 Constructor called.");

        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            Debug.WriteLine($"[Page] BindingContext changed: {BindingContext?.GetType().Name}");
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("[Page] TestForm5 OnAppearing triggered.");
        }
    }
}
