using System.Reflection;

namespace ca.whittaker.Maui.Controls;

public class ResourceHelper : IDisposable
{
    // Method to verify if a resource exists
    private static bool ResourceExists(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceNames().Any(name => name.EndsWith(resourceName, StringComparison.Ordinal));
    }

    // Method to get an ImageSource based on button state and theme
    public ImageSource? GetImageSource(ButtonStateEnum buttonState, BaseButtonTypeEnum baseButtonType, SizeEnum size, bool reverseTheme = false)
    {
        try
        {
            var assembly = GetType().Assembly;
            string assemblyName = assembly?.GetName()?.Name ?? "";
            string suffix = GetResourceSuffix(buttonState, reverseTheme);

            string resourceName = $"{assemblyName}.Resources.Images.{baseButtonType.ToString().ToLower()}_{(int)size}_mauiimage{suffix}.png";

            if (ResourceExists(resourceName))
            {
                return ImageSource.FromResource(resourceName, assembly);
            }

            return null; // Return null if resource does not exist
        }
        catch
        {
            return null; // Return null if fails
        }
    }

    private static string GetResourceSuffix(ButtonStateEnum buttonState, bool reverseTheme)
    {
        AppTheme currentTheme = GetCurrentTheme();
        string enabled = currentTheme == AppTheme.Dark ? "_disabled" : "";
        string disabled = currentTheme == AppTheme.Dark ? "" : "_disabled";

        return buttonState == ButtonStateEnum.Disabled ^ reverseTheme ? enabled : disabled;
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
    // Implement IDisposable
    public void Dispose()
    {
        // Dispose logic here
        GC.SuppressFinalize(this);
    }

}
