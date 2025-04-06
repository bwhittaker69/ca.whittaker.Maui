//#define DEBUGOUT
using System.Collections.Concurrent;
using System.Reflection;

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
        public ImageSource? GetImageSource(ButtonStateEnum buttonState, ButtonIconEnum baseButtonType, SizeEnum sizeEnum, CancellationToken cancellationToken = default)
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
#if DEBUGOUT
            Debug.WriteLine($"Attempting to load resource: {resourceName}");
#endif
            cancellationToken.ThrowIfCancellationRequested();

            // Return cached ImageSource if available.
            if (_cache.TryGetValue(resourceName, out var cachedImage))
            {
#if DEBUGOUT
                Debug.WriteLine($"Returning cached resource: {resourceName}");
#endif
                return cachedImage;
            }

            if (ResourceExists(resourceName, assembly))
            {
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    var imageSource = ImageSource.FromResource(resourceName, assembly);
                    _cache.TryAdd(resourceName, imageSource);
                    return imageSource;
                }
                catch (Exception ex)
                {
#if DEBUGOUT
                    Debug.WriteLine($"Error loading image resource: {ex}");
#endif
                    return null;
                }
#pragma warning restore CS0168 // Variable is declared but never used
            }
            else
            {
#if DEBUGOUT
                Debug.WriteLine($"Resource not found: {resourceName}");
#endif
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
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                return Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
            }
            catch (Exception ex)
            {
#if DEBUGOUT
                Debug.WriteLine($"Error retrieving current theme: {ex}");
#endif
                return AppTheme.Unspecified;
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
