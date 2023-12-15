namespace ca.whittaker.Maui.Controls.Buttons;

public class SigninButton : ButtonBase
{

    public static readonly BindableProperty SigninButtonTypeProperty = BindableProperty.Create(
        propertyName: nameof(SigninButtonType),
        returnType: typeof(SiginButtonTypeEnum),
        declaringType: typeof(ButtonBase),
        defaultValue: SiginButtonTypeEnum.Generic,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: OnButtonTypeChanged);

    private static void OnButtonTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
            if (bindable is SigninButton signinButton && newValue is SiginButtonTypeEnum)
            {
                switch (newValue)
                {
                    case SiginButtonTypeEnum.Facebook:
                        signinButton.ButtonType = ImageResourceEnum.Facebook;
                        break;
                    case SiginButtonTypeEnum.Google:
                        signinButton.ButtonType = ImageResourceEnum.Google;
                        break;
                    case SiginButtonTypeEnum.Linkedin:
                        signinButton.ButtonType = ImageResourceEnum.Linkedin;
                        break;
                    case SiginButtonTypeEnum.Tiktok:
                        signinButton.ButtonType = ImageResourceEnum.Tiktok;
                        break;
                    case SiginButtonTypeEnum.Microsoft:
                        signinButton.ButtonType = ImageResourceEnum.Microsoft;
                        break;
                    case SiginButtonTypeEnum.Apple:
                        signinButton.ButtonType = ImageResourceEnum.Apple;
                        break;
                    default:
                        signinButton.ButtonType = ImageResourceEnum.Signin;
                        break;
                }
            }
    }

    public SiginButtonTypeEnum SigninButtonType
    {
        get => (SiginButtonTypeEnum)GetValue(SigninButtonTypeProperty);
        set => SetValue(SigninButtonTypeProperty, value);
    }

    public SigninButton() : base(ImageResourceEnum.Signin)
    {

    }
}
