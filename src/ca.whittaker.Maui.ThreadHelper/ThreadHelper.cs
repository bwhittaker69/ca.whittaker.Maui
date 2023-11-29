using System;
using Microsoft.Maui.Dispatching;

namespace ca.whittaker.Maui.ThreadHelper;

public static class ThreadHelper
{
    public static void ExecuteOnMainThread(Action action)
    {
        if (MainThread.IsMainThread)
        {
            // If already on the main thread, execute the action directly
            action();
        }
        else
        {
            // If not on the main thread, schedule the action on the main thread
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}
