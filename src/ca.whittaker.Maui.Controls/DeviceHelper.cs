using Microsoft.Maui.Devices;

namespace ca.whittaker.Maui.Controls;
public static class DeviceHelper
{
    public static int GetImageSizeForDevice(SizeEnum scaleFactor)
    {
        double density = DeviceDisplay.MainDisplayInfo.Density + ((double)scaleFactor / 10);
        if (density <= 1) return 12; // mdpi
        if (density <= 1.5) return 24; // hdpi
        if (density <= 2) return 24; // xhdpi
        if (density <= 3) return 24; // xxhdpi
        return 48; // xxxhdpi or higher
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
        var dip = (int)size;
        var density = DeviceDisplay.MainDisplayInfo.Density; // e.g., 1.0, 2.0, 3.0
        return (int)Math.Round(dip * density);
    }

}
