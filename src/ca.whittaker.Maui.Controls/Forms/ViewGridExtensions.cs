using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Forms;

public static class ViewGridExtensions
{
    private static bool IsInGrid(this View? control)
    {
        if (control == null)
            return false;

        Element? parent = control.Parent;
        while (parent != null)
        {
            if (parent is Grid)
                return true;

            parent = (parent as VisualElement)?.Parent;
        }

        return false;
    }

    private static (int row, int col, int colSpan) GetPositionInGrid(this View? control)
    {
        if (control == null)
            return (-1, -1, 1);

        Element? parent = control.Parent;
        while (parent != null)
        {
            if (parent is Grid grid)
            {
                int row = grid.GetRow(control);
                int col = grid.GetColumn(control);
                int span = grid.GetColumnSpan(control);
                return (row, col, span);
            }

            parent = (parent as VisualElement)?.Parent;
        }

        return (-1, -1, 1);
    }

    public static void BringToFront(this View? control)
    {
        if (control == null || !control.IsInGrid())
            return;

        var (row, col, _) = control.GetPositionInGrid();
        if (row < 0 || col < 0)
            return;

        var grid = control.GetParentGrid();
        if (grid == null)
            return;

        // Filter to View FIRST so we can use Controls Grid attached properties
        var siblings = grid.Children
                           .OfType<View>()
                           .Where(v =>
                               Microsoft.Maui.Controls.Grid.GetRow(v) == row &&
                               Microsoft.Maui.Controls.Grid.GetColumn(v) == col)
                           .ToList();

        if (siblings.Count == 0)
            return;

        // Normalize ZIndex so the smallest becomes 0
        int minIndex = siblings.Min(v => v.ZIndex);
        foreach (var v in siblings)
            v.ZIndex -= minIndex;

        // Put target on top (one higher than current max among siblings)
        int maxZ = siblings.Where(v => v != control)
                           .DefaultIfEmpty(control)
                           .Max(v => v.ZIndex);

        control.ZIndex = maxZ + 1;
    }

    public static void MoveToBack(this View? control)
    {
        if (control == null || !control.IsInGrid())
            return;

        var (row, col, _) = control.GetPositionInGrid();
        if (row < 0 || col < 0)
            return;

        var grid = control.GetParentGrid();
        if (grid == null)
            return;

        var siblings = grid.Children
                           .OfType<View>()
                           .Where(v =>
                               Microsoft.Maui.Controls.Grid.GetRow(v) == row &&
                               Microsoft.Maui.Controls.Grid.GetColumn(v) == col)
                           .ToList();

        if (siblings.Count == 0)
            return;

        // Normalize to start at 0
        int minZ = siblings.Min(v => v.ZIndex);
        foreach (var v in siblings)
            v.ZIndex -= minZ;

        // Everything except target gets bumped up starting at 1
        int z = 1;
        foreach (var v in siblings.OrderBy(v => v == control ? int.MinValue : v.ZIndex))
            if (v != control)
                v.ZIndex = z++;

        // Target goes to the back
        control.ZIndex = 0;
    }

    private static Grid? GetParentGrid(this View? control)
    {
        if (control == null)
            return null;

        Element? parent = control.Parent;
        while (parent != null)
        {
            if (parent is Grid grid)
                return grid;

            parent = (parent as VisualElement)?.Parent;
        }

        return null;
    }
}
