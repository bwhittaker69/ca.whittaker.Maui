using System;
using ca.whittaker.Maui.Controls.Buttons;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a standardized contract for form field controls used inside a Form.<br/>
/// <br/>
/// <b>Core Responsibilities:</b><br/>
/// - Exposes field-specific state (Access Mode, Validation, Change Tracking).<br/>
/// - Supports undo/reset behavior.<br/>
/// - Supports dynamic layout updates (label width, control width).<br/>
/// - Raises events (`FieldHasChanges`, `FieldHasValidationChanges`) to notify the parent Form.<br/>
/// - Allows the parent Form to control field focus/unfocus, clear, undo, and save operations.<br/>
/// <br/>
/// <b>Key Properties:</b><br/>
/// - `FieldAccessMode`: Determines if the field is editable, editing, view-only, or hidden.<br/>
/// - `FieldChangeState`: Tracks whether the field has unsaved changes.<br/>
/// - `FieldValidationState`: Tracks whether the field's current value is valid.<br/>
/// <br/>
/// <b>Key Methods:</b><br/>
/// - `Field_Clear()`: Clears the field.<br/>
/// - `Field_UndoValue()`: Reverts to original value.<br/>
/// - `Field_SaveAndMarkAsReadOnly()`: Commits current value and locks the field.<br/>
/// - `Field_NotifyHasChanges()` and `Field_NotifyValidationChanges()`: Raise change/validation events.<br/>
/// <br/>
/// <b>Implementation Note:</b><br/>
/// - Typically implemented by a base control such as `BaseFormField&lt;T&gt;`.<br/>
/// </summary>
public class Form : ContentView
{
    #region Fields
    public Form()
    {
        base.HorizontalOptions = LayoutOptions.Fill;
        base.VerticalOptions = LayoutOptions.Start; // size to content
    }
    private SaveButton? _formButtonSaveAction;
    private EditButton? _formButtonEditAction;
    private CancelButton? _formButtonCancelAction;
    private Label? _formLabel;
    private Label? _formLabelNotification;
    private bool _formStatusEvaluating;

    private static readonly Thickness _buttonMargin = new(5);

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(Form));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Form));

    public static readonly BindableProperty FormButtonSizeProperty = BindableProperty.Create(
        nameof(FormButtonSize),
        typeof(SizeEnum),
        typeof(Form),
        defaultValue: SizeEnum.Small,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormButtonSizeChanged);

    public static readonly BindableProperty FormAccessModeProperty = BindableProperty.Create(
        nameof(FormAccessMode),
        typeof(FormAccessModeEnum),
        typeof(Form),
        defaultValue: FormAccessModeEnum.Editable,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormAccessModeChanged);

    public static readonly BindableProperty FormCancelButtonTextProperty = BindableProperty.Create(
        nameof(FormCancelButtonText),
        typeof(string),
        typeof(Form),
        defaultValue: "cancel",
        propertyChanged: OnFormCancelButtonTextChanged);

    public static readonly BindableProperty FormEditButtonTextProperty = BindableProperty.Create(
        nameof(FormEditButtonText),
        typeof(string),
        typeof(Form),
        defaultValue: "edit",
        propertyChanged: OnFormEditButtonTextChanged);

    public static readonly BindableProperty FormHasChangesProperty = BindableProperty.Create(
        nameof(FormHasChanges),
        typeof(bool),
        typeof(Form),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly BindableProperty FormHasErrorsProperty = BindableProperty.Create(
        nameof(FormHasErrors),
        typeof(bool),
        typeof(Form),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly BindableProperty FormNameProperty = BindableProperty.Create(
        nameof(FormName),
        typeof(string),
        typeof(Form),
        defaultValue: "",
        propertyChanged: OnFormNameChanged);

    public static readonly BindableProperty FormSaveButtonTextProperty = BindableProperty.Create(
        nameof(FormSaveButtonText),
        typeof(string),
        typeof(Form),
        defaultValue: "save",
        propertyChanged: OnFormSaveButtonTextChanged,
        defaultBindingMode: BindingMode.TwoWay);

    #endregion Fields

    #region Events

    // Declare the event to notify subscribers when the form is saved.
    //public event EventHandler<EventArgs>? FormSaving;
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

    private static void ApplyButtonSizing(ButtonBase? button, SizeEnum requested)
    {
        if (button is null) return;

        button.EnforceMinTouchTarget = true;
        button.ButtonSize = requested;

        var targetHeight = DeviceHelper.GetImageSizeForDevice(requested, enforceMinTouchTarget: true);
        button.HeightRequest = targetHeight;
        button.MinimumHeightRequest = Math.Max(button.MinimumHeightRequest, Math.Max(targetHeight, 44));

        var targetWidth = Math.Max(button.MinimumWidthRequest, targetHeight * 2);
        button.MinimumWidthRequest = targetWidth;
        if (button.WidthRequest <= 0 || button.WidthRequest < targetWidth)
            button.WidthRequest = targetWidth;

        button.UpdateUI();
    }

    private void ApplyAllButtonSizing()
    {
        ApplyButtonSizing(_formButtonEditAction, FormButtonSize);
        ApplyButtonSizing(_formButtonCancelAction, FormButtonSize);
        ApplyButtonSizing(_formButtonSaveAction, FormButtonSize);
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

    private static void UpdateButtonState(ButtonBase? button, ButtonStateEnum state)
    {
        if (button is null) return;
        if (button.ButtonState != state)
            button.ButtonState = state;
        button.UpdateUI();
    }

    private static void UpdateButtonText(ButtonBase? button, string text)
    {
        if (button is null) return;
        button.Text = text;
        button.UpdateUI();
    }

    private TButton CreateActionButton<TButton>(string text, ButtonIconEnum icon, EventHandler handler, ButtonStateEnum initialState)
        where TButton : ButtonBase, IButtonBase, new()
    {
        var button = new TButton
        {
            Text = text,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Margin = _buttonMargin,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            ButtonState = initialState,
            ButtonIcon = icon,
            IsVisible = true
        };

        button.Clicked += handler;
        ApplyButtonSizing(button, FormButtonSize);
        return button;
    }
    private static void OnFormButtonSizeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Form form || newValue is not SizeEnum requested) return;
        UiThreadHelper.RunOnMainThread(form.ApplyAllButtonSizing);
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

    private static void OnFormCancelButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
            UiThreadHelper.RunOnMainThread(() => UpdateButtonText(form._formButtonCancelAction, newText));
    }

    private static void OnFormEditButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
        {
            UiThreadHelper.RunOnMainThread(() =>
            {
                UpdateButtonText(form._formButtonEditAction, newText);
            });
        }
    }

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

    private static void OnFormSaveButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
            UiThreadHelper.RunOnMainThread(() => UpdateButtonText(form._formButtonSaveAction, newText));
    }


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
                UpdateButtonState(_formButtonCancelAction, ButtonStateEnum.Hidden);
                UpdateButtonState(_formButtonSaveAction, ButtonStateEnum.Hidden);
                UpdateButtonState(_formButtonEditAction, ButtonStateEnum.Enabled);
                break;

            case FormAccessModeEnum.Editing:
                var cancelState = FormHasChanges
                    ? (FormHasErrors ? ButtonStateEnum.Disabled : ButtonStateEnum.Enabled)
                    : ButtonStateEnum.Disabled;

                UpdateButtonState(_formButtonCancelAction, cancelState);
                UpdateButtonState(_formButtonSaveAction, ButtonStateEnum.Enabled);
                UpdateButtonState(_formButtonEditAction, ButtonStateEnum.Hidden);
                break;

            case FormAccessModeEnum.ViewOnly:
            case FormAccessModeEnum.Hidden:
                UpdateButtonState(_formButtonCancelAction, ButtonStateEnum.Hidden);
                UpdateButtonState(_formButtonSaveAction, ButtonStateEnum.Hidden);
                UpdateButtonState(_formButtonEditAction, ButtonStateEnum.Hidden);
                break;
        }
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
    {
        return EnumerateFields().All(field => field.FieldChangeState != ChangeStateEnum.Changed);
    }

    private bool FormFieldsCheckAreValid() =>
        EnumerateFields().All(field => field.FieldValidationState == ValidationStateEnum.Valid);

    /// <summary>
    /// puts form into "read" mode with edit button visible
    /// </summary>
    private void FormFieldsConfigAccessEditable()
    {
        ForEachField(field =>
        {
            if (!field.FieldReadOnly)
                field.FieldAccessMode = FieldAccessModeEnum.Editable;
        });
    }

    /// <summary>
    /// puts form into "read/write" mode with save and cancel buttons visible
    /// </summary>
    private void FormFieldsConfigAccessEditing()
    {
        ForEachField(field => field.FieldAccessMode = FieldAccessModeEnum.Editing);
    }

    /// <summary>
    /// puts form into "hidden" mode with everything hidden
    /// </summary>
    private void FormFieldsConfigAccessHidden()
    {
        ForEachField(field => field.FieldAccessMode = FieldAccessModeEnum.Hidden);
    }

    /// <summary>
    /// puts form into "view only" mode with no buttons visible
    /// </summary>
    private void FormFieldsConfigViewOnlyMode()
    {
        ForEachField(field => field.FieldAccessMode = FieldAccessModeEnum.ViewOnly);
    }

    /// <summary>
    /// updates original value with current value, and sets form to readonly
    /// </summary>
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
        _formButtonEditAction = CreateActionButton<EditButton>(
            FormEditButtonText,
            ButtonIconEnum.Edit,
            OnFormEditButtonClicked,
            ButtonStateEnum.Enabled);

        _formButtonSaveAction = CreateActionButton<SaveButton>(
            FormSaveButtonText,
            ButtonIconEnum.Save,
            OnFormSaveButtonClicked,
            ButtonStateEnum.Hidden);

        _formButtonCancelAction = CreateActionButton<CancelButton>(
            FormCancelButtonText,
            ButtonIconEnum.Cancel,
            OnFormCancelButtonClicked,
            ButtonStateEnum.Hidden);

        // --- Labels ---
        _formLabelNotification = new Label
        {
            Text = string.Empty,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false,
            TextColor = Colors.Red
        };

        _formLabel = new Label
        {
            Text = FormName,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = !string.IsNullOrEmpty(FormName)
        };
        
        // --- Header Grid layout ---
        var header = new Grid
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            ColumnSpacing = 10,
            RowSpacing = 6,
            Margin = new Thickness(5),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // 0: title
                new RowDefinition { Height = GridLength.Auto }, // 1: buttons row (save/cancel)
                new RowDefinition { Height = GridLength.Auto }, // 2: edit button row
                new RowDefinition { Height = GridLength.Auto }  // 3: notification
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Row 0: title (span both columns)
        header.Add(_formLabel, 0, 0);
        Grid.SetColumnSpan(_formLabel, 2);

        // Row 1: left = cancel, right = save
        header.Add(_formButtonCancelAction, 0, 1);
        header.Add(_formButtonSaveAction, 1, 1);

        // Row 2: edit button centered, span both columns
        header.Add(_formButtonEditAction, 0, 2);
        Grid.SetColumnSpan(_formButtonEditAction, 2);

        // Row 3: notification (span both columns)
        header.Add(_formLabelNotification, 0, 3);
        Grid.SetColumnSpan(_formLabelNotification, 2);

        // reflect current state
        FormConfigButtonStates();

        return header;
    }


    private void FormInitialize()
    {
        void _initializeUI()
        {
            var header = BuildHeaderGrid(); // your existing header grid creation

            if (Content is Layout existing)
            {
                if (IsVerticalStack(existing))
                {
                    // just insert header at the top
                    if (existing.Children.Any())
                        existing.Children.Insert(0, header);
                    else
                        existing.Children.Add(header);
                }
                else
                {
                    // wrap non-stacking layouts (like Grid) with a vertical stack
                    var wrapper = new VerticalStackLayout { Spacing = 0, Padding = 0, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Start };
                    Content = wrapper;                 // this detaches 'existing' from ContentView
                    wrapper.Children.Add(header);
                    wrapper.Children.Add(existing);
                }
            }
            else if (Content is View singleView)
            {
                // wrap a lone view
                var wrapper = new VerticalStackLayout { Spacing = 0, Padding = 0, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Start };
                Content = wrapper;                     // detaches 'singleView' from ContentView
                wrapper.Children.Add(header);
                wrapper.Children.Add(singleView);
            }
            else
            {
                // no content yet — create a stack with just the header
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

    private void OnFieldHasChanges(object? sender, HasChangesEventArgs e)
    {
        FormEvaluateStatus();
    }

    private void OnFieldHasValidationChanges(object? sender, ValidationDataChangesEventArgs e) => FormEvaluateStatus();

    private void OnFormCancelButtonClicked(object? sender, EventArgs e)
    {
        // undo any changes to all fields
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

        //
        // loop over each field, set original value to datasource value
        //
        if (hasChanges)
            FormFieldsMarkAsSaved();

        //
        // set form state to "editable"
        //
        SetFormAccessMode(FormAccessModeEnum.Editable);

        // Raise the FormSaved event.
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


    // Protected virtual method to raise the event.
    protected virtual void OnFormSaved(FormSavedEventArgs e)
    {
        FormSaved?.Invoke(this, e);

        if (SaveCommand?.CanExecute(SaveCommandParameter) == true)
            SaveCommand.Execute(SaveCommandParameter);

    }
    /// <summary>
    /// Recursively calls Refresh() on all fields in the form.
    /// </summary>
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
        // Hit everything that already exists
        foreach (var v in this.GetVisualTreeDescendants().OfType<View>())
            MinSizeHelper.ClearMinimumsRecursively(v);         // calls your OnHandlerChanged logic per control

        // And anything that appears later
        this.DescendantAdded += (_, e) =>
        {
            if (e.Element is View v)
                MinSizeHelper.ClearMinimumsRecursively(v);
        };
    }


    #endregion Protected Methods
}
