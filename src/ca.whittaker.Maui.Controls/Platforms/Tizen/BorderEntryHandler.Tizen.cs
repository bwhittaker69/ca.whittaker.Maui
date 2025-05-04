//using ElmSharp;
//using ca.whittaker.Maui.Controls;
//using Microsoft.Maui.Handlers;
//using Microsoft.Maui.Platform;

//namespace ca.whittaker.Maui.Controls
//{
//    public partial class BorderEntryHandler : EntryHandler
//    {
//        static partial void PlatformMapBorderWidth(EntryHandler handler, BorderEntry view)
//        {
//            // ElmSharp.Entry has no border-thickness API; wrap in a Frame if needed.
//        }

//        static partial void PlatformMapBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            if (handler.PlatformView is Entry native && view != null)
//                native.BackgroundColor = view.BorderColor.ToPlatform();
//        }

//        static partial void PlatformMapFocusedBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            if (handler.PlatformView is Entry native && view != null)
//            {
//                native.Focused += (s, e) =>
//                    native.BackgroundColor = view.FocusedBorderColor.ToPlatform();
//                native.Unfocused += (s, e) =>
//                    native.BackgroundColor = view.BorderColor.ToPlatform();
//            }
//        }
//    }
//}
