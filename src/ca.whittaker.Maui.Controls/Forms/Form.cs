using ca.whittaker.Maui.Controls.Buttons;
using System.Diagnostics;
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
    private readonly SizeEnum DefaultButtonSize = SizeEnum.Normal;

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(Form));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Form));

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

    private static void OnFormAccessModeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is Form form && newValue is FormAccessModeEnum newAccessMode)
        {
            if (oldValue == null || !oldValue.Equals(newValue))
            {
                Debug.WriteLine($"[Form] : OnFormAccessModeChanged({newAccessMode.ToString()})");
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
        {
            UiThreadHelper.RunOnMainThread(() =>
            {
                if (form._formButtonSaveAction != null)
                    form._formButtonSaveAction.Text = newText;
            });
        }
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
        {
            if (form._formButtonCancelAction != null)
                UiThreadHelper.RunOnMainThread(() => { form._formButtonCancelAction.Text = newText; });
        }
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
    }

    private void FormEvaluateStatus()
    {
        if (_formStatusEvaluating)
            return;

        _formStatusEvaluating = true;
        Debug.WriteLine("[Form] : FormEvaluateStatus()");
        FormHasErrors = !FormFieldsCheckAreValid();
        FormHasChanges = !FormFieldsCheckArePristine();
        FormConfigButtonStates();
        _formStatusEvaluating = false;
    }

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
            Debug.WriteLine($"[Form] : FormFieldsWireUp() : {element.FieldLabelText.ToString()} : OnFieldHasChanges");
            Debug.WriteLine($"[Form] : FormFieldsWireUp() : {element.FieldLabelText.ToString()} : OnFieldHasValidationChanges");
            element.OnHasChanges += OnFieldHasChanges;
            element.OnHasValidationChanges += OnFieldHasValidationChanges;
        }
    }

    private void FormInitialize()
    {
        void _initializeUI()
        {
            _formButtonEditAction = new EditButton
            {
                Text = FormEditButtonText,
                ButtonSize = DefaultButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Enabled,
                ButtonIcon = ButtonIconEnum.Edit,
                IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
            };
            _formButtonEditAction.Clicked += OnFormEditButtonClicked;
            _formButtonEditAction.UpdateUI();

            _formButtonCancelAction = new CancelButton
            {
                Text = FormSaveButtonText,
                ButtonSize = DefaultButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Hidden,
                ButtonIcon = ButtonIconEnum.Save,
                IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
            };
            _formButtonCancelAction.Clicked += OnFormSaveButtonClicked;
            _formButtonCancelAction.UpdateUI();

            _formButtonSaveAction = new SaveButton
            {
                Text = FormCancelButtonText,
                ButtonSize = DefaultButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Hidden,
                ButtonIcon = ButtonIconEnum.Cancel,
                IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
            };
            _formButtonSaveAction.Clicked += OnFormCancelButtonClicked;
            _formButtonSaveAction.UpdateUI();

            _formLabelNotification = new Label
            {
                Text = "",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                TextColor = Colors.Red,
            };

            _formLabel = new Label
            {
                Text = FormName,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = !string.IsNullOrEmpty(FormName)
            };

            var gridLayout = new Grid
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Fill,
                Margin = new Thickness(5),
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            gridLayout.Add(_formLabel, 0, 0);
            gridLayout.Add(_formButtonCancelAction, 0, 1);
            gridLayout.Add(_formButtonSaveAction, 1, 1);
            gridLayout.Add(_formButtonEditAction, 0, 2);
            gridLayout.Add(_formLabelNotification, 0, 3);
            gridLayout.SetColumnSpan(_formLabel, 2);
            gridLayout.SetColumnSpan(_formLabelNotification, 2);
            gridLayout.SetColumnSpan(_formButtonEditAction, 2);

            if (Content is Layout existingLayout)
            {
                if (existingLayout.Children.Any())
                    existingLayout.Children.Insert(0, gridLayout);
                else
                    existingLayout.Children.Add(gridLayout);
            }
            else if (Content is View existingElement)
            {
                var containerLayout = new StackLayout();
                containerLayout.Children.Add(existingElement);
                containerLayout.Children.Add(gridLayout);
                Content = containerLayout;
            }
            else
            {
                Content = gridLayout;
            }
        }

        UiThreadHelper.RunOnMainThread(_initializeUI);
        FormFieldsWireUp();
    }

    private void OnFieldHasChanges(object? sender, HasChangesEventArgs e)
    {
        if (sender is IBaseFormField field)
        {
            Debug.WriteLine($"[Form] FieldHasChanges fired from: {field.FieldLabelText}, NewState: {e.HasChanged} {e}");
        }
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

    protected override void OnParentSet()
    {
        base.OnParentSet();
        FormInitialize();
        FormEvaluateStatus();
        OnFormAccessModeChanged(bindable: this, oldValue: null, newValue: FormAccessMode);
    }

    #endregion Protected Methods
}