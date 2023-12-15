using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Gestures;
public class SwipingContainer : ContentView
{
    double panY;
    double startY; // Starting Y-coordinate of the gesture
    bool isSwipeGesture = false; // Flag to indicate if the gesture is a swipe
    Stopwatch stopwatch = new Stopwatch(); // Stopwatch to measure the duration of the gesture

    // Constants for customization
    private readonly double SwipeThreshold; // Swipe threshold (percentage of height)
    private readonly double SwipeVelocityThreshold; // Velocity threshold for swipe recognition
    private const int AnimationDuration = 250; // Animation duration in milliseconds
    private readonly PanGestureRecognizer panGesture;


    public event EventHandler<EventArgs> LoadNextElement;

    public SwipingContainer()
    {
        int screenDensity = GetScreenDensityForDevice();
        // Adjust thresholds based on screen density
        SwipeThreshold = 0.3; // Example adjustment
        SwipeVelocityThreshold = 200 * screenDensity; // Example adjustment

        panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);


    }

    async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        panGesture.PanUpdated -= OnPanUpdated;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                startY = Content.TranslationY;
                panY = Content.TranslationY;
                isSwipeGesture = false;
                stopwatch.Restart(); // Start the stopwatch
                break;
            case GestureStatus.Running:
                double translationY = panY + e.TotalY;
                //if (Math.Abs(translationY - startY) > Height * SwipeThreshold)
                //{
                isSwipeGesture = true;
                //}
                Content.TranslationY = Math.Clamp(translationY, -(Content.Height / 3), (Content.Height / 3));
                break;
            case GestureStatus.Completed:
                stopwatch.Stop(); // Stop the stopwatch
                double totalY = startY - Content.TranslationY;

                double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                double velocityY = Math.Abs((totalY - startY) / elapsedSeconds); // Calculate the velocity

                bool largeSwipe = ((Content.Height / 3) * SwipeThreshold < Math.Abs(totalY));
                bool fastSwipe = (velocityY > SwipeVelocityThreshold);

                if (isSwipeGesture && (largeSwipe || fastSwipe))
                {
                    //Initiate the animation to snap to the next element
                    var tcs = new TaskCompletionSource<bool>();
                    await Content.TranslateTo(0, -(Content.Height / 3), AnimationDuration, Easing.CubicInOut);
                    tcs.SetResult(true);

                    //Wait for the animation to complete

                    await tcs.Task;

                    //Now invoke the LoadNextElement event
                    LoadNextElement?.Invoke(this, new EventArgs());
                }
                else
                {
                    // Animate back to original position
                    await Content.TranslateTo(0, 0, AnimationDuration, Easing.CubicInOut);
                }
                break;
        }
        panGesture.PanUpdated += OnPanUpdated;
    }
    public static int GetScreenDensityForDevice()
    {
        double density = DeviceDisplay.MainDisplayInfo.Density;
        if (density <= 1) return 1; // mdpi
        if (density <= 1.5) return 2; // hdpi
        if (density <= 2) return 3; // xhdpi
        if (density <= 3) return 4; // xxhdpi
        return 5; // xxxhdpi or higher
    }
    public static double GetScreenHeightForDevice()
    {
        return DeviceDisplay.MainDisplayInfo.Height;
    }

}

