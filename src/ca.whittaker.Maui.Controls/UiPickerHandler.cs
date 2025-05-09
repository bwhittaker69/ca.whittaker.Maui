// BorderPickerHandler.cs (shared mapper)
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiPickerHandler : PickerHandler
    {
        public static new IPropertyMapper<IPicker, UiPickerHandler> Mapper = new PropertyMapper<IPicker, UiPickerHandler>(PickerHandler.Mapper)
        {
            [nameof(UiPicker.BorderWidth)] = MapBorderWidth,
            [nameof(UiPicker.BorderColor)] = MapBorderColor,
            [nameof(UiPicker.TextColor)] = MapTextColor,
            [nameof(UiPicker.FocusedBorderColor)] = MapFocusedBorderColor,
            [nameof(UiPicker.Focusable)] = MapFocusable,
        };
        static partial void PlatformMapFocusable(UiPickerHandler handler, UiPicker? view);
        static void MapFocusable(UiPickerHandler handler, IPicker view) => PlatformMapFocusable(handler, view as UiPicker);
        public UiPickerHandler() : base(Mapper) { }

        static void MapBorderWidth(UiPickerHandler handler, IPicker view) => PlatformMapBorderWidth(handler, view as UiPicker);
        static void MapBorderColor(UiPickerHandler handler, IPicker view) => PlatformMapBorderColor(handler, view as UiPicker);
        static void MapTextColor(UiPickerHandler handler, IPicker view) => PlatformMapTextColor(handler, view as UiPicker);
        static void MapFocusedBorderColor(UiPickerHandler handler, IPicker view) => PlatformMapFocusedBorderColor(handler, view as UiPicker);

        static partial void PlatformMapBorderWidth(UiPickerHandler handler, UiPicker? view);
        static partial void PlatformMapBorderColor(UiPickerHandler handler, UiPicker? view);
        static partial void PlatformMapTextColor(UiPickerHandler handler, UiPicker? view);
        static partial void PlatformMapFocusedBorderColor(UiPickerHandler handler, UiPicker? view);
    }
}