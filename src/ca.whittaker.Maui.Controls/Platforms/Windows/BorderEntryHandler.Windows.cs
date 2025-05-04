//using Microsoft.Maui.Handlers;
//using Microsoft.UI.Xaml;
//using Microsoft.UI.Xaml.Controls;
//using Microsoft.UI.Xaml.Media;
//using ca.whittaker.Maui.Controls;
//using Microsoft.Maui.Platform;
//using Thickness = Microsoft.UI.Xaml.Thickness;

//namespace ca.whittaker.Maui.Controls
//{
//    public partial class BorderEntryHandler : EntryHandler
//    {
//        static partial void PlatformMapBorderWidth(EntryHandler handler, BorderEntry view)
//        {
//            if (handler.PlatformView is TextBox native && view != null)
//                native.BorderThickness = new Thickness(view.BorderWidth);
//        }

//        static partial void PlatformMapBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            if (handler.PlatformView is TextBox native && view != null)
//            {
//                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(view.BorderColor.ToWindowsColor());
//                native.BorderBrush = brush;
//                native.Resources["TextControlBorderBrushPointerOver"] = brush;
//            }
//        }

//        static partial void PlatformMapFocusedBorderColor(EntryHandler handler, BorderEntry view)
//        {
//            if (handler.PlatformView is TextBox native && view != null)
//            {
//                var focusedBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(view.FocusedBorderColor.ToWindowsColor());
//                native.GotFocus += (s, e) => native.BorderBrush = focusedBrush;
//                native.LostFocus += (s, e) =>
//                    native.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(view.BorderColor.ToWindowsColor());

//                native.Resources["TextControlFocusVisualPrimaryBrush"] = focusedBrush;
//                native.Resources["TextControlFocusVisualSecondaryBrush"] = focusedBrush;
//            }
//        }
//    }
//}
