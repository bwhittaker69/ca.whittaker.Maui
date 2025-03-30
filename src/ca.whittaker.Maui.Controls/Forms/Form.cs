using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

using ca.whittaker.Maui.Controls.Buttons;
using System.Windows.Input;

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

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(Form));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Form));

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
        propertyName: nameof(FormState),
        returnType: typeof(FormStateEnum),
        declaringType: typeof(Form),
        defaultValue: FormStateEnum.ReadOnly,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormStateChanged);

    private SaveButton? _buttonCancel;
    private EditButton? _buttonEdit;
    private CancelButton? _buttonSave;
    private Label? _labelForm;
    private Label? _labelNotification;
    private bool IsEditing = false;

    //private StackLayout? _stackedLayout;
    private SizeEnum cButtonSize = SizeEnum.XXSmall;

    public Form()
    {
    }

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

    public string FormEditButtonText
    {
        get => (string)GetValue(FormEditButtonTextProperty);
        set => SetValue(FormEditButtonTextProperty, value);
    }
    public string FormCancelButtonText
    {
        get => (string)GetValue(FormCancelButtonTextProperty);
        set => SetValue(FormCancelButtonTextProperty, value);
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

    public FormStateEnum FormState
    {
        get => (FormStateEnum)GetValue(FormStateProperty);
        set => SetValue(FormStateProperty, value);
    }
    // called when user clicks cancel button
    // also should be called when the page form is OnDissapearing
    public void CancelForm()
    {
        foreach (BaseFormElement t in this.GetVisualTreeDescendants().OfType<BaseFormElement>())
        {
            if (t is TextBoxElement tbe) { tbe.Disable(); }
            if (t is CheckBoxElement cbe) { cbe.Disable(); }
        }
        IsEditing = false;
        EvaluateForm();
    }

    // called when user clicks edit button
    public void EditForm()
    {
        foreach (BaseFormElement t in this.GetVisualTreeDescendants().OfType<BaseFormElement>())
        {
            if (t is TextBoxElement tbe) { tbe.Enable(); }
            if (t is CheckBoxElement cbe) { cbe.Enable(); }
        }
        IsEditing = true;
        EvaluateForm();
    }

    /// <summary>
    /// Called when the parent of the form is set. This method is responsible for initializing and wiring up controls within the form.
    /// </summary>
    protected override void OnParentSet()
    {
        base.OnParentSet();
        InitializeFormControls();
        WireUpControls();
        EvaluateForm();
    }

    private static void OnFormEditButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
        {
            void UpdateUI()
            {
                if (form._buttonEdit != null)
                    form._buttonEdit.Text = newText;
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


    private static void OnFormCancelButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText)
        {
            void UpdateUI()
            {
                if (form._buttonCancel != null)
                    form._buttonCancel.Text = newText;
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

    private static void OnFormNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newName)
        {
            void UpdateUI()
            {
                if (form._labelForm != null)
                {
                    form._labelForm.Text = newName;
                    form._labelForm.IsVisible = !string.IsNullOrEmpty(newName);
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

    private static void OnFormSaveButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is string newText && form._buttonSave != null)
        {
            void UpdateUI()
            {
                form._buttonSave.Text = newText;
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

    private static void OnFormStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Form form && newValue is FormStateEnum newState)
        {
            void UpdateUI()
            {
                if (oldValue != newValue)
                {

                    if (newState == FormStateEnum.Initialize)
                    {
                        form.InitForm();
                        form.IsVisible = true;
                        form.FormState = FormStateEnum.Editing;
                    }
                    else if (newState == FormStateEnum.Saved)
                    {
                        form.SavedForm();
                        form.IsVisible = true;
                        form.FormState = FormStateEnum.Editing;
                    }
                    else if (newState == FormStateEnum.Undo)
                    {
                        form.CancelForm();
                        form.IsVisible = true;
                        form.FormState = FormStateEnum.Editing;
                    }
                    else if (newState == FormStateEnum.Clear)
                    {
                        form.ClearForm();
                        form.IsVisible = true;
                        form.FormState = FormStateEnum.Editing;
                    }
                    else if (newState == FormStateEnum.Hidden)
                    {
                        form.IsVisible = false;
                        form.FormState = FormStateEnum.Hidden;
                    }
                    form.UpdateFormControlStates();
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

    private void ButtonEdit_Clicked(object? sender, EventArgs e)
    {
        IsEditing = true;
    }

    private void ButtonSave_Clicked(object? sender, EventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private FormStateEnum CalcFormState()
    {
        if (!IsVisible)
        {
            return FormStateEnum.Hidden;
        }
        else if (!IsEnabled)
        {
            return FormStateEnum.ViewOnly;
        }
        else
        {
            if (IsEditing)
                return FormStateEnum.Editing;
            else
                return FormStateEnum.ReadOnly;        
        }
    }

    private void ClearForm()
    {
        foreach (BaseFormElement t in this.GetVisualTreeDescendants().OfType<BaseFormElement>())
        {
            if (t is TextBoxElement tbe) tbe.Clear();
            if (t is CheckBoxElement cbe) cbe.Clear();
            t.Unfocus();
        }
    }

    private void CustomTextBox_HasChanges(object? sender, HasChangesEventArgs e) => EvaluateForm();

    private void CustomTextBox_HasValidationChanges(object? sender, ValidationDataChangesEventArgs e) => EvaluateForm();

    private void EvaluateForm()
    {
        FormHasErrors = !IsFormDataValid();
        FormHasChanges = !HasFormNotChanged();
        FormState = CalcFormState();
        UpdateFormControlStates();
    }

    private bool HasFormNotChanged()
    {
        foreach (BaseFormElement t in this.GetVisualTreeDescendants().OfType<BaseFormElement>())
        {
            //Console.WriteLine(t.ChangeState.ToString());
            if (t.ChangeState == ChangeStateEnum.Changed)
                return false;
        }
        return true;
    }

    private void Hide()
    {
        Console.WriteLine("hide");
        var descendants = this.GetVisualTreeDescendants().OfType<BaseFormElement>();

        foreach (var element in descendants)
        {
            Console.WriteLine($"{nameof(element)}.IsVisible = false");
            element.IsVisible = false;
        }
    }

    private void InitializeFormControls()
    {
        void UpdateUI()
        {
            _buttonEdit = new()
            {
                Text = FormEditButtonText,
                ButtonSize = cButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Enabled,
                ButtonType = ImageResourceEnum.Edit,
            };
            _buttonEdit.Clicked += (sender, e) => EditForm();
            _buttonEdit.UpdateUI();
            _buttonSave = new()
            {
                Text = FormSaveButtonText,
                ButtonSize = cButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Hidden,
                ButtonType = ImageResourceEnum.Save,
            };
            _buttonSave.Clicked += ButtonSave_Clicked;
            _buttonSave.UpdateUI();
            _buttonCancel = new()
            {
                Text = FormCancelButtonText,
                ButtonSize = cButtonSize,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ButtonState = ButtonStateEnum.Hidden,
                ButtonType = ImageResourceEnum.Cancel,
            };
            _buttonCancel.Clicked += (sender, e) => CancelForm();
            _buttonCancel.UpdateUI();
            _labelNotification = new Label
            {
                Text = "",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                TextColor = Colors.Red
            };
            _labelForm = new Label
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
            gridLayout.Add(_labelForm, 0, 0); // Column 0, Row 0
            gridLayout.Add(_buttonSave, 0, 1); // Column 0, Row 1
            gridLayout.Add(_buttonCancel, 1, 1); // Column 1, Row 1
            gridLayout.Add(_buttonEdit, 0, 2); // Column 0, Row 1
            gridLayout.Add(_labelNotification, 0, 3); // Column 0, Row 2
            gridLayout.SetColumnSpan(_labelForm, 2); // Span _labelForm across both columns
            gridLayout.SetColumnSpan(_labelNotification, 2); // Span _labelNotification across both columns
            gridLayout.SetColumnSpan(_buttonEdit, 2); // Span _buttonEdit across both columns

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
            UpdateUI();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
        WireUpControls();
    }
    private bool IsFormDataValid() =>
        this.GetVisualTreeDescendants().OfType<TextBoxElement>().All(ctb => ctb.ValidationState == ValidationStateEnum.Valid);

    private void SavedForm()
    {
        foreach (BaseFormElement t in this.GetVisualTreeDescendants().OfType<BaseFormElement>())
        {
            if (t is TextBoxElement tbe) tbe.Saved();
            if (t is CheckBoxElement cbe) cbe.Saved();
            t.Unfocus();
        }
    }
    private void InitForm()
    {
        foreach (BaseFormElement t in this.GetVisualTreeDescendants().OfType<BaseFormElement>())
        {
            if (t is TextBoxElement tbe) tbe.InitField();
            if (t is CheckBoxElement cbe) cbe.InitField();
            t.Unfocus();
        }
    }

    private void Show()
    {
        Console.WriteLine("show");
        var descendants = this.GetVisualTreeDescendants().OfType<BaseFormElement>();

        foreach (var element in descendants)
        {
            Console.WriteLine($"{nameof(element)}.IsVisible = true");
            element.IsVisible = true;
        }
    }

    private void UpdateFormControlStates()
    {
        using (ResourceHelper resourceHelper = new())
        {
            switch (FormState)
            {
                case FormStateEnum.ViewOnly:
                    Show();
                    IsEnabled = false;
                    IsVisible = true;
                    SetButtonImages(resourceHelper, !FormHasChanges, !FormHasErrors, SizeEnum.Large);
                    break;

                case FormStateEnum.Editing:
                case FormStateEnum.ReadOnly:
                    Show();
                    IsEnabled = true;
                    IsVisible = true;
                    SetButtonImages(resourceHelper, !FormHasChanges, !FormHasErrors, SizeEnum.Large);
                    break;

                case FormStateEnum.Hidden:
                    IsEnabled = false;
                    IsVisible = false;
                    Hide();
                    break;
            }
        }

        void SetButtonImages(ResourceHelper resourceHelper, bool noChanges, bool noErrors, SizeEnum size)
        {
            if (_buttonSave == null && _buttonCancel == null && _buttonEdit == null) return;

            if (FormState == FormStateEnum.Editing)
            {
                // *************
                //   EDIT MODE
                // *************
                _buttonEdit!.ButtonState = ButtonStateEnum.Hidden;
                if (noChanges)
                {
                    _buttonSave!.ButtonState = ButtonStateEnum.Disabled;
                    _buttonCancel!.ButtonState = ButtonStateEnum.Enabled;
                }
                else if (noErrors)
                {
                    _buttonSave!.ButtonState = ButtonStateEnum.Enabled;
                    _buttonCancel!.ButtonState = ButtonStateEnum.Enabled;
                }
                else
                {
                    _buttonSave!.ButtonState = ButtonStateEnum.Disabled;
                    _buttonCancel!.ButtonState = ButtonStateEnum.Enabled;
                }
            }
            else if (FormState == FormStateEnum.ReadOnly)
            {
                // *************
                //   READ MODE
                // *************
                _buttonSave!.ButtonState = ButtonStateEnum.Hidden;
                _buttonCancel!.ButtonState = ButtonStateEnum.Hidden;
                _buttonEdit!.ButtonState = ButtonStateEnum.Enabled;
            }
            else if (FormState == FormStateEnum.ViewOnly)
            {
                // *************
                //   VIEW MODE
                // *************
                _buttonSave!.ButtonState = ButtonStateEnum.Hidden;
                _buttonCancel!.ButtonState = ButtonStateEnum.Hidden;
                _buttonEdit!.ButtonState = ButtonStateEnum.Hidden;
            }
        }
    }

    private void WireUpControls()
    {
        var elements = this.GetVisualTreeDescendants().OfType<BaseFormElement>();

        if (!elements.Any())
            throw new InvalidOperationException("Form missing controls");

        foreach (var element in elements)
        {
            element.HasChanges += CustomTextBox_HasChanges;
            element.HasValidationChanges += CustomTextBox_HasValidationChanges;
        }
    }
}
