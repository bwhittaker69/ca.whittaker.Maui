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

    // Disables descendant controls: Entry, Picker, Editor, and ContentView.
    // Labels remain enabled if fieldLabelVisible is true.
    public static void DisableDescendantControls(View root, bool fieldLabelVisible)
    {
        foreach (var c in root.GetVisualTreeDescendants())
        {
            Debug.WriteLine($"{c.ToString()}");
            if (c is Entry e)
            {
                e.IsEnabled = true;
                e.IsReadOnly = true;
            }
            if (c is StackLayout sl)
                sl.IsEnabled = true;
            if (c is Picker p)
                p.IsEnabled = false;
            if (c is DatePicker dp)
                dp.IsEnabled = false;
            if (c is Editor ed)
            {
                ed.IsEnabled = true;
                ed.IsReadOnly = true;
            }
            if (c is ContentView cv)
                cv.IsEnabled = true;
            // labels are always enabled
            if (c is Label l && fieldLabelVisible)
                l.IsEnabled = true;
        }
    }

    // Enables descendant controls: Entry, Picker, Editor, and ContentView.
    // Labels remain enabled if fieldLabelVisible is true.
    public static void EnableDescendantControls(View root, bool fieldLabelVisible)
    {
        foreach (var c in root.GetVisualTreeDescendants())
        {
            Debug.WriteLine($"{c.ToString()}");
            if (c is Entry e)
            {
                e.IsEnabled = true;
                e.IsReadOnly = false;
            }
            if (c is StackLayout sl)
                sl.IsEnabled = true;
            if (c is DatePicker dp)
                dp.IsEnabled = true;
            if (c is Picker p)
                p.IsEnabled = true;
            if (c is Editor ed)
            {
                ed.IsEnabled = true;
                ed.IsReadOnly = false;
            }
            if (c is ContentView cv)
                cv.IsEnabled = true;
            if (c is Label l && fieldLabelVisible)
                l.IsEnabled = true;
        }
    }
}
