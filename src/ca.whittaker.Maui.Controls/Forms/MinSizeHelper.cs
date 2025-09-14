using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Forms;

internal static class MinSizeHelper
{
    public static void ClearMinimumsRecursively(View root)
    {
        Walk(root, v => ClearPlatformMinimums(v));
    }

    private static void Walk(View v, Action<View> action)
    {
        action(v);

        // Layout.Children is IList<IView> – cast to Controls.View before recursing
        if (v is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is View childView)
                    Walk(childView, action);
            }
            return;
        }

        // Handle single-content hosts; content might be IView – cast to View before recursing
        if (v is ContentView cv && cv.Content is View cvChild)
            Walk(cvChild, action);

        if (v is Border border && border.Content is View borderChild)
            Walk(borderChild, action);

        if (v is ScrollView sv && sv.Content is View svChild)
            Walk(svChild, action);
    }

    private static void ClearPlatformMinimums(View v)
    {
        // MAUI mins
        v.MinimumHeightRequest = 0;
        v.MinimumWidthRequest = 0;

        ApplyPlatform(v);

        // Re-apply when handler attaches
        v.HandlerChanged -= OnHandlerChanged;
        v.HandlerChanged += OnHandlerChanged;

        static void OnHandlerChanged(object? sender, EventArgs e)
        {
            if (sender is View view) ApplyPlatform(view);
        }

        static void ApplyPlatform(View view)
        {
#if WINDOWS
            if (view.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement fe)
            {
                fe.MinHeight = 0;
                fe.MinWidth = 0;
            }
#endif
#if ANDROID
            if (view.Handler?.PlatformView is Android.Views.View av)
            {
                av.SetMinimumHeight(0);
                av.SetMinimumWidth(0);
            }
#endif
        }
    }
}
