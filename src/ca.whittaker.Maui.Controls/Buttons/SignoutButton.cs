namespace ca.whittaker.Maui.Controls.Buttons;

public class SignoutButton : ButtonBase
{
    public static readonly BindableProperty SignoutButtonStateProperty = BindableProperty.Create(
        propertyName: nameof(SignoutButtonState),
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
                buttonBase.SetButtonState((ButtonStateEnum)newValue);
            }
    }

    public ButtonStateEnum SignoutButtonState
    {
        get => (ButtonStateEnum)GetValue(SignoutButtonStateProperty);
        set =>  SetValue(SignoutButtonStateProperty, value);
    }


    public SignoutButton() : base(BaseButtonTypeEnum.Signout)
    {
    }
}
