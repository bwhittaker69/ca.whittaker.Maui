using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ca.whittaker.Maui.Controls;

public static partial class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseBorderHandlers(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<UiEntry, UiEntryHandler>();
            handlers.AddHandler<UiEditor, UiEditorHandler>();
            handlers.AddHandler<UiPicker, UiPickerHandler>();
            handlers.AddHandler<UiDatePicker, UiDatePickerHandler>();
        });
        return builder;
    }
}
