// Platforms/Android/UiEntryHandler.Android.cs
using Android.Graphics.Drawables;
using Android.Widget;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiEntryHandler : EntryHandler
    {
        static partial void PlatformMapBorderWidth(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is EditText native && view != null && view.BorderColor != null)
            {
                var gd = native.Background as GradientDrawable ?? new GradientDrawable();
                gd.SetStroke((int)view.BorderWidth, view.BorderColor.ToPlatform());
                native.Background = gd;
            }
        }
        static partial void PlatformMapFocusable(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is MauiDatePicker native && view != null)
            {
                native.Focusable = view.Focusable;
                native.FocusableInTouchMode = view.Focusable;
            }
        }

        static partial void PlatformMapBorderColor(UiEntryHandler handler, UiEntry? view)
            => PlatformMapBorderWidth(handler, view);

        static partial void PlatformMapTextColor(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is EditText native && view != null && view.TextColor != null)
                native.SetTextColor(view.TextColor.ToPlatform());
        }

        static partial void PlatformMapFocusedBorderColor(UiEntryHandler handler, UiEntry? view)
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
