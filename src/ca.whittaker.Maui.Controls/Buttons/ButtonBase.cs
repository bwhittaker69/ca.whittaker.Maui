using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;
using System.Reflection;
using VisualElement = Microsoft.Maui.Controls.VisualElement;

namespace ca.whittaker.Maui.Controls.Buttons;

public interface IButtonBase : IButton
{
    #region Properties

    ButtonIconEnum? ButtonIcon { get; set; }
    SizeEnum? ButtonSize { get; set; }
    ButtonStateEnum? ButtonState { get; set; }
    string DisabledText { get; set; }
    string PressedText { get; set; }
    string Text { get; set; }

    #endregion Properties

    #region Public Methods

    void Disabled();
    void Enabled();
    void Hide();
    void UpdateUI();

    #endregion Public Methods
}

public static class TypeHelper
{
    #region Public Methods

    public static List<Type> GetNonAbstractClasses()
    {
        return Assembly.GetExecutingAssembly()
                       .GetTypes()
                       .Where(t => t.IsClass
                                && !t.IsAbstract
                                && t.Namespace == "ca.whittaker.Maui.Controls.Buttons"
                                && typeof(IButtonBase).IsAssignableFrom(t))
                       .ToList();
    }

    #endregion Public Methods
}

public abstract class ButtonBase : Button, IButtonBase
{
    #region Fields

    private bool _updateUI = false;

    public static readonly BindableProperty ButtonIconProperty = BindableProperty.Create(
        propertyName: nameof(ButtonIcon),
        returnType: typeof(ButtonIconEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create(
        propertyName: nameof(ButtonSize),
        returnType: typeof(SizeEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: SizeEnum.Normal,
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty ButtonStateProperty = BindableProperty.Create(
        propertyName: nameof(ButtonState),
        returnType: typeof(ButtonStateEnum?),
        declaringType: typeof(ButtonBase),
        defaultValue: ButtonStateEnum.Enabled,
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty DisabledTextProperty = BindableProperty.Create(
        propertyName: nameof(DisabledText),
        returnType: typeof(string),
        declaringType: typeof(ButtonBase),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty PressedTextProperty = BindableProperty.Create(
        propertyName: nameof(PressedText),
        returnType: typeof(string),
        declaringType: typeof(ButtonBase),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    public new static readonly BindableProperty TextProperty = BindableProperty.Create(
        propertyName: nameof(Text),
        returnType: typeof(string),
        declaringType: typeof(ButtonBase),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    #endregion Fields

    #region Ctor

    public ButtonBase(ButtonIconEnum buttonType) : base()
    {
        ButtonIcon = buttonType;
        base.PropertyChanged += Button_PropertyChanged;
    }

    #endregion Ctor

    #region Properties

    // previously used 2.8 factor for windows, now no longer appropriate with newer MAUI
    private double heightMultiplier => DeviceInfo.Platform == DevicePlatform.WinUI ? 1.0 : 1.0;
    private const double DefaultSpacing = 0d;

    public ButtonIconEnum? ButtonIcon
    {
        get => (ButtonIconEnum?)GetValue(ButtonIconProperty);
        set => SetValue(ButtonIconProperty, value);
    }

    public SizeEnum? ButtonSize
    {
        get => (SizeEnum?)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public ButtonStateEnum? ButtonState
    {
        get => (ButtonStateEnum?)GetValue(ButtonStateProperty);
        set => SetValue(ButtonStateProperty, value);
    }

    public string DisabledText
    {
        get => (string)GetValue(DisabledTextProperty);
        set => SetValue(DisabledTextProperty, value);
    }

    public string PressedText
    {
        get => (string)GetValue(PressedTextProperty);
        set => SetValue(PressedTextProperty, value);
    }

    public new string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion Properties

    #region Private Methods (DRY core)

    private void Button_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if ((e.PropertyName == nameof(ButtonState))
            || (e.PropertyName == nameof(ButtonIcon))
            || (e.PropertyName == nameof(ButtonSize))
            || (e.PropertyName == nameof(WidthRequest))
            || (e.PropertyName == nameof(HeightRequest))
            || (e.PropertyName == nameof(Text)))
        {
            UpdateUI();
        }
    }

    private void ReapplyLayout()
    {
        InvalidateMeasure();
        (Parent as VisualElement)?.InvalidateMeasure();
    }

    private void ApplyVisual(bool isEnabled, bool isVisible, ButtonStateEnum stateForIcon)
    {
        void _update()
        {
            base.BatchBegin();
            base.IsEnabled = isEnabled;
            base.IsVisible = isVisible;

            if (ButtonSize != null)
            {
                base.HeightRequest = DeviceHelper.GetImageSizeForDevice((SizeEnum)ButtonSize) * heightMultiplier;

                // Image
                if (ButtonIcon != null)
                {
                    base.ImageSource = new ResourceHelper().GetImageSource(stateForIcon, (ButtonIconEnum)ButtonIcon, (SizeEnum)ButtonSize);
                }

                // Text (prefer state-specific text if provided)
                string? text = stateForIcon switch
                {
                    ButtonStateEnum.Disabled when !string.IsNullOrWhiteSpace(DisabledText) => DisabledText,
                    ButtonStateEnum.Pressed when !string.IsNullOrWhiteSpace(PressedText) => PressedText,
                    _ => Text
                };

                var pos = base.ContentLayout.Position;



                if (!string.IsNullOrEmpty(text))
                {
                    switch (pos)
                    {
                        case ButtonContentLayout.ImagePosition.Right:
                            base.Text = String.Concat(text, "  ");
                            break;
                        case ButtonContentLayout.ImagePosition.Left:
                            base.Text = String.Concat("  ", text);
                            break;
                    }
                }

                // Spacing only matters if both image and text are present
                bool hasImage = ButtonIcon != null;
                bool hasText = !string.IsNullOrEmpty(base.Text);

                if (hasImage && hasText)
                {
                    base.ContentLayout = new ButtonContentLayout(pos, 0d);
                    Handler?.UpdateValue(nameof(Button.ContentLayout));
                    ReapplyLayout();

                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                        base.Padding = new Thickness(0);
                }
            }

            base.BatchCommit();
        }

        UiThreadHelper.RunOnMainThread(_update);
    }

    #endregion Private Methods (DRY core)

    #region Configure Methods (thin wrappers)

    private void ConfigureEnabled()
        => ApplyVisual(isEnabled: true, isVisible: true, stateForIcon: ButtonStateEnum.Enabled);

    private void ConfigureDisabled()
        => ApplyVisual(isEnabled: false, isVisible: true, stateForIcon: ButtonStateEnum.Disabled);

    private void ConfigurePressed()
        => ApplyVisual(isEnabled: true, isVisible: true, stateForIcon: ButtonStateEnum.Pressed);

    private void ConfigureHidden()
        => ApplyVisual(isEnabled: true, isVisible: false, stateForIcon: ButtonStateEnum.Enabled);

    #endregion Configure Methods

    #region Protected Overrides

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
            UpdateUI(); // re-apply after native view exists
        ReapplyLayout();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        UpdateUI();
    }

    #endregion Protected Overrides

    #region Public Methods

    public void Disabled()
    {
        if (ButtonState != ButtonStateEnum.Disabled)
            ButtonState = ButtonStateEnum.Disabled;
    }

    public void Enabled()
    {
        if (ButtonState != ButtonStateEnum.Enabled)
            ButtonState = ButtonStateEnum.Enabled;
    }

    public void Hide()
    {
        if (ButtonState != ButtonStateEnum.Hidden)
            ButtonState = ButtonStateEnum.Hidden;
    }

    public void UpdateUI()
    {
        if (_updateUI) return;
        _updateUI = true;

        if (ButtonState != null)
        {
            switch (ButtonState)
            {
                case ButtonStateEnum.Enabled: ConfigureEnabled(); break;
                case ButtonStateEnum.Disabled: ConfigureDisabled(); break;
                case ButtonStateEnum.Pressed: ConfigurePressed(); break;
                case ButtonStateEnum.Hidden: ConfigureHidden(); break;
            }
        }

        _updateUI = false;
    }

    #endregion Public Methods
}
