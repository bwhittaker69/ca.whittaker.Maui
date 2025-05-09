// BorderDatePickerHandler.cs (shared mapper)
using Microsoft.Maui.Handlers;

namespace ca.whittaker.Maui.Controls
{
    public partial class UiEditorHandler : EditorHandler
    {
        public static new IPropertyMapper<IEditor, UiEditorHandler> Mapper = new PropertyMapper<IEditor, UiEditorHandler>(EditorHandler.Mapper)
            {
                [nameof(UiEditor.BorderWidth)] = MapBorderWidth,
                [nameof(UiEditor.Focusable)] = MapFocusable,
                [nameof(UiEditor.BorderColor)] = MapBorderColor,
                [nameof(UiEditor.TextColor)] = MapTextColor,
                [nameof(UiEditor.FocusedBorderColor)] = MapFocusedBorderColor,
            };

        public UiEditorHandler() : base(Mapper) { }

        static void MapFocusable(UiEditorHandler handler, IEditor view) => PlatformMapFocusable(handler, view as UiEditor);
        static void MapBorderWidth(UiEditorHandler handler, IEditor view) => PlatformMapBorderWidth(handler, view as UiEditor);
        static void MapBorderColor(UiEditorHandler handler, IEditor view) => PlatformMapBorderColor(handler, view as UiEditor);
        static void MapTextColor(UiEditorHandler handler, IEditor view) => PlatformMapTextColor(handler, view as UiEditor);
        static void MapFocusedBorderColor(UiEditorHandler handler, IEditor view) => PlatformMapFocusedBorderColor(handler, view as UiEditor);

        static partial void PlatformMapFocusable(UiEditorHandler handler, UiEditor? view);
        static partial void PlatformMapBorderWidth(UiEditorHandler handler, UiEditor? view);
        static partial void PlatformMapBorderColor(UiEditorHandler handler, UiEditor? view);
        static partial void PlatformMapTextColor(UiEditorHandler handler, UiEditor? view);
        static partial void PlatformMapFocusedBorderColor(UiEditorHandler handler, UiEditor? view);
    }
}