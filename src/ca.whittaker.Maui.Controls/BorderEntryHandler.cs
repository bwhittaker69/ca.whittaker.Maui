//using Microsoft.Maui.Handlers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ca.whittaker.Maui.Controls;

//// BorderEntryHandler.cs (shared)
//public partial class BorderEntryHandler : EntryHandler
//{
//    public static new IPropertyMapper<IEntry, EntryHandler> Mapper = new PropertyMapper<IEntry, EntryHandler>(EntryHandler.Mapper)
//    {
//        [nameof(BorderEntry.BorderWidth)] = MapBorderWidth,
//        [nameof(BorderEntry.BorderColor)] = MapBorderColor,
//        [nameof(BorderEntry.FocusedBorderColor)] = MapFocusedBorderColor,
//        // You can add more mappings here...
//    };

//    public BorderEntryHandler() : base(Mapper) { }

//    static void MapBorderWidth(EntryHandler handler, IEntry view) => PlatformMapBorderWidth(handler, view as BorderEntry);
//    static void MapBorderColor(EntryHandler handler, IEntry view) => PlatformMapBorderColor(handler, view as BorderEntry);
//    static void MapFocusedBorderColor(EntryHandler handler, IEntry view) => PlatformMapFocusedBorderColor(handler, view as BorderEntry);

//    static partial void PlatformMapBorderWidth(EntryHandler handler, BorderEntry view);
//    static partial void PlatformMapBorderColor(EntryHandler handler, BorderEntry view);
//    static partial void PlatformMapFocusedBorderColor(EntryHandler handler, BorderEntry view);

//}
