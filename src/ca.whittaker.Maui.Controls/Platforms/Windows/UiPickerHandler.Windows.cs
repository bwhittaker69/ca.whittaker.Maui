// Platforms/Windows/UiPickerHandler.Windows.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Thickness = Microsoft.UI.Xaml.Thickness;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiPickerHandler : PickerHandler
    {
        static partial void PlatformMapBorderWidth(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is ComboBox native && view != null)
            {
                native.BorderThickness = new Thickness(view.BorderWidth);
            }
        }
        static partial void PlatformMapFocusable(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is ComboBox native && view != null)
            {
                native.IsTabStop = view.Focusable;
            }
        }

        static partial void PlatformMapBorderColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is ComboBox native && view?.BorderColor != null)
            {
                native.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(view.BorderColor.ToWindowsColor());
            }
        }

        static partial void PlatformMapTextColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is ComboBox native && view != null)
            {
                // Apply immediately
                if (view.TextColor != null)
                    native.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(view.TextColor.ToWindowsColor());
                else
                    native.ClearValue(Control.ForegroundProperty);
            }
        }

        static partial void PlatformMapFocusedBorderColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is ComboBox native && view != null)
            {
                // Detach any previous handlers
                native.GotFocus -= OnGotFocus;
                native.LostFocus -= OnLostFocus;

                // Local handlers capture the view reference
                void OnGotFocus(object? sender, RoutedEventArgs e)
                {
                    if (view.FocusedBorderColor != null)
                        native.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(view.FocusedBorderColor.ToWindowsColor());
                }

                void OnLostFocus(object? sender, RoutedEventArgs e)
                {
                    if (view.BorderColor != null)
                        native.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(view.BorderColor.ToWindowsColor());
                }

                native.GotFocus += OnGotFocus;
                native.LostFocus += OnLostFocus;
            }
        }
    }
}
