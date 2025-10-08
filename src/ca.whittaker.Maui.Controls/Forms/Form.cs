using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ca.whittaker.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Form container with standardized field orchestration (access mode, change/validation tracking),
/// compact header using icon images (edit/save/cancel), and size-to-content layout.
/// </summary>
public class Form : ContentView
{
    #region Fields

    public Form()
    {
        base.HorizontalOptions = LayoutOptions.Fill;
        base.VerticalOptions = LayoutOptions.Start; // size to content
    }

    // Compact icon controls
    private Image? _imgEdit, _imgSave, _imgCancel;
    private Grid? _headerGrid;

    private Label? _formLabel;
    private Label? _formLabelNotification;
    private bool _formStatusEvaluating;

    private static readonly Thickness _iconMargin = new(6);

    // Bindables
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(Form));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(Form));

    // Kept for compatibility (not used by icon header)
    public static readonly BindableProperty FormButtonSizeProperty = BindableProperty.Create(
        nameof(FormButtonSize), typeof(SizeEnum), typeof(Form),
        defaultValue: SizeEnum.Small,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormButtonSizeChanged);

    public static readonly BindableProperty FormAccessModeProperty = BindableProperty.Create(
        nameof(FormAccessMode), typeof(FormAccessModeEnum), typeof(Form),
        defaultValue: FormAccessModeEnum.Editable,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormAccessModeChanged);

    public static readonly BindableProperty FormCancelButtonTextProperty = BindableProperty.Create(
        nameof(FormCancelButtonText), typeof(string), typeof(Form),
        defaultValue: "cancel",
        propertyChanged: OnFormCancelButtonTextChanged);

    public static readonly BindableProperty FormEditButtonTextProperty = BindableProperty.Create(
        nameof(FormEditButtonText), typeof(string), typeof(Form),
        defaultValue: "edit",
        propertyChanged: OnFormEditButtonTextChanged);

    public static readonly BindableProperty FormHasChangesProperty = BindableProperty.Create(
        nameof(FormHasChanges), typeof(bool), typeof(Form),
        defaultValue: false, defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly BindableProperty FormHasErrorsProperty = BindableProperty.Create(
        nameof(FormHasErrors), typeof(bool), typeof(Form),
        defaultValue: false, defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly BindableProperty FormNameProperty = BindableProperty.Create(
        nameof(FormName), typeof(string), typeof(Form),
        defaultValue: "", propertyChanged: OnFormNameChanged);

    public static readonly BindableProperty FormSaveButtonTextProperty = BindableProperty.Create(
        nameof(FormSaveButtonText), typeof(string), typeof(Form),
        defaultValue: "save",
        propertyChanged: OnFormSaveButtonTextChanged,
        defaultBindingMode: BindingMode.TwoWay);

    #endregion Fields

    #region Events

    public event EventHandler<FormSavedEventArgs>? FormSaved;

    #endregion Events

    #region Properties

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public FormAccessModeEnum FormAccessMode
    {
        get => (FormAccessModeEnum)GetValue(FormAccessModeProperty);
        set => SetValue(FormAccessModeProperty, value);
    }

    // Retained for XAML compatibility; not used by icon header
    public SizeEnum FormButtonSize
    {
        get => (SizeEnum)GetValue(FormButtonSizeProperty);
        set => SetValue(FormButtonSizeProperty, value);
    }

    public string FormCancelButtonText
    {
        get => (string)GetValue(FormCancelButtonTextProperty);
        set => SetValue(FormCancelButtonTextProperty, value);
    }

    public string FormEditButtonText
    {
        get => (string)GetValue(FormEditButtonTextProperty);
        set => SetValue(FormEditButtonTextProperty, value);
    }

    public bool FormHasChanges
    {
        get => (bool)GetValue(FormHasChangesProperty);
        set => SetValue(FormHasChangesProperty, value);
    }

    public bool FormHasErrors
    {
        get => (bool)GetValue(FormHasErrorsProperty);
        set => SetValue(FormHasErrorsProperty, value);
    }

    public string FormName
    {
        get => (string)GetValue(FormNameProperty);
        set => SetValue(FormNameProperty, value);
    }

    public string FormSaveButtonText
    {
        get => (string)GetValue(FormSaveButtonTextProperty);
        set => SetValue(FormSaveButtonTextProperty, value);
    }

    public new LayoutOptions HorizontalOptions
    {
        get => base.HorizontalOptions;
        set => base.HorizontalOptions = value;
    }

    public new LayoutOptions VerticalOptions
    {
        get => base.VerticalOptions;
        set => base.VerticalOptions = value;
    }

    #endregion Properties

    #region Private Methods

    private static readonly BindableProperty IconTypeProperty = BindableProperty.CreateAttached(
        "FormIconType",
        typeof(ButtonIconEnum?),
        typeof(Form),
        defaultValue: null);

    private static void SetIconMetadata(Image image, ButtonIconEnum icon)
        => image.SetValue(IconTypeProperty, icon);

    private static ButtonIconEnum? GetIconMetadata(Image image)
        => (ButtonIconEnum?)image.GetValue(IconTypeProperty);

    private Image MakeIcon(ButtonIconEnum icon, EventHandler onTapped)
    {
        var img = new Image
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = _iconMargin,
            Aspect = Aspect.AspectFit
        };

        SetIconMetadata(img, icon);
        UpdateIconImage(img, ButtonStateEnum.Enabled);

        var tap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        tap.Tapped += (_, __) => onTapped(img, EventArgs.Empty);
        img.GestureRecognizers.Add(tap);

        SemanticProperties.SetDescription(img, icon.ToString().ToLowerInvariant());
        return img;
    }

    private void UpdateIconImage(Image image, ButtonStateEnum state)
    {
        var icon = GetIconMetadata(image);
        if (icon is null)
            return;

        using var helper = new ResourceHelper();
        var asset = helper.GetImageAsset(state, icon.Value, FormButtonSize);

        if (asset is not null)
        {
            image.Source = asset.Source;
            image.WidthRequest = asset.DipSize;
            image.HeightRequest = asset.DipSize;
            image.MinimumWidthRequest = asset.DipSize;
            image.MinimumHeightRequest = asset.DipSize;
        }
        else
        {
            const double fallback = 24; // keep layout stable if resource missing
            image.Source = $"{icon.Value.ToString().ToLowerInvariant()}.png";
            image.WidthRequest = fallback;
            image.HeightRequest = fallback;
            image.MinimumWidthRequest = fallback;
            image.MinimumHeightRequest = fallback;
        }
    }

    private IEnumerable<IBaseFormField> EnumerateFields()
        => this.GetVisualTreeDescendants().OfType<IBaseFormField>();

    private void ForEachField(Action<IBaseFormField> action)
    {
        foreach (var field in EnumerateFields())
            action(field);
    }

    private void SetFormAccessMode(FormAccessModeEnum accessMode)
    {
        if (FormAccessMode != accessMode)
            FormAccessMode = accessMode;
    }

    private void RequestRelayout()
    {
        _headerGrid?.InvalidateMeasure();
        (_headerGrid?.Parent as VisualElement)?.InvalidateMeasure();

        this.InvalidateMeasure();
        (this.Parent as VisualElement)?.InvalidateMeasure();
    }

    private void SetIconState(Image? img, bool visible, bool enabled)
    {
        if (img is null) return;

        var state = enabled ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled;
        UpdateIconImage(img, state);

        img.IsVisible = visible;
        img.Opacity = enabled ? 1.0 : 0.4;
        img.InputTransparent = !enabled; // disables tap when “disabled”
        img.InvalidateMeasure();
    }

    // Refresh icon layout when host toggles FormButtonSize (retained for XAML compatibility)
    private static void OnFormButtonSizeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is Form form)
            form.FormConfigButtonStates();
    }

    private static void OnFormAccessModeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is Form form && newValue is FormAccessModeEnum newAccessMode)
        {
            if (oldValue == null || !oldValue.Equals(newValue))
            {
                switch (newAccessMode)
                {
                    case FormAccessModeEnum.Editable:
                        form.FormFieldsConfigAccessEditable();
                        break;

                    case FormAccessModeEnum.Editing:
                        form.FormFieldsConfigAccessEditing();
                        break;

                    case FormAccessModeEnum.ViewOnly:
                        form.FormFieldsConfigViewOnlyMode();
                        break;

                    case FormAccessModeEnum.Hidden:
                        form.FormFieldsConfigAccessHidden();
                        break;
                }
                form.FormConfigButtonStates();
            }
        }
    }

    // Text changes are irrelevant for icon-only header; keep for compatibility
    private static void OnFormCancelButtonTextChanged(BindableObject bindable, object oldValue, object newValue) { }
    private static void OnFormEditButtonTextChanged(BindableObject bindable, object oldValue, object newValue) { }
    private static void OnFormNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newName)
        {
            if (form._formLabel != null)
                UiThreadHelper.RunOnMainThread(() =>
                {
                    form._formLabel.Text = newName;
                    form._formLabel.IsVisible = !string.IsNullOrEmpty(newName);
                });
        }
    }
    private static void OnFormSaveButtonTextChanged(BindableObject bindable, object oldValue, object newValue) { }

    private void FormClear()
    {
        ForEachField(field =>
        {
            switch (field)
            {
                case TextBoxField textBox:
                    textBox.Clear();
                    break;
                case CheckBoxField checkBox:
                    checkBox.Clear();
                    break;
            }

            field.Unfocus();
        });
    }

    private void FormConfigButtonStates()
    {
        switch (FormAccessMode)
        {
            case FormAccessModeEnum.Editable:
                SetIconState(_imgCancel, visible: false, enabled: false);
                SetIconState(_imgSave, visible: false, enabled: false);
                SetIconState(_imgEdit, visible: true, enabled: true);
                break;

            case FormAccessModeEnum.Editing:
                var canCancel = FormHasChanges && !FormHasErrors;
                SetIconState(_imgCancel, visible: true, enabled: canCancel);
                SetIconState(_imgSave, visible: true, enabled: true);
                SetIconState(_imgEdit, visible: false, enabled: false);
                break;

            default:
                SetIconState(_imgCancel, visible: false, enabled: false);
                SetIconState(_imgSave, visible: false, enabled: false);
                SetIconState(_imgEdit, visible: false, enabled: false);
                break;
        }

        RequestRelayout();
    }

    private void FormEvaluateStatus()
    {
        if (_formStatusEvaluating)
            return;

        _formStatusEvaluating = true;
        FormHasErrors = !FormFieldsCheckAreValid();
        FormHasChanges = !FormFieldsCheckArePristine();
        FormConfigButtonStates();
        _formStatusEvaluating = false;
    }

    private static bool IsVerticalStack(Layout layout) =>
        layout is VerticalStackLayout ||
        (layout is StackLayout s && s.Orientation == StackOrientation.Vertical);

    private bool FormFieldsCheckArePristine()
        => EnumerateFields().All(field => field.FieldChangeState != ChangeStateEnum.Changed);

    private bool FormFieldsCheckAreValid()
        => EnumerateFields().All(field => field.FieldValidationState == ValidationStateEnum.Valid);

    private void FormFieldsConfigAccessEditable()
    {
        ForEachField(field =>
        {
            if (!field.FieldReadOnly)
                field.FieldAccessMode = FieldAccessModeEnum.Editable;
        });
    }

    private void FormFieldsConfigAccessEditing()
    {
        ForEachField(field => field.FieldAccessMode = FieldAccessModeEnum.Editing);
    }

    private void FormFieldsConfigAccessHidden()
    {
        ForEachField(field => field.FieldAccessMode = FieldAccessModeEnum.Hidden);
    }

    private void FormFieldsConfigViewOnlyMode()
    {
        ForEachField(field => field.FieldAccessMode = FieldAccessModeEnum.ViewOnly);
    }

    private void FormFieldsMarkAsSaved()
    {
        ForEachField(field => field.Save());
    }

    private void FormFieldsWireUp()
    {
        var fields = EnumerateFields().ToList();
        if (!fields.Any())
            throw new InvalidOperationException("Form missing controls");

        foreach (var field in fields)
        {
            field.OnHasChanges += OnFieldHasChanges;
            field.OnHasValidationChanges += OnFieldHasValidationChanges;
        }
    }

    private Grid BuildHeaderGrid()
    {
        _imgEdit = MakeIcon(ButtonIconEnum.Edit, OnFormEditButtonClicked);
        _imgSave = MakeIcon(ButtonIconEnum.Save, OnFormSaveButtonClicked);
        _imgCancel = MakeIcon(ButtonIconEnum.Cancel, OnFormCancelButtonClicked);

        var header = new Grid
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            ColumnSpacing = 10,
            RowSpacing = 6,
            Margin = new Thickness(5),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // 0 title
                new RowDefinition { Height = GridLength.Auto }, // 1 cancel|save
                new RowDefinition { Height = GridLength.Auto }, // 2 edit
                new RowDefinition { Height = GridLength.Auto }  // 3 notification
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        _formLabelNotification ??= new Label
        {
            Text = string.Empty,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false,
            TextColor = Colors.Red
        };

        _formLabel ??= new Label
        {
            Text = FormName,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = !string.IsNullOrEmpty(FormName)
        };

        // Row 0: title
        header.Add(_formLabel, 0, 0);
        Grid.SetColumnSpan(_formLabel, 2);

        // Row 1: cancel | save
        header.Add(_imgCancel, 0, 1);
        header.Add(_imgSave, 1, 1);

        // Row 2: edit (centered)
        header.Add(_imgEdit, 0, 2);
        Grid.SetColumnSpan(_imgEdit, 2);

        // Row 3: notification
        header.Add(_formLabelNotification, 0, 3);
        Grid.SetColumnSpan(_formLabelNotification, 2);

        _headerGrid = header;
        FormConfigButtonStates();
        return header;
    }

    private void FormInitialize()
    {
        void _initializeUI()
        {
            var header = BuildHeaderGrid();

            if (Content is Layout existing)
            {
                if (IsVerticalStack(existing))
                {
                    if (existing.Children.Any())
                        existing.Children.Insert(0, header);
                    else
                        existing.Children.Add(header);
                }
                else
                {
                    var wrapper = new VerticalStackLayout
                    {
                        Spacing = 0,
                        Padding = 0,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Start
                    };
                    Content = wrapper;
                    wrapper.Children.Add(header);
                    wrapper.Children.Add(existing);
                }
            }
            else if (Content is View singleView)
            {
                var wrapper = new VerticalStackLayout
                {
                    Spacing = 0,
                    Padding = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start
                };
                Content = wrapper;
                wrapper.Children.Add(header);
                wrapper.Children.Add(singleView);
            }
            else
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Padding = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    Children = { header }
                };
            }
        }

        UiThreadHelper.RunOnMainThread(_initializeUI);
        FormFieldsWireUp();
        ApplyMinSizeFixes();
    }

    private void OnFieldHasChanges(object? sender, HasChangesEventArgs e) => FormEvaluateStatus();
    private void OnFieldHasValidationChanges(object? sender, ValidationDataChangesEventArgs e) => FormEvaluateStatus();

    private void OnFormCancelButtonClicked(object? sender, EventArgs e)
    {
        ForEachField(field => field.Undo());
        SetFormAccessMode(FormAccessModeEnum.Editable);
    }

    private void OnFormEditButtonClicked(object? sender, EventArgs e)
    {
        SetFormAccessMode(FormAccessModeEnum.Editing);
    }

    private void OnFormSaveButtonClicked(object? sender, EventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);

        bool hasChanges = !FormFieldsCheckArePristine();
        if (hasChanges)
            FormFieldsMarkAsSaved();

        SetFormAccessMode(FormAccessModeEnum.Editable);
        OnFormSaved(new FormSavedEventArgs(hasChanges));
    }

    #endregion Private Methods

    #region Protected Methods

    public static readonly BindableProperty SaveCommandProperty =
        BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(Form));

    public static readonly BindableProperty SaveCommandParameterProperty =
        BindableProperty.Create(nameof(SaveCommandParameter), typeof(object), typeof(Form));

    public ICommand SaveCommand
    {
        get => (ICommand)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public object SaveCommandParameter
    {
        get => GetValue(SaveCommandParameterProperty);
        set => SetValue(SaveCommandParameterProperty, value);
    }

    protected virtual void OnFormSaved(FormSavedEventArgs e)
    {
        FormSaved?.Invoke(this, e);

        if (SaveCommand?.CanExecute(SaveCommandParameter) == true)
            SaveCommand.Execute(SaveCommandParameter);
    }

    /// <summary>Recursively calls Refresh() on all fields in the form.</summary>
    public void RefreshAllFields()
    {
        ForEachField(field => field.Refresh());
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        FormInitialize();
        FormEvaluateStatus();
        OnFormAccessModeChanged(bindable: this, oldValue: null, newValue: FormAccessMode);
    }

    private void ApplyMinSizeFixes()
    {
        foreach (var v in this.GetVisualTreeDescendants().OfType<View>())
            MinSizeHelper.ClearMinimumsRecursively(v);

        this.DescendantAdded += (_, e) =>
        {
            if (e.Element is View v)
                MinSizeHelper.ClearMinimumsRecursively(v);
        };
    }

    #endregion Protected Methods
}
