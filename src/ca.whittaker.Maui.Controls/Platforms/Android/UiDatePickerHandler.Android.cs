// Platforms/Android/UiDatePickerHandler.Android.cs
using Android.Graphics.Drawables;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiDatePickerHandler : DatePickerHandler
    {
        static partial void PlatformMapBorderWidth(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is MauiDatePicker native && view != null)
            {
                var gd = native.Background as GradientDrawable ?? new GradientDrawable();
                gd.SetStroke((int)view.BorderWidth, view.BorderColor.ToPlatform());
                native.Background = gd;
            }
        }

        static partial void PlatformMapBorderColor(UiDatePickerHandler handler, UiDatePicker? view)
            => PlatformMapBorderWidth(handler, view);

        static partial void PlatformMapTextColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is MauiDatePicker native && view != null && view.TextColor != null)
                native.SetTextColor(view.TextColor.ToPlatform());
        }
        static partial void PlatformMapFocusable(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is MauiDatePicker native && view != null)
            {
                native.Focusable = view.Focusable;
                native.FocusableInTouchMode = view.Focusable;
            }
        }
        static partial void PlatformMapFocusedBorderColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is MauiDatePicker native && view != null)
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
