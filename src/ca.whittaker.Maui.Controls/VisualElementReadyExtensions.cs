using Microsoft.Maui;               // VisualElement
using Microsoft.Maui.Controls;      // Page, Button, etc.
#if IOS || MACCATALYST
using UIKit;                        // UIView, UIButton, etc.
#endif

namespace ca.whittaker.Maui.Controls;

public static class VisualElementReadyExtensions
{
    public static async Task WaitUntilReadyAsync(this VisualElement v)
    {
#if IOS || MACCATALYST
        while (v.Handler?.PlatformView is not UIView uiv ||
               uiv.Window is null ||
               v.Width <= 0 || v.Height <= 0)
        {
            await Task.Delay(16);
        }
#else
    // On other platforms, just wait for width/height > 0
    while (v.Width <= 0 || v.Height <= 0)
    {
        await Task.Delay(16);
    }
#endif
    }
}
