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
    public static readonly BindableProperty AllLowerCaseProperty = BindableProperty.Create(
        propertyName: nameof(AllLowerCase),
        returnType: typeof(bool),
        declaringType: typeof(TextBox),
        defaultValue: false);

    public static readonly BindableProperty AllowWhiteSpaceProperty = BindableProperty.Create(
        propertyName: nameof(AllowWhiteSpace),
        returnType: typeof(bool),
        declaringType: typeof(TextBox),
        defaultValue: true);

    public static readonly BindableProperty ChangeStateProperty = BindableProperty.Create(
        propertyName: nameof(ChangeState),
        returnType: typeof(ChangeStateEnum),
        declaringType: typeof(TextBox),
        defaultValue: ChangeStateEnum.NotChanged,
        defaultBindingMode: BindingMode.TwoWay);

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

    public static readonly BindableProperty MandatoryProperty = BindableProperty.Create(
        propertyName: nameof(Mandatory),
        returnType: typeof(bool),
        declaringType: typeof(TextBox),
        defaultValue: false);


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
            _entry.MaxLength = (int)newValue;
    }

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

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        propertyName: nameof(Text),
        returnType: typeof(string),
        declaringType: typeof(TextBox),
        defaultValue: string.Empty,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnTextChanged);

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextBox)bindable;
        control._entry.Text = (string)newValue;
    }

    public static readonly BindableProperty ValidationStateProperty = BindableProperty.Create(
        propertyName: nameof(ValidationState),
        returnType: typeof(ValidationStateEnum),
        declaringType: typeof(TextBox),
        defaultValue: ValidationStateEnum.Valid,
        defaultBindingMode: BindingMode.TwoWay);

    public Entry _entry;
    public Button _buttonUndo;
    public Label _label;
    public Label _labelNotification;
    private bool _isOriginalTextSet = false;
    private string _originalText = string.Empty;
    private bool _previousHasChangedState = false;
    private bool _previousInvalidDataState = false;

    public TextBox()
    {


        void UpdateUI()
        {
            _entry = CreateEntry();
            _label = CreateLabel();
            _labelNotification = CreateNotificationLabel();
            _buttonUndo = CreateUndoButton();
            Content = CreateLayoutGrid(); 
            _buttonUndo.Pressed += (s, e) => Undo();
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
        InitializeTextBox();
    }

    public event EventHandler<HasChangesEventArgs>? HasChanges;

    public event EventHandler<ValidationDataChangesEventArgs>? HasValidationChanges;

    public bool AllLowerCase { get => (bool)GetValue(AllLowerCaseProperty); set => SetValue(AllLowerCaseProperty, value); }

    public bool AllowWhiteSpace { get => (bool)GetValue(AllowWhiteSpaceProperty); set => SetValue(AllowWhiteSpaceProperty, value); }

    public ChangeStateEnum ChangeState { get => (ChangeStateEnum)GetValue(ChangeStateProperty); set => SetValue(ChangeStateProperty, value); }

    public FieldTypeEnum FieldType { get => (FieldTypeEnum)GetValue(FieldTypeProperty); set => SetValue(FieldTypeProperty, value); }

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    public bool Mandatory { get => (bool)GetValue(MandatoryProperty); set => SetValue(MandatoryProperty, value); }

    public int MaxLength { get => (int)GetValue(MaxLengthProperty); set => SetValue(MaxLengthProperty, value); }

    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

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

    public ValidationStateEnum ValidationState { get => (ValidationStateEnum)GetValue(ValidationStateProperty); set => SetValue(ValidationStateProperty, value); }

    public void Clear()
    {
        _originalText = "";
        Text = "";
        UpdateValidationState();
    }

    public void Saved()
    {
        _originalText = Text;
        UpdateValidationState();
    }

    public void Undo()
    {
        Text = _originalText;
        UpdateValidationState();
    }

    private static readonly Regex emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || email.Length < 5)
            return false;

        return emailRegex.IsMatch(email);
    }


    private static bool IsValidUrl(string urlString)
    {
        if (string.IsNullOrEmpty(urlString) || urlString.Length < 5)
            return false;

        if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uriResult))
            return false;

        return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
    }

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
        else if (FieldType == FieldTypeEnum.Email && !IsValidEmail(text))
        {
            return ValidationStateEnum.FormatError;
        }
        else if (FieldType == FieldTypeEnum.Url && !IsValidUrl(text))
        {
            return ValidationStateEnum.FormatError;
        }

        return ValidationStateEnum.Valid;
    }

    private Entry CreateEntry()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var entry = new Entry
        {
            Placeholder = Placeholder,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center
        };
#pragma warning restore CS0618 // Type or member is obsolete
        entry.TextChanged += Entry_TextChanged;
        entry.Focused += Entry_Focused;
        entry.Unfocused += Entry_Unfocused;

        return entry;
    }

    private Label CreateLabel()
    {
        return new Label
        {
            Text = Label,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };
    }

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
        Grid.SetColumnSpan(_labelNotification, 3);

        return grid;
    }

    private static Label CreateNotificationLabel()
    {
        return new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false,
            TextColor = Colors.Red
        };
    }

    private static Button CreateUndoButton()
    {
        return new Button
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            ImageSource = new ResourceHelper().GetImageSource(ButtonStateEnum.Disabled, BaseButtonTypeEnum.Undo, SizeEnum.Normal, true),
            BackgroundColor = Colors.Transparent,
            WidthRequest = 20,
            HeightRequest = 20,
            BorderWidth = 0,
            Margin = new Thickness(0),
            Padding = new Thickness(5, 0, 0, 0)
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
            void UpdateUI()
            {
                using (ResourceHelper resourceHelper = new ())
                {
                    _buttonUndo.ImageSource = resourceHelper.GetImageSource(hasChanged ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled, BaseButtonTypeEnum.Undo, SizeEnum.Normal, true);
                }
                _previousHasChangedState = hasChanged;
                ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
                HasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
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

    private void EvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
    {
        bool isValid = IsValidData(Text);
        if (_previousInvalidDataState != isValid || forceRaise)
        {
            void UpdateUI()
            {
                _previousInvalidDataState = isValid;
                ValidationState = isValid ? ValidationStateEnum.Valid : ValidationStateEnum.FormatError;
                HasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));
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

    private void InitializeTextBox()
    {
        _originalText = Text;
    }

    private bool IsValidData(string text)
    {
        // mandatory field, so its not valid
        if (Mandatory && string.IsNullOrEmpty(text))
            return false;
        // not mandatory and no text, so its valid
        if (!Mandatory && string.IsNullOrEmpty(text))
            return true;
        // type is email and its not valid
        if (FieldType == FieldTypeEnum.Email && !IsValidEmail(text))
            return false;
        // type is url and its not valid
        if (FieldType == FieldTypeEnum.Url && !IsValidUrl(text))
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

        _labelNotification.Text = notificationMessage;
        _labelNotification.IsVisible = isNotificationVisible;

        ValidationState = validationState; // Set the validation state based on calculated value
    }
    private void UpdateValidationState()
    {
        EvaluateToRaiseHasChangesEvent();
        EvaluateToRaiseValidationChangesEvent();
    }
}