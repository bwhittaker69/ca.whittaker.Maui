// Platforms/iOS/UiDatePickerHandler.iOS.cs
using ca.whittaker.Maui.Controls;
using CoreAnimation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using UIKit;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiDatePickerHandler : DatePickerHandler
    {
        static partial void PlatformMapBorderWidth(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null)
                native.Layer.BorderWidth = (nfloat)view.BorderWidth;
        }
        static partial void PlatformMapFocusable(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null)
            {
                native.UserInteractionEnabled = view.Focusable;
            }
        }

        static partial void PlatformMapBorderColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null && view.BorderColor != null)
                native.Layer.BorderColor = view.BorderColor.ToCGColor();
        }

        static partial void PlatformMapTextColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null && view.TextColor != null)
                native.TextColor = view.TextColor.AsUIColor();
        }

        static partial void PlatformMapFocusedBorderColor(UiDatePickerHandler handler, UiDatePicker? view)
        {
            if (handler.PlatformView is UITextField native && view != null)
            {
                // Detach any prior handlers
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
