namespace ca.whittaker.Maui.Controls.Buttons;

public abstract class ButtonBase2 : Button, IButton
{

    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(
        propertyName: nameof(ButtonSize),
        returnType: typeof(SizeEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay);

    public SizeEnum? ButtonSize
    {
        get => (SizeEnum?)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public static readonly BindableProperty ButtonStateProperty = BindableProperty.Create(
        propertyName: nameof(ButtonState),
        returnType: typeof(ButtonStateEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay);

    public ButtonStateEnum? ButtonState
    {
        get => (ButtonStateEnum?)GetValue(ButtonStateProperty);
        set => SetValue(ButtonStateProperty, value);
    }

    public static readonly BindableProperty ButtonTypeProperty = BindableProperty.Create(
        propertyName: nameof(ButtonType),
        returnType: typeof(BaseButtonTypeEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay);

    public BaseButtonTypeEnum? ButtonType
    {
        get => (BaseButtonTypeEnum?)GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    public new static readonly BindableProperty TextProperty = BindableProperty.Create(
        propertyName: nameof(Text),
        returnType: typeof(string),
        declaringType: typeof(ButtonBase),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    public new string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty DisabledTextProperty = BindableProperty.Create(
        propertyName: nameof(DisabledText),
        returnType: typeof(string),
        declaringType: typeof(ButtonBase),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    public string DisabledText
    {
        get => (string)GetValue(DisabledTextProperty);
        set => SetValue(DisabledTextProperty, value);
    }

    public static readonly BindableProperty PressedTextProperty = BindableProperty.Create(
        propertyName: nameof(PressedText),
        returnType: typeof(string),
        declaringType: typeof(ButtonBase),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    public string PressedText
    {
        get => (string)GetValue(PressedTextProperty);
        set => SetValue(PressedTextProperty, value);
    }

    private bool _baseUseDeviceTheme;

    public ButtonBase2(BaseButtonTypeEnum buttonType) : base()
    {

        base.BorderWidth = 0;
        base.Margin = 0;
        base.Padding = 0;

        ButtonType = buttonType;

        _baseUseDeviceTheme = false;

        base.PropertyChanged += Button_PropertyChanged;

        base.Pressed += ButtonBase_Pressed;
        base.Released += ButtonBase_Released;

    }

    private void Button_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if ((e.PropertyName == nameof(ButtonState))
            || (e.PropertyName == nameof(ButtonType))
            || (e.PropertyName == nameof(ButtonSize))
            || (e.PropertyName == nameof(Text)))
            ConfigureButton();
    }
    private bool IsPressed = false;
    private void ButtonBase_Released(object? sender, EventArgs e)
    {
        IsPressed = false;
    }

    private void ButtonBase_Pressed(object? sender, EventArgs e)
    {
        IsPressed = true;
    }

    public void ConfigureButton()
    {
        void UpdateUI()
        {

            if ((ButtonState != null)
                && (ButtonType != null)
                && (ButtonSize != null))
            {
                base.BatchBegin();
                switch (ButtonState)
                {
                    case ButtonStateEnum.Enabled:
                        base.IsVisible = true;
                        base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Enabled, (BaseButtonTypeEnum)ButtonType, (SizeEnum)ButtonSize, _baseUseDeviceTheme);
                        base.Text = (Text == null ? "" : Text);
                        break;
                    case ButtonStateEnum.Disabled:
                        base.IsVisible = true;
                        base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Disabled, (BaseButtonTypeEnum)ButtonType, (SizeEnum)ButtonSize, _baseUseDeviceTheme);
                        base.Text = (DisabledText != null && DisabledText != "" ? DisabledText : Text);
                        break;
                    case ButtonStateEnum.Pressed:
                        base.IsVisible = true;
                        base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Disabled, (BaseButtonTypeEnum)ButtonType, (SizeEnum)ButtonSize, _baseUseDeviceTheme);
                        base.Text = (PressedText != null && PressedText != "" ? PressedText : Text);
                        break;
                    case ButtonStateEnum.Hidden:
                        base.IsVisible = false;
                        break;
                }
                base.BatchCommit();
            }
            else
            {
                // hide button if not configured
                base.IsVisible = false;
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

}
