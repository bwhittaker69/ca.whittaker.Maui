namespace ca.whittaker.Maui.Controls;
public static class UiThreadHelper
{
    public static void RunOnMainThread(Action action)
    {
#if !IOS && !ANDROID && !MACCATALYST && !WINDOWS
        action();
#else
        if (MainThread.IsMainThread)
            action();
        else
            MainThread.BeginInvokeOnMainThread(action);
#endif
    }
}