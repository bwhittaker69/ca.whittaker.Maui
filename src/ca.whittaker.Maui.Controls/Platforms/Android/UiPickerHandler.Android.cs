// Platforms/Android/UiPickerHandler.Android.cs
using Android.Graphics.Drawables;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiPickerHandler : PickerHandler
    {
        static partial void PlatformMapBorderWidth(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is MauiPicker native && view != null)
            {
                var gd = native.Background as GradientDrawable ?? new GradientDrawable();
                gd.SetStroke((int)view.BorderWidth, view.BorderColor.ToPlatform());
                native.Background = gd;
            }
        }
        static partial void PlatformMapFocusable(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is MauiPicker native && view != null)
            {
                native.Focusable = view.Focusable;
                native.FocusableInTouchMode = view.Focusable;
            }
        }

        static partial void PlatformMapBorderColor(UiPickerHandler handler, UiPicker? view)
            => PlatformMapBorderWidth(handler, view);

        static partial void PlatformMapTextColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is MauiPicker native && view != null && view.TextColor != null)
                native.SetTextColor(view.TextColor.ToPlatform());
        }

        static partial void PlatformMapFocusedBorderColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is MauiPicker native && view != null)
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
