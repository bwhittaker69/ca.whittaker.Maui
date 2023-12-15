namespace ca.whittaker.Maui.Controls.Gestures;
public class SwipLeftRightGesture : ContentView
{
    public event EventHandler<SwipedEventArgs> SwipeLeft;
    public event EventHandler<SwipedEventArgs> SwipeRight;

    public SwipLeftRightGesture()
    {
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Left));
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Right));
    }

    SwipeGestureRecognizer GetSwipeGestureRecognizer(SwipeDirection direction)
    {
        SwipeGestureRecognizer swipe = new SwipeGestureRecognizer { Direction = direction };
        switch (direction)
        {
            case SwipeDirection.Left: swipe.Swiped += (sender, e) => SwipeLeft?.Invoke(this, e); break;
            case SwipeDirection.Right: swipe.Swiped += (sender, e) => SwipeRight?.Invoke(this, e); break;
        }
        return swipe;
    }
}