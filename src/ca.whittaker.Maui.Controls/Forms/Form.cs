using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

using ca.whittaker.Maui.Controls.Buttons;
using System.Windows.Input;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a form control within a MAUI application, providing integrated state management,
/// validation, and control wiring for a composite user input interface.
///
/// This control aggregates multiple child controls derived from <c>BaseFormElement</c> (such as
/// <c>TextBoxElement</c> and <c>CheckBoxElement</c>) and coordinates their change and validation
/// states to update the overall form status. It exposes properties for command handling, form
/// state (enabled, disabled, hidden, etc.), and button texts for save/cancel actions.
///
/// Form Layout Overview:
/// - A header label displays the form's title (if specified).
/// - Two buttons (Save and Cancel) are provided for form submission and cancellation.
/// - A notification label is used to show validation errors or messages.
/// - Child controls (e.g., text boxes and checkboxes) are added to the visual tree of the form,
///   and their states are monitored to determine if the form data has changed or contains errors.
///
/// Use Case Examples:
/// - A User Profile Form:
///     • <c>TextBoxElement</c> is used to capture fields like the user's name and email.
///     • <c>CheckBoxElement</c> captures boolean options, such as "Is Public".
///     • The form aggregates these elements, enabling the Save button only when changes occur
///       and all inputs are valid.
/// - A Settings Form:
///     • Multiple input controls capture configuration options.
///     • The Undo functionality on each child control allows the user to revert individual changes
///       before submitting the form.
///
/// The control automatically wires up events from its child controls (for detecting changes and validation
/// errors) and updates its bindable properties (e.g., <c>FormHasChanges</c>, <c>FormHasErrors</c>, <c>FormState</c>)
/// accordingly. This ensures a consistent and responsive form experience.
/// </summary>
public class Form : ContentView
{

    #region Private Fields

    private SaveButton? _formButtonCancel;

    private EditButton? _formButtonEdit;

    private CancelButton? _formButtonSave;

    private Label? _formLabel;

    private Label? _formLabelNotification;

    //private StackLayout? _stackedLayout;
    private SizeEnum cButtonSize = SizeEnum.XXSmall;

    private bool IsFormEditing = false;

    #endregion Private Fields

    #region Public Fields

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(Form));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Form));

    public static readonly BindableProperty FormAccessModeProperty = BindableProperty.Create(
        propertyName: nameof(FormAccessMode),
        returnType: typeof(FormAccessModeEnum),
        declaringType: typeof(Form),
        defaultValue: FormAccessModeEnum.Editable,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormAccessModeChanged);

    public static readonly BindableProperty FormCancelButtonTextProperty = BindableProperty.Create(
                propertyName: nameof(FormCancelButtonText),
            returnType: typeof(string),
            declaringType: typeof(Form),
            defaultValue: "cancel",
            propertyChanged: OnFormCancelButtonTextChanged);

    public static readonly BindableProperty FormEditButtonTextProperty = BindableProperty.Create(
            propertyName: nameof(FormEditButtonText),
            returnType: typeof(string),
            declaringType: typeof(Form),
            defaultValue: "edit",
            propertyChanged: OnFormEditButtonTextChanged);

    public static readonly BindableProperty FormHasChangesProperty = BindableProperty.Create(
        propertyName: nameof(FormHasChanges),
        returnType: typeof(bool),
        declaringType: typeof(Form),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly BindableProperty FormHasErrorsProperty = BindableProperty.Create(
        propertyName: nameof(FormHasErrors),
        returnType: typeof(bool),
        declaringType: typeof(Form),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly BindableProperty FormNameProperty = BindableProperty.Create(
        propertyName: nameof(FormName),
        returnType: typeof(string),
        declaringType: typeof(Form),
        defaultValue: "",
        propertyChanged: OnFormNameChanged);

    public static readonly BindableProperty FormSaveButtonTextProperty = BindableProperty.Create(
        propertyName: nameof(FormSaveButtonText),
        returnType: typeof(string),
        declaringType: typeof(Form),
        defaultValue: "save",
        propertyChanged: OnFormSaveButtonTextChanged,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty FormStateProperty = BindableProperty.Create(
        propertyName: nameof(FormFieldsState),
        returnType: typeof(FormFieldsStateEnum),
        declaringType: typeof(Form),
        defaultValue: FormFieldsStateEnum.ReadOnly,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormFieldStatesChanged);

    #endregion Public Fields

    #region Public Properties

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

    public FormFieldsStateEnum FormFieldsState
    {
        get => (FormFieldsStateEnum)GetValue(FormStateProperty);
        set => SetValue(FormStateProperty, value);
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

    #endregion Public Properties

    #region Private Methods

    private static void OnFormAccessModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is FormAccessModeEnum newAccessMode)
        {
            void _onFormAccessModeChanges_UpdateUI()
            {
                if (oldValue != newValue)
                {
                    if (newAccessMode == FormAccessModeEnum.Editable)
                    {
                        form.FormFieldsState = FormFieldsStateEnum.Editing;
                    }
                    else if (newAccessMode == FormAccessModeEnum.ViewOnly)
                    {
                        form.FormFieldsState = FormFieldsStateEnum.ReadOnly;
                    }
                    form.FormConfigureStates();
                }
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _onFormAccessModeChanges_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _onFormAccessModeChanges_UpdateUI());
            }
        }
    }

    private static void OnFormCancelButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
        {
            void _onFormCancelButtonTextChanged_UpdateUI()
            {
                if (form._formButtonCancel != null)
                    form._formButtonCancel.Text = newText;
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _onFormCancelButtonTextChanged_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _onFormCancelButtonTextChanged_UpdateUI());
            }
        }
    }

    private static void OnFormEditButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
        {
            void _onFormEditButtonTextChanged_UpdateUI()
            {
                if (form._formButtonEdit != null)
                    form._formButtonEdit.Text = newText;
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _onFormEditButtonTextChanged_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _onFormEditButtonTextChanged_UpdateUI());
            }
        }
    }

    private static void OnFormNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newName)
        {
            void _onFormNameChanged_UpdateUI()
            {
                if (form._formLabel != null)
                {
                    form._formLabel.Text = newName;
                    form._formLabel.IsVisible = !string.IsNullOrEmpty(newName);
                }
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _onFormNameChanged_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _onFormNameChanged_UpdateUI());
            }
        }
    }

    private static void OnFormSaveButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText && form._formButtonSave != null)
        {
            void _onFormSaveButtonTextChanged_UpdateUI()
            {
                form._formButtonSave.Text = newText;
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _onFormSaveButtonTextChanged_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _onFormSaveButtonTextChanged_UpdateUI());
            }
        }
    }
    private static void OnFormFieldStatesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is FormFieldsStateEnum newState)
        {
            void _onFormFieldStatesChanged_UpdateUI()
            {
                if (oldValue != newValue)
                {
                    form.FormConfigureStates();
                }
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _onFormFieldStatesChanged_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _onFormFieldStatesChanged_UpdateUI());
            }
        }
    }
    private bool FormElementsArePristine()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            if (t.FieldChangeState == ChangeStateEnum.Changed)
                return false;
        }
        return true;
    }

    private void FormClear()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            if (t is TextBoxField tbe) tbe.FieldClear();
            if (t is CheckBoxField cbe) cbe.FieldClear();
            t.FieldUnfocus();
        }
    }

    //private void FormFieldsHide()
    //{
    //    var formElements = this.GetVisualTreeDescendants().OfType<BaseFormField>();

    //    foreach (var element in formElements)
    //    {
    //        Console.WriteLine($"{nameof(element)}.IsVisible = false");
    //        element.IsVisible = false;
    //    }
    //}

    private void FormFieldsMarkAsReadOnly()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            Debug.WriteLine($"{t.FieldLabelText} : FormFieldsMarkAsReadOnly()");
            if (t is TextBoxField tbe) tbe.FieldMarkAsReadOnly();
            if (t is CheckBoxField cbe) cbe.FieldMarkAsReadOnly();
            t.FieldUnfocus();
        }
    }


    private void FormFieldsMarkAsEditable()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            Debug.WriteLine($"{t.FieldLabelText} : FormFieldsMarkAsEditable()");
            if (t is TextBoxField tbe) tbe.FieldMarkAsEditable();
            if (t is CheckBoxField cbe) cbe.FieldMarkAsEditable();
        }
    }

    private void FormFieldsSavedAndMarkAsReadOnly()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            if (t is TextBoxField tbe) tbe.FieldSavedAndMarkAsReadOnly();
            if (t is CheckBoxField cbe) cbe.FieldSavedAndMarkAsReadOnly();
            t.FieldUnfocus();
        }
    }

    private bool FormFieldsDetectAreValid() =>
            this.GetVisualTreeDescendants().OfType<TextBoxField>().All(ctb => ctb.FieldValidationState == ValidationStateEnum.Valid);

    private bool _formStatusEvaluating = false;
    private void FormEvaluateStatus()
    {
        if (_formStatusEvaluating) return;
        _formStatusEvaluating = true;
        Debug.WriteLine($"FormEvaluateStatus()");
        FormHasErrors = !FormFieldsDetectAreValid();
        FormHasChanges = !FormElementsArePristine();
        FormConfigureStates();
        _formStatusEvaluating = false;
    }

    private void FormFieldsWireUp()
    {
        var formElements = this.GetVisualTreeDescendants().OfType<BaseFormField>();

        if (!formElements.Any())
            throw new InvalidOperationException("Form missing controls");

        foreach (var element in formElements)
        {
            element.FieldHasChanges += OnFieldHasChanges;
            element.FieldHasValidationChanges += OnFieldHasValidationChanges;
        }
    }

    private void FormInitialize()
    {
        void _formInitialize_UI()
        {
            _formButtonEdit = new()
            {
                Text = FormEditButtonText,
                ButtonSize = cButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Enabled,
                ButtonType = ImageResourceEnum.Edit,
                IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
            };
            _formButtonEdit.Clicked += (sender, e) => FormEnterEditingMode();
            _formButtonEdit.UpdateUI();
            _formButtonSave = new()
            {
                Text = FormSaveButtonText,
                ButtonSize = cButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Hidden,
                ButtonType = ImageResourceEnum.Save,
                IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
            };
            _formButtonSave.Clicked += OnFormSaveButtonClicked;
            _formButtonSave.UpdateUI();
            _formButtonCancel = new()
            {
                Text = FormCancelButtonText,
                ButtonSize = cButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Hidden,
                ButtonType = ImageResourceEnum.Cancel,
                IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
            };
            _formButtonCancel.Clicked += (sender, e) => FormEnterReadOnlyMode();
            _formButtonCancel.UpdateUI();
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
                Margin = new Thickness(5, 5, 5, 5),
                // Define rows
                RowDefinitions =
                        {
                            new RowDefinition { Height = GridLength.Auto }, // Row for _labelForm
                            new RowDefinition { Height = GridLength.Auto }, // Row for Save & Cancel Buttons
                            new RowDefinition { Height = GridLength.Auto }, // Row for Edit Button
                            new RowDefinition { Height = GridLength.Auto }  // Row for _labelNotification
                        },

                // Define columns for the buttons
                ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star }, // Column for _buttonSave
                            new ColumnDefinition { Width = GridLength.Star }  // Column for _buttonCancel
                        }
            };

            // Add controls to the grid
            gridLayout.Add(_formLabel, 0, 0); // Column 0, Row 0
            gridLayout.Add(_formButtonSave, 0, 1); // Column 0, Row 1
            gridLayout.Add(_formButtonCancel, 1, 1); // Column 1, Row 1
            gridLayout.Add(_formButtonEdit, 0, 2); // Column 0, Row 1
            gridLayout.Add(_formLabelNotification, 0, 3); // Column 0, Row 2
            gridLayout.SetColumnSpan(_formLabel, 2); // Span _labelForm across both columns
            gridLayout.SetColumnSpan(_formLabelNotification, 2); // Span _labelNotification across both columns
            gridLayout.SetColumnSpan(_formButtonEdit, 2); // Span _buttonEdit across both columns

            if (this.Content is Layout existingLayout)
            {
                // Check if the existing content is a layout (e.g., Grid, StackLayout)
                if (existingLayout.Children.Count > 0)
                {
                    // If there are existing children, insert the new element as the first child
                    existingLayout.Children.Insert(0, gridLayout);
                }
                else
                {
                    // If there are no existing children, just add the new element to the layout
                    existingLayout.Children.Add(gridLayout);
                }
            }
            else if (this.Content is View existingElement)
            {
                // If the existing content is a single element, create a new container (e.g., StackLayout)
                // and add both the existing element and the new element to it
                StackLayout containerLayout = new StackLayout();
                containerLayout.Children.Add(existingElement);
                containerLayout.Children.Add(gridLayout);
                this.Content = containerLayout;
            }
            else
            {
                // If there's no existing content, set the new element as the content
                this.Content = gridLayout;
            }
        }

        // Check if on the main thread and update UI accordingly
        if (MainThread.IsMainThread)
        {
            _formInitialize_UI();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => _formInitialize_UI());
        }
        FormFieldsWireUp();
    }

    private void FormConfigureStates()
    {
        Debug.WriteLine($"FormConfigureStates()");

        using (ResourceHelper resourceHelper = new())
        {
            // ************
            //  FIELDSTATE
            // ************
            switch (FormFieldsState)
            {
                case FormFieldsStateEnum.ReadOnly:
                    FormFieldsMarkAsReadOnly();
                    break;

                case FormFieldsStateEnum.Editing:
                    FormFieldsMarkAsEditable();
                    break;

                case FormFieldsStateEnum.Hidden:
                    FormMakeHidden();
                    break;
            }
            // ***********
            //  FORMSTATE
            // ***********
            _formRefreshState_ConfigureButtonImages(resourceHelper, !FormHasChanges, !FormHasErrors, SizeEnum.Large);
        }

        void _formRefreshState_ConfigureButtonImages(ResourceHelper resourceHelper, bool noChanges, bool noErrors, SizeEnum size)
        {
            if (_formButtonSave == null && _formButtonCancel == null && _formButtonEdit == null) return;

            // ***********
            //  EDITABLE
            // ***********
            if (FormAccessMode == FormAccessModeEnum.Editable)
            {

                // *************
                //   EDIT MODE
                // *************
                if (FormFieldsState == FormFieldsStateEnum.Editing)
                {
                    _formButtonEdit!.ButtonState = ButtonStateEnum.Hidden;
                    if (noChanges)
                    {
                        _formButtonSave!.ButtonState = ButtonStateEnum.Disabled;
                        _formButtonCancel!.ButtonState = ButtonStateEnum.Enabled;
                    }
                    else if (noErrors)
                    {
                        _formButtonSave!.ButtonState = ButtonStateEnum.Enabled;
                        _formButtonCancel!.ButtonState = ButtonStateEnum.Enabled;
                    }
                    else
                    {
                        _formButtonSave!.ButtonState = ButtonStateEnum.Disabled;
                        _formButtonCancel!.ButtonState = ButtonStateEnum.Enabled;
                    }
                }
                // *************
                //   READ MODE
                // *************
                else if (FormFieldsState == FormFieldsStateEnum.ReadOnly)
                {
                    _formButtonSave!.ButtonState = ButtonStateEnum.Hidden;
                    _formButtonCancel!.ButtonState = ButtonStateEnum.Hidden;
                    _formButtonEdit!.ButtonState = ButtonStateEnum.Enabled;
                }
            }
            else if (   FormAccessMode == FormAccessModeEnum.ViewOnly
                     || FormAccessMode == FormAccessModeEnum.Hidden)
            {
                // **********************
                //   VIEWONLY OR HIDDEN
                // **********************
                _formButtonSave!.ButtonState = ButtonStateEnum.Hidden;
                _formButtonCancel!.ButtonState = ButtonStateEnum.Hidden;
                _formButtonEdit!.ButtonState = ButtonStateEnum.Hidden;
            }
        }
    }

    private void OnFormEditButtonClicked(object? sender, EventArgs e)
    {
        IsFormEditing = true;
    }

    private void OnFieldHasChanges(object? sender, HasChangesEventArgs e) => FormEvaluateStatus();

    private void OnFieldHasValidationChanges(object? sender, ValidationDataChangesEventArgs e) => FormEvaluateStatus();

    private void OnFormSaveButtonClicked(object? sender, EventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    #endregion Private Methods

    #region Protected Methods

    /// <summary>
    /// Called when the parent of the form is set. This method is responsible for initializing and wiring up controls within the form.
    /// </summary>
    protected override void OnParentSet()
    {
        base.OnParentSet();
        FormInitialize();
        FormFieldsWireUp();
        FormEvaluateStatus();
    }

    #endregion Protected Methods

    #region Public Methods

    /// <summary>
    /// puts form into "hidden" mode with all buttons and fields hidden
    /// </summary>
    public void FormEnterEditingMode()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            if (t is TextBoxField tbe) { tbe.FieldMarkAsEditable(); }
            if (t is CheckBoxField cbe) { cbe.FieldMarkAsEditable(); }
        }
        FormFieldsState = FormFieldsStateEnum.Editing;
        FormEvaluateStatus();
    }

    /// <summary>
    /// puts form into "hidden" mode with save and cancel buttons visible
    /// </summary>
    public void FormMakeHidden()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            if (t is TextBoxField tbe) { tbe.FieldHide(); }
            if (t is CheckBoxField cbe) { cbe.FieldHide(); }
        }
        FormFieldsState = FormFieldsStateEnum.Hidden;
        FormEvaluateStatus();
    }

    /// <summary>
    /// puts form into "read" mode with edit button visible
    /// </summary>
    public void FormEnterReadOnlyMode()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            if (t is TextBoxField tbe) { tbe.FieldMarkAsReadOnly(); }
            if (t is CheckBoxField cbe) { cbe.FieldMarkAsReadOnly(); }
        }
        FormFieldsState = FormFieldsStateEnum.ReadOnly;
        FormEvaluateStatus();
    }

    /// <summary>
    /// puts form into "view only" mode with no buttons visible
    /// </summary>
    public void FormEnterViewOnlyMode()
    {
        foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
        {
            if (t is TextBoxField tbe) { tbe.FieldMarkAsReadOnly(); }
            if (t is CheckBoxField cbe) { cbe.FieldMarkAsReadOnly(); }
        }
        FormFieldsState = FormFieldsStateEnum.ReadOnly;
        FormEvaluateStatus();
    }

    #endregion Public Methods

}