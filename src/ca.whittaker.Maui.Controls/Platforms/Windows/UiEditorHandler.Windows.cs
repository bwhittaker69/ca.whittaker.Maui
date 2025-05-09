// Platforms/Windows/UiEditorHandler.Windows.cs
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
    public partial class UiEditorHandler : EditorHandler
    {
        static partial void PlatformMapBorderWidth(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                native.BorderThickness = new Thickness(view.BorderWidth);
            }
        }
        static partial void PlatformMapFocusable(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                native.IsTabStop = view.Focusable;
            }
        }

        static partial void PlatformMapBorderColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is TextBox native && view != null && view.BorderColor != null)
            {
                var brush = new SolidColorBrush(view.BorderColor.ToWindowsColor());
                native.BorderBrush = brush;
                native.Resources["TextControlBorderBrushPointerOver"] = brush;
            }
        }

        static partial void PlatformMapTextColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                if (view.TextColor != null)
                    native.Foreground = new SolidColorBrush(view.TextColor.ToWindowsColor());
                else
                    native.ClearValue(Control.ForegroundProperty);
            }
        }

        static partial void PlatformMapFocusedBorderColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is TextBox native && view != null)
            {
                if (view.FocusedBorderColor != null)
                {
                    var focusedBrush = new SolidColorBrush(view.FocusedBorderColor.ToWindowsColor());

                    // Detach previous handlers
                    native.GotFocus -= OnGotFocus;
                    native.LostFocus -= OnLostFocus;

                    // Prepare brushes
                    native.Resources["TextControlFocusVisualPrimaryBrush"] = focusedBrush;
                    native.Resources["TextControlFocusVisualSecondaryBrush"] = focusedBrush;

                    // Local handlers
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
