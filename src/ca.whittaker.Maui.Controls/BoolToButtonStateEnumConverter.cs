using System;
using System.Globalization;

namespace ca.whittaker.Maui.Controls;

public class BoolToButtonStateEnumConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isLive)
        {
            return isLive ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled; // Adjust these enum values based on your ButtonStateEnum
        }
        return ButtonStateEnum.Enabled; // Or your default enum state
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
