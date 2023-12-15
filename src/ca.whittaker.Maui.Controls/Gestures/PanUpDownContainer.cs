using System.ComponentModel.DataAnnotations;

namespace ca.whittaker.Maui.Controls.Gestures;
public class PanUpDownContainer : ContentView
{
    double panY;
    DateTime? startTime;

    public PanUpDownContainer()
    {
        // Set PanGestureRecognizer.TouchPoints to control the
        // number of touch points needed to pan
        PanGestureRecognizer panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);
    }

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // get start time
                startTime = DateTime.UtcNow;
                break;
            case GestureStatus.Running:
                // Translate and pan.
                double boundsY = Content.Height;
                Content.TranslationY = Math.Clamp(panY + e.TotalY, -boundsY, boundsY);
                break;

            case GestureStatus.Completed:
                if (startTime != null)
                {
                    int seconds = (DateTime.UtcNow - startTime.Value).Seconds;
                    startTime = null;

                    //if (seconds > 3)
                    //{
                    //    // return to home position
                    //    Content.TranslateTo(0, 0, 250, Easing.CubicInOut);
                    //}
                }
                // Store the translation applied during the pan
                panY = Content.TranslationY;
                break;
        }
    }
}