using Microsoft.Maui.ApplicationModel;
using System;

namespace ca.whittaker.Maui.Controls.Forms;

public static class ThreadHelper
{
    public static void InvokeOnMainThread(Action action)
    {
        if (MainThread.IsMainThread)
            action();
        else
            MainThread.BeginInvokeOnMainThread(action);
    }
}
