using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Gestures;

public class SwipeUpContainer
    : ContentView
{
    private double _startY; // Starting Y-coordinate of the gesture
    private double _panY;
    private bool _isSwipeGesture; // Flag to indicate if the gesture is a swipe
    private readonly Stopwatch _stopwatch = new Stopwatch(); // Stopwatch to measure the duration of the gesture

    // Constants for customization
    private const double SwipeThreshold = 0.1; // Swipe threshold (percentage of height)
    private double _swipeVelocityThreshold; // Velocity threshold for swipe recognition
    private const int AnimationDuration = 250; // Animation duration in milliseconds
    private const int baseVelocityThreshold = 200; // Base velocity threshold for swipe recognition

    private readonly PanGestureRecognizer _panGesture;

    public event EventHandler<EventArgs> LoadNextElement;
    public bool AllowedSwipeUp { get => (bool)GetValue(AllowedSwipeUpProperty); set => SetValue(AllowedSwipeUpProperty, value); }

    public static readonly BindableProperty AllowedSwipeUpProperty =
        BindableProperty.Create(propertyName: nameof(AllowedSwipeUp), returnType: typeof(bool), declaringType: typeof(SwipeUpContainer), defaultValue: false);
    public bool IsSwipingUp { get => (bool)GetValue(IsSwipingUpProperty); set => SetValue(IsSwipingUpProperty, value); }

    public static readonly BindableProperty IsSwipingUpProperty =
        BindableProperty.Create(propertyName: nameof(IsSwipingUp), returnType: typeof(bool), declaringType: typeof(SwipeUpContainer), defaultValue: false);


    public SwipeUpContainer()
    {
        _swipeVelocityThreshold = CalculateSwipeVelocityThreshold();
        _panGesture = new PanGestureRecognizer();
        _panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(_panGesture);
    }
    private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                HandleGestureStart(e);
                break;
            case GestureStatus.Running:
                HandleGestureRunning(e);
                break;
            case GestureStatus.Completed:
                await HandleGestureCompleted();
                break;
        }
    }

    private void HandleGestureStart(PanUpdatedEventArgs e)
    {
        _startY = _panY = Content.TranslationY;
        _isSwipeGesture = false;
        _stopwatch.Restart();
        IsSwipingUp = (Content.TranslationY != _startY);
    }

    private void HandleGestureRunning(PanUpdatedEventArgs e)
    {
        IsSwipingUp = (Content.TranslationY != _startY);
        if (e.TotalY > 0) return; // Only allow swiping up (not down    
        if (AllowedSwipeUp)
        {
            double translationY = _panY + e.TotalY;
            _isSwipeGesture = Math.Abs(translationY - _startY) > (Content.Height / 3) * SwipeThreshold;
            Content.TranslationY = Math.Clamp(translationY, -(Content.Height / 3), (Content.Height / 3));
        }
    }
    private async Task HandleGestureCompleted()
    {
        if (AllowedSwipeUp)
        {
            _stopwatch.Stop();
            double totalY = _startY - Content.TranslationY;
            double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            double velocityY = Math.Abs(totalY / elapsedSeconds);

            bool largeSwipe = (Content.Height / 3) * SwipeThreshold < Math.Abs(totalY);
            bool fastSwipe = velocityY > _swipeVelocityThreshold;

            if (_isSwipeGesture && (largeSwipe || fastSwipe))
            {
                await Content.TranslateTo(0, -(Content.Height / 3), AnimationDuration, Easing.CubicInOut);
                LoadNextElement?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await Content.TranslateTo(0, 0, AnimationDuration, Easing.CubicInOut);
            }
        }
        IsSwipingUp = false;
    }

    private static double CalculateSwipeVelocityThreshold()
    {
        double density = DeviceDisplay.MainDisplayInfo.Density;
        int screenDensity = density switch
        {
            <= 1 => 1,
            <= 1.5 => 2,
            <= 2 => 3,
            <= 3 => 4,
            _ => 5
        };
        return baseVelocityThreshold * screenDensity;
    }
}
