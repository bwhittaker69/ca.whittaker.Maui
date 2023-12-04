namespace ca.whittaker.Maui.Controls
{
    public static class DeviceHelper
    {

        public static int GetImageSizeForDevice(SizeEnum scaleFactor)
        {
            // Adjust these thresholds and sizes as needed
            double density = DeviceDisplay.MainDisplayInfo.Density + ((double)scaleFactor / 10);
            if (density <= 1) return 12; // mdpi
            if (density <= 1.5) return 24; // hdpi
            if (density <= 2) return 48; // xhdpi
            if (density <= 3) return 64; // xxhdpi
            return 72; // xxxhdpi or higher
        }

    }
}
