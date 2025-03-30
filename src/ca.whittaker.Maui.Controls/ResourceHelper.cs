using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

using System.Reflection;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls
{
    public class ResourceHelper : IDisposable
    {
        // Method to verify if a resource exists
        public static bool ResourceExists(string resourceName)
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
        public ImageSource? GetImageSource(ButtonStateEnum buttonState, ImageResourceEnum baseButtonType, SizeEnum sizeEnum)
        {
            try
            {
                var assembly = GetType().Assembly;
                string assemblyName = GetAssemblyName();
                string theme = GetResourceTheme();
                string state = GetResourceState(buttonState);

                // Determine image size based on device screen density
                int size = DeviceHelper.GetImageSizeForDevice(sizeEnum);

                string resourceName = $"{assemblyName}.Resources.Images.{baseButtonType.ToString().ToLower()}_{size}{theme}{state}.png";

                Debug.WriteLine(resourceName);

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

        private static string GetResourceTheme()
        {
            AppTheme currentTheme = GetCurrentTheme();
            return (currentTheme != AppTheme.Dark ? "_dark" : "_light");
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
