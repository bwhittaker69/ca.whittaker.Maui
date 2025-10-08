using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Behaviours;
public class CompactHeightBehavior : Behavior<View>
{
    public int Dip { get; set; } = 28;

    View? _target;

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _target = bindable;

        // React to any updates that might re-assert sizes
        bindable.PropertyChanged += OnPropChanged;
        bindable.SizeChanged += OnSizeChanged;

        Apply();
    }

    protected override void OnDetachingFrom(View bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.PropertyChanged -= OnPropChanged;
        bindable.SizeChanged -= OnSizeChanged;
        _target = null;
    }

    void OnPropChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // These are commonly touched by Button visual logic
        if (e.PropertyName is nameof(View.HeightRequest)
            or nameof(View.MinimumHeightRequest)
            or nameof(View.WidthRequest)
            or nameof(VisualElement.IsVisible))
        {
            Apply();
        }
    }

    void OnSizeChanged(object? sender, EventArgs e) => Apply();

    void Apply()
    {
        if (_target is null) return;
        // enforce our compact size (one-way)
        if (_target.HeightRequest != Dip) _target.HeightRequest = Dip;
        if (_target.MinimumHeightRequest != Dip) _target.MinimumHeightRequest = Dip;
    }
}