using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ca.whittaker.Maui.Controls.Gestures;
public class TapContainer : ContentView
{

    public TapContainer()
    {

        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += TapGesture_Tapped;
        GestureRecognizers.Add(tapGesture);
    }
    public event EventHandler<TappedEventArgs> OnTapped;

    private void Tapped(TappedEventArgs e)
    {
        OnTapped?.Invoke(this, e);
    }

    private void TapGesture_Tapped(object? sender, TappedEventArgs e)
    {
        Tapped(e);
    }

}