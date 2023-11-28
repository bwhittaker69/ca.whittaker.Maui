namespace ca.whittaker.Maui.Controls.Buttons;

public abstract class ButtonBase : Button
{

    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(
        propertyName: nameof(ButtonSize),
        returnType: typeof(SizeEnum),
        declaringType: typeof(ButtonBase),
        defaultValue: SizeEnum.Normal,
        defaultBindingMode: BindingMode.OneWay);

    public BaseButtonTypeEnum _baseButtonType = BaseButtonTypeEnum.Save;

    public SizeEnum ButtonSize
    {
        get => (SizeEnum)GetValue(ButtonSizeProperty);
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
    public enum BaseButtonTypeEnum { Signin, Signout, Save, Edit, Cancel, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }

    private ImageSource SetImageSource(ButtonStateEnum buttonState)
    {
        return ImageSource.FromFile($"{_baseButtonType.ToString().ToLower()}_{((int)ButtonSize).ToString()}_mauiimage{(buttonState.Equals(ButtonStateEnum.Disabled) ? "_disabled" : "")}.png");
    }
}
