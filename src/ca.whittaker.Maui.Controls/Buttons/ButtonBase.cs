namespace ca.whittaker.Maui.Controls.Buttons;

public abstract class ButtonBase : Button
{
    public enum BaseButtonTypeEnum { Signin, Signout, Cancel, Save, Undo, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }

    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(
        propertyName: nameof(ButtonSize),
        returnType: typeof(ButtonSizeEnum),
        declaringType: typeof(ButtonBase),
        defaultValue: ButtonSizeEnum.Normal,
        defaultBindingMode: BindingMode.OneWay);

    public BaseButtonTypeEnum _baseButtonType = BaseButtonTypeEnum.Save;

    public ButtonSizeEnum ButtonSize
    {
        get => (ButtonSizeEnum)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public ButtonBase(BaseButtonTypeEnum baseButtonType) : base()
    {
        _baseButtonType = baseButtonType;
    }
   
    public void SetButtonState(ButtonStateEnum buttonState = ButtonStateEnum.Enabled)
    {
        switch (buttonState)
        {
            case ButtonStateEnum.Enabled:
                IsVisible = true;
                IsEnabled = true;
                ImageSource = SetImageSource(buttonState);
                break;
            case ButtonStateEnum.Disabled:
                IsVisible = true;
                IsEnabled = false;
                ImageSource = SetImageSource(buttonState);
                break;
            case ButtonStateEnum.Hidden:
                IsEnabled = false;
                IsVisible = false;
                break;
        }
    }

    private ImageSource SetImageSource(ButtonStateEnum buttonState)
    {
        return ImageSource.FromFile($"{_baseButtonType}_{((int)ButtonSize).ToString()}_mauiimage{(buttonState.Equals(ButtonStateEnum.Disabled) ? "_disabled" : "")}.png");
    }
}
