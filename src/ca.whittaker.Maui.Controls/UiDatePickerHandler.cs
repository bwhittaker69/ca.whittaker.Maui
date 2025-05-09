// BorderDatePickerHandler.cs (shared mapper)
using Microsoft.Maui.Handlers;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiDatePickerHandler : DatePickerHandler
    {
        public static new IPropertyMapper<IDatePicker, UiDatePickerHandler> Mapper = new PropertyMapper<IDatePicker, UiDatePickerHandler>(DatePickerHandler.Mapper)
            {
                [nameof(UiDatePicker.BorderWidth)] = MapBorderWidth,
                [nameof(UiDatePicker.BorderColor)] = MapBorderColor,
                [nameof(UiDatePicker.TextColor)] = MapTextColor,
            [nameof(UiDatePicker.FocusedBorderColor)] = MapFocusedBorderColor,
            [nameof(UiDatePicker.Focusable)] = MapFocusable,
        };
        static partial void PlatformMapFocusable(UiDatePickerHandler handler, UiDatePicker? view);
        static void MapFocusable(UiDatePickerHandler handler, IDatePicker view) => PlatformMapFocusable(handler, view as UiDatePicker);
        static void MapBorderWidth(UiDatePickerHandler handler, IDatePicker view) => PlatformMapBorderWidth(handler, view as UiDatePicker);
        static void MapBorderColor(UiDatePickerHandler handler, IDatePicker view) => PlatformMapBorderColor(handler, view as UiDatePicker);
        static void MapTextColor(UiDatePickerHandler handler, IDatePicker view) => PlatformMapTextColor(handler, view as UiDatePicker);
        static void MapFocusedBorderColor(UiDatePickerHandler handler, IDatePicker view) => PlatformMapFocusedBorderColor(handler, view as UiDatePicker);
        public UiDatePickerHandler() : base(Mapper) { }

        static partial void PlatformMapBorderWidth(UiDatePickerHandler handler, UiDatePicker? view);
        static partial void PlatformMapBorderColor(UiDatePickerHandler handler, UiDatePicker? view);
        static partial void PlatformMapTextColor(UiDatePickerHandler handler, UiDatePicker? view);
        static partial void PlatformMapFocusedBorderColor(UiDatePickerHandler handler, UiDatePicker? view);
    }
}