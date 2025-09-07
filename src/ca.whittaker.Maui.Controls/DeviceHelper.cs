using Microsoft.Maui.Devices;

namespace ca.whittaker.Maui.Controls;
public static class DeviceHelper
{
    public static int GetImageSizeForDevice(SizeEnum scaleFactor)
    {
        double density = DeviceDisplay.MainDisplayInfo.Density + ((double)scaleFactor / 10);
        if (density <= 1) return 12; // mdpi
        if (density <= 1.5) return 12; // hdpi
        if (density <= 2) return 24; // xhdpi
        if (density <= 3) return 24; // xxhdpi
        return 48; // xxxhdpi or higher
    }
}
