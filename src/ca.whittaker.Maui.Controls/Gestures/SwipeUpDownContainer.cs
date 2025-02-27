using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Gestures;

public class SwipeUpDownContainer : ContentView
{
    private double _startY;
    private double _panY;
    private bool _isSwipeGesture;
    private readonly Stopwatch _stopwatch = new Stopwatch();

    private const double SwipeThreshold = 0.1;
    private const int AnimationDuration = 250;
    private const int BaseVelocityThreshold = 200;

    private readonly PanGestureRecognizer? _panGesture;
    public event EventHandler<EventArgs>? SwipedNextElement;
    public event EventHandler<EventArgs>? SwipedPreviousElement;

    private int _currentIndex = 0;

    public bool CanSwipeUp
    {
        get => (bool)GetValue(CanSwipeUpProperty);
        set => SetValue(CanSwipeUpProperty, value);
    }
    public static readonly BindableProperty CanSwipeUpProperty =
        BindableProperty.Create(nameof(CanSwipeUp), typeof(bool), typeof(SwipeUpDownContainer), false, BindingMode.OneWay);

    public bool CanSwipeDown
    {
        get => (bool)GetValue(CanSwipeDownProperty);
        set => SetValue(CanSwipeDownProperty, value);
    }
    public static readonly BindableProperty CanSwipeDownProperty =
        BindableProperty.Create(nameof(CanSwipeDown), typeof(bool), typeof(SwipeUpDownContainer), false, BindingMode.OneWay);

    public bool IsSwipingUp
    {
        get => (bool)GetValue(IsSwipingUpProperty);
        set => SetValue(IsSwipingUpProperty, value);
    }
    public static readonly BindableProperty IsSwipingUpProperty =
        BindableProperty.Create(nameof(IsSwipingUp), typeof(bool), typeof(SwipeUpDownContainer), false);

    public bool IsSwipingDown
    {
        get => (bool)GetValue(IsSwipingDownProperty);
        set => SetValue(IsSwipingDownProperty, value);
    }
    public static readonly BindableProperty IsSwipingDownProperty =
        BindableProperty.Create(nameof(IsSwipingDown), typeof(bool), typeof(SwipeUpDownContainer), false);

    public SwipeUpDownContainer()
    {
        _panGesture = new PanGestureRecognizer();
        _panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(_panGesture);
    }

    // Resets the state for a new swipe gesture.
    public void Goto(int elementNo)
    {
        double elementHeight = GetHeightPerElement();
        if (elementHeight == 0) return;

        _startY = _panY = elementHeight;

        Content.TranslationX = 0;
        Content.TranslationY = (elementNo - 1) * _startY;

        _isSwipeGesture = false;
        IsSwipingUp = false;
        IsSwipingDown = false;

    }


    // Handles the updates of the pan gesture.
    private async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
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
        _startY = _panY = Content?.TranslationY ?? 0;
        _isSwipeGesture = false;
        _stopwatch.Restart();
    }

    // Handles the ongoing swipe gesture.
    private void HandleGestureRunning(PanUpdatedEventArgs e)
    {
        double translationY = _panY + e.TotalY;
        _isSwipeGesture = Math.Abs(translationY - _startY) > GetHeightPerElement() * SwipeThreshold;
        Content.TranslationY = translationY; //  Math.Clamp(translationY, -GetHeightPerElement(), GetHeightPerElement());

        UpdateIsSwiping(Content.TranslationY);

        if ((IsSwipingUp && !CanSwipeUp) || (IsSwipingDown && !CanSwipeDown))
        {
            Content.TranslationY = _startY; // Reset the translation if swiping is not allowed
        }
    }

    // Completes the gesture and triggers the swipe action if conditions are met.
    private async Task HandleGestureCompleted()
    {
        if (Content == null) return;

        _stopwatch.Stop();
        bool isLargeSwipe = IsLargeSwipe();
        bool isFastSwipe = IsFastSwipe();

        if (_isSwipeGesture && (isLargeSwipe || isFastSwipe))
        {
            if (Content.TranslationY < _startY)
            {
                if (CanSwipeUp && _currentIndex < (Content as VerticalStackLayout)?.Children.Count - 1)
                {
                    _currentIndex++;
                    await Content.TranslateTo(0, -_currentIndex * GetHeightPerElement(), AnimationDuration, Easing.CubicInOut);
                    SwipedNextElement?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    await Content.TranslateTo(0, -_currentIndex * GetHeightPerElement(), AnimationDuration, Easing.CubicInOut);
                }
            }
            else
            {
                if (CanSwipeDown && _currentIndex > 0)
                {
                    _currentIndex--;
                    await Content.TranslateTo(0, -_currentIndex * GetHeightPerElement(), AnimationDuration, Easing.CubicInOut);
                    SwipedPreviousElement?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    await Content.TranslateTo(0, -_currentIndex * GetHeightPerElement(), AnimationDuration, Easing.CubicInOut);
                }
            }
        }
        else
        {
            await Content.TranslateTo(0, -_currentIndex * GetHeightPerElement(), AnimationDuration, Easing.CubicInOut);
        }

        IsSwipingUp = false;
        IsSwipingDown = false;
    }

    // Updates the IsSwipingUp and IsSwipingDown properties based on current translation.
    private void UpdateIsSwiping(double translationY)
    {
        IsSwipingUp = translationY < _startY;
        IsSwipingDown = translationY > _startY;
    }

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
    private double GetHeightPerElement()
    {
        if (Content is VerticalStackLayout layout && layout.Children.Count > 0)
        {
            return Content.Height / layout.Children.Count;
        }
        return 0;
    }

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
