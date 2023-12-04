using System.Reflection;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace ca.whittaker.Maui.Controls
{
    public class ResourceHelper : IDisposable
    {
        // Method to verify if a resource exists
        private static bool ResourceExists(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceNames().Any(name => name.EndsWith(resourceName, StringComparison.Ordinal));
        }

        public string GetAssemblyName()
        {
            var assembly = GetType().Assembly;
            return assembly?.GetName()?.Name ?? "";
        }

        // Method to get an ImageSource based on button state and theme
        public ImageSource? GetImageSource(ButtonStateEnum buttonState, BaseButtonTypeEnum baseButtonType)
        {
            try
            {
                var assembly = GetType().Assembly;
                string assemblyName = GetAssemblyName();
                string theme = GetResourceTheme();
                string state = GetResourceState(buttonState);

                // Determine image size based on device screen density
                int size = GetImageSizeForDevice();

                string resourceName = $"{assemblyName}.Resources.Images.{baseButtonType.ToString().ToLower()}_{size}{theme}{state}.png";

                if (ResourceExists(resourceName))
                {
                    return ImageSource.FromResource(resourceName, assembly);
                }
                else
                {
                    Console.WriteLine($"Resource {resourceName} does not exist");
                    return null; // Return null if resource does not exist
                }
            }
            catch
            {
                return null; // Return null if fails
            }
        }

        private static int GetImageSizeForDevice()
        {
            // Adjust these thresholds and sizes as needed
            double density = DeviceDisplay.MainDisplayInfo.Density;
            if (density <= 1) return 12; // mdpi
            if (density <= 1.5) return 24; // hdpi
            if (density <= 2) return 48; // xhdpi
            if (density <= 3) return 64; // xxhdpi
            return 72; // xxxhdpi or higher
        }

        private static string GetResourceTheme()
        {
            AppTheme currentTheme = GetCurrentTheme();
            return (currentTheme == AppTheme.Dark ? "_dark" : "_light");
        }

        private static string GetResourceState(ButtonStateEnum buttonState)
        {
            return (buttonState == ButtonStateEnum.Disabled ? "_disabled" : "");
        }


        private static AppTheme GetCurrentTheme()
        {
            try
            {
                return Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
            }
            catch
            {
                return AppTheme.Unspecified;
            }
        }

        public void Dispose()
        {
            // Dispose logic here
            GC.SuppressFinalize(this);
        }
    }
}
