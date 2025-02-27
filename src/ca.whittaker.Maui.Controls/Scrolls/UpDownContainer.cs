using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using System.Windows.Input;

namespace ca.whittaker.Maui.Controls.Scrolls;
public class UpDownContainer : ContentView
{
    public static readonly BindableProperty PanYProperty = BindableProperty.Create(
                    propertyName: nameof(PanY),
                    returnType: typeof(double),
                    declaringType: typeof(UpDownContainer),
                    defaultValue: null,
                    defaultBindingMode: BindingMode.TwoWay);

    public double PanY
    {
        get => (double)GetValue(PanYProperty);
        set => SetValue(PanYProperty, value);
    }

    public static readonly BindableProperty AllowedSwipeUpProperty = BindableProperty.Create(
           propertyName: nameof(AllowedSwipeUp),
           returnType: typeof(bool),
           declaringType: typeof(UpDownContainer),
           defaultValue: null,
           defaultBindingMode: BindingMode.OneWay);

    public bool AllowedSwipeUp
    {
        get => (bool)GetValue(AllowedSwipeUpProperty);
        set => SetValue(AllowedSwipeUpProperty, value);
    }

    public static readonly BindableProperty AllowedSwipeDownProperty = BindableProperty.Create(
           propertyName: nameof(AllowedSwipeDown),
           returnType: typeof(bool),
           declaringType: typeof(UpDownContainer),
           defaultValue: null,
           defaultBindingMode: BindingMode.OneWay);

    public bool AllowedSwipeDown
    {
        get => (bool)GetValue(AllowedSwipeDownProperty);
        set => SetValue(AllowedSwipeDownProperty, value);
    }


    public UpDownContainer()
    {
        // Set PanGestureRecognizer.TouchPoints to control the
        // number of touch points needed to pan
        PanGestureRecognizer panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);

        GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Up));
        //GestureRecognizers.Add(GetSwipeGestureRecognizer(SwipeDirection.Down));

    }

    void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                // Translate and pan.
                double boundsX = Content.Width;
                double boundsY = Content.Height;
                Content.TranslationY = Math.Clamp(PanY + e.TotalY, -boundsY, boundsY);
                break;

            case GestureStatus.Completed:
                // Store the translation applied during the pan
                PanY = Content.TranslationY;
                break;
        }
    }

    SwipeGestureRecognizer GetSwipeGestureRecognizer(SwipeDirection direction)
    {
        SwipeGestureRecognizer swipe = new SwipeGestureRecognizer { Direction = direction };
        switch (direction)
        {
            case SwipeDirection.Up: SwipeUp(); break;
            //case SwipeDirection.Down: swipe.Swiped += (sender, e) => SwipeDown?.Invoke(this, e); break;
        }
        return swipe;
    }
    public event EventHandler<EventArgs>? LoadNext;

    private void SwipeUp()
    {
        Content.TranslationY = Content.Height;
        LoadNext?.Invoke(this, new EventArgs());
    }
}