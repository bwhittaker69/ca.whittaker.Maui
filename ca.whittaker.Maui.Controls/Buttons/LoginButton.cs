namespace ca.whittaker.Maui.Controls.Buttons;

public class LoginButton : ButtonBase
{
    public static readonly BindableProperty LoginButtonStateProperty = BindableProperty.Create(
        propertyName: nameof(LoginButtonState),
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

    public ButtonStateEnum LoginButtonState
    {
        get => (ButtonStateEnum)GetValue(LoginButtonStateProperty);
        set =>  SetValue(LoginButtonStateProperty, value);
    }


    public LoginButton() : base()
    {
    }
}
