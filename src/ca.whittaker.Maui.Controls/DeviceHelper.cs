using System;
using System.Linq;
using Microsoft.Maui.Devices;

namespace ca.whittaker.Maui.Controls;

public static class DeviceHelper
{
    public static int GetImageSizeForDevice(SizeEnum size, bool enforceMinTouchTarget = false)
    {
        var isDesktop = DeviceInfo.Platform == DevicePlatform.WinUI && DeviceInfo.Idiom == DeviceIdiom.Desktop;

        if (isDesktop && !enforceMinTouchTarget)
        {
            return size switch
            {
                SizeEnum.XXSmall => 28,
                SizeEnum.XSmall => 32,
                SizeEnum.Small => 36,
                SizeEnum.Normal => 40,
                SizeEnum.Large => 48,
                _ => 40
            };
        }

        // touch-friendly map
        return size switch
        {
            SizeEnum.XXSmall => 40,
            SizeEnum.XSmall => 40,
            SizeEnum.Small => 40,
            SizeEnum.Normal => 40,
            SizeEnum.Large => 48,
            _ => 40
        };
    }

    public static ButtonSizeEnum GetButtonSizeForDevice(SizeEnum size, bool enforceMinTouchTarget = false)
    {
        // Keep your existing enum mapping; the dip calculation above is what drives height/icon.
        // If you map by dip somewhere else, make sure it uses GetImageSizeForDevice(...).
        return (ButtonSizeEnum)size;
    }


    public static ButtonSizeEnum GetButtonSizeEnumForDevice(SizeEnum scaleFactor, bool enforceMinTouchTarget = true)
        => GetSizeForDevice<ButtonSizeEnum>(scaleFactor, enforceMinTouchTarget);


    public enum UiScaleProfile { Compact, Comfortable, Touch }

    // Raster buckets you ship
    public enum ImageSizes
    {
        Size12 = 12, Size16 = 16, Size18 = 18, Size24 = 24, Size29 = 29,
        Size32 = 32, Size36 = 36, Size40 = 40, Size48 = 48, Size56 = 56,
        Size58 = 58, Size64 = 64, Size72 = 72, Size76 = 76, Size80 = 80,
        Size87 = 87, Size96 = 96, Size108 = 108, Size120 = 120, Size128 = 128
    }

    // Optional global overrides
    public static UiScaleProfile? GlobalScaleProfileOverride { get; set; }

    public static (int Dip, int BucketPx) GetLayoutAndBucket(SizeEnum sizeEnum, bool enforceMinTouchTarget = true)
    {
        var dip = GetImageSizeForDevice(sizeEnum, enforceMinTouchTarget);
        var bucketEnum = GetImageAssetBucket(sizeEnum);
        var bucketPx = Convert.ToInt32(bucketEnum);
        return (dip, bucketPx);
    }

    public static (int Dip, int BucketPx) GetIconForText(
        SizeEnum sizeEnum, double fontSize, double lineHeightFactor = 1.2, double maxRatio = 0.92, bool enforceMinTouchTarget = true)
    {
        var dipTarget = GetImageSizeForDevice(sizeEnum, enforceMinTouchTarget);
        var textHeightDip = Math.Max(1, fontSize * lineHeightFactor);
        var clampedDip = (int)Math.Floor(Math.Min(dipTarget, textHeightDip * maxRatio));

        var density = NormalizeDensity(DeviceDisplay.MainDisplayInfo.Density);
        var desiredPx = (int)Math.Round(clampedDip * density);
        var bucketEnum = GetClosestEnumValue(
            Enum.GetValues<ImageSizes>().OrderBy(v => (int)v).ToArray(),
            desiredPx);
        var bucketPx = (int)(object)bucketEnum;

        return (Math.Clamp(clampedDip, 12, 128), bucketPx);
    }

    // ===== Platform-aware profile defaults =================================

    private static UiScaleProfile GetPlatformDefaultProfile(DevicePlatform platform, DeviceIdiom idiom)
    {
        // Desktop Windows tends to favor denser UI by default
        if (platform == DevicePlatform.WinUI && idiom == DeviceIdiom.Desktop)
            return UiScaleProfile.Compact;

        // iOS leans “comfy” for readability
        if (platform == DevicePlatform.iOS || platform == DevicePlatform.MacCatalyst)
            return UiScaleProfile.Comfortable;

        // Android phones/tablets typically like larger touch targets
        if (platform == DevicePlatform.Android &&
            (idiom == DeviceIdiom.Phone || idiom == DeviceIdiom.Tablet))
            return UiScaleProfile.Touch;

        // TVs are pure touch/remote targets
        if (idiom == DeviceIdiom.TV)
            return UiScaleProfile.Touch;

        // Watches are dense but tiny
        if (idiom == DeviceIdiom.Watch)
            return UiScaleProfile.Compact;

        // Fallback
        return UiScaleProfile.Comfortable;
    }

    // ===== Public API =======================================================

    public static ImageSizes GetImageAssetBucket(SizeEnum scaleFactor, bool enforceMinTouchTarget = true)
    {
        var chosenButton = GetButtonSizeForDevice(scaleFactor, enforceMinTouchTarget);
        int desiredPixels = GetPixelSize(chosenButton);
        return GetClosestEnumValue(_imageSizes, desiredPixels);
    }

    public static int GetDipSize<TEnum>(TEnum size) where TEnum : struct, Enum
        => Convert.ToInt32(size);

    public static int GetPixelSize<TEnum>(TEnum size) where TEnum : struct, Enum
    {
        var dip = GetDipSize(size);
        var density = NormalizeDensity(DeviceDisplay.MainDisplayInfo.Density);
        return (int)Math.Round(dip * density);
    }

    public static TEnum GetSizeForDevice<TEnum>(SizeEnum scaleFactor, bool enforceMinTouchTarget = true)
        where TEnum : struct, Enum
    {
        var sizes = GetSortedEnumValues<TEnum>(); // ascending ints
        var m = GetMetrics();

        // Base DIP by platform + idiom + density + screen class
        double baseDip = DetermineBaseDip(m);

        // Find closest enum to base
        int baseIndex = FindClosestIndexByDip(sizes, baseDip);

        // Apply requested scale as step offsets (SizeEnum is in steps of 5)
        int offsetSteps = (int)scaleFactor / 5;
        int targetIndex = ClampIndex(baseIndex + offsetSteps, sizes.Length);

        // Per-platform min touch target
        if (enforceMinTouchTarget)
        {
            int minTouch = GetPlatformMinTouchDip(m.Platform, m.Idiom);
            while (targetIndex < sizes.Length - 1 && GetDipSize(sizes[targetIndex]) < minTouch)
                targetIndex++;
        }

        return sizes[targetIndex];
    }

    // ===== Metrics & Heuristics ============================================

    private readonly record struct Metrics(
        DevicePlatform Platform,
        DeviceIdiom Idiom,
        DisplayOrientation Orientation,
        double Density,
        double ShortestDip,
        double LongestDip
    );

    private static Metrics GetMetrics()
    {
        var info = GetDisplayInfo();
        var density = NormalizeDensity(info.Density);

        double shortestDip = 0, longestDip = 0;
        if (info.Width > 0 && info.Height > 0 && density > 0)
        {
            double minPx = Math.Min(info.Width, info.Height);
            double maxPx = Math.Max(info.Width, info.Height);
            shortestDip = minPx / density;
            longestDip = maxPx / density;
        }

        return new Metrics(
            DeviceInfo.Platform,
            DeviceInfo.Idiom,
            info.Orientation,
            density,
            shortestDip,
            longestDip
        );
    }

    // Platform-aware base DIP calculation
    private static double DetermineBaseDip(in Metrics m)
    {
        // 0) Start with a profile baseline (platform + idiom)
        var profile = GlobalScaleProfileOverride ?? GetPlatformDefaultProfile(m.Platform, m.Idiom);
        double baseDip = profile switch
        {
            UiScaleProfile.Compact => 24,
            UiScaleProfile.Comfortable => 32,
            UiScaleProfile.Touch => 48,
            _ => 32
        };

        // 1) Idiom nudge
        if (m.Idiom == DeviceIdiom.Desktop && m.Platform == DevicePlatform.WinUI) baseDip += 8; // roomier desktop controls
        else if (m.Idiom == DeviceIdiom.Tablet) baseDip += profile == UiScaleProfile.Touch ? 8 : 4;
        else if (m.Idiom == DeviceIdiom.TV) baseDip += 12;
        else if (m.Idiom == DeviceIdiom.Watch) baseDip = Math.Max(20, baseDip - 8);

        // 2) Orientation nudge
        if (m.Orientation == DisplayOrientation.Landscape) baseDip += 2;

        // 3) Density tuning — slightly different aggressiveness by platform
        // 3) Density tuning — slightly different aggressiveness by platform
        int add2, add4, add6, add8, add12, lowSub;

        var platform = m.Platform;

        // 3) Density tuning — slightly different aggressiveness by platform
        if (platform == DevicePlatform.Android)
        {
            (add2, add4, add6, add8, add12, lowSub) = (2, 4, 6, 8, 12, 4);
        }
        else if (platform == DevicePlatform.iOS || platform == DevicePlatform.MacCatalyst)
        {
            (add2, add4, add6, add8, add12, lowSub) = (2, 3, 5, 7, 10, 3);
        }
        else if (platform == DevicePlatform.WinUI)
        {
            (add2, add4, add6, add8, add12, lowSub) = (1, 2, 3, 4, 6, 2);
        }
        else
        {
            (add2, add4, add6, add8, add12, lowSub) = (2, 3, 5, 7, 10, 3);
        }

        if (m.Density <= 1.10) baseDip = Math.Max(baseDip - lowSub, 20);
        else if (m.Density >= 3.50) baseDip += add12;
        else if (m.Density >= 3.00) baseDip += add8;
        else if (m.Density >= 2.50) baseDip += add6;
        else if (m.Density >= 2.00) baseDip += add4;
        else baseDip += add2; // tiny bump for >1.1 && <2.0

        // 4) Screen-class tuning via shortest DIP
        if (m.ShortestDip >= 1000) baseDip += 8;
        else if (m.ShortestDip >= 900) baseDip += 6;
        else if (m.ShortestDip > 700) baseDip += 3;
        else if (m.ShortestDip > 0 && m.ShortestDip <= 480) baseDip = Math.Max(baseDip - 2, 20);

        // 5) External-display-ish tablet behaves like desktop
        if (m.Idiom == DeviceIdiom.Tablet && m.ShortestDip >= 1000)
            baseDip = Math.Max(baseDip, 64);

        // Final clamp to your enum span
        return Math.Clamp(baseDip, 12, 128);
    }

    private static int GetPlatformMinTouchDip(DevicePlatform platform, DeviceIdiom idiom)
    {
        if (platform == DevicePlatform.Android)
            return 48;
        if (platform == DevicePlatform.iOS || platform == DevicePlatform.MacCatalyst)
            return 44;
        if (platform == DevicePlatform.WinUI)
            return idiom == DeviceIdiom.Desktop ? 40 : 44;
        return 44;
    }

    // ===== Plumbing =========================================================

    private static readonly ButtonSizeEnum[] _buttonSizes = GetSortedEnumValues<ButtonSizeEnum>();
    private static readonly ImageSizes[] _imageSizes = GetSortedEnumValues<ImageSizes>();

    private static TEnum[] GetSortedEnumValues<TEnum>() where TEnum : struct, Enum
        => Enum.GetValues<TEnum>().OrderBy(v => Convert.ToInt32(v)).ToArray();

    private static DisplayInfo GetDisplayInfo()
    {
        try { return DeviceDisplay.MainDisplayInfo; }
        catch { return default; }
    }

    private static double NormalizeDensity(double raw)
    {
        if (double.IsNaN(raw) || raw <= 0) return 1.0;
        return Math.Clamp(raw, 0.75, 5.0);
    }

    private static int FindClosestIndexByDip<TEnum>(TEnum[] sizes, double targetDip) where TEnum : struct, Enum
    {
        int closestIndex = 0;
        double smallestDiff = double.MaxValue;
        for (int i = 0; i < sizes.Length; i++)
        {
            double diff = Math.Abs(GetDipSize(sizes[i]) - targetDip);
            if (diff < smallestDiff) { smallestDiff = diff; closestIndex = i; }
        }
        return closestIndex;
    }

    private static TEnum GetClosestEnumValue<TEnum>(TEnum[] sizes, int targetPixels) where TEnum : struct, Enum
    {
        int closestIndex = 0, smallestDiff = int.MaxValue;
        for (int i = 0; i < sizes.Length; i++)
        {
            int value = Convert.ToInt32(sizes[i]);
            int diff = Math.Abs(value - targetPixels);
            if (diff < smallestDiff) { smallestDiff = diff; closestIndex = i; }
        }
        return sizes[closestIndex];
    }

    private static int ClampIndex(int index, int length)
        => Math.Min(Math.Max(index, 0), length - 1);


    // ---- (Optional) Keep this overload for your existing callers -----------
    internal static double DetermineBaseDip(double density, DeviceIdiom idiom, double minDip)
    {
        // Rebuild metrics using live platform & simple minDip as shortest dimension
        var m = new Metrics(DeviceInfo.Platform, idiom, GetDisplayInfo().Orientation,
                            NormalizeDensity(density), Math.Max(0, minDip), Math.Max(0, minDip));
        return DetermineBaseDip(m);
    }
}
