using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls;

// BorderEntryHandler.cs (shared)
public partial class UiEntryHandler : EntryHandler
{
    public static new IPropertyMapper<IEntry, UiEntryHandler> Mapper = new PropertyMapper<IEntry, UiEntryHandler>(EntryHandler.Mapper)
    {
        [nameof(UiEntry.BorderWidth)] = MapBorderWidth,
        [nameof(UiEntry.BorderColor)] = MapBorderColor,
        [nameof(UiEntry.TextColor)] = MapTextColor,
        [nameof(UiEntry.FocusedBorderColor)] = MapFocusedBorderColor,
        [nameof(UiEntry.Focusable)] = MapFocusable,
    };
    static partial void PlatformMapFocusable(UiEntryHandler handler, UiEntry? view);
    static void MapFocusable(UiEntryHandler handler, IEntry view) => PlatformMapFocusable(handler, view as UiEntry);

    public UiEntryHandler() : base(Mapper) { }

    static void MapBorderWidth(UiEntryHandler handler, IEntry? view) => PlatformMapBorderWidth(handler, view as UiEntry);
    static void MapBorderColor(UiEntryHandler handler, IEntry? view) => PlatformMapBorderColor(handler, view as UiEntry);
    static void MapTextColor(UiEntryHandler handler, IEntry? view) => PlatformMapTextColor(handler, view as UiEntry);
    static void MapFocusedBorderColor(UiEntryHandler handler, IEntry? view) => PlatformMapFocusedBorderColor(handler, view as UiEntry);

    static partial void PlatformMapBorderWidth(UiEntryHandler handler, UiEntry? view);
    static partial void PlatformMapBorderColor(UiEntryHandler handler, UiEntry? view);
    static partial void PlatformMapTextColor(UiEntryHandler handler, UiEntry? view);
    static partial void PlatformMapFocusedBorderColor(UiEntryHandler handler, UiEntry? view);

}
