using Microsoft.Maui.Graphics.Platform;
using System.Diagnostics;
//using BorderEntry = Microsoft.Maui.Controls.Entry;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control that combines several UI elements.
/// </summary>
public partial class TextBoxField : BaseFormField<string>
{
    protected override void Field_ConfigAccessModeEditing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_HidePlaceholders();

            _textBox.IsVisible = true;
            _textBox.IsEnabled = true;
            _textBox.InputTransparent = false;

            Field_RefreshLayout();
        });
    }

    protected override void Field_ConfigAccessModeViewing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            ControlVisualHelper.UnfocusDescendantControls(this);

            _textBox.IsVisible = false;
            _textBox.IsEnabled = false;

            Field_ShowPlaceholders();
            Field_RefreshLayout();
        });
    }


    protected override string Field_GetDisplayText()
    {
        if (_textBox?.Text == null)
            return string.Empty;
        return _textBox?.Text ?? String.Empty;
    }
    protected override string? Field_GetCurrentValue() => _textBox.Text;
    protected override List<View> Field_GetControls() => new List<View>() { _textBox };
    #region Fields

    private UiEntry _textBox;

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

    public static readonly BindableProperty TextBoxDataTypeProperty = BindableProperty.Create(
        propertyName: nameof(TextBoxDataType),
        returnType: typeof(TextBoxDataTypeEnum),
        declaringType: typeof(TextBoxField),
        defaultValue: TextBoxDataTypeEnum.Plaintext,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((TextBoxField)bindable).OnTextBoxDataTypeChanged(newValue));

    public static readonly BindableProperty TextBoxMaxLengthProperty = BindableProperty.Create(
        propertyName: nameof(TextBoxMaxLength),
        returnType: typeof(int),
        declaringType: typeof(TextBoxField),
        defaultValue: 255,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((TextBoxField)bindable).OnTextBoxMaxLengthPropertyChanged(newValue));

    public static readonly BindableProperty TextBoxPlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(TextBoxPlaceholder),
        returnType: typeof(string),
        declaringType: typeof(TextBoxField),
        defaultValue: string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((TextBoxField)bindable).OnTextBoxPlaceholderPropertyChanged(newValue));

    #endregion Fields

    #region Public Constructors
    public TextBoxField() : base()
    {
        _textBox = new UiEntry
        {
            VerticalOptions = new LayoutOptions(LayoutAlignment.Fill, true),
            HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true),
            ZIndex = 10
        };

        SaveBackgroundColor((SolidColorBrush?)(Color)_textBox.BackgroundColor);

        _textBox.IsEnabled = true;
        _textBox.ReturnCommand = new Command(TextBox_ReturnPressedCommand);
        _textBox.TextChanged += TextBox_TextChanged;

        TextBox_SetPlaceholderText(TextBoxPlaceholder);
        TextBox_SetMaxLength(TextBoxMaxLength);

        Initialize();

        SaveTextColor(this.FieldLabel?.TextColor);

    }

    #endregion Public Constructors

    #region Properties

    public bool TextBoxAllLowerCase
    {
        get => (bool)GetValue(TextBoxAllLowerCaseProperty);
        set => SetValue(TextBoxAllLowerCaseProperty, value);
    }

    public bool TextBoxAllowWhiteSpace
    {
        get => (bool)GetValue(TextBoxAllowWhiteSpaceProperty);
        set => SetValue(TextBoxAllowWhiteSpaceProperty, value);
    }

    public TextBoxDataTypeEnum TextBoxDataType
    {
        get => (TextBoxDataTypeEnum)GetValue(TextBoxDataTypeProperty);
        set
        {
            SetValue(TextBoxDataTypeProperty, value);
            TextBox_ConfigureKeyboard();
        }
    }

    public int TextBoxMaxLength
    {
        get => (int)GetValue(TextBoxMaxLengthProperty);
        set => SetValue(TextBoxMaxLengthProperty, value);
    }

    public string TextBoxPlaceholder
    {
        get => (string)GetValue(TextBoxPlaceholderProperty);
        set => SetValue(TextBoxPlaceholderProperty, value);
    }

    #endregion Properties

    #region Private Methods

    private void OnTextBoxDataTypeChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _textBox.Keyboard = TextBox_GetKeyboardForFieldType((TextBoxDataTypeEnum)newValue);
            });
        });
    }

    private void OnTextBoxMaxLengthPropertyChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _textBox.MaxLength = (int)newValue;
            });
        });
    }

    private void OnTextBoxPlaceholderPropertyChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _textBox.Placeholder = newValue?.ToString() ?? "";
            });
        });
    }

    private void TextBox_ConfigureKeyboard()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _textBox.Keyboard = TextBox_GetKeyboardForFieldType(TextBoxDataType);
            });
        });
    }

    private Keyboard TextBox_GetKeyboardForFieldType(TextBoxDataTypeEnum fieldType) =>
        fieldType switch
        {
            TextBoxDataTypeEnum.Email => Keyboard.Email,
            TextBoxDataTypeEnum.Url => Keyboard.Url,
            TextBoxDataTypeEnum.Richtext => Keyboard.Chat,
            TextBoxDataTypeEnum.Plaintext => Keyboard.Plain,
            TextBoxDataTypeEnum.Numeric => Keyboard.Numeric,
            TextBoxDataTypeEnum.Currency => Keyboard.Numeric,
            TextBoxDataTypeEnum.Integer => Keyboard.Numeric,
            _ => Keyboard.Default,
        };

    private void TextBox_ProcessAndSetText(string newText)
    {
        string filteredValue = InputValidator.FilterUsernameFilter(TextBoxDataType == TextBoxDataTypeEnum.Username,
                                    InputValidator.FilterEmailFilter(TextBoxDataType == TextBoxDataTypeEnum.Email,
                                        InputValidator.FilterAllLowercase(TextBoxAllLowerCase != TextBoxAllLowerCase,
                                            InputValidator.FilterAllowWhiteSpace(TextBoxAllowWhiteSpace != TextBoxAllowWhiteSpace,
                                                InputValidator.FilterPlaintext(TextBoxDataType == TextBoxDataTypeEnum.Plaintext,
                                                    InputValidator.FilterRichtext(TextBoxDataType == TextBoxDataTypeEnum.Richtext,
                                                        InputValidator.FilterNumeric(TextBoxDataType == TextBoxDataTypeEnum.Numeric,
                                                            InputValidator.FilterNumeric(TextBoxDataType == TextBoxDataTypeEnum.Currency,
                                                                InputValidator.FilterNumeric(TextBoxDataType == TextBoxDataTypeEnum.Integer,
                                                                    InputValidator.FilterSingleLine(filter: true, newText)
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                );
        if (_textBox.Text != filteredValue)
        {
            UiThreadHelper.RunOnMainThread(() =>
            {
                Field_PerformBatchUpdate(() =>
                {
                    _textBox.Text = filteredValue;
                });
            });
        }
        Field_SetDataSourceValue(filteredValue);
    }

    private void TextBox_ReturnPressedCommand(object obj)
    {
        if (FieldCommand?.CanExecute(FieldCommandParameter) == true)
            FieldCommand.Execute(FieldCommandParameter);
    }

    private void TextBox_SetMaxLength(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                if (newValue != null)
                    _textBox.MaxLength = (int)newValue;
            });
        });
    }

    private void TextBox_SetPlaceholderText(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _textBox.Placeholder = newValue == null ? "" : (string)newValue;
            });
        });
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        TextBox_ProcessAndSetText(e.NewTextValue);
        Field_UpdateValidationAndChangedState();
        Field_UpdateNotificationMessage();
        FieldLastValue = e.NewTextValue;
    }

    #endregion Private Methods

    #region Protected Methods
    protected override string Field_GetFormatErrorMessage() =>
                TextBoxDataType switch
                {
                    TextBoxDataTypeEnum.Email => "Invalid email format.",
                    TextBoxDataTypeEnum.Url => "Invalid URL.",
                    TextBoxDataTypeEnum.Username => "Invalid Username.",
                    TextBoxDataTypeEnum.Richtext => "Invalid message.",
                    TextBoxDataTypeEnum.Plaintext => "Invalid text.",
                    TextBoxDataTypeEnum.Currency => "Invalid format.",
                    TextBoxDataTypeEnum.Numeric => "Invalid format.",
                    TextBoxDataTypeEnum.Integer => "Invalid format.",
                    _ => throw new Exception("Invalid format, unspecified textbox field type")
                };

    protected override bool Field_HasChangedFromLast() =>
        !FieldAreValuesEqual(FieldLastValue, _textBox.Text);

    protected override bool Field_HasChangedFromOriginal()
    {
        return !FieldAreValuesEqual(FieldOriginalValue, _textBox?.Text);
    }

    protected override bool Field_HasFormatError()
    {
        if (String.IsNullOrEmpty(Field_GetCurrentValue()))
            return Field_HasRequiredError();
        if (TextBoxDataType == TextBoxDataTypeEnum.Email && !InputValidator.IsValidEmail(_textBox.Text))
            return true;
        if (TextBoxDataType == TextBoxDataTypeEnum.Username && !InputValidator.IsValidUsername(_textBox.Text))
            return true;
        if (TextBoxDataType == TextBoxDataTypeEnum.Url && !InputValidator.IsValidUrl(_textBox.Text))
            return true;
        if (TextBoxDataType == TextBoxDataTypeEnum.Richtext && !InputValidator.IsValidRichtext(_textBox.Text))
            return true;
        if (TextBoxDataType == TextBoxDataTypeEnum.Plaintext && !InputValidator.IsValidPlaintext(_textBox.Text))
            return true;
        if (TextBoxDataType == TextBoxDataTypeEnum.Currency && !InputValidator.IsValidCurrency(_textBox.Text))
            return true;
        if (TextBoxDataType == TextBoxDataTypeEnum.Numeric && !InputValidator.IsValidNumeric(_textBox.Text))
            return true;
        if (TextBoxDataType == TextBoxDataTypeEnum.Integer && !InputValidator.IsValidInteger(_textBox.Text))
            return true;
        return false;
    }

    protected override bool Field_HasRequiredError() => FieldMandatory && string.IsNullOrEmpty(_textBox.Text);

    protected override void Field_OriginalValue_SetToClear()
    {
        FieldOriginalValue = String.Empty;
        Field_SetValue(FieldOriginalValue);
    }

    protected override void Field_SetValue(string? value)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _textBox.TextChanged -= TextBox_TextChanged;   // suppress recursion
                _textBox.Text = value ?? string.Empty;
                _textBox.TextChanged += TextBox_TextChanged;

                if (FieldAccessMode != FieldAccessModeEnum.Editing)
                    Field_ShowPlaceholders();
            });
        });
    }

    protected override void Field_RefreshLayout()
    {

    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
    }


    #endregion Protected Methods

}

