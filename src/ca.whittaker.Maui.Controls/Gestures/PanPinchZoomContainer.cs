namespace ca.whittaker.Maui.Controls.Gestures;
public class PanPinchZoomContainer : ContentView
{
    double currentScale = 1;
    double startScale = 1;
    double xOffset = 0;
    double yOffset = 0;

    public PanPinchZoomContainer()
    {
        Initialize();

        PinchGestureRecognizer pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += OnPinchUpdated;
        GestureRecognizers.Add(pinchGesture);

        PanGestureRecognizer panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);

    }

    public void Initialize()
    {
        currentScale = 1;
        startScale = 1;
        xOffset = 0;
        yOffset = 0;
    }

    public void Reset()
    {
        currentScale = 1;
        startScale = 1;
        xOffset = 0;
        yOffset = 0;
        Content.Scale = 1;
        Content.TranslationX = 0;
        Content.TranslationY = 0;
    }


    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                {
                    if (currentScale > 1)
                    {
                        // Calculate new translation values but ensure they do not go beyond 0.
                        var newTranslationX = Math.Max(0, xOffset + e.TotalX);
                        var newTranslationY = Math.Max(0, yOffset + e.TotalY);

                        // If scaled, adjust translation to prevent moving beyond top-left corner.
                        if (currentScale > 1)
                        {
                            newTranslationX = Math.Min(newTranslationX, Content.Width * (currentScale - 1));
                            newTranslationY = Math.Min(newTranslationY, Content.Height * (currentScale - 1));
                        }

                        Content.TranslationX = newTranslationX;
                        Content.TranslationY = newTranslationY;
                    }
                    break;
                }
            case GestureStatus.Completed:
                {
                    // Store the translation applied during the pan
                    xOffset = Content.TranslationX;
                    yOffset = Content.TranslationY;
                    break;

                }
        }
    }

    void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            // Store the current scale factor applied to the wrapped user interface element,
            // and zero the components for the center point of the translate transform.
            startScale = Content.Scale;
            Content.AnchorX = 0;
            Content.AnchorY = 0;
        }
        if (e.Status == GestureStatus.Running)
        {
            // Calculate the scale factor to be applied.
            currentScale += (e.Scale - 1) * startScale;
            currentScale = Math.Max(1, currentScale);

            // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
            // so get the X and Y pixel coordinates.
            double renderedX = Content.X + xOffset;
            double deltaX = renderedX / Width;
            double deltaWidth = Width / (Content.Width * startScale);
            double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

            double renderedY = Content.Y + yOffset;
            double deltaY = renderedY / Height;
            double deltaHeight = Height / (Content.Height * startScale);
            double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

            // Calculate the transformed element pixel coordinates.
            double targetX = xOffset - (originX * Content.Width) * (currentScale - startScale);
            double targetY = yOffset - (originY * Content.Height) * (currentScale - startScale);

            // Apply translation based on the change in origin.
            Content.TranslationX = Math.Clamp(targetX, -Content.Width * (currentScale - 1), 0);
            Content.TranslationY = Math.Clamp(targetY, -Content.Height * (currentScale - 1), 0);

            // Apply scale factor
            Content.Scale = currentScale;
        }
        if (e.Status == GestureStatus.Completed)
        {
            // Store the translation delta's of the wrapped user interface element.
            xOffset = Content.TranslationX;
            yOffset = Content.TranslationY;
        }
    }
}
