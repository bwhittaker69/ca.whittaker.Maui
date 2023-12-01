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
    public string GetAssemblyName()
    {
        var assembly = GetType().Assembly;
        return assembly?.GetName()?.Name ?? "";
    }
    // Method to get an ImageSource based on button state and theme
    public ImageSource? GetImageSource(ButtonStateEnum buttonState, BaseButtonTypeEnum baseButtonType, SizeEnum size, bool useDeviceTheming = false)
    {
        //Console.Write("GetImageSource: ");
        try
        {
            var assembly = GetType().Assembly;
            string assemblyName = GetAssemblyName();
            string suffix = GetResourceSuffix(buttonState, useDeviceTheming);

            string resourceName = $"{assemblyName}.Resources.Images.{baseButtonType.ToString().ToLower()}_{(int)size}_mauiimage{suffix}.png";

            //Console.Write(resourceName);

            if (ResourceExists(resourceName))
            {
                //Console.WriteLine(" - FOUND");
                return ImageSource.FromResource(resourceName, assembly);
            }
            //Console.WriteLine(" - NOT FOUND");
            return null; // Return null if resource does not exist
        }
        catch
        {
            //Console.WriteLine(" - ERROR");
            return null; // Return null if fails
        }
    }

    private static string GetResourceSuffix(ButtonStateEnum buttonState, bool useDeviceTheming)
    {
        AppTheme currentTheme = GetCurrentTheme();
        string themeEnabled = currentTheme == AppTheme.Dark ? "_disabled" : "";
        string themeDisabled = currentTheme == AppTheme.Dark ? "" : "_disabled";
        string enabled = "";
        string disabled = "_disabled";
        if (useDeviceTheming)
            return buttonState == ButtonStateEnum.Disabled ? disabled : enabled;
        else
            return buttonState == ButtonStateEnum.Disabled ? themeDisabled : themeEnabled;
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
