﻿using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;

namespace ca.whittaker.Maui.Controls.Buttons;

public abstract class ButtonBase : Button, IButton
{
    private double heightMultiplier => DeviceInfo.Platform == DevicePlatform.WinUI ? 2.8 : 1.0;

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
        defaultValue: ButtonStateEnum.Enabled,
        defaultBindingMode: BindingMode.OneWay);

    public ButtonStateEnum? ButtonState
    {
        get => (ButtonStateEnum?)GetValue(ButtonStateProperty);
        set => SetValue(ButtonStateProperty, value);
    }

    public static readonly BindableProperty ButtonTypeProperty = BindableProperty.Create(
        propertyName: nameof(ButtonType),
        returnType: typeof(ImageResourceEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay);

    public ImageResourceEnum? ButtonType
    {
        get => (ImageResourceEnum?)GetValue(ButtonTypeProperty);
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

    public ButtonBase(ImageResourceEnum buttonType) : base()
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
    public void Enabled()
    {
        if (ButtonState != ButtonStateEnum.Enabled)
            ButtonState = ButtonStateEnum.Enabled;
    }
    public void Disabled()
    {
        if (ButtonState != ButtonStateEnum.Disabled)
            ButtonState = ButtonStateEnum.Disabled;
    }
    public void Hide()
    {
        if (ButtonState != ButtonStateEnum.Hidden)
            ButtonState = ButtonStateEnum.Hidden;
    }
    private bool _updateUI = false;
    public void UpdateUI()
    {
        if (this._updateUI) return;
        this._updateUI = true;
        void _update()
        {
            if (ButtonState != null)
            {
                base.BatchBegin();
                switch (ButtonState)
                {
                    case ButtonStateEnum.Enabled:
                        {
                            ConfigureEnabled();
                            break;
                        }
                    case ButtonStateEnum.Disabled:
                        {
                            ConfigureDisabled();
                            break;
                        }
                    case ButtonStateEnum.Pressed:
                        {
                            ConfigurePressed();
                            break;
                        }
                    case ButtonStateEnum.Hidden:
                        {
                            ConfigureHidden();
                            break;
                        }
                }
                base.BatchCommit();
            }
        }

        if (MainThread.IsMainThread)
        {
            _update();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => _update());
        }
        this._updateUI = false;
    }
    
    private void ConfigureEnabled()
    {
        base.IsEnabled = true;
        base.IsVisible = true;

        if (ButtonSize != null)
        {
            base.HeightRequest = DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) * heightMultiplier;
            if (ButtonType != null)
            {
                base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Enabled, (ImageResourceEnum)ButtonType, (SizeEnum)ButtonSize);
            }
            if (!string.IsNullOrEmpty(Text))
            {
                base.Text = Text;
            }
        }
    }
    private void ConfigureDisabled()
    {
        base.IsEnabled = false;
        base.IsVisible = true;

        if (ButtonSize != null)
        {
            base.HeightRequest = DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) * heightMultiplier;
            if (ButtonType != null)
            {
                base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Disabled, (ImageResourceEnum)ButtonType, (SizeEnum)ButtonSize);
            }
            if (!string.IsNullOrEmpty(DisabledText))
            {
                base.Text = DisabledText;
            }
        }
    }
    private void ConfigurePressed()
    {
        if (ButtonSize != null)
        {
            base.HeightRequest = DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) * heightMultiplier;
            if (ButtonType != null)
            {
                base.ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Disabled, (ImageResourceEnum)ButtonType, (SizeEnum)ButtonSize);
            }
        }
    }

    private void ConfigureHidden()
    {
        base.IsEnabled = true;
        base.IsVisible = false;
    }
}
