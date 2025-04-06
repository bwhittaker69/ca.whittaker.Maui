using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

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
                        signinButton.ButtonIcon = ButtonIconEnum.Facebook;
                        break;
                    case SiginButtonTypeEnum.Google:
                        signinButton.ButtonIcon = ButtonIconEnum.Google;
                        break;
                    case SiginButtonTypeEnum.Linkedin:
                        signinButton.ButtonIcon = ButtonIconEnum.Linkedin;
                        break;
                    case SiginButtonTypeEnum.Tiktok:
                        signinButton.ButtonIcon = ButtonIconEnum.Tiktok;
                        break;
                    case SiginButtonTypeEnum.Microsoft:
                        signinButton.ButtonIcon = ButtonIconEnum.Microsoft;
                        break;
                    case SiginButtonTypeEnum.Apple:
                        signinButton.ButtonIcon = ButtonIconEnum.Apple;
                        break;
                    default:
                        signinButton.ButtonIcon = ButtonIconEnum.Signin;
                        break;
                }
            }
    }

    public SiginButtonTypeEnum SigninButtonType
    {
        get => (SiginButtonTypeEnum)GetValue(SigninButtonTypeProperty);
        set => SetValue(SigninButtonTypeProperty, value);
    }

    public SigninButton() : base(ButtonIconEnum.Signin)
    {

    }
}
