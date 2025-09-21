#undef BUTTONBASE_TRACE   

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;
using System.Reflection;
using System.Diagnostics; // <-- added
using VisualElement = Microsoft.Maui.Controls.VisualElement;
using Image = Microsoft.Maui.Controls.Image;

namespace ca.whittaker.Maui.Controls.Buttons;

public interface IButtonBase : IButton
{
    ButtonIconEnum? ButtonIcon { get; set; }
    SizeEnum ButtonSize { get; set; }            // ← was SizeEnum?
    ButtonStateEnum? ButtonState { get; set; }
    string DisabledText { get; set; }
    string PressedText { get; set; }
    string Text { get; set; }

    void Disabled();
    void Enabled();
    void Hide();
    void UpdateUI();
}

public static class TypeHelper
{
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
}

public abstract partial class ButtonBase : Button, IButtonBase
{
    #region Fields

    private bool _updateUI = false;

    public static readonly BindableProperty ButtonIconProperty = BindableProperty.Create(
        nameof(ButtonIcon),
        typeof(ButtonIconEnum?),
        typeof(ButtonBase),
        defaultValue: null,
        propertyChanged: (b, o, n) => ((ButtonBase)b).UpdateUI());

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

    public static readonly BindableProperty ButtonSizeProperty =
        BindableProperty.Create(
            nameof(ButtonSize),
            typeof(SizeEnum),
            typeof(ButtonBase),
            SizeEnum.Small,
            propertyChanged: OnButtonSizeChanged);

    #endregion Fields

    #region Ctor

    public ButtonBase(ButtonIconEnum buttonType) : base()
    {
        ButtonIcon = buttonType;
        base.PropertyChanged += Button_PropertyChanged;
        this.Loaded += async (_, __) =>
        {
            DLog($"[ButtonBase] Loaded -> calling SafeUpdateUIAsync. (Text='{Text}', Size={ButtonSize}, State={ButtonState})");
            await SafeUpdateUIAsync();
        };

        // Live size tracking (after layout)
        this.SizeChanged += (_, __) => DumpSize("SizeChanged");
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

    public SizeEnum ButtonSize
    {
        get => (SizeEnum)GetValue(ButtonSizeProperty);
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
            DLog($"[ButtonBase] PropertyChanged: {e.PropertyName}");
            UpdateUI();
        }
    }

    private void ReapplyLayout()
    {
        InvalidateMeasure();
        (Parent as VisualElement)?.InvalidateMeasure();
    }

    private Task _lastUiTask = Task.CompletedTask;

    private void ApplyVisual(bool isEnabled, bool isVisible, ButtonStateEnum stateForIcon)
        => _ = ApplyVisualAsync(isEnabled, isVisible, stateForIcon); // fire-and-forget but serialized below

    // stateful version
    private async Task ApplyVisualAsync(bool isEnabled, bool isVisible, ButtonStateEnum stateForIcon)
    {
        await this.WaitUntilReadyAsync();

        ApplyIconSizingToButton();

        _lastUiTask = _lastUiTask.ContinueWith(async _ =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                base.BatchBegin();

                base.IsEnabled = isEnabled;
                base.IsVisible = isVisible;

                var buttonH = ResolveButtonHeight(ButtonSize);
                var iconPx = ResolveIconSize(ButtonSize);
                DLog($"[ButtonBase] ApplyVisualAsync START state={stateForIcon}, buttonH={buttonH}, iconPx={iconPx}");

                // Button sizing
                base.HeightRequest = buttonH; // or MinimumHeightRequest = buttonH
                base.MinimumHeightRequest = Math.Max(base.MinimumHeightRequest, buttonH);
                base.MinimumWidthRequest = Math.Max(base.MinimumWidthRequest, iconPx);

                // Icon source
                if (ButtonIcon != null)
                {
                    base.ImageSource = new ResourceHelper().GetImageSource(
                        stateForIcon,
                        (ButtonIconEnum)ButtonIcon!,
                        ButtonSize);

                    if (base.ImageSource is FontImageSource fis)
                    {
                        fis.Size = iconPx;
                        DLog($"[ButtonBase] ApplyVisualAsync set FontImageSource.Size={fis.Size}");
                    }
                    else
                    {
                        DLog($"[ButtonBase] ApplyVisualAsync set Bitmap ImageSource (no intrinsic dp control).");
                    }
                }

                // Text selection
                string? text = stateForIcon switch
                {
                    ButtonStateEnum.Disabled when !string.IsNullOrWhiteSpace(DisabledText) => DisabledText,
                    ButtonStateEnum.Pressed when !string.IsNullOrWhiteSpace(PressedText) => PressedText,
                    _ => Text
                };

                var pos = base.ContentLayout.Position;
                if (!string.IsNullOrEmpty(text))
                {
                    base.Text = pos switch
                    {
                        ButtonContentLayout.ImagePosition.Right => text + "  ",
                        ButtonContentLayout.ImagePosition.Left => "  " + text,
                        _ => text
                    };
                }

                // Image + text spacing
                bool hasImage = ButtonIcon != null;
                bool hasText = !string.IsNullOrEmpty(base.Text);
                if (hasImage && hasText)
                {
                    base.ContentLayout = new ButtonContentLayout(pos, 0d);
                    Handler?.UpdateValue(nameof(Button.ContentLayout));
                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                        base.Padding = new Thickness(0);
                }

                base.BatchCommit();

                DumpSize("ApplyVisualAsync END");
            });
        }).Unwrap();
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

    private async Task SafeUpdateUIAsync()
    {
        await this.WaitUntilReadyAsync();
        DLog("[ButtonBase] SafeUpdateUIAsync -> UpdateUI()");
        UpdateUI();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();


        // Let MAUI side be tiny if you want it
        MinimumHeightRequest = 0;
        MinimumWidthRequest = 0;

#if WINDOWS
    if (Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement fe)
    {
        // WinUI enforces 44x44 by default – turn it off
        fe.MinHeight = 0;
        fe.MinWidth  = 0;
    }
#endif

#if ANDROID
        if (Handler?.PlatformView is Android.Views.View av)
        {
            av.SetMinimumHeight(0);
            av.SetMinimumWidth(0);
        }
#endif



        DLog($"[ButtonBase] OnHandlerChanged (Handler!=null -> {Handler != null})");
        if (Handler != null)
            _ = SafeUpdateUIAsync();
        ReapplyLayout();
#if IOS
        ApplyiOSButtonConfig();
#endif
        DumpSize("OnHandlerChanged");
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        DLog("[ButtonBase] OnParentSet -> SafeUpdateUIAsync()");
        _ = SafeUpdateUIAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        DLog($"[ButtonBase] OnSizeAllocated width={width:F1}, height={height:F1}");
        DumpSize("OnSizeAllocated");
    }

    private static void OnButtonSizeChanged(BindableObject b, object ov, object nv)
    {
        if (b is ButtonBase btn && nv is SizeEnum)
        {
            DLog($"[ButtonBase] OnButtonSizeChanged ov={ov}, nv={nv}");
            btn.ApplyIconSizingToButton();
            btn.ReapplyLayout();
            btn.DumpSize("OnButtonSizeChanged END");
        }
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

    private void ApplyIconSizingToButton()
    {
        var buttonH = ResolveButtonHeight(ButtonSize);
        var iconPx = ResolveIconSize(ButtonSize);
        DLog($"[ButtonBase] ApplyIconSizingToButton buttonH={buttonH}, iconPx={iconPx}");

        if (buttonH > 0)
        {
            if (HeightRequest < buttonH)
                HeightRequest = buttonH; // or set MinimumHeightRequest instead if you prefer

            MinimumHeightRequest = Math.Max(MinimumHeightRequest, buttonH);
        }
        MinimumWidthRequest = Math.Max(MinimumWidthRequest, iconPx);

        if (ImageSource is FontImageSource fis)
        {
            fis.Size = iconPx;
            // force invalidate on some platforms
            ImageSource = null;
            ImageSource = fis;
            DLog($"[ButtonBase] ApplyIconSizingToButton set FontImageSource.Size={fis.Size}");
        }

        DumpSize("ApplyIconSizingToButton END");
    }

    public void UpdateUI()
    {
        if (_updateUI) return;
        _updateUI = true;

        DLog($"[ButtonBase] UpdateUI ENTER (State={ButtonState}, Size={ButtonSize}, Text='{Text}')");

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

        ApplyIconSizingToButton();

        _updateUI = false;

        DumpSize("UpdateUI EXIT");
    }

    #endregion Public Methods

    #region Sizing helpers

    private double ResolveButtonHeight(SizeEnum size) =>
        DeviceHelper.GetImageSizeForDevice(size) * (DeviceInfo.Platform == DevicePlatform.WinUI ? 1.0 : 1.0);

    private double ResolveIconSize(SizeEnum size)
    {
        var h = DeviceHelper.GetImageSizeForDevice(size);
        return Math.Round(h * 0.55); // e.g., ~24 inside a 44 button
    }

    private void DumpSize(string where)
    {
        var isFontImg = ImageSource is FontImageSource;
        var fontImgSize = (ImageSource as FontImageSource)?.Size;

        DLog(
            $"[ButtonBase] {where} :: " +
            $"Text='{Text}', SizeEnum={ButtonSize}, State={ButtonState}, " +
            $"HeightReq={HeightRequest:F1}, MinH={MinimumHeightRequest:F1}, " +
            $"WidthReq={WidthRequest:F1}, MinW={MinimumWidthRequest:F1}, " +
            $"Actual=({Width:F1}x{Height:F1}), " +
            $"ImageSource={(ImageSource == null ? "null" : (isFontImg ? "FontImageSource" : "Bitmap"))}, " +
            $"FontImgSize={(isFontImg ? fontImgSize?.ToString("F1") : "-")}, " +
            $"ButtonH={ResolveButtonHeight(ButtonSize):F1}, IconPx={ResolveIconSize(ButtonSize):F1}, " +
            $"Platform={DeviceInfo.Platform}");
    }

    #endregion Sizing helpers

    [Conditional("DEBUG")]
    [Conditional("BUTTONBASE_TRACE")]
    private static void DLog(string message)
    {
#if BUTTONBASE_TRACE
        Debug.WriteLine(message);
#endif
    }

}

