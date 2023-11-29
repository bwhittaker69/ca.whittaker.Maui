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
    public string ButtonText { get; set; } = string.Empty;
    public SizeEnum ButtonSize
    {
        get => (SizeEnum)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public ButtonBase(BaseButtonTypeEnum baseButtonType) : base()
    {
        _baseButtonType = baseButtonType;
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
    public void SetButtonState(ButtonStateEnum buttonState = ButtonStateEnum.Enabled)
    {
        void UpdateUI()
        {
            switch (buttonState)
            {
                case ButtonStateEnum.Enabled:
                    base.IsVisible = true;
                    base.IsEnabled = true;
                    base.ImageSource = GetImageSource(buttonState);
                    break;
                case ButtonStateEnum.Disabled:
                    base.IsVisible = true;
                    base.IsEnabled = true;
                    base.ImageSource = GetImageSource(buttonState);
                    break;
                case ButtonStateEnum.Hidden:
                    base.IsEnabled = false;
                    base.IsVisible = false;
                    break;
            }
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

    public enum BaseButtonTypeEnum { Signin, Signout, Save, Edit, Cancel, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }

    private ImageSource GetImageSource(ButtonStateEnum buttonState)
    {
        var assembly = this.GetType().Assembly;
        string? assemblyName = assembly.GetName().Name;
        AppTheme? currentTheme = Application.Current.RequestedTheme;
        string lightThemeEnabled = "";
        string lightThemeDisabled = "_disabled";
        string darkThemeEnabled = "_disabled";
        string darkThemeDisabled = "";
        string enabled = "";
        string disabled = "";
        if (currentTheme == AppTheme.Dark)
        {
            enabled = darkThemeEnabled;
            disabled = darkThemeDisabled;
        }
        else if (currentTheme == AppTheme.Light)
        {
            enabled = lightThemeEnabled;
            disabled = lightThemeDisabled;
        }
        else
        {
            enabled = lightThemeEnabled;
            disabled = lightThemeDisabled;
        }

        string resourceName = $"{assemblyName}.Resources.Images.{_baseButtonType.ToString().ToLower()}_{((int)ButtonSize).ToString()}_mauiimage{(buttonState.Equals(ButtonStateEnum.Disabled) ? disabled : enabled)}.png";
        return ImageSource.FromResource(resourceName, assembly);
    }
}
