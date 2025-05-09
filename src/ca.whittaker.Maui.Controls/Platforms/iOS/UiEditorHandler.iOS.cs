// Platforms/iOS/UiEditorHandler.iOS.cs
using ca.whittaker.Maui.Controls;
using CoreAnimation;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using UIKit;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiEditorHandler : EditorHandler
    {
        static partial void PlatformMapBorderWidth(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is UITextView native && view != null)
                native.Layer.BorderWidth = (nfloat)view.BorderWidth;
        }

        static partial void PlatformMapFocusable(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is UITextView native && view != null)
            {
                native.UserInteractionEnabled = view.Focusable;
            }
        }

        static partial void PlatformMapBorderColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is UITextView native && view != null && view.BorderColor != null)
                native.Layer.BorderColor = view.BorderColor.ToCGColor();
        }

        static partial void PlatformMapTextColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is UITextView native && view != null && view.TextColor != null)
                native.TextColor = view.TextColor.AsUIColor();
        }

        static partial void PlatformMapFocusedBorderColor(UiEditorHandler handler, UiEditor? view)
        {
            if (handler.PlatformView is UITextView native && view != null)
            {
                // Remove old observers
                NSNotificationCenter.DefaultCenter.RemoveObserver(native, UITextView.TextDidBeginEditingNotification, native);
                NSNotificationCenter.DefaultCenter.RemoveObserver(native, UITextView.TextDidEndEditingNotification, native);

                // Add new ones
                NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidBeginEditingNotification, _ =>
                {
                    if (view.FocusedBorderColor != null)
                        native.Layer.BorderColor = view.FocusedBorderColor.ToCGColor();
                }, native);

                NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidEndEditingNotification, _ =>
                {
                    if (view.BorderColor != null)
                        native.Layer.BorderColor = view.BorderColor.ToCGColor();
                }, native);
            }
        }
    }
}
