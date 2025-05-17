using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Forms;

public static class ControlVisualHelper
{
    /// <summary>
    /// Copies the Enabled (Normal) state setters to the Disabled state for any MAUI control.
    /// </summary>
    public static void MatchDisabledToEnabled(VisualElement root)
    {
        foreach (View v in root.GetVisualTreeDescendants())
        {
            foreach (VisualElement c in v.GetVisualTreeDescendants())
            {
                var groups = VisualStateManager.GetVisualStateGroups(c);
                var common = groups?.FirstOrDefault(g => g.Name == "CommonStates");
                if (common == null)
                    return;

                var normal = common.States
                    .OfType<VisualState>()
                    .FirstOrDefault(s => s.Name == "Normal");
                var disabled = common.States
                    .OfType<VisualState>()
                    .FirstOrDefault(s => s.Name == "Disabled");

                if (normal == null || disabled == null)
                    return;

                // Clear any existing Disabled setters, then copy from Normal
                disabled.Setters.Clear();
                foreach (var setter in normal.Setters)
                    disabled.Setters.Add(new Setter
                    {
                        Property = setter.Property,
                        Value = setter.Value
                    });
            }
        }
    }

    /// <summary>
    /// Returns the Color defined for TextColor in the Normal visual state of the given element.
    /// </summary>
    public static Color? GetNormalTextColor(View element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        var groups = VisualStateManager.GetVisualStateGroups(element);
        var common = groups?.FirstOrDefault(g => g.Name == "CommonStates");
        if (common == null)
            return null;

        var normal = common.States.OfType<VisualState>().FirstOrDefault(s => s.Name == "Normal");
        if (normal == null)
            return null;

        // look for a TextColor setter
        var textSetter = normal.Setters
            .OfType<Setter>()
            .FirstOrDefault(s => s.Property?.PropertyName == "TextColor");
        if (textSetter?.Value is Color c)
            return c;

        // fallback: check for ForegroundColor
        var fgSetter = normal.Setters
            .OfType<Setter>()
            .FirstOrDefault(s => s.Property?.PropertyName == "ForegroundColor");
        if (fgSetter?.Value is Color fc)
            return fc;

        return null;
    }

    // Hide descendant controls: Entry, Picker, Editor, and ContentView.
    // everything is hidden
    public static void DisableAllDescendantControls<T>(this BaseFormField<T> root)
    {
        foreach (var c in root.GetVisualTreeDescendants())
        {
            if (c is UiEntry e)
                e.IsEnabled = false;
            if (c is StackLayout sl)
                sl.IsEnabled = false;
            if (c is UiPicker p)
                p.IsEnabled = false;
            if (c is DatePicker dp)
                dp.IsEnabled = false;
            if (c is Editor ed)
                ed.IsEnabled = false;
            if (c is ContentView cv)
                cv.IsEnabled = false;
            // labels are always enabled
            if (c is Label l)
                l.IsEnabled = true;
        }
    }
    public static void EnableAllDescendantControls<T>(this BaseFormField<T> root)
    {
        foreach (var c in root.GetVisualTreeDescendants())
        {
            if (c is UiEntry e)
                e.IsEnabled = true;
            if (c is StackLayout sl)
                sl.IsEnabled = true;
            if (c is UiPicker p)
                p.IsEnabled = true;
            if (c is DatePicker dp)
                dp.IsEnabled = true;
            if (c is Editor ed)
                ed.IsEnabled = true;
            if (c is ContentView cv)
                cv.IsEnabled = true;
            // labels are always enabled
            if (c is Label l)
                l.IsEnabled = true;
        }
    }


    // Hide descendant controls: Entry, Picker, Editor, and ContentView.
    // everything is hidden
    public static void HideAllDescendantControls<T>(this BaseFormField<T> root)
    {
        foreach (var c in root.GetVisualTreeDescendants())
        {
            if (c is UiEntry e)
                e.IsVisible = false;
            if (c is StackLayout sl)
                sl.IsVisible = false;
            if (c is UiPicker p)
                p.IsVisible = false;
            if (c is DatePicker dp)
                dp.IsVisible = false;
            if (c is Editor ed)
                ed.IsVisible = false;
            if (c is ContentView cv)
                cv.IsVisible = false;
            // labels are always enabled
            if (c is Label l)
                l.IsVisible = true;
        }
    }

    // Hide descendant controls: Entry, Picker, Editor, and ContentView.
    // everything is hidden
    public static void UnfocusDescendantControls<T>(this BaseFormField<T> root)
    {
        root.Unfocus();
        foreach (IVisualTreeElement c in root.GetVisualTreeDescendants())
        {
            if (c is Grid g)
                g.Unfocus();
            if (c is UiEntry e)
                e.Unfocus();
            if (c is StackLayout sl)
                sl.Unfocus();
            if (c is UiPicker p)
                p.Unfocus();
            if (c is UiDatePicker dp)
                dp.Unfocus();
            if (c is CheckBox cb)
                cb.Unfocus();
            if (c is UiEditor ed)
                ed.Unfocus();
            if (c is ContentView cv)
                cv.Unfocus();
            // labels are always enabled
            if (c is Label l)
                l.Unfocus();
        }
    }

}
