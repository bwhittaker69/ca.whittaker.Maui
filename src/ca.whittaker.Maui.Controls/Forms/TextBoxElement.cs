using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Entry = Microsoft.Maui.Controls.Entry;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control that combines several UI elements:
/// 
/// - A Grid layout that organizes:
/// 
///   • A FieldLabel (Label) for displaying the field's title.
///   • An Entry control for text input.
///   • A ButtonUndo for reverting changes.
///   • A FieldNotification label for showing validation or error messages.
///   
/// Grid Layout Overview:
/// +-------------------+-------------------+----------------------------------+
/// | FieldLabel        | _entry            | ButtonUndo                       |
/// +-------------------+-------------------+----------------------------------+
/// | FieldNotification (spans all three columns)                              |
/// +--------------------------------------------------------------------------+
/// 
/// This composite control supports text manipulation, filtering, and validation.
/// 
/// <para>
/// Differences:
/// - Exposes its text via a dedicated two-way bindable property named <c>TextBoxSource</c>.
/// - Includes loopback prevention logic when synchronizing the Entry’s text with the binding.
/// - Intended for scenarios where robust two-way binding and extra text processing are required.
/// </para>
/// </summary>
public class TextBoxElement : BaseFormElement
{
    public static readonly BindableProperty AllLowerCaseProperty =
        BindableProperty.Create(propertyName: nameof(AllLowerCase), returnType: typeof(bool), declaringType: typeof(TextBoxElement), defaultValue: false);

    public static readonly BindableProperty AllowWhiteSpaceProperty =
        BindableProperty.Create(propertyName: nameof(AllowWhiteSpace), returnType: typeof(bool), declaringType: typeof(TextBoxElement), defaultValue: true);

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(Form));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Form));

    public static readonly BindableProperty FieldTypeProperty =
        BindableProperty.Create(propertyName: nameof(FieldType), returnType: typeof(FieldTypeEnum), declaringType: typeof(TextBoxElement), defaultValue: FieldTypeEnum.Text, propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxElement)bindable).OnFieldTypeChanged(newValue); });

    public static readonly BindableProperty MandatoryProperty =
        BindableProperty.Create(propertyName: nameof(Mandatory), returnType: typeof(bool), declaringType: typeof(TextBoxElement), defaultValue: false);

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(propertyName: nameof(MaxLength), returnType: typeof(int), declaringType: typeof(TextBoxElement), defaultValue: 255, propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxElement)bindable).OnMaxLengthPropertyChanged(newValue); });

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(propertyName: nameof(Placeholder), returnType: typeof(string), declaringType: typeof(TextBoxElement), defaultValue: string.Empty, propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxElement)bindable).OnPlaceholderPropertyChanged(newValue); });

    public static readonly BindableProperty TextBoxSourceProperty =
        BindableProperty.Create(propertyName: nameof(TextBoxSource), returnType: typeof(string), declaringType: typeof(TextBoxElement), defaultValue: string.Empty, defaultBindingMode: BindingMode.TwoWay, 
            propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxElement)bindable).OnTextBoxSourcePropertyChanged(newValue, oldValue); });


    public new LayoutOptions HorizontalOptions
    {
        get => base.HorizontalOptions;
        set
        {
            //((Grid)this.Children[0]).HorizontalOptions = value;
            //((Grid)Content).HorizontalOptions = value;
            base.HorizontalOptions = value;
        }
    }

    public new LayoutOptions VerticalOptions
    {
        get => base.VerticalOptions;
        set
        {
            ((Grid)this.Children[0]).HorizontalOptions = value;
            ((Grid)Content).VerticalOptions = value;
            base.VerticalOptions = value;
        }
    }



    private void ProcessAndSetText(string newText)
    {
        _entry.TextChanged -= Entry_TextChanged;

        string filteredValue = ProcessUsernameFilter(
                                  ProcessEmailFilter(
                                      ProcessAllLowercase(
                                          ProcessAllowWhiteSpace(newText ?? ""))));

        if (_entry.Text != filteredValue)
            _entry.Text = filteredValue;

        TextBoxSource = filteredValue;
        _entry.TextChanged += Entry_TextChanged;
    }

    private bool _isOriginalTextSet = false;
    private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;
    private static readonly Regex emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private Entry _entry;
    private bool _onTextBoxSourcePropertyChanging = false;
    private string _originalText = string.Empty;
    private bool _previousHasChangedState = false;
    private bool _previousInvalidDataState = false;
    public TextBoxElement()
    {
        _entry = new Entry
        {
            VerticalOptions = LayoutOptions.Center, // Ensure vertical centering
        };

        SetPlaceholderText(Placeholder);
        SetMaxLength(MaxLength);

        FieldLabel = CreateLabel();
        FieldNotification = CreateNotificationLabel();
        ButtonUndo = CreateUndoButton();
        Content = CreateLayoutGrid();

        ButtonUndo.Pressed += (s, e) => Undo();
        _entry.ReturnCommand = new Command(Return_Pressed);

        _entry.TextChanged += Entry_TextChanged;
        _entry.Focused += Entry_Focused;
        _entry.Unfocused += Entry_Unfocused;

    }

    private bool HasChangedFromOriginalText()
    {
        return _originalText != _entry.Text;
    }

    //protected override void OnBindingContextChanged()
    //{
    //    base.OnBindingContextChanged();
    //    if (!_isOriginalTextSet)
    //    {
    //        var initialText = this.BindingContext as string; // Adjust as per your context
    //        SetOriginalText(initialText);
    //        _isOriginalTextSet = true;
    //    }
    //}


    public bool AllLowerCase { get => (bool)GetValue(AllLowerCaseProperty); set => SetValue(AllLowerCaseProperty, value); }

    public bool AllowWhiteSpace { get => (bool)GetValue(AllowWhiteSpaceProperty); set => SetValue(AllowWhiteSpaceProperty, value); }

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

    public FieldTypeEnum FieldType { 
        get => (FieldTypeEnum)GetValue(FieldTypeProperty);
        set 
        { 
            SetValue(FieldTypeProperty, value);
            ConfigureKeyboard();
        }
    }

    public bool Mandatory { get => (bool)GetValue(MandatoryProperty); set => SetValue(MandatoryProperty, value); }

    public int MaxLength { get => (int)GetValue(MaxLengthProperty); set => SetValue(MaxLengthProperty, value); }

    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

    public string TextBoxSource { get => (string)GetValue(TextBoxSourceProperty); set { SetValue(TextBoxSourceProperty, value); } }

    public void Clear()
    {
        _entry.TextChanged -= Entry_TextChanged;
        _entry.Text = "";
        _entry.TextChanged += Entry_TextChanged;
        SetOriginalText("");
        UpdateValidationAndChangedState();
        _entry.Unfocus();
    }

    public string GetText()
    {
        return _entry.Text;
    }

    public void InitField()
    {
        SetOriginalText(TextBoxSource);
        UpdateValidationAndChangedState();
        _entry.Unfocus();
    }

    public void Saved()
    {
        SetOriginalText(_entry.Text);
        UpdateValidationAndChangedState();
        _entry.Unfocus();
    }

    public void SetOriginalText(string originalText)
    {
        _isOriginalTextSet = true;
        _originalText = originalText;
        _entry.TextChanged -= Entry_TextChanged;
        _entry.Text = originalText;
        _entry.TextChanged += Entry_TextChanged;
        EvaluateToRaiseValidationChangesEvent();
    }
    private bool _undo = false;
    public void Undo()
    {
        if (!HasChangedFromOriginalText()) return;
        if (_undo) return;
        _undo = true;
        SetOriginalText(_originalText);
        ButtonUndo?.Disabled();

        //ButtonUndo?.Disabled();
        //_entry.TextChanged -= Entry_TextChanged;
        //_entry.Text = _originalText;
        //UpdateValidationAndChangedState();
        //_entry.TextChanged += Entry_TextChanged;
        _entry.Unfocus();
        _undo = false;
    }
    private bool _disable = false;
    public void Disable()
    {
        if (_disable) return;
        _disable = true;
        SetOriginalText(_originalText);
        ButtonUndo?.Hide();
        _entry.IsEnabled = false;
        _entry.Unfocus();
        _disable = false;
    }
    private bool _enable = false;
    public void Enable()
    {
        if (_enable) return;
        _enable = true;
        SetOriginalText(_originalText);
        ButtonUndo?.Disabled();
        _entry.IsEnabled = true;
        _entry.Unfocus();
        _enable = false;
    }

    public override void Unfocus()
    {
        base.Unfocus();
        _entry.Unfocus();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        RefreshUI();
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
        RefreshUI();
    }

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
        if (Mandatory && string.IsNullOrEmpty(text))
        {
            return ValidationStateEnum.RequiredFieldError;
        }
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

    [Obsolete]
    private Entry CreateEntry()
    {
        var entry = new Entry
        {
            Placeholder = Placeholder,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.CenterAndExpand, // Ensure vertical centering
        };

        entry.TextChanged += Entry_TextChanged;
        entry.Focused += Entry_Focused;
        entry.Unfocused += Entry_Unfocused;

        return entry;
    }

    private Grid CreateLayoutGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(DeviceHelper.GetImageSizeForDevice(cUndoButtonSize) * 2, GridUnitType.Absolute)  },
                },
            RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
        };
        grid.VerticalOptions = LayoutOptions.Fill;
        grid.VerticalOptions= LayoutOptions.Fill;
        grid.Add(FieldLabel, 0, 0);
        grid.Add(_entry, 1, 0);
        grid.Add(ButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        grid.SetColumnSpan(FieldNotification, 3);

        return grid;
    }

    private void Entry_Focused(object? sender, FocusEventArgs e)
    {
    }

    private void Entry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ProcessAndSetText(e.NewTextValue);
        UpdateValidationAndChangedState();
        UpdateNotificationMessage(_entry.Text);
    }

    private void Entry_Unfocused(object? sender, FocusEventArgs e)
    {
        Debug.WriteLine("Entry_Unfocused()");
        UpdateNotificationMessage(_entry.Text);
    }
    private bool _evaluateToRaiseHasChangesEvent = false;
    bool _updatingUI = false;
    private void EvaluateToRaiseHasChangesEvent()
    {
        if (_evaluateToRaiseHasChangesEvent) return;
        _evaluateToRaiseHasChangesEvent = true;
        Debug.WriteLine("EvaluateToRaiseHasChangesEvent()");
        bool hasChanged = _originalText != _entry.Text;
        if (_previousHasChangedState != hasChanged)
        {
            void _updateUI()
            {
                if (_updatingUI) return;
                _updatingUI = true;
                using (ResourceHelper resourceHelper = new())
                {
                    if (ButtonUndo != null)
                    {
                        ButtonUndo.ImageSource =
                            resourceHelper.GetImageSource(
                                buttonState: hasChanged ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled,
                                baseButtonType: ImageResourceEnum.Undo,
                                sizeEnum: cUndoButtonSize);

                        if (hasChanged)
                        {
                            if (HasChangedFromOriginalText())
                                ButtonUndo.Enabled();
                            else
                                ButtonUndo.Disabled();
                            //ButtonUndo.Pressed += (s, e) => Undo();
                        }
                        else
                            ButtonUndo.Disabled();

                    }
                }
                _previousHasChangedState = hasChanged;
                ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
                RaiseHasChanges(hasChanged);
                _updatingUI = false;
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _updateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _updateUI());
            }
        }
        _evaluateToRaiseHasChangesEvent = false;
    }
    private bool _evaluateToRaiseValidationChangesEvent = false;

    private void EvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
    {
        if (_evaluateToRaiseValidationChangesEvent) return;
        _evaluateToRaiseValidationChangesEvent = true;
        bool isValid = IsValidData(_entry.Text);
        if (_previousInvalidDataState != isValid || forceRaise)
        {
            void UpdateUI()
            {
                _previousInvalidDataState = isValid;
                ValidationState = isValid ? ValidationStateEnum.Valid : ValidationStateEnum.FormatError;
                RaiseValidationChanges(isValid);
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
        _evaluateToRaiseValidationChangesEvent = false;
    }
    private void ConfigureKeyboard()
    {
        _entry.Keyboard = GetKeyboardForFieldType(FieldType);

    }
    private Keyboard GetKeyboardForFieldType(FieldTypeEnum fieldType)
    {
        return fieldType switch
        {
            FieldTypeEnum.Email => Keyboard.Email,
            FieldTypeEnum.Url => Keyboard.Url,
            FieldTypeEnum.Chat => Keyboard.Chat,
            _ => Keyboard.Default,
        };
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

    private void OnFieldTypeChanged(object newValue)
    {
        _entry.Keyboard = GetKeyboardForFieldType((FieldTypeEnum)newValue);
    }

    private void OnMaxLengthPropertyChanged(object newValue)
    {
        _entry.MaxLength = (int)newValue;
    }

    private void OnPlaceholderPropertyChanged(object newValue)
    {
        _entry.Placeholder = newValue?.ToString() ?? "";
    }

    private void OnTextBoxSourcePropertyChanged(object newValue, object oldValue)
    {
        if (!_isOriginalTextSet)
        {
            // prevent loop back
            if (!_onTextBoxSourcePropertyChanging)
            {
                _onTextBoxSourcePropertyChanging = true;
                _entry.TextChanged -= Entry_TextChanged;
                _entry.Text = (string)newValue;
                _entry.TextChanged += Entry_TextChanged;
                _onTextBoxSourcePropertyChanging = false;
            }
        }
    }

    private string ProcessAllLowercase(string text)
    {
        return AllLowerCase ? text.ToLower() : text;
    }

    private string ProcessAllowWhiteSpace(string text)
    {
        return AllowWhiteSpace ? text : text.Replace(" ", "");
    }


    private string ProcessEmailFilter(string text)
    {
        return FieldType == FieldTypeEnum.Email ? Regex.Replace(text, @"[^a-zA-Z0-9!#$%&'*+/=?^_`{|}~.@-]", "") : text;
    }

    private string ProcessUsernameFilter(string text)
    {
        return FieldType == FieldTypeEnum.Username ? Regex.Replace(text, @"[^a-zA-Z0-9-._]", "") : text;
    }

    private void RefreshUI()
    {
        BatchBegin();
        if (Content is Grid grid)
        {
            if (grid != null)
            {
                if (grid.ColumnDefinitions != null)
                {
                    if (grid.ColumnDefinitions.Count == 3)
                    {
                        grid.ColumnDefinitions[0].Width = new GridLength(LabelWidth, GridUnitType.Absolute);
                        grid.ColumnDefinitions[1].Width = GridLength.Star;
                        grid.ColumnDefinitions[2].Width = new GridLength(DeviceHelper.GetImageSizeForDevice(cUndoButtonSize) * 2, GridUnitType.Absolute);
                    }
                }
                grid.HeightRequest = HeightRequest;
                grid.WidthRequest = WidthRequest;
                if (FieldLabel != null)
                {
                    FieldLabel.HeightRequest = HeightRequest;
                    FieldLabel.WidthRequest = LabelWidth;
                }
                if (_entry != null)
                {
                    _entry.WidthRequest = -1;
                    _entry.HeightRequest = HeightRequest;
                }
                if (ButtonUndo != null)
                {
                    ButtonUndo.WidthRequest = DeviceHelper.GetImageSizeForDevice(cUndoButtonSize) * 2;
                    ButtonUndo.HeightRequest = HeightRequest;
                }
            }
        }
        BatchCommit();
    }

    private void Return_Pressed(object obj)
    {
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }
    private void SetMaxLength(object newValue)
    {
        if (newValue != null)
            _entry.MaxLength = (int)newValue;
    }

    private void SetPlaceholderText(object newValue)
    {
        _entry.Placeholder = newValue == null ? "" : (string)newValue;
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
        if (FieldNotification != null)
        {
            FieldNotification.Text = notificationMessage;
            FieldNotification.IsVisible = isNotificationVisible;
        }

        ValidationState = validationState; // Set the validation state based on calculated value
    }
    private bool _updateValidationAndChangedState = false;
    private void UpdateValidationAndChangedState()
    {
        if (_updateValidationAndChangedState) return;
        _updateValidationAndChangedState = true;
        EvaluateToRaiseHasChangesEvent();
        EvaluateToRaiseValidationChangesEvent();
        _updateValidationAndChangedState = false;
    }
}
