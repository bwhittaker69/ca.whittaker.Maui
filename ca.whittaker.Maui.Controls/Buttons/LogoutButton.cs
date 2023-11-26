
using System.Runtime.CompilerServices;

namespace ca.whittaker.Maui.Controls.Buttons;

public class LogoutButton : ButtonBase
{
    // ChangeState Bindable Property
    public static readonly BindableProperty LogoutButtonStateProperty = BindableProperty.Create(
        propertyName: nameof(LogoutButtonState),
        returnType: typeof(ButtonStateEnum),
        declaringType: typeof(ButtonBase),
        defaultValue: ButtonStateEnum.Disabled,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: OnPropertyChanged);

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
            if (bindable is ButtonBase buttonBase)
            {
                buttonBase.ConfigureButton((ButtonStateEnum)newValue);
            }
    }

    public ButtonStateEnum LogoutButtonState
    {
        get => (ButtonStateEnum)GetValue(LogoutButtonStateProperty);
        set =>  SetValue(LogoutButtonStateProperty, value);
    }


    public LogoutButton() : base()
    {
        // Constructor logic if needed
    }
}
