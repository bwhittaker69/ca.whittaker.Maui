using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Entry = Microsoft.Maui.Controls.Entry;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace ca.whittaker.Maui.Controls.Forms;
public interface ITextBoxElement : IBaseFormField
{

    #region Public Properties

    string FieldDataSource { get; set; }
    bool TextBoxAllLowerCase { get; set; }
    bool TextBoxAllowWhiteSpace { get; set; }
    TextBoxFieldTypeEnum TextBoxFieldType { get; set; }
    int TextBoxMaxLength { get; set; }
    string TextBoxPlaceholder { get; set; }

    #endregion Public Properties

    #region Public Methods

    string GetText();

    #endregion Public Methods

}


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
public partial class TextBoxField : BaseFormField, ITextBoxElement
{

    #region Private Fields

    private static readonly Regex emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private string _originalValue = string.Empty;
    private Entry _textBox;

    #endregion Private Fields

    #region Public Fields

    public static readonly BindableProperty FieldDataSourceProperty = BindableProperty.Create(
        propertyName: nameof(FieldDataSource),
        returnType: typeof(string),
        declaringType: typeof(TextBoxField),
        defaultValue: string.Empty,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxField)bindable).OnFieldDataSourcePropertyChanged(newValue, oldValue); });

    public static readonly BindableProperty TextBoxAllLowerCaseProperty = BindableProperty.Create(
            propertyName: nameof(TextBoxAllLowerCase), 
        returnType: typeof(bool), 
        declaringType: typeof(TextBoxField),
        defaultValue: false);

    public static readonly BindableProperty TextBoxAllowWhiteSpaceProperty = BindableProperty.Create(
        propertyName: nameof(TextBoxAllowWhiteSpace), 
        returnType: typeof(bool), 
        declaringType: typeof(TextBoxField), 
        defaultValue: true);
    public static readonly BindableProperty TextBoxFieldTypeProperty = BindableProperty.Create(
            propertyName: nameof(TextBoxFieldType), 
        returnType: typeof(TextBoxFieldTypeEnum), 
        declaringType: typeof(TextBoxField), 
        defaultValue: TextBoxFieldTypeEnum.Text, 
        propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxField)bindable).OnTextBoxFieldTypeChanged(newValue); });

    public static readonly BindableProperty TextBoxMaxLengthProperty = BindableProperty.Create(
        propertyName: nameof(TextBoxMaxLength), 
        returnType: typeof(int), 
        declaringType: typeof(TextBoxField),
        defaultValue: 255, 
        propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxField)bindable).OnTextBoxMaxLengthPropertyChanged(newValue); });

    public static readonly BindableProperty TextBoxPlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(TextBoxPlaceholder), 
        returnType: typeof(string), 
        declaringType: typeof(TextBoxField), 
        defaultValue: string.Empty, 
        propertyChanged: (bindable, oldValue, newValue) => { ((TextBoxField)bindable).OnTextBoxPlaceholderPropertyChanged(newValue); });

    #endregion Public Fields

    #region Public Constructors

    public TextBoxField()
    {
        // *********
        //   ENTRY
        // *********
        _textBox = new Entry
        {
            VerticalOptions = LayoutOptions.Center, // Ensure vertical centering
        };
        _textBox.ReturnCommand = new Command(TextBoxReturn_PressedCommand);
        _textBox.TextChanged += TextBox_TextChanged;
        _textBox.Focused += Field_Focused;
        _textBox.Unfocused += Field_Unfocused;

        FieldLabel = FieldCreateLabel();
        FieldNotification = FieldCreateNotificationLabel();
        ButtonUndo = FieldCreateUndoButton(fieldAccessMode: FieldAccessMode);
        Content = FieldCreateLayoutGrid();
        ButtonUndo.Pressed += OnFieldButtonUndoPressed;

        TextBoxSetPlaceholderText(TextBoxPlaceholder);
        TextBoxSetMaxLength(TextBoxMaxLength);

    }

    #endregion Public Constructors

    #region Public Properties

    public string FieldDataSource { get => (string)GetValue(FieldDataSourceProperty); set { SetValue(FieldDataSourceProperty, value); } }
    public bool TextBoxAllLowerCase { get => (bool)GetValue(TextBoxAllLowerCaseProperty); set => SetValue(TextBoxAllLowerCaseProperty, value); }
    public bool TextBoxAllowWhiteSpace { get => (bool)GetValue(TextBoxAllowWhiteSpaceProperty); set => SetValue(TextBoxAllowWhiteSpaceProperty, value); }
    public TextBoxFieldTypeEnum TextBoxFieldType
    {
        get => (TextBoxFieldTypeEnum)GetValue(TextBoxFieldTypeProperty);
        set
        {
            SetValue(TextBoxFieldTypeProperty, value);
            TextBoxConfigureKeyboard();
        }
    }

    public int TextBoxMaxLength { get => (int)GetValue(TextBoxMaxLengthProperty); set => SetValue(TextBoxMaxLengthProperty, value); }
    public string TextBoxPlaceholder { get => (string)GetValue(TextBoxPlaceholderProperty); set => SetValue(TextBoxPlaceholderProperty, value); }

    #endregion Public Properties

    #region Private Methods

    private static bool TextBoxIsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || email.Length < 5)
            return false;

        return emailRegex.IsMatch(email);
    }

    private static bool TextBoxIsValidUrl(string urlString)
    {
        if (string.IsNullOrEmpty(urlString) || urlString.Length < 5)
            return false;

        if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uriResult))
            return false;

        return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
    }


    private void OnTextBoxFieldTypeChanged(object newValue)
    {
        _textBox.Keyboard = TextBoxGetKeyboardForFieldType((TextBoxFieldTypeEnum)newValue);
    }

    private void OnTextBoxMaxLengthPropertyChanged(object newValue)
    {
        _textBox.MaxLength = (int)newValue;
    }

    private void OnTextBoxPlaceholderPropertyChanged(object newValue)
    {
        _textBox.Placeholder = newValue?.ToString() ?? "";
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
                        grid.ColumnDefinitions[0].Width = new GridLength(FieldLabelWidth, GridUnitType.Absolute);
                        grid.ColumnDefinitions[1].Width = GridLength.Star;
                        grid.ColumnDefinitions[2].Width = new GridLength(DeviceHelper.GetImageSizeForDevice(cUndoButtonSize) * 2, GridUnitType.Absolute);
                    }
                }
                grid.HeightRequest = HeightRequest;
                grid.WidthRequest = WidthRequest;
                if (FieldLabel != null)
                {
                    FieldLabel.HeightRequest = HeightRequest;
                    FieldLabel.WidthRequest = FieldLabelWidth;
                }
                if (_textBox != null)
                {
                    _textBox.WidthRequest = -1;
                    _textBox.HeightRequest = HeightRequest;
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

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        TextBoxProcessAndSetText(e.NewTextValue);
        FieldUpdateValidationAndChangedState();
        FieldUpdateNotificationMessage();
    }

    private void TextBoxConfigureKeyboard()
    {
        _textBox.Keyboard = TextBoxGetKeyboardForFieldType(TextBoxFieldType);

    }

    private Keyboard TextBoxGetKeyboardForFieldType(TextBoxFieldTypeEnum fieldType)
    {
        return fieldType switch
        {
            TextBoxFieldTypeEnum.Email => Keyboard.Email,
            TextBoxFieldTypeEnum.Url => Keyboard.Url,
            TextBoxFieldTypeEnum.Chat => Keyboard.Chat,
            _ => Keyboard.Default,
        };
    }

    private string TextBoxProcessAllLowercase(string text)
    {
        return TextBoxAllLowerCase ? text.ToLower() : text;
    }

    private string TextBoxProcessAllowWhiteSpace(string text)
    {
        return TextBoxAllowWhiteSpace ? text : text.Replace(" ", "");
    }

    private void TextBoxProcessAndSetText(string newText)
    {
        _textBox.TextChanged -= TextBox_TextChanged;

        string filteredValue = TextBoxProcessUsernameFilter(
                                  TextBoxProcessEmailFilter(
                                      TextBoxProcessAllLowercase(
                                          TextBoxProcessAllowWhiteSpace(newText ?? ""))));

        if (_textBox.Text != filteredValue)
            _textBox.Text = filteredValue;

        FieldDataSource = filteredValue;
        _textBox.TextChanged += TextBox_TextChanged;
    }

    private string TextBoxProcessEmailFilter(string text)
    {
        return TextBoxFieldType == TextBoxFieldTypeEnum.Email ? Regex.Replace(text, @"[^a-zA-Z0-9!#$%&'*+/=?^_`{|}~.@-]", "") : text;
    }

    private string TextBoxProcessUsernameFilter(string text)
    {
        return TextBoxFieldType == TextBoxFieldTypeEnum.Username ? Regex.Replace(text, @"[^a-zA-Z0-9-._]", "") : text;
    }

    private void TextBoxReturn_PressedCommand(object obj)
    {
        if (FieldCommand?.CanExecute(FieldCommandParameter) == true)
        {
            FieldCommand.Execute(FieldCommandParameter);
        }
    }

    private void TextBoxSetMaxLength(object newValue)
    {
        if (newValue != null)
            _textBox.MaxLength = (int)newValue;
    }

    private void TextBoxSetPlaceholderText(object newValue)
    {
        _textBox.Placeholder = newValue == null ? "" : (string)newValue;
    }

    #endregion Private Methods

    #region Protected Methods

    protected override Grid FieldCreateLayoutGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(FieldLabelWidth, GridUnitType.Absolute) },
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
        grid.VerticalOptions = LayoutOptions.Fill;
        grid.Add(FieldLabel, 0, 0);
        grid.Add(_textBox, 1, 0);
        grid.Add(ButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        grid.SetColumnSpan(FieldNotification, 3);

        return grid;
    }

    protected override void FieldDisable()
    {
        if (_fieldDisabling) return;
        _fieldDisabling = true;
        FieldSetOriginalValue(_originalValue);
        ButtonUndo?.Hide();
        _textBox.IsEnabled = false;
        //_textBox.Unfocus();
        _fieldDisabling = false;
    }

    protected override void FieldEnable()
    {
        if (_fieldEnabling) return;
        _fieldEnabling = true;
        FieldSetOriginalValue(_originalValue);
        ButtonUndo?.Disabled();
        _textBox.IsEnabled = true;
        FieldUnfocus();
        _fieldEnabling = false;
    }

    protected override string FieldGetFormatErrorMessage()
    {
        switch (TextBoxFieldType)
        {
            case TextBoxFieldTypeEnum.Email: return "Invalid email format.";
            case TextBoxFieldTypeEnum.Url: return "Invalid URL.";
            case TextBoxFieldTypeEnum.Username: return "Invalid Username.";
            case TextBoxFieldTypeEnum.Chat: return "Invalid message.";
            case TextBoxFieldTypeEnum.Text: return "Invalid text.";
            default: throw new Exception("invalid format, unspecified textbox field type");
        }
    }

    protected override bool FieldHasChanged()
    {
        return _originalValue != _textBox.Text;
    }
    protected override bool FieldHasFormatError()
    {
        if (!FieldMandatory && string.IsNullOrEmpty(_textBox.Text))
            // no error:  not mandatory, and empty value
            return false;

        if (TextBoxFieldType == TextBoxFieldTypeEnum.Email && !TextBoxIsValidEmail(_textBox.Text))
        {
            // error:  invalid email format
            return true;
        }
        else if (TextBoxFieldType == TextBoxFieldTypeEnum.Url && !TextBoxIsValidUrl(_textBox.Text))
        {
            // error:  invalid url format
            return true;
        }
        // no error: otherwise
        return false;
    }

    protected override bool FieldHasRequiredError()
    {
        string text = _textBox.Text;
        if (FieldMandatory)
        {
            if (string.IsNullOrEmpty(text))
                // mandatory, but no text
                return true;
            else
                // mandatory, has text
                return false;
        }
        // not mandatory
        return false;
    }

    protected override bool FieldHasValidData()
    {
        string text = _textBox.Text;
        if (FieldHasRequiredError())
            // invalid: mandatory field, but no text is entered
            return false;
        if (!FieldMandatory && string.IsNullOrEmpty(text))
            // valid: not mandatory, and no text is entered
            return true; 
        if (TextBoxFieldType == TextBoxFieldTypeEnum.Email && !TextBoxIsValidEmail(text))
            // invalid: not email format
            return false;
        if (TextBoxFieldType == TextBoxFieldTypeEnum.Url && !TextBoxIsValidUrl(text))
            // invalid: not url format
            return false;
        // all tests pass, we are valid
        return true;
    }

    protected void FieldSetOriginalValue(string originalValue)
    {
        _fieldIsOriginalValueSet = true;
        _originalValue = originalValue;

        _textBox.TextChanged -= TextBox_TextChanged;
        _textBox.Text = originalValue;
        _textBox.TextChanged += TextBox_TextChanged;
        FieldEvaluateToRaiseValidationChangesEvent();
    }

    protected override void OnFieldButtonUndoPressed(object? sender, EventArgs e)
    {
        if (!FieldHasChanged()) return;
        if (_undoing) return;
        _undoing = true;
        FieldSetOriginalValue(_originalValue);
        ButtonUndo?.Disabled();
        //_textBox.Unfocus();
        _undoing = false;
    }

    protected override void OnFieldDataSourcePropertyChanged(object newValue, object oldValue)
    {
        if (!_fieldIsOriginalValueSet)
        {
            // prevent loop back
            if (!_onFieldDataSourcePropertyChanging)
            {
                _onFieldDataSourcePropertyChanging = true;
                _textBox.TextChanged -= TextBox_TextChanged;
                _textBox.Text = (string)newValue;
                _textBox.TextChanged += TextBox_TextChanged;
                _onFieldDataSourcePropertyChanging = false;
            }
        }
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

    #endregion Protected Methods

    #region Public Methods

    public override void FieldClear()
    {
        _textBox.TextChanged -= TextBox_TextChanged;
        _textBox.Text = "";
        _textBox.TextChanged += TextBox_TextChanged;
        FieldSetOriginalValue("");
        FieldUpdateValidationAndChangedState();
        //_textBox.Unfocus();
    }
    // make field editable, set original value from data source
    public override void FieldMarkAsEditable()
    {
        FieldAccessMode = FieldAccessModeEnum.Editable;
        FieldSetOriginalValue(FieldDataSource);
        FieldEnable();
        FieldUpdateValidationAndChangedState();
    }
    // make field readonly, and revert back to original value
    public override void FieldMarkAsReadOnly()
    {
        FieldAccessMode = FieldAccessModeEnum.ReadOnly;
        FieldSetOriginalValue(_textBox.Text);
        FieldDisable();
        FieldUpdateValidationAndChangedState();
    }
    // make field readonly, and save the current value as original value
    public override void FieldSavedAndMarkAsReadOnly()
    {
        FieldAccessMode = FieldAccessModeEnum.ReadOnly;
        FieldSetOriginalValue(_textBox.Text);
        FieldDisable(); 
        FieldUpdateValidationAndChangedState();
    }
    // make field hidden
    public override void FieldHide()
    {
        FieldAccessMode = FieldAccessModeEnum.Hidden;
        _textBox.IsVisible = false;
        FieldLabel!.IsVisible = false;
        FieldNotification!.IsVisible = false;
        ButtonUndo!.IsVisible = false;
        Content.IsVisible = false;
    }
    protected override void FieldUnhide()
    {
        _textBox.IsVisible = true;
        FieldLabel!.IsVisible = true;
        Content.IsVisible = true;
    }
    public override void FieldUnfocus()
    {
        base.FieldUnfocus();
        _textBox.Unfocus();
    }

    public string GetText()
    {
        return _textBox.Text;
    }

    #endregion Public Methods

}
