﻿using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ca.whittaker.Maui.Controls.Gestures;
public class SwipeUpGesture : ContentView
{
    public event EventHandler<SwipedEventArgs>? SwipeUp;

    public SwipeUpGesture()
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