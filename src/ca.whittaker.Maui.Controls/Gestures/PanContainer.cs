using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ca.whittaker.Maui.Controls.Gestures;
public class PanContainer : ContentView
{
    double panX, panY;

    public PanContainer()
    {
        // Set PanGestureRecognizer.TouchPoints to control the
        // number of touch points needed to pan
        PanGestureRecognizer panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);
    }

    void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                // Translate and pan.
                double boundsX = Content.Width;
                double boundsY = Content.Height;
                Content.TranslationX = Math.Clamp(panX + e.TotalX, -boundsX, boundsX);
                Content.TranslationY = Math.Clamp(panY + e.TotalY, -boundsY, boundsY);
                break;

            case GestureStatus.Completed:
                // Store the translation applied during the pan
                panX = Content.TranslationX;
                panY = Content.TranslationY;
                break;
        }
    }
}