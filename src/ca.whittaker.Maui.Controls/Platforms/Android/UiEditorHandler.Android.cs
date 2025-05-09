// Platforms/Android/UiEditorHandler.Android.cs
using Android.Graphics.Drawables;
using Android.Widget;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiEditorHandler : EditorHandler
    {
        static partial void PlatformMapBorderWidth(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is EditText native && view != null && view.BorderColor != null)
            {
                var gd = native.Background as GradientDrawable ?? new GradientDrawable();
                gd.SetStroke((int)view.BorderWidth, view.BorderColor.ToPlatform());
                native.Background = gd;
            }
        }
        static partial void PlatformMapFocusable(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is EditText native && view != null)
            {
                native.Focusable = view.Focusable;
                native.FocusableInTouchMode = view.Focusable;
            }
        }

        static partial void PlatformMapBorderColor(UiEditorHandler handler, UiEditor? view)
        {
            // reuse border‐width logic to apply both width & color
            PlatformMapBorderWidth(handler, view);
        }

        static partial void PlatformMapTextColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is EditText native && view != null && view.TextColor != null)
                native.SetTextColor(view.TextColor.ToPlatform());
        }

        static partial void PlatformMapFocusedBorderColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is EditText native && view != null)
            {
                native.FocusChange += (s, e) =>
                {
                    var color = e.HasFocus
                        ? view.FocusedBorderColor.ToPlatform()
                        : view.BorderColor.ToPlatform();
                    var gd = native.Background as GradientDrawable;
                    gd?.SetStroke((int)view.BorderWidth, color);
                };
            }
        }
    }
}
