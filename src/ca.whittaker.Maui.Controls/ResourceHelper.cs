using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System;
using System.Collections.Concurrent;

namespace ca.whittaker.Maui.Controls
{
    public class ResourceHelper : IDisposable
    {
        // Thread-safe cache for loaded image resources.
        private static readonly ConcurrentDictionary<string, ImageSource?> _cache = new ConcurrentDictionary<string, ImageSource?>();

        // Verifies if a resource exists in the provided assembly.
        public static bool ResourceExists(string resourceName, Assembly assembly)
        {
            return assembly.GetManifestResourceNames()
                           .Any(name => name.EndsWith(resourceName, StringComparison.Ordinal));
        }

        public string GetAssemblyName()
        {
            var assembly = GetType().Assembly;
            return assembly?.GetName()?.Name ?? string.Empty;
        }

        // Retrieves an ImageSource based on button state, resource type, and size with caching.
        public ImageSource? GetImageSource(ButtonStateEnum buttonState, ImageResourceEnum baseButtonType, SizeEnum sizeEnum, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var assembly = GetType().Assembly;
            cancellationToken.ThrowIfCancellationRequested();

            string assemblyName = GetAssemblyName();
            string theme = GetResourceTheme();
            string state = GetResourceState(buttonState);

            // Determine image size based on device screen density.
            int size = DeviceHelper.GetImageSizeForDevice(sizeEnum);

            string resourceName = $"{assemblyName}.Resources.Images.{baseButtonType.ToString().ToLowerInvariant()}_{size}{theme}{state}.png";
            Debug.WriteLine($"Attempting to load resource: {resourceName}");
            cancellationToken.ThrowIfCancellationRequested();

            // Return cached ImageSource if available.
            if (_cache.TryGetValue(resourceName, out var cachedImage))
            {
                Debug.WriteLine($"Returning cached resource: {resourceName}");
                return cachedImage;
            }

            if (ResourceExists(resourceName, assembly))
            {
                try
                {
                    var imageSource = ImageSource.FromResource(resourceName, assembly);
                    _cache.TryAdd(resourceName, imageSource);
                    return imageSource;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading image resource: {ex}");
                    return null;
                }
            }
            else
            {
                Debug.WriteLine($"Resource not found: {resourceName}");
                return null;
            }
        }

        private static string GetResourceTheme()
        {
            AppTheme currentTheme = GetCurrentTheme();
            return (currentTheme == AppTheme.Dark ? "_dark" : "_light");
        }

        private static string GetResourceState(ButtonStateEnum buttonState)
        {
            return (buttonState == ButtonStateEnum.Disabled ? "_disabled" : string.Empty);
        }

        private static AppTheme GetCurrentTheme()
        {
            try
            {
                return Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving current theme: {ex}");
                return AppTheme.Unspecified;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
