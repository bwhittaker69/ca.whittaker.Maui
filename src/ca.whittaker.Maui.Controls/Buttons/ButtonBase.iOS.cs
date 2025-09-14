#if IOS
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace ca.whittaker.Maui.Controls.Buttons;

public abstract partial class ButtonBase
{
    void ApplyiOSButtonConfig()
    {
        if (Handler?.PlatformView is UIButton btn)
        {
            var cfg = UIButtonConfiguration.FilledButtonConfiguration;
            cfg.BaseBackgroundColor = (BackgroundColor ?? Colors.Transparent).ToPlatform();
            cfg.Title = Text ?? string.Empty;
            cfg.CornerStyle = UIButtonConfigurationCornerStyle.Medium;

            btn.Configuration = cfg;
        }
    }
}
#endif
