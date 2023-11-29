using System.Net.Mail;
using System.Text.RegularExpressions;
using Entry = Microsoft.Maui.Controls.Entry;
using Label = Microsoft.Maui.Controls.Label;

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
    /// Bindable property to configure the keyboard type and input validators.
    /// </summary>
    public static readonly BindableProperty FieldTypeProperty = BindableProperty.Create(
        propertyName: nameof(FieldType),
        returnType: typeof(FieldTypeEnum),
        declaringType: typeof(TextBox),
        defaultValue: FieldTypeEnum.Text,
        propertyChanged: OnFieldTypeChanged);
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
    /// Bindable property for the label of the text box.
    /// </summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        propertyName: nameof(Label),
        returnType: typeof(string),
        declaringType: typeof(TextBox),
        defaultValue: string.Empty,
        propertyChanged: OnLabelPropertyChanged);
    private static void OnLabelPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        control.SetLabelText(newValue);
    }
    private void SetLabelText(object newValue)
    {
        _label.Text = (newValue == null ? "" : newValue.ToString());
    }

    /// <summary>
    /// Bindable property to specify if the text box is mandatory.
    /// </summary>
    public static readonly BindableProperty MandatoryProperty = BindableProperty.Create(
        propertyName: nameof(Mandatory),
        returnType: typeof(bool),
        declaringType: typeof(TextBox),
        defaultValue: false,
        propertyChanged: OnMandatoryPropertyChanged);
    private static void OnMandatoryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        control.SetMandatory(newValue);
    }
    private void SetMandatory(object newValue)
    {
        if (newValue != null)
            _mandatory = (bool)newValue;
    }

    /// <summary>
    /// Bindable property to set the maximum length of the text.
    /// </summary>
    public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(
        propertyName: nameof(MaxLength),
        returnType: typeof(int),
        declaringType: typeof(TextBox),
        defaultValue: 255,
        propertyChanged: OnMaxLengthPropertyChanged);
    private static void OnMaxLengthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        control.SetMaxLength(newValue);
    }
    private void SetMaxLength(object newValue)
    {
        if (newValue != null)
            _entry.MaxLength =  (int)newValue;
    }

    /// <summary>
    /// sets the placeholder text of the textbox (value shown when no text is entered)
    /// </summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(Placeholder),
        returnType: typeof(string),
        declaringType: typeof(TextBox),
        defaultValue: string.Empty,
        propertyChanged: OnPlaceholderPropertyChanged);
    private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        control.SetPlaceholderText(newValue);
    }
    private void SetPlaceholderText(object newValue)
    {
        _entry.Placeholder = newValue == null ? "" : (string)newValue;
    }

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
    /// Called when the text property of the text box changes.
    /// </summary>
    /// <param name="bindable">The text box instance.</param>
    /// <param name="oldValue">The old text value.</param>
    /// <param name="newValue">The new text value.</param>
    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        control._entry.Text = (string)newValue;
        control._labelDebug.Text = $"ChangeState: {control.ChangeState}  ValidationState: {control.ValidationState}";
    }


    /// <summary>
    /// Bindable property to indicate the validation state of the text box.
    /// </summary>
    public static readonly BindableProperty ValidationStateProperty = BindableProperty.Create(
        propertyName: nameof(ValidationState),
        returnType: typeof(ValidationStateEnum),
        declaringType: typeof(TextBox),
        defaultValue: ValidationStateEnum.Valid,
        defaultBindingMode: BindingMode.TwoWay);

    private readonly Label _labelDebug;
    private readonly Entry _entry;
    private readonly Button _buttonUndo;
    public readonly Label _label;
    private readonly Label _labelNotification;
    private bool _isOriginalTextSet = false;
    private string _originalText = string.Empty;
    private bool _previousHasChangedState = false;
    private bool _previousInvalidDataState = false;
    private bool _mandatory = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBox"/> class.
    /// </summary>
    public TextBox()
    {

        _entry = CreateEntry();
        _label = CreateLabel();
        _labelDebug = CreateLabel();
        _labelNotification = CreateNotificationLabel();
        _buttonUndo = CreateUndoButton();

        _buttonUndo.Pressed += (s, e) => { Undo(); };

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
    /// Gets or sets the the keyboard type and validation rules for text input.
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
    /// Clears the text box original and current text values.
    /// </summary>
    public void Clear()
    {
        _originalText = "";
        Text = "";
        UpdateValidationState();
    }

    /// <summary>
    /// Text box has been saved, update original text to current text value.
    /// </summary>
    public void Saved()
    {
        _originalText = Text;
        UpdateValidationState();
    }

    /// <summary>
    /// Undo any changes, by restoring the text value to the original value.
    /// </summary>
    public void Undo()
    {
        Text = _originalText;
        UpdateValidationState();
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
            HorizontalOptions = LayoutOptions.FillAndExpand,
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
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(7, GridUnitType.Star)  },
                    new ColumnDefinition { Width = 24 }
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
        grid.Add(_buttonUndo, 2, 0);
        grid.Add(_labelNotification, 0, 1);
        //grid.Add(_labelDebug, 0, 2);
        Grid.SetColumnSpan(_labelNotification, 3);
        Grid.SetColumnSpan(_labelDebug, 3);


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
    private Button CreateUndoButton()
    {
        return new Button
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            ImageSource = CalcUndoImage(false),
            BackgroundColor = Colors.Transparent,
            WidthRequest = 20,
            HeightRequest = 20,
            BorderWidth = 0,
            Margin = new Thickness(0, 0, 0, 0),
            Padding = new Thickness(5, 0, 0, 0) 
        };
    }

    /// <summary>
    /// Handler for the Focused event of the entry control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void Entry_Focused(object? sender, FocusEventArgs e)
    {


    }

    /// <summary>
    /// Handler for the TextChanged event of the entry control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data containing old and new text values.</param>
    private void Entry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ProcessAndSetText(e.NewTextValue);
        UpdateValidationState();
        UpdateNotificationMessage(Text);
    }

    /// <summary>
    /// Handler for the Unfocused event of the entry control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void Entry_Unfocused(object? sender, FocusEventArgs e)
    {
        UpdateNotificationMessage(Text);
    }


    private ImageSource CalcUndoImage(bool hasChanged)
    {
        var assembly = this.GetType().Assembly;
        string? assemblyName = assembly.GetName().Name;
        AppTheme? currentTheme = Application.Current.RequestedTheme;
        string lightThemeEnabled = "_disabled";
        string lightThemeDisabled = "";
        string darkThemeEnabled = "";
        string darkThemeDisabled = "_disabled";
        string enabled = "";
        string disabled = "";
        if (currentTheme == AppTheme.Dark)
        {
            enabled = darkThemeEnabled;
            disabled = darkThemeDisabled;
        }
        else if (currentTheme == AppTheme.Light)
        {
            enabled = lightThemeEnabled;
            disabled = lightThemeDisabled;
        }
        else
        {
            enabled = lightThemeEnabled;
            disabled = lightThemeDisabled;
        }

        string resourceName = $"{assemblyName}.Resources.Images.undo_12_mauiimage{(hasChanged ? disabled : enabled)}.png";
        return ImageSource.FromResource(resourceName, assembly);
    }

    /// <summary>
    /// Evaluates and raises the HasChanges event if the text has been modified.
    /// </summary>
    private void EvaluateToRaiseHasChangesEvent()
    {
        bool hasChanged = _originalText != Text;
        if (_previousHasChangedState != hasChanged)
        {
            _buttonUndo.ImageSource = CalcUndoImage(hasChanged);
            _previousHasChangedState = hasChanged;
            ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
            HasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
        }
    }

    /// <summary>
    /// Evaluates and raises the ValidationChanges event based on the current validation state.
    /// </summary>
    /// <param name="forceRaise">Forces the event to be raised even if there's no change in validation state.</param>
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

    /// <summary>
    /// Initializes the properties of the text box.
    /// </summary>
    private void InitializeProperties()
    {
        _originalText = Text;
    }

    /// <summary>
    /// Checks if the entered data is valid based on the current field type and mandatory status.
    /// </summary>
    /// <param name="text">The text to validate.</param>
    /// <returns>True if the data is valid, otherwise false.</returns>    
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

    /// <summary>
    /// Processes the text to apply the AllLowerCase filter.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>Processed text with lowercase applied if needed.</returns>
    private string ProcessAllLowercase(string text)
    {
        return AllLowerCase ? text.ToLower() : text;
    }

    /// <summary>
    /// Processes the text to apply the AllowWhiteSpace filter.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>Processed text with or without white spaces.</returns>
    private string ProcessAllowWhiteSpace(string text)
    {
        return AllowWhiteSpace ? text : text.Replace(" ", "");
    }

    /// <summary>
    /// Processes the new text value and applies various filters based on the text box settings.
    /// </summary>
    /// <param name="newText">The new text to process and set.</param>
    private void ProcessAndSetText(string newText)
    {
        Text = ProcessUsernameFilter(
            ProcessEmailFilter(
                ProcessAllLowercase(
                    ProcessAllowWhiteSpace(newText ?? ""))));
    }

    /// <summary>
    /// Processes the text to apply the email format filter.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>Processed text formatted as an email.</returns>
    private string ProcessEmailFilter(string text)
    {
        return FieldType == FieldTypeEnum.Email ? Regex.Replace(text, @"[^a-zA-Z0-9!#$%&'*+/=?^_`{|}~.@-]", "") : text;
    }

    /// <summary>
    /// Processes the text to apply the username format filter.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>Processed text formatted as a username.</returns>
    private string ProcessUsernameFilter(string text)
    {
        return FieldType == FieldTypeEnum.Username ? Regex.Replace(text, @"[^a-zA-Z0-9_]", "") : text;
    }

    /// <summary>
    /// Updates the notification message based on the current validation state of the text.
    /// </summary>
    /// <param name="text">The text to validate and update the notification for.</param>
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

        _labelNotification.Text = notificationMessage;
        _labelNotification.IsVisible = isNotificationVisible;

        ValidationState = validationState; // Set the validation state based on calculated value
    }

    /// <summary>
    /// Updates the validation state and raises relevant events based on the current text.
    /// </summary>
    private void UpdateValidationState()
    {
        EvaluateToRaiseHasChangesEvent();
        EvaluateToRaiseValidationChangesEvent();
    }
}
