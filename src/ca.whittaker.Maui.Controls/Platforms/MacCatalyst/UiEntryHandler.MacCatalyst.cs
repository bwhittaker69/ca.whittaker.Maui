// Platforms/MacCatalyst/BorderEntryHandler.MacCatalyst.cs
using CoreAnimation;
using UIKit;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiEntryHandler : EntryHandler
    {
        static partial void PlatformMapBorderWidth(UiEntryHandler handler, UiEntry? view)
        {
            if (view != null && handler != null)
                if (handler.PlatformView is UITextField native && view != null)
                    native.Layer.BorderWidth = (nfloat)view.BorderWidth;
        }
        static partial void PlatformMapFocusable(UiEntryHandler handler, UiEntry? view)
        {
            if (handler.PlatformView is UITextField native && view != null)
            {
                native.UserInteractionEnabled = view.Focusable;
            }
        }
        static partial void PlatformMapBorderColor(UiEntryHandler handler, UiEntry? view)
        {
            if (view != null && handler != null)
                if (handler.PlatformView is UITextField native && view != null)
                    native.Layer.BorderColor = view.BorderColor.ToCGColor();
        }
        static partial void PlatformMapTextColor(UiEntryHandler handler, UiEntry? view)
        {
        }
        static partial void PlatformMapFocusedBorderColor(UiEntryHandler handler, UiEntry? view)
        {
            if (view != null && handler != null)
                if (handler.PlatformView is UITextField native && view != null)
                {
                    native.EditingDidBegin += (s, e) =>
                        native.Layer.BorderColor = view.FocusedBorderColor.ToCGColor();
                    native.EditingDidEnd += (s, e) =>
                        native.Layer.BorderColor = view.BorderColor.ToCGColor();
                }
        }
    }
}
