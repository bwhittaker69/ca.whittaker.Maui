using ca.whittaker.Maui.Controls.Buttons;
using System.Windows.Input;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a form control within a Maui application, providing state management and validation capabilities.
/// </summary>
public class Form : StackLayout
{
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
        propertyChanged: OnFormSaveButtonTextChanged);

    public static readonly BindableProperty FormSizeProperty = BindableProperty.Create(
        propertyName: nameof(FormSize),
        returnType: typeof(SizeEnum),
        declaringType: typeof(Form),
        defaultValue: SizeEnum.Normal,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty FormStateProperty = BindableProperty.Create(
        propertyName: nameof(FormState),
        returnType: typeof(FormStateEnum),
        declaringType: typeof(Form),
        defaultValue: FormStateEnum.Disabled,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFormStateChanged);

    public Button _buttonCancel;

    public Button _buttonSave;

    public Label _labelForm;

    public Label _labelNotification;

    private bool HasNoChanges = false;

    private bool HasNoErrors = false;

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

    public string FormCancelButtonText
    {
        get => (string)GetValue(FormCancelButtonTextProperty);
        set => SetValue(FormCancelButtonTextProperty, value);
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

    public SizeEnum FormSize
    {
        get => (SizeEnum)GetValue(FormSizeProperty);
        set => SetValue(FormSizeProperty, value);
    }

    public FormStateEnum FormState
    {
        get => (FormStateEnum)GetValue(FormStateProperty);
        set => SetValue(FormStateProperty, value);
    }

    /// <summary>
    /// Called when the parent of the form is set. This method is responsible for initializing and wiring up controls within the form.
    /// </summary>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        _buttonSave = new Button
        {
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand
        };
        _buttonSave.Text = FormSaveButtonText;
        _buttonSave.Clicked += _buttonSave_Clicked;
        _buttonCancel = new Button
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            BackgroundColor = Colors.Transparent
        };
        _buttonCancel.Text = FormCancelButtonText;
        _buttonCancel.Clicked += (sender, e) => CancelForm();
        _labelNotification = CreateNotificationLabel();
        _labelForm = CreateFormLabel();

        Insert(0, CreateFormLayoutGrid());

        WireUpControls();

    }

    private static void OnFormCancelButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
        {
            ((Form)bindable)._buttonCancel.Text = (string)newValue;
        }
    }

    private static void OnFormNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
        {
            ((Form)bindable)._labelForm.Text = newValue == null ? "" : newValue.ToString();
            ((Form)bindable)._labelForm.IsVisible = newValue == null || newValue.ToString() == "" ? false : true;
        }
    }

    private static void OnFormSaveButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
        {
            ((Form)bindable)._buttonSave.Text = (string)newValue;
        }
    }

    private static void OnFormStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
        {
            if ((FormStateEnum)newValue == FormStateEnum.Undo)
            {
                ((Form)bindable).CancelForm();
                ((Form)bindable).FormState = FormStateEnum.Enabled;
            }
            if ((FormStateEnum)newValue == FormStateEnum.Saved)
            {
                ((Form)bindable).SavedForm();
                ((Form)bindable).FormState = FormStateEnum.Enabled;
            }
            if ((FormStateEnum)newValue == FormStateEnum.Clear)
            {
                ((Form)bindable).ClearForm();
                ((Form)bindable).FormState = FormStateEnum.Enabled;
            }
            ((Form)bindable).UpdateFormControlStates();
        }
    }

    private void _buttonSave_Clicked(object? sender, EventArgs e)
    {
        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
    }

    private ImageSource CalcCancelImage(bool hasChanged)
    {
        return ImageSource.FromFile($"cancel_24_mauiimage{(hasChanged ? "" : "_disabled")}.png");
    }

    private FormStateEnum CalcFormState()
    {
        if (!IsVisible)
        {
            return FormStateEnum.Hidden;
        }
        else if (!IsEnabled)
        {
            return FormStateEnum.Disabled;
        }
        else
        {
            return FormStateEnum.Enabled;
        }
    }

    private void CancelForm()
    {
        foreach (TextBox t in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            t.Undo();
        }
    }

    private void ClearForm()
    {
        foreach (TextBox t in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            t.Clear();
        }
    }

    /// <summary>
    /// Creates a form label control.
    /// </summary>
    /// <returns>The created form label control.</returns>
    private Label CreateFormLabel()
    {
        return new Label
        {
            Text = FormName,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = FormName == null || FormName == "" ? false : true
        };
    }

    /// <summary>
    /// Creates the layout grid for the form.
    /// </summary>
    /// <returns>The created grid layout.</returns>
    private Grid CreateFormLayoutGrid()
    {
        _labelNotification.HorizontalOptions = LayoutOptions.Center;
        _labelForm.HorizontalOptions = LayoutOptions.Center;
        _buttonSave.HorizontalOptions = LayoutOptions.Center;
        _buttonCancel.HorizontalOptions = LayoutOptions.Center;
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        grid.Add(_labelForm, 0, 0);
        grid.Add(_buttonSave, 0, 1);
        grid.Add(_buttonCancel, 1, 1);
        grid.Add(_labelNotification, 0, 2);

        Grid.SetColumnSpan(_labelForm, 2);
        Grid.SetColumnSpan(_labelNotification, 2);

        return grid;
    }

    /// <summary>
    /// Creates the notification label for the text box.
    /// </summary>
    /// <returns>The created notification label.</returns>
    private Label CreateNotificationLabel()
    {
        return new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false,
            TextColor = Colors.Red
        };
    }

    private void CustomTextBox_HasChanges(object? sender, HasChangesEventArgs e) => EvaluateForm();

    private void CustomTextBox_HasValidationChanges(object? sender, ValidationDataChangesEventArgs e) => EvaluateForm();

    private void EvaluateForm()
    {
        HasNoErrors = IsFormDataValid();
        HasNoChanges = HasFormNotChanged();
        FormState = CalcFormState();
        UpdateFormControlStates();
    }

    private bool HasFormNotChanged()
    {
        foreach (TextBox t in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            Console.WriteLine(t.ChangeState.ToString());
            if (t.ChangeState == ChangeStateEnum.Changed)
                return false;
        }
        return true;
    }

    private bool IsFormDataValid() =>
        this.GetVisualTreeDescendants().OfType<TextBox>().All(ctb => ctb.ValidationState == ValidationStateEnum.Valid);

    private void SavedForm()
    {
        foreach (TextBox t in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            t.Saved();
        }
    }

    private void UpdateFormControlStates()
    {

        switch (FormState)
        {
            case FormStateEnum.Enabled:
                IsEnabled = true;
                IsVisible = true;
                if (HasNoChanges)
                {
                    _buttonSave.ImageSource = GetSaveButtonImageSource(ButtonStateEnum.Disabled);
                    _buttonCancel.ImageSource = GetCancelButtonImageSource(ButtonStateEnum.Disabled);
                }
                else
                {
                    if (HasNoErrors)
                    {
                        _buttonSave.ImageSource = GetSaveButtonImageSource(ButtonStateEnum.Enabled);
                        _buttonCancel.ImageSource = GetCancelButtonImageSource(ButtonStateEnum.Enabled);
                    }
                    else
                    {
                        _buttonSave.ImageSource = GetSaveButtonImageSource(ButtonStateEnum.Disabled);
                        _buttonCancel.ImageSource = GetCancelButtonImageSource(ButtonStateEnum.Enabled);
                    }
                }
                break;

            case FormStateEnum.Disabled:
                IsEnabled = false;
                _buttonSave.ImageSource = GetSaveButtonImageSource(ButtonStateEnum.Disabled);
                break;

            case FormStateEnum.Hidden:
                IsVisible = false;
                //_buttonSave.ImageSource = GetSaveButtonImageSource(ButtonStateEnum.Hidden);
                break;
        }
    }


    private ImageSource GetSaveButtonImageSource(ButtonStateEnum buttonState)
    {
        var assembly = this.GetType().Assembly;
        string? assemblyName = assembly.GetName().Name;
        string enabled = "";
        string disabled = "_disabled";
        string resourceName = $"{assemblyName}.Resources.Images.save_12_mauiimage{(buttonState.Equals(ButtonStateEnum.Disabled) ? disabled : enabled)}.png";
        return ImageSource.FromResource(resourceName, assembly);
    }

    private ImageSource GetCancelButtonImageSource(ButtonStateEnum buttonState)
    {
        var assembly = this.GetType().Assembly;
        string? assemblyName = assembly.GetName().Name;
        string enabled = "";
        string disabled = "_disabled";
        string resourceName = $"{assemblyName}.Resources.Images.cancel_12_mauiimage{(buttonState.Equals(ButtonStateEnum.Disabled) ? disabled : enabled)}.png";
        return ImageSource.FromResource(resourceName, assembly);
    }
    private void WireUpControls()
    {
        var controlCount = this.GetVisualTreeDescendants().Count;
        if (controlCount == 0) throw new InvalidOperationException("Form missing controls");

        foreach (var customTextBox in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            customTextBox.HasChanges += CustomTextBox_HasChanges;
            customTextBox.HasValidationChanges += CustomTextBox_HasValidationChanges;
        }
        return;
    }
    private void FixWidthFirstColumn()
    {
        var controlCount = this.GetVisualTreeDescendants().Count;
        if (controlCount == 0) throw new InvalidOperationException("Form missing controls");

        int iMaxWidth = 0;
        foreach (var customTextBox in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            if (customTextBox._label.Width > iMaxWidth) iMaxWidth = (int)customTextBox._label.Width;
        }
        foreach (var customTextBox in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            customTextBox._label.WidthRequest = iMaxWidth;
        }
        return;
    }

}