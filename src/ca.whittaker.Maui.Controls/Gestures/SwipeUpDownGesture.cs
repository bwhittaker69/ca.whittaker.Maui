using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ca.whittaker.Maui.Controls.Gestures;
public class SwipeUpDownGesture : ContentView
{
    public event EventHandler<SwipedEventArgs> SwipeUp;
    public event EventHandler<SwipedEventArgs> SwipeDown;

    public SwipeUpDownGesture()
    {
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Up));
        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Down));
    }

    SwipeGestureRecognizer GetSwipeGestureRecognizer(SwipeDirection direction)
    {
        SwipeGestureRecognizer swipe = new SwipeGestureRecognizer { Direction = direction };
        switch (direction)
        {
            case SwipeDirection.Up: swipe.Swiped += (sender, e) => SwipeUp?.Invoke(this, e); break;
            case SwipeDirection.Down: swipe.Swiped += (sender, e) => SwipeDown?.Invoke(this, e); break;
        }
        return swipe;
    }
}