namespace ca.whittaker.Maui.Controls;
public static class UiThreadHelper
{
    // internal runner delegate, defaults to our real implementation
    private static Action<Action> _runner = DefaultRun;

    // public API now invokes the runner
    public static void RunOnMainThread(Action action) =>
        _runner(action);

    // for unit tests so they can override (or pass null to restore default behavior)
    public static void SetRunOnMainThreadHandler(Action<Action>? runner) =>
        _runner = runner ?? DefaultRun;

    // original platform‐specific code, pulled into a private helper
    private static void DefaultRun(Action action)
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