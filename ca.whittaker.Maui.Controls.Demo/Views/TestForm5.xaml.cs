using System.Diagnostics;
using System.Globalization;

namespace ca.whittaker.Maui.Controls.Demo.Views
{
    //[XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class TestForm5 : ContentPage
    {
        public TestForm5()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.ToString()}");
            }
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

    /// <summary>
    /// Converts between an Enum value and a Boolean (true if equals ConverterParameter).
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            if (targetType == null) return null;
            if (value == null || parameter == null)
                return false;

            string enumValue = value?.ToString() ?? String.Empty;
            string targetValue = parameter.ToString() ?? String.Empty;
            return enumValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            if (targetType == null) return null;
            if (!(value is bool boolValue) || parameter == null)
                return BindableProperty.UnsetValue;

            return boolValue
                ? Enum.Parse(targetType, parameter.ToString() ?? String.Empty, true)
                : BindableProperty.UnsetValue;
        }
    }

}
