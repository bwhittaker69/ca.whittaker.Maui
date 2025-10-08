using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Behaviours;

public class IconSizeBehavior : Behavior<Button>
{
    public static readonly BindableProperty DipProperty =
        BindableProperty.Create(nameof(Dip), typeof(int), typeof(IconSizeBehavior), 16);

    public int Dip
    {
        get => (int)GetValue(DipProperty);
        set => SetValue(DipProperty, value);
    }

    private Button? _button;

    protected override void OnAttachedTo(Button bindable)
    {
        base.OnAttachedTo(bindable);
        _button = bindable;

        // Re-run when template/handler attaches
        bindable.HandlerChanged += Button_HandlerChanged;
        bindable.Loaded += Button_Loaded;
    }

    protected override void OnDetachingFrom(Button bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.HandlerChanged -= Button_HandlerChanged;
        bindable.Loaded -= Button_Loaded;
        _button = null;
    }

    private void Button_Loaded(object? sender, EventArgs e) => TryResize();
    private void Button_HandlerChanged(object? sender, EventArgs e) => TryResize();

    private void TryResize()
    {
        if (_button == null) return;

        // Find the first Image in the visual tree (works for most templates)
        var image = _button.GetVisualTreeDescendants().OfType<Image>().FirstOrDefault();
        if (image == null) return;

        image.WidthRequest = Dip;
        image.HeightRequest = Dip;
        image.Aspect = Aspect.AspectFit;
        image.Margin = new Thickness(0); // kill excessive padding, if any
    }
}