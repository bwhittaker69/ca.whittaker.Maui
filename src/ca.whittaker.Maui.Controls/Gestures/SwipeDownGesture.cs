namespace ca.whittaker.Maui.Controls.Gestures;
public class SwipeDownContainer : ContentView
{
    public event EventHandler<SwipedEventArgs> SwipeUp;

    public SwipeDownContainer()
    {
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Up));
    }

    SwipeGestureRecognizer GetSwipeGestureRecognizer(SwipeDirection direction)
    {
        SwipeGestureRecognizer swipe = new SwipeGestureRecognizer { Direction = direction };
        switch (direction)
        {
            case SwipeDirection.Up: swipe.Swiped += (sender, e) => SwipeUp?.Invoke(this, e); break;
        }
        return swipe;
    }
}