using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;

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
    public ButtonBase(BaseButtonTypeEnum baseButtonType) : base()
    {
        _baseButtonType = baseButtonType;
        base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Enabled, baseButtonType, ButtonSize, false);
    }

    public SizeEnum ButtonSize
    {
        get => (SizeEnum)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public string ButtonText { get; set; } = string.Empty;
    public void SetButtonState(ButtonStateEnum buttonState = ButtonStateEnum.Enabled)
    {
        void UpdateUI()
        {
            switch (buttonState)
            {
                case ButtonStateEnum.Enabled:
                case ButtonStateEnum.Disabled:
                    base.IsVisible = true;
                    break;
                case ButtonStateEnum.Hidden:
                    base.IsVisible = false;
                    break;
            }

            base.ImageSource = new ResourceHelper().GetImageSource(buttonState, _baseButtonType, ButtonSize, false);


        }

        // Check if on the main thread and update UI accordingly
        if (MainThread.IsMainThread)
        {
            UpdateUI();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
    }

    public void SetButtonText(string text)
    {
        ButtonText = text;

        void UpdateUI()
        {
            base.Text = ButtonText;
        }
        // Check if on the main thread and update UI accordingly
        if (MainThread.IsMainThread)
        {
            UpdateUI();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
    }
}
