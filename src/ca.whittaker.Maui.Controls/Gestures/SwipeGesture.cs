namespace ca.whittaker.Maui.Controls.Gestures;
public class SwipeContainer : ContentView
{
    public event EventHandler<SwipedEventArgs>? SwipeUp;
    public event EventHandler<SwipedEventArgs>? SwipeDown;
    public event EventHandler<SwipedEventArgs>? SwipeLeft;
    public event EventHandler<SwipedEventArgs>? SwipeRight;

    public SwipeContainer()
    {
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Left));
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Right));
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Up));
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Down));
    }

    SwipeGestureRecognizer GetSwipeGestureRecognizer(SwipeDirection direction)
    {
        SwipeGestureRecognizer swipe = new SwipeGestureRecognizer { Direction = direction };
        switch (direction)
        {
            case SwipeDirection.Left: swipe.Swiped += (sender, e) => SwipeLeft?.Invoke(this, e); break;
            case SwipeDirection.Right: swipe.Swiped += (sender, e) => SwipeRight?.Invoke(this, e); break;
            case SwipeDirection.Up: swipe.Swiped += (sender, e) => SwipeUp?.Invoke(this, e); break;
            case SwipeDirection.Down: swipe.Swiped += (sender, e) => SwipeDown?.Invoke(this, e); break;
        }
        return swipe;
    }
}