namespace ca.whittaker.Maui.Controls.Buttons;

public abstract class ButtonBase : Button, IButton
{

    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(
        propertyName: nameof(ButtonSize),
        returnType: typeof(SizeEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: SizeEnum.Normal,
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


    public ButtonBase(BaseButtonTypeEnum buttonType) : base()
    {
        ButtonType = buttonType;
        base.PropertyChanged += Button_PropertyChanged;
    }


    protected override void OnParentSet()
    {
        base.OnParentSet();
        UpdateUI();
    }

    private void Button_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if ((e.PropertyName == nameof(ButtonState))
            || (e.PropertyName == nameof(ButtonType))
            || (e.PropertyName == nameof(ButtonSize))
            || (e.PropertyName == nameof(WidthRequest))
            || (e.PropertyName == nameof(HeightRequest))
            || (e.PropertyName == nameof(Text)))
            UpdateUI();
    }


    public void UpdateUI()
    {
        void UpdateUI()
        {
            if (ButtonState != null)
            {
                base.BatchBegin();
                switch (ButtonState)
                {
                    case ButtonStateEnum.Enabled:
                        {
                            base.IsVisible = true;
                            if (ButtonSize != null)
                            {
                                base.HeightRequest = DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) + DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) * 0.4;
                                if (ButtonType != null)
                                {
                                    base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Enabled, (BaseButtonTypeEnum)ButtonType, (SizeEnum)ButtonSize);
                                }
                                if (!String.IsNullOrEmpty(Text))
                                {
                                    base.Text = Text;
                                }
                            }
                            break;
                        }
                    case ButtonStateEnum.Pressed:
                    case ButtonStateEnum.Disabled:
                        {
                            base.IsVisible = true;
                            if (ButtonSize != null)
                            {
                                base.HeightRequest = DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) + DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) * 0.4;
                                if (ButtonType != null)
                                {
                                    base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Disabled, (BaseButtonTypeEnum)ButtonType, (SizeEnum)ButtonSize);
                                }
                                if (!String.IsNullOrEmpty(DisabledText))
                                {
                                    base.Text = DisabledText;
                                }
                            }
                            break;
                        }
                    case ButtonStateEnum.Hidden:
                        base.IsVisible = false;
                        break;
                }
                base.BatchCommit();
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
