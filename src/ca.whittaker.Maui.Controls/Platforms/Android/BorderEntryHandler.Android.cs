//using Android.Graphics.Drawables;
//using Android.Widget;                        // Add this
//using ca.whittaker.Maui.Controls;            // for BorderEntry
//using Microsoft.Maui.Handlers;
//using Microsoft.Maui.Platform;               // for ToPlatform()

//namespace ca.whittaker.Maui.Controls
//{
//    // Android-specific partial implementation of BorderEntryHandler
//    public partial class BorderEntryHandler : EntryHandler
//    {
//        // Called by the shared mapper
//        static partial void PlatformMapBorderWidth(EntryHandler handler, BorderEntry view)
//        {
//            if (view != null && handler != null)
//                if (handler.PlatformView is EditText native && view != null)
//                {
//                    var gd = native.Background as GradientDrawable ?? new GradientDrawable();
//                    gd.SetStroke((int)view.BorderWidth, view.BorderColor.ToPlatform());
//                    native.Background = gd;
//                }
//        }

//        static partial void PlatformMapBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            // reuse border width logic for color
//            if (view != null && handler != null)
//                PlatformMapBorderWidth(handler, view);
//        }

//        static partial void PlatformMapFocusedBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            if (view != null && handler != null)
//                if (handler.PlatformView is EditText native && view != null)
//                {
//                    native.FocusChange += (s, e) =>
//                    {
//                        var color = e.HasFocus ? view.FocusedBorderColor : view.BorderColor;
//                        var gd = native.Background as GradientDrawable;
//                        gd?.SetStroke((int)view.BorderWidth, color.ToPlatform());
//                    };
//                }
//        }
//    }
//}
