using System;
using System.Linq;
using Microsoft.Maui.Devices;

namespace ca.whittaker.Maui.Controls;

public static class DeviceHelper
{
    private static readonly ButtonSizeEnum[] _buttonSizes = Enum
        .GetValues<ButtonSizeEnum>()
        .OrderBy(s => (int)s)
        .ToArray();

    /// <summary>
    /// Returns a DIP size suitable for MAUI layout (platform independent visual size).
    /// </summary>
    public static int GetImageSizeForDevice(SizeEnum scaleFactor)
        => GetDipSize(GetButtonSizeForDevice(scaleFactor));

    /// <summary>
    /// Selects the closest <see cref="ButtonSizeEnum"/> for the current device metrics
    /// and the requested <paramref name="scaleFactor"/>.
    /// </summary>
    public static ButtonSizeEnum GetButtonSizeForDevice(SizeEnum scaleFactor)
    {
        double density = GetDisplayDensity();
        var displayInfo = GetDisplayInfo();
        double minDip = CalculateShortestDimensionInDip(displayInfo, density);

        double targetDip = DetermineBaseDip(density, DeviceInfo.Idiom, minDip);
        int baseIndex = FindClosestButtonSizeIndex(targetDip);

        int offsetSteps = (int)scaleFactor / 5; // SizeEnum increments in steps of 5
        int targetIndex = ClampIndex(baseIndex + offsetSteps);

        return _buttonSizes[targetIndex];
    }

    /// <summary>
    /// Returns the resource bucket (in raw pixels) closest to the required asset size for the current device.
    /// </summary>
    public static int GetImageAssetBucket(SizeEnum scaleFactor)
    {
        var buttonSize = GetButtonSizeForDevice(scaleFactor);
        double density = GetDisplayDensity();
        double desiredPixels = GetDipSize(buttonSize) * density;

        int bucketIndex = FindClosestButtonSizeIndex(desiredPixels);
        return GetDipSize(_buttonSizes[bucketIndex]);
    }

    /// <summary>
    /// Returns a DIP size suitable for MAUI layout (platform independent visual size).
    /// </summary>
    public static int GetDipSize(ButtonSizeEnum size) => (int)size;

    /// <summary>
    /// Returns physical pixel size for raster assets if/when you need raw pixels.
    /// </summary>
    public static int GetPixelSize(ButtonSizeEnum size)
    {
        var dip = GetDipSize(size);
        var density = GetDisplayDensity();
        return (int)Math.Round(dip * density);
    }

    private static DisplayInfo GetDisplayInfo()
    {
        try
        {
            return DeviceDisplay.MainDisplayInfo;
        }
        catch
        {
            return default;
        }
    }

    private static double GetDisplayDensity()
    {
        double density;
        try
        {
            density = DeviceDisplay.MainDisplayInfo.Density;
        }
        catch
        {
            density = 1.0;
        }

        if (double.IsNaN(density) || density <= 0)
            density = 1.0;

        return density;
    }

    private static double CalculateShortestDimensionInDip(DisplayInfo info, double density)
    {
        if (info.Width <= 0 || info.Height <= 0 || density <= 0)
            return 0;

        double minPixels = Math.Min(info.Width, info.Height);
        return minPixels / density;
    }

    private static double DetermineBaseDip(double density, DeviceIdiom idiom, double minDip)
    {
        double baseDip;

        if (idiom == DeviceIdiom.Desktop)
            baseDip = 64;
        else if (idiom == DeviceIdiom.TV)
            baseDip = 72;
        else if (idiom == DeviceIdiom.Tablet)
            baseDip = 56;
        else if (idiom == DeviceIdiom.Watch)
            baseDip = 36;
        else
            baseDip = 48;

        if (density <= 1.1)
            baseDip = Math.Max(baseDip - 4, 32);
        else if (density >= 3.5)
            baseDip = Math.Min(baseDip + 12, 112);
        else if (density >= 3.0)
            baseDip = Math.Min(baseDip + 8, 104);
        else if (density >= 2.5)
            baseDip = Math.Min(baseDip + 6, 96);
        else if (density >= 2.0)
            baseDip = Math.Min(baseDip + 4, 88);

        if (minDip >= 900)
            baseDip = Math.Min(baseDip + 8, 120);
        else if (minDip > 700)
            baseDip = Math.Min(baseDip + 4, 116);
        else if (minDip > 0 && minDip <= 480)
            baseDip = Math.Max(baseDip - 4, 28);

        return Math.Clamp(baseDip, 12, 128);
    }

    private static int FindClosestButtonSizeIndex(double targetDip)
    {
        int closestIndex = 0;
        double smallestDiff = double.MaxValue;

        for (int i = 0; i < _buttonSizes.Length; i++)
        {
            double diff = Math.Abs(GetDipSize(_buttonSizes[i]) - targetDip);
            if (diff < smallestDiff)
            {
                smallestDiff = diff;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private static int ClampIndex(int index)
        => Math.Min(Math.Max(index, 0), _buttonSizes.Length - 1);
}
