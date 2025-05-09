// Platforms/iOS/UiPickerHandler.iOS.cs
using ca.whittaker.Maui.Controls;
using CoreAnimation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using UIKit;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiPickerHandler : PickerHandler
    {
        static partial void PlatformMapBorderWidth(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null)
                native.Layer.BorderWidth = (nfloat)view.BorderWidth;
        }
        static partial void PlatformMapFocusable(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null)
            {
                native.UserInteractionEnabled = view.Focusable;
            }
        }
        static partial void PlatformMapBorderColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null && view.BorderColor != null)
                native.Layer.BorderColor = view.BorderColor.ToCGColor();
        }

        static partial void PlatformMapTextColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null && view.TextColor != null)
                native.TextColor = view.TextColor.AsUIColor();
        }

        static partial void PlatformMapFocusedBorderColor(UiPickerHandler handler, UiPicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null)
            {
                native.EditingDidBegin -= OnBegin;
                native.EditingDidEnd -= OnEnd;

                void OnBegin(object? s, EventArgs e)
                {
                    if (view.FocusedBorderColor != null)
                        native.Layer.BorderColor = view.FocusedBorderColor.ToCGColor();
                }

                void OnEnd(object? s, EventArgs e)
                {
                    if (view.BorderColor != null)
                        native.Layer.BorderColor = view.BorderColor.ToCGColor();
                }

                native.EditingDidBegin += OnBegin;
                native.EditingDidEnd += OnEnd;
            }
        }
    }
}
