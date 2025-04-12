using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Linq;

namespace ca.whittaker.Maui.Controls.Forms
{
    public static class ControlVisualHelper
    {
        // Disables descendant controls: Entry, Picker, Editor, and ContentView.
        // Labels remain enabled if fieldLabelVisible is true.
        public static void DisableDescendantControls(View root, bool fieldLabelVisible)
        {
            foreach (var c in root.GetVisualTreeDescendants())
            {
//                Debug.WriteLine($"{c.ToString()}");
                if (c is Entry e)
                    e.IsEnabled = false;
                if (c is StackLayout sl)
                    sl.IsEnabled = false;
                if (c is Picker p)
                    p.IsEnabled = false;
                if (c is DatePicker dp)
                    dp.IsEnabled = false;
                if (c is Editor ed)
                    ed.IsEnabled = false;
                if (c is ContentView cv)
                    cv.IsEnabled = false;
                // If Label is encountered and fieldLabelVisible is true, keep it enabled.
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
                if (c is Entry e)
                    e.IsEnabled = true;
                if (c is StackLayout sl)
                    sl.IsEnabled = true;
                if (c is DatePicker dp)
                    dp.IsEnabled = true;
                if (c is Picker p)
                    p.IsEnabled = true;
                if (c is Editor ed)
                    ed.IsEnabled = true;
                if (c is ContentView cv)
                    cv.IsEnabled = true;
                // If Label is encountered and fieldLabelVisible is true, keep it enabled.
                if (c is Label l && fieldLabelVisible)
                    l.IsEnabled = true;
            }
        }
    }
}
