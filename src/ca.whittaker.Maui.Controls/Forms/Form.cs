using ca.whittaker.Maui.Controls.Buttons;
using System.Diagnostics;
using System.Reflection;
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

    private SaveButton? _formButtonSaveAction;
    private EditButton? _formButtonEditAction;
    private CancelButton? _formButtonCancelAction;
    private Label? _formLabel;
    private Label? _formLabelNotification;
    private bool _formStatusEvaluating = false;

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

    private static ButtonSizeEnum ResolveEffective(SizeEnum requested)
        => DeviceHelper.GetButtonSizeForDevice(requested, enforceMinTouchTarget: true);

    // Sets ButtonSize whether the control expects SizeEnum or ButtonSizeEnum
    private static void ApplyButtonSizeSmart(object? button, SizeEnum requested)
    {
        if (button is null) return;

        var prop = button.GetType().GetProperty("ButtonSize", BindingFlags.Instance | BindingFlags.Public);
        if (prop is null || !prop.CanWrite) return;

        if (prop.PropertyType == typeof(SizeEnum))
        {
            // Control expects logical scale; just pass through
            prop.SetValue(button, requested);
        }
        else if (prop.PropertyType == typeof(ButtonSizeEnum))
        {
            // Control expects DIP bucket; resolve then assign
            prop.SetValue(button, ResolveEffective(requested));
        }

        // Nudge minimum touch height (platform-aware) for accessibility
        var hitDip = DeviceHelper.GetImageSizeForDevice(requested, enforceMinTouchTarget: true) + 6;
        if (button is VisualElement ve)
            ve.MinimumHeightRequest = hitDip;

        // Call UpdateUI if present
        var update = button.GetType().GetMethod("UpdateUI", BindingFlags.Instance | BindingFlags.Public);
        update?.Invoke(button, null);
    }

    private void ApplyAllButtons(SizeEnum requested)
    {
        ApplyButtonSizeSmart(_formButtonEditAction, requested);
        ApplyButtonSizeSmart(_formButtonCancelAction, requested);
        ApplyButtonSizeSmart(_formButtonSaveAction, requested);
    }
    private static void OnFormButtonSizeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Form form || newValue is not SizeEnum requested) return;
        UiThreadHelper.RunOnMainThread(() => form.ApplyAllButtons(requested));
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
            UiThreadHelper.RunOnMainThread(() => { form._formButtonCancelAction!.Text = newText; });
    }

    private static void OnFormEditButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
        {
            UiThreadHelper.RunOnMainThread(() =>
            {
                if (form._formButtonEditAction != null)
                    form._formButtonEditAction.Text = newText;
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
            UiThreadHelper.RunOnMainThread(() => { form._formButtonSaveAction!.Text = newText; });
    }


    private void FormClear()
    {
        foreach (IBaseFormField field in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
        {
            if (field is TextBoxField textBox)
                textBox.Clear();
            if (field is CheckBoxField checkBox)
                checkBox.Clear();
            field.Unfocus();
        }
    }

    private void FormConfigButtonStates()
    {
        if (FormAccessMode == FormAccessModeEnum.Editable)
        {
            // Editable: show only the edit button
            if (_formButtonCancelAction != null)
                _formButtonCancelAction.ButtonState = ButtonStateEnum.Hidden;
            if (_formButtonSaveAction != null)
                _formButtonSaveAction.ButtonState = ButtonStateEnum.Hidden;
            if (_formButtonEditAction != null)
                _formButtonEditAction.ButtonState = ButtonStateEnum.Enabled;
        }
        else if (FormAccessMode == FormAccessModeEnum.Editing)
        {
            // Editing: show save and cancel buttons
            if (_formButtonCancelAction != null)
                _formButtonCancelAction.ButtonState = FormHasChanges
                    ? (FormHasErrors ? ButtonStateEnum.Disabled : ButtonStateEnum.Enabled)
                    : ButtonStateEnum.Disabled;
            if (_formButtonSaveAction != null)
                _formButtonSaveAction.ButtonState = ButtonStateEnum.Enabled;
            if (_formButtonEditAction != null)
                _formButtonEditAction.ButtonState = ButtonStateEnum.Hidden;
        }
        else if (FormAccessMode == FormAccessModeEnum.ViewOnly || FormAccessMode == FormAccessModeEnum.Hidden)
        {
            // ViewOnly/Hidden: hide all buttons
            if (_formButtonCancelAction != null)
                _formButtonCancelAction.ButtonState = ButtonStateEnum.Hidden;
            if (_formButtonSaveAction != null)
                _formButtonSaveAction.ButtonState = ButtonStateEnum.Hidden;
            if (_formButtonEditAction != null)
                _formButtonEditAction.ButtonState = ButtonStateEnum.Hidden;
        }
        _formButtonCancelAction?.UpdateUI();
        _formButtonSaveAction?.UpdateUI();
        _formButtonEditAction?.UpdateUI();
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
        foreach (var field in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
        {
            if (field.FieldChangeState == ChangeStateEnum.Changed)
                return false;
        }
        return true;
    }

    private bool FormFieldsCheckAreValid() =>
                    this.GetVisualTreeDescendants().OfType<IBaseFormField>().All(field => field.FieldValidationState == ValidationStateEnum.Valid);

    /// <summary>
    /// puts form into "read" mode with edit button visible
    /// </summary>
    private void FormFieldsConfigAccessEditable()
    {
        foreach (IBaseFormField t in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
        {
            if (!t.FieldReadOnly)
                t.FieldAccessMode = FieldAccessModeEnum.Editable;
        }
    }

    /// <summary>
    /// puts form into "read/write" mode with save and cancel buttons visible
    /// </summary>
    private void FormFieldsConfigAccessEditing()
    {
        foreach (IBaseFormField t in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
            t.FieldAccessMode = FieldAccessModeEnum.Editing;
    }

    /// <summary>
    /// puts form into "hidden" mode with everything hidden
    /// </summary>
    private void FormFieldsConfigAccessHidden()
    {
        foreach (IBaseFormField t in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
            t.FieldAccessMode = FieldAccessModeEnum.Hidden;
    }

    /// <summary>
    /// puts form into "view only" mode with no buttons visible
    /// </summary>
    private void FormFieldsConfigViewOnlyMode()
    {
        foreach (IBaseFormField t in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
            t.FieldAccessMode = FieldAccessModeEnum.ViewOnly;
    }

    /// <summary>
    /// updates original value with current value, and sets form to readonly
    /// </summary>
    private void FormFieldsMarkAsSaved()
    {
        foreach (IBaseFormField t in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
            t.Save();
    }

    private void FormFieldsWireUp()
    {
        var formElements = this.GetVisualTreeDescendants().OfType<IBaseFormField>();
        if (!formElements.Any())
            throw new InvalidOperationException("Form missing controls");

        foreach (var element in formElements)
        {
            element.OnHasChanges += OnFieldHasChanges;
            element.OnHasValidationChanges += OnFieldHasValidationChanges;
        }
    }
    private Grid BuildHeaderGrid()
    {

        int dip = DeviceHelper.GetImageSizeForDevice(FormButtonSize, enforceMinTouchTarget: true);
        // or, if you specifically need the enum of DIPs:
        var dipEnum = ResolveEffective(FormButtonSize);

        // --- Buttons ---
        _formButtonEditAction = new EditButton
        {
            Text = FormEditButtonText,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Margin = 5,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            ButtonState = ButtonStateEnum.Enabled,
            ButtonIcon = ButtonIconEnum.Edit,
            IsVisible = true
        };
        _formButtonEditAction.Clicked += OnFormEditButtonClicked;
        _formButtonEditAction.UpdateUI();
        ApplyButtonSizeSmart(_formButtonEditAction, FormButtonSize);

        _formButtonSaveAction = new SaveButton
        {
            Text = FormSaveButtonText,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Margin = 5,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            ButtonState = ButtonStateEnum.Hidden,
            ButtonIcon = ButtonIconEnum.Save,
            IsVisible = true
        };
        _formButtonSaveAction.Clicked += OnFormSaveButtonClicked;
        _formButtonSaveAction.UpdateUI();
        ApplyButtonSizeSmart(_formButtonSaveAction, FormButtonSize);

        _formButtonCancelAction = new CancelButton
        {
            Text = FormCancelButtonText,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Margin = 5,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            ButtonState = ButtonStateEnum.Hidden,
            ButtonIcon = ButtonIconEnum.Cancel,
            IsVisible = true
        };
        _formButtonCancelAction.Clicked += OnFormCancelButtonClicked;
        _formButtonCancelAction.UpdateUI();
        ApplyButtonSizeSmart(_formButtonCancelAction, FormButtonSize);

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
            VerticalOptions = LayoutOptions.Fill,
            
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
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
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
                    var wrapper = new VerticalStackLayout { Spacing = 0, Padding = 0 };
                    Content = wrapper;                 // this detaches 'existing' from ContentView
                    wrapper.Children.Add(header);
                    wrapper.Children.Add(existing);
                }
            }
            else if (Content is View singleView)
            {
                // wrap a lone view
                var wrapper = new VerticalStackLayout { Spacing = 0, Padding = 0 };
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
        foreach (var field in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
                field.Undo();

        // set form to editable state
        if (FormAccessMode != FormAccessModeEnum.Editable)
            FormAccessMode = FormAccessModeEnum.Editable;


    }

    private void OnFormEditButtonClicked(object? sender, EventArgs e)
    {
        if (FormAccessMode != FormAccessModeEnum.Editing)
            FormAccessMode = FormAccessModeEnum.Editing;
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
        FormAccessMode = FormAccessModeEnum.Editable;

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
        foreach (var field in this.GetVisualTreeDescendants().OfType<IBaseFormField>())
            field.Refresh();
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