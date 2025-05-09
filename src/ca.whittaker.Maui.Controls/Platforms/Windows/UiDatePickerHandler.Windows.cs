// Platforms/Windows/UiDatePickerHandler.Windows.cs
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using Thickness = Microsoft.UI.Xaml.Thickness;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiDatePickerHandler : DatePickerHandler
    {
        static partial void PlatformMapBorderWidth(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is CalendarDatePicker native && view != null)
                native.BorderThickness = new Thickness(view.BorderWidth);
        }
        static partial void PlatformMapFocusable(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is CalendarDatePicker native && view != null)
            {
                native.IsTabStop = view.Focusable;
            }
        }
        static partial void PlatformMapBorderColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is CalendarDatePicker native && view != null && view.BorderColor != null)
                native.BorderBrush = new SolidColorBrush(view.BorderColor.ToWindowsColor());
        }

        static partial void PlatformMapTextColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is CalendarDatePicker native && view != null && view.TextColor != null)
                native.Foreground = new SolidColorBrush(view.TextColor.ToWindowsColor());
        }

        static partial void PlatformMapFocusedBorderColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is CalendarDatePicker native && view != null)
            {
                // Local handlers
                void OnGotFocus(object? s, RoutedEventArgs e)
                {
                    if (view.FocusedBorderColor != null)
                        native.BorderBrush = new SolidColorBrush(view.FocusedBorderColor.ToWindowsColor());
                }

                void OnLostFocus(object? s, RoutedEventArgs e)
                {
                    if (view.BorderColor != null)
                        native.BorderBrush = new SolidColorBrush(view.BorderColor.ToWindowsColor());
                }

                // Detach any previous handlers
                native.GotFocus -= OnGotFocus;
                native.LostFocus -= OnLostFocus;

                // Attach handlers
                native.GotFocus += OnGotFocus;
                native.LostFocus += OnLostFocus;
            }
        }
    }
}
