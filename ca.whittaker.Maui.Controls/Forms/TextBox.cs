using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control with various properties for text manipulation and validation.
/// </summary>
public class TextBox : ContentView
{
    /// <summary>
    /// Bindable property to specify if all text should be in lowercase.
    /// </summary>
    public static readonly BindableProperty AllLowerCaseProperty = BindableProperty.Create(
        propertyName: nameof(AllLowerCase),
        returnType: typeof(bool),
        declaringType: typeof(TextBox),
        defaultValue: false);

    /// <summary>
    /// Bindable property to specify if white space is allowed in the text.
    /// </summary>
    public static readonly BindableProperty AllowWhiteSpaceProperty = BindableProperty.Create(
        propertyName: nameof(AllowWhiteSpace),
        returnType: typeof(bool),
        declaringType: typeof(TextBox),
        defaultValue: true);

    /// <summary>
    /// Bindable property to indicate the change state of the text box.
    /// </summary>
    public static readonly BindableProperty ChangeStateProperty = BindableProperty.Create(
        propertyName: nameof(ChangeState),
        returnType: typeof(ChangeStateEnum),
        declaringType: typeof(TextBox),
        defaultValue: ChangeStateEnum.NotChanged,
        defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Bindable property to set the keyboard type for the text box.
    /// </summary>
    public static readonly BindableProperty FieldTypeProperty = BindableProperty.Create(
        propertyName: nameof(FieldType),
        returnType: typeof(FieldTypeEnum),
        declaringType: typeof(TextBox),
        defaultValue: FieldTypeEnum.Text,
        propertyChanged: OnFieldTypeChanged);

    /// <summary>
    /// Bindable property for the label of the text box.
    /// </summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        propertyName: nameof(Label),
        returnType: typeof(string),
        declaringType: typeof(TextBox),
        defaultValue: string.Empty);

    /// <summary>
    /// Bindable property to specify if the text box is mandatory.
    /// </summary>
    public static readonly BindableProperty MandatoryProperty = BindableProperty.Create(
        propertyName: nameof(Mandatory),
        returnType: typeof(bool),
        declaringType: typeof(TextBox),
        defaultValue: false);

    /// <summary>
    /// Bindable property to set the maximum length of the text.
    /// </summary>
    public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(
        propertyName: nameof(MaxLength),
        returnType: typeof(int),
        declaringType: typeof(TextBox),
        defaultValue: 255);

    /// <summary>
    /// Bindable property for the placeholder text in the text box.
    /// </summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(Placeholder),
        returnType: typeof(string),
        declaringType: typeof(TextBox),
        defaultValue: string.Empty);

    /// <summary>
    /// Bindable property for the main text of the text box.
    /// </summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        propertyName: nameof(Text),
        returnType: typeof(string),
        declaringType: typeof(TextBox),
        defaultValue: string.Empty,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnTextChanged);

    /// <summary>
    /// Bindable property to indicate the validation state of the text box.
    /// </summary>
    public static readonly BindableProperty ValidationStateProperty = BindableProperty.Create(
        propertyName: nameof(ValidationState),
        returnType: typeof(ValidationStateEnum),
        declaringType: typeof(TextBox),
        defaultValue: ValidationStateEnum.Valid,
        defaultBindingMode: BindingMode.TwoWay);

    private readonly Label _debugLabel;
    private readonly Entry _entry;
    private readonly Label _label;
    private readonly Label _notificationLabel;
    private bool _isOriginalTextSet = false;
    private string _originalText = string.Empty;
    private bool _previousHasChangedState = false;
    private bool _previousInvalidDataState = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBox"/> class.
    /// </summary>
    public TextBox()
    {
        _entry = CreateEntry();
        _label = CreateLabel();
        _debugLabel = CreateLabel();
        _notificationLabel = CreateNotificationLabel();

        var grid = CreateLayoutGrid();
        Content = grid;

        InitializeProperties();
    }

    /// <summary>
    /// Event raised when there are changes in the text box.
    /// </summary>
    public event EventHandler<HasChangesEventArgs>? HasChanges;

    /// <summary>
    /// Event raised when there are validation changes in the text box.
    /// </summary>
    public event EventHandler<ValidationDataChangesEventArgs>? HasValidationChanges;


    /// <summary>
    /// Gets or sets a value indicating whether all text should be in lowercase.
    /// </summary>
    public bool AllLowerCase { get => (bool)GetValue(AllLowerCaseProperty); set => SetValue(AllLowerCaseProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether white space is allowed in the text.
    /// </summary>
    public bool AllowWhiteSpace { get => (bool)GetValue(AllowWhiteSpaceProperty); set => SetValue(AllowWhiteSpaceProperty, value); }

    /// <summary>
    /// Gets or sets the change state of the text box.
    /// </summary>
    public ChangeStateEnum ChangeState { get => (ChangeStateEnum)GetValue(ChangeStateProperty); set => SetValue(ChangeStateProperty, value); }

    /// <summary>
    /// Gets or sets the field type which determines the keyboard type for input.
    /// </summary>
    public FieldTypeEnum FieldType { get => (FieldTypeEnum)GetValue(FieldTypeProperty); set => SetValue(FieldTypeProperty, value); }

    /// <summary>
    /// Gets or sets the label text for the text box.
    /// </summary>
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the text box is mandatory.
    /// </summary>
    public bool Mandatory { get => (bool)GetValue(MandatoryProperty); set => SetValue(MandatoryProperty, value); }

    /// <summary>
    /// Gets or sets the maximum length of the text.
    /// </summary>
    public int MaxLength { get => (int)GetValue(MaxLengthProperty); set => SetValue(MaxLengthProperty, value); }

    /// <summary>
    /// Gets or sets the placeholder text for the text box.
    /// </summary>
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

    /// <summary>
    /// Gets or sets the main text of the text box.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            if (!_isOriginalTextSet)
            {
                _originalText = value;
                _isOriginalTextSet = true;
                EvaluateToRaiseValidationChangesEvent(true);
            }
        }
    }

    /// <summary>
    /// Gets or sets the validation state of the text box.
    /// </summary>
    public ValidationStateEnum ValidationState { get => (ValidationStateEnum)GetValue(ValidationStateProperty); set => SetValue(ValidationStateProperty, value); }

    /// <summary>
    /// Resets the change state to the current text value.
    /// </summary>
    public void Clear()
    {
        _originalText = "";
        Text = "";
        UpdateValidationState();
    }

    /// <summary>
    /// Resets the change state to the current text value.
    /// </summary>
    public void ResetChangeState()
    {
        _originalText = Text;
        UpdateValidationState();
    }

    /// <summary>
    /// Method called when a property of the text box is changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected override void OnPropertyChanged(string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(Label):
                _label.Text = Label;
                break;
            case nameof(Placeholder):
                _entry.Placeholder = Placeholder;
                break;
            case nameof(MaxLength):
                _entry.MaxLength = MaxLength;
                break;
        }
    }

    /// <summary>
    /// Validates an email address.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid, otherwise false.</returns>
    private static bool IsValidData_Email(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email) || email.Length < 5)
                return false;
            var mailAddress = new MailAddress(email);
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a URL.
    /// </summary>
    /// <param name="urlString">The URL string to validate.</param>
    /// <returns>True if the URL is valid, otherwise false.</returns>
    private static bool IsValidData_Url(string urlString)
    {
        if (string.IsNullOrEmpty(urlString) || urlString.Length < 5)
            return false;

        if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uriResult))
            return false;

        if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            return false;

        return Regex.IsMatch(uriResult.AbsoluteUri, @"^(http|https):\/\/[^\s$.?#].[^\s]*$");
    }

    /// <summary>
    /// Called when the field type of the text box changes.
    /// </summary>
    /// <param name="bindable">The text box instance.</param>
    /// <param name="oldValue">The old field type value.</param>
    /// <param name="newValue">The new field type value.</param>
    private static void OnFieldTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        var newType = (FieldTypeEnum)newValue;

        control._entry.Keyboard = newType switch
        {
            FieldTypeEnum.Email => Keyboard.Email,
            FieldTypeEnum.Url => Keyboard.Url,
            FieldTypeEnum.Chat => Keyboard.Chat,
            _ => Keyboard.Default,
        };
    }

    /// <summary>
    /// Called when the text property of the text box changes.
    /// </summary>
    /// <param name="bindable">The text box instance.</param>
    /// <param name="oldValue">The old text value.</param>
    /// <param name="newValue">The new text value.</param>
    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        control._entry.Text = (string)newValue;
        control._debugLabel.Text = $"ChangeState: {control.ChangeState}  ValidationState: {control.ValidationState}";
    }

    /// <summary>
    /// Calculates the validation state based on the current text.
    /// </summary>
    /// <param name="text">The text to validate.</param>
    /// <returns>The validation state.</returns>
    private ValidationStateEnum CalculateValidationState(string text)
    {
        // mandatory field, so its not valid
        if (Mandatory && string.IsNullOrEmpty(text))
        {
            return ValidationStateEnum.RequiredFieldError;
        }
        // not mandatory and no text, so its valid
        else if (!Mandatory && string.IsNullOrEmpty(text))
        {
            return ValidationStateEnum.Valid;
        }
        else if (FieldType == FieldTypeEnum.Email && !IsValidData_Email(text))
        {
            return ValidationStateEnum.FormatError;
        }
        else if (FieldType == FieldTypeEnum.Url && !IsValidData_Url(text))
        {
            return ValidationStateEnum.FormatError;
        }

        return ValidationStateEnum.Valid;
    }

    /// <summary>
    /// Creates the entry control for the text box.
    /// </summary>
    /// <returns>The created entry control.</returns>
    private Entry CreateEntry()
    {
        var entry = new Entry
        {
            Placeholder = Placeholder,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };
        entry.TextChanged += Entry_TextChanged;
        entry.Focused += Entry_Focused;
        entry.Unfocused += Entry_Unfocused;

        return entry;
    }

    /// <summary>
    /// Creates a label control.
    /// </summary>
    /// <returns>The created label control.</returns>
    private Label CreateLabel()
    {
        return new Label
        {
            Text = Label,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };
    }

    /// <summary>
    /// Creates the layout grid for the text box.
    /// </summary>
    /// <returns>The created grid layout.</returns>
    private Grid CreateLayoutGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
            RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
        };

        grid.Add(_label, 0, 0);
        grid.Add(_entry, 1, 0);
        grid.Add(_notificationLabel, 0, 1);
        //grid.Add(_debugLabel, 0, 2);
        Grid.SetColumnSpan(_notificationLabel, 2);
        Grid.SetColumnSpan(_debugLabel, 2);


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


    private void Entry_Focused(object? sender, FocusEventArgs e)
    {

    }

    private void Entry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ProcessAndSetText(e.NewTextValue);
        UpdateValidationState();
        UpdateNotificationMessage(Text);
    }

    private void Entry_Unfocused(object? sender, FocusEventArgs e)
    {
        UpdateNotificationMessage(Text);
    }

    private void EvaluateToRaiseHasChangesEvent()
    {
        bool hasChanged = _originalText != Text;
        if (_previousHasChangedState != hasChanged)
        {
            _previousHasChangedState = hasChanged;
            ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
            HasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
        }
    }

    private void EvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
    {
        bool isValid = IsValidData(Text);
        if (_previousInvalidDataState != isValid || forceRaise)
        {
            _previousInvalidDataState = isValid;
            ValidationState = isValid ? ValidationStateEnum.Valid : ValidationStateEnum.FormatError;
            HasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));
        }
    }

    private void InitializeProperties()
    {
        _originalText = Text;
    }

    // Implementation of IsValidData and related methods
    private bool IsValidData(string text)
    {
        // mandatory field, so its not valid
        if (Mandatory && string.IsNullOrEmpty(text))
            return false;
        // not mandatory and no text, so its valid
        if (!Mandatory && string.IsNullOrEmpty(text))
            return true;
        // type is email and its not valid
        if (FieldType == FieldTypeEnum.Email && !IsValidData_Email(text))
            return false;
        // type is url and its not valid
        if (FieldType == FieldTypeEnum.Url && !IsValidData_Url(text))
            return false;
        // all tests pass, we are valid
        return true;
    }

    private string ProcessAllLowercase(string text)
    {
        return AllLowerCase ? text.ToLower() : text;
    }

    private string ProcessAllowWhiteSpace(string text)
    {
        return AllowWhiteSpace ? text : text.Replace(" ", "");
    }

    private void ProcessAndSetText(string newText)
    {
        Text = ProcessUsernameFilter(
            ProcessEmailFilter(
                ProcessAllLowercase(
                    ProcessAllowWhiteSpace(newText ?? ""))));
    }

    private string ProcessEmailFilter(string text)
    {
        return FieldType == FieldTypeEnum.Email ? Regex.Replace(text, @"[^a-zA-Z0-9!#$%&'*+/=?^_`{|}~.@-]", "") : text;
    }

    private string ProcessUsernameFilter(string text)
    {
        return FieldType == FieldTypeEnum.Username ? Regex.Replace(text, @"[^a-zA-Z0-9_]", "") : text;
    }

    private void UpdateNotificationMessage(string text)
    {
        var validationState = CalculateValidationState(text);
        string notificationMessage = "";
        bool isNotificationVisible = false;

        switch (validationState)
        {
            case ValidationStateEnum.RequiredFieldError:
                notificationMessage = "Field is required.";
                isNotificationVisible = true;
                break;
            case ValidationStateEnum.FormatError:
                notificationMessage = FieldType == FieldTypeEnum.Email ? "Invalid email format." : "Invalid URL.";
                isNotificationVisible = true;
                break;
        }

        _notificationLabel.Text = notificationMessage;
        _notificationLabel.IsVisible = isNotificationVisible;

        ValidationState = validationState; // Set the validation state based on calculated value
    }

    private void UpdateValidationState()
    {
        EvaluateToRaiseHasChangesEvent();
        EvaluateToRaiseValidationChangesEvent();
    }
}
