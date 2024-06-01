using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Gestures;

public class SwipeUpContainer : ContentView
{
    private double _startY;
    private double _panY;
    private bool _isSwipeGesture;
    private readonly Stopwatch _stopwatch = new Stopwatch();

    private const double SwipeThreshold = 0.1;
    private const int AnimationDuration = 250;
    private const int BaseVelocityThreshold = 200;

    private readonly PanGestureRecognizer _panGesture;
    public event EventHandler<EventArgs> LoadNextElement;

    public bool AllowedSwipeUp { get => (bool)GetValue(AllowedSwipeUpProperty); set => SetValue(AllowedSwipeUpProperty, value); }
    public static readonly BindableProperty AllowedSwipeUpProperty =
        BindableProperty.Create(nameof(AllowedSwipeUp), typeof(bool), typeof(SwipeUpContainer), false);

    public bool IsSwipingUp { get => (bool)GetValue(IsSwipingUpProperty); set => SetValue(IsSwipingUpProperty, value); }
    public static readonly BindableProperty IsSwipingUpProperty =
        BindableProperty.Create(nameof(IsSwipingUp), typeof(bool), typeof(SwipeUpContainer), false);

    public SwipeUpContainer()
    {
        _panGesture = new PanGestureRecognizer();
        _panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(_panGesture);
    }

    // Resets the state for a new swipe gesture.
    public async Task Goto(int elementNo)
    {
        _startY = _panY = GetHeightPerElement();

        Content.TranslationX = 0;
        Content.TranslationY = (elementNo - 1) * _startY;

        _isSwipeGesture = false;
    }

    // Handles the updates of the pan gesture.
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

    // Initializes the gesture start parameters.
    private void HandleGestureStart(PanUpdatedEventArgs e)
    {
        _startY = _panY = Content.TranslationY;
        _isSwipeGesture = false;
        _stopwatch.Restart();
        UpdateIsSwipingUp();
    }

    // Handles the ongoing swipe gesture.
    private void HandleGestureRunning(PanUpdatedEventArgs e)
    {
        if (e.TotalY > 0 || !AllowedSwipeUp) return;

        double translationY = _panY + e.TotalY;
        _isSwipeGesture = Math.Abs(translationY - _startY) > GetHeightPerElement() * SwipeThreshold;
        Content.TranslationY = Math.Clamp(translationY, -GetHeightPerElement(), GetHeightPerElement());

        UpdateIsSwipingUp();
    }

    // Completes the gesture and triggers the swipe action if conditions are met.
    private async Task HandleGestureCompleted()
    {
        if (AllowedSwipeUp)
        {
            _stopwatch.Stop();
            bool isLargeSwipe = IsLargeSwipe();
            bool isFastSwipe = IsFastSwipe();

            if (_isSwipeGesture && (isLargeSwipe || isFastSwipe))
            {
                await Content.TranslateTo(0, -GetHeightPerElement(), AnimationDuration, Easing.CubicInOut);
                LoadNextElement?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await Content.TranslateTo(0, 0, AnimationDuration, Easing.CubicInOut);
            }
        }
        IsSwipingUp = false;
    }

    // Updates the IsSwipingUp property based on current translation.
    private void UpdateIsSwipingUp() => IsSwipingUp = Content.TranslationY != _startY;

    // Determines if the swipe is large enough based on threshold.
    private bool IsLargeSwipe() => GetHeightPerElement() * SwipeThreshold < Math.Abs(_startY - Content.TranslationY);

    // Determines if the swipe is fast enough based on velocity.
    private bool IsFastSwipe()
    {
        double totalY = _startY - Content.TranslationY;
        double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        double velocityY = Math.Abs(totalY / elapsedSeconds);
        return velocityY > CalculateSwipeVelocityThreshold();
    }

    // Gets the height per element in the queue.
    private double GetHeightPerElement() => Content.Height / ((VerticalStackLayout)Content).Count;

    // Calculates the velocity threshold for swipe recognition.
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
        return BaseVelocityThreshold * screenDensity;
    }
}
