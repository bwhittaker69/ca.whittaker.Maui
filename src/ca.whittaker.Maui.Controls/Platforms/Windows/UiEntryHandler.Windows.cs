// Platforms/Windows/UiEntryHandler.Windows.cs
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Platform;
using Thickness = Microsoft.UI.Xaml.Thickness;
using SolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiEntryHandler : EntryHandler
    {
        static partial void PlatformMapBorderWidth(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                native.BorderThickness = new Thickness(view.BorderWidth);
            }
        }
        static partial void PlatformMapFocusable(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                native.IsTabStop = view.Focusable;
            }
        }
        static partial void PlatformMapBorderColor(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is TextBox native && view != null && view.BorderColor != null)
            {
                if (view.BorderColor != null)
                {
                    var brush = new SolidColorBrush(view.BorderColor.ToWindowsColor());
                    native.BorderBrush = brush;
                    native.Resources["TextControlBorderBrushPointerOver"] = brush;
                }
            }
        }

        static partial void PlatformMapTextColor(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                if (view.TextColor != null)
                    native.Foreground = new SolidColorBrush(view.TextColor.ToWindowsColor());
                else
                    native.ClearValue(Control.ForegroundProperty);
            }
        }

        static partial void PlatformMapFocusedBorderColor(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                if (view.FocusedBorderColor != null)
                {
                    var focusedBrush = new SolidColorBrush(view.FocusedBorderColor.ToWindowsColor());

                    // Detach any previous handlers
                    native.GotFocus -= OnGotFocus;
                    native.LostFocus -= OnLostFocus;

                    // Prepare focused brush and resources
                    native.Resources["TextControlFocusVisualPrimaryBrush"] = focusedBrush;
                    native.Resources["TextControlFocusVisualSecondaryBrush"] = focusedBrush;

                    // Local handlers capturing view
                    void OnGotFocus(object? s, RoutedEventArgs e) =>
                        native.BorderBrush = focusedBrush;

                    void OnLostFocus(object? s, RoutedEventArgs e) =>
                        native.BorderBrush = view.BorderColor != null
                            ? new SolidColorBrush(view.BorderColor.ToWindowsColor())
                            : null!;

                    // Attach handlers
                    native.GotFocus += OnGotFocus;
                    native.LostFocus += OnLostFocus;

                }
            }
        }
    }
}
