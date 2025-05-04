//using CoreAnimation;
//using UIKit;
//using ca.whittaker.Maui.Controls;
//using Microsoft.Maui.Handlers;
//using Microsoft.Maui.Platform;

//namespace ca.whittaker.Maui.Controls
//{
//    public partial class BorderEntryHandler : EntryHandler
//    {
//        static partial void PlatformMapBorderWidth(EntryHandler handler, BorderEntry view)
//        {
//            if (view != null && handler != null)
//                if (handler.PlatformView is UITextField native && view != null)
//                    native.Layer.BorderWidth = (nfloat)view.BorderWidth;
//        }

//        static partial void PlatformMapBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            if (view != null && handler != null)
//                if (handler.PlatformView is UITextField native && view != null)
//                    native.Layer.BorderColor = view.BorderColor.ToCGColor();
//        }

//        static partial void PlatformMapFocusedBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            if (view != null && handler != null)
//                if (handler.PlatformView is UITextField native && view != null)
//                {
//                    native.EditingDidBegin += (s, e) =>
//                        native.Layer.BorderColor = view.FocusedBorderColor.ToCGColor();
//                    native.EditingDidEnd += (s, e) =>
//                        native.Layer.BorderColor = view.BorderColor.ToCGColor();
//                }
//        }
//    }
//}
