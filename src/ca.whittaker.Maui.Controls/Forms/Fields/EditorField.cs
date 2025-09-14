using Editor = Microsoft.Maui.Controls.Editor;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control that combines several UI elements.
/// </summary>
public partial class EditorField : BaseFormField<string>
{
    protected override void Field_ConfigAccessModeEditing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_HidePlaceholders();

            Field_PerformBatchUpdate(() =>
            {
                _editorBox.IsVisible = true;
                _editorBox.IsEnabled = true;
                _editorBox.InputTransparent = false;
            });

            Field_RefreshLayout();
        });
    }

    protected override void Field_ConfigAccessModeViewing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            ControlVisualHelper.UnfocusDescendantControls(this);

            Field_PerformBatchUpdate(() =>
            {
                _editorBox.IsVisible = false;
                _editorBox.IsEnabled = false;
            });

            Field_ShowPlaceholders();
            Field_RefreshLayout();
        });
    }

    protected override string Field_GetDisplayText()
    {
        if (_editorBox?.Text == null)
            return string.Empty;
        return _editorBox?.Text ?? String.Empty;
    }
    protected override List<View> Field_GetControls() => new List<View>() { _editorBox };
    protected override string? Field_GetCurrentValue() => _editorBox.Text;
    #region Fields

    private UiEditor _editorBox;

    public static readonly BindableProperty EditorDataTypeProperty = BindableProperty.Create(
        propertyName: nameof(EditorDataType),
        returnType: typeof(EditorDataTypeEnum),
        declaringType: typeof(EditorField),
        defaultValue: EditorDataTypeEnum.Plaintext,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((EditorField)bindable).OnEditorDataTypeChanged(newValue));

    public static readonly BindableProperty EditorMaxSizeProperty = BindableProperty.Create(
        propertyName: nameof(EditorMaxSize),
        returnType: typeof(int),
        declaringType: typeof(EditorField),
        defaultValue: 512,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((EditorField)bindable).OnEditorMaxSizePropertyChanged(newValue));

    public static readonly BindableProperty EditorPlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(EditorPlaceholder),
        returnType: typeof(string),
        declaringType: typeof(EditorField),
        defaultValue: string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((EditorField)bindable).OnEditorPlaceholderPropertyChanged(newValue));

    public static readonly BindableProperty EditorRowCountProperty = BindableProperty.Create(
            propertyName: nameof(EditorRowCount),
        returnType: typeof(int),
        declaringType: typeof(EditorField),
        defaultValue: 5,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((EditorField)bindable).OnEditorRowCountPropertyChanged(newValue));

    #endregion Fields

    #region Public Constructors

    public EditorField()
    {
        _editorBox = new UiEditor
        {
            VerticalOptions = LayoutOptions.Fill,   // CHANGED (was Center)
            HorizontalOptions = LayoutOptions.Fill,
            MinimumHeightRequest = 0,
            MinimumWidthRequest = 0
        };

        _editorBox.IsEnabled = true;
        _editorBox.TextChanged += Editor_TextChanged;

        Editor_SetPlaceholderText(EditorPlaceholder);
        Editor_SetMaxSize(EditorMaxSize);

        // Initialize 
        Initialize();

        SaveTextColor(FieldLabel?.TextColor);
        SaveBorderWidth(_editorBox.BorderWidth);

    }
    #endregion Public Constructors

    #region Properties

    public EditorDataTypeEnum EditorDataType
    {
        get => (EditorDataTypeEnum)GetValue(EditorDataTypeProperty);
        set
        {
            SetValue(EditorDataTypeProperty, value);
            Editor_ConfigureKeyboard();
        }
    }

    public int EditorMaxSize
    {
        get => (int)GetValue(EditorMaxSizeProperty);
        set => SetValue(EditorMaxSizeProperty, value);
    }

    public string EditorPlaceholder
    {
        get => (string)GetValue(EditorPlaceholderProperty);
        set => SetValue(EditorPlaceholderProperty, value);
    }

    public int EditorRowCount
    {
        get => (int)GetValue(EditorRowCountProperty);
        set => SetValue(EditorRowCountProperty, value);
    }

    #endregion Properties

    #region Private Methods

    private void Editor_ConfigureKeyboard()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _editorBox.Keyboard = Editor_GetKeyboardForFieldType(EditorDataType);
            });
        });
    }

    private Keyboard Editor_GetKeyboardForFieldType(EditorDataTypeEnum fieldType) =>
        fieldType switch
        {
            EditorDataTypeEnum.Richtext => Keyboard.Chat,
            EditorDataTypeEnum.Plaintext => Keyboard.Plain,
            _ => Keyboard.Default,
        };

    private void Editor_ProcessAndSetText(string newText)
    {
        string filteredValue = InputValidator.FilterPlaintext(EditorDataType == EditorDataTypeEnum.Plaintext,
                                    InputValidator.FilterRichtext(EditorDataType == EditorDataTypeEnum.Richtext, newText)
                               );
        if (_editorBox.Text != filteredValue)
            _editorBox.Text = filteredValue;
        Field_SetDataSourceValue(filteredValue);
    }

    private void Editor_SetMaxSize(object newValue)
    {
        if (newValue != null)
            _editorBox.MaxLength = (int)newValue;
    }

    private void Editor_SetPlaceholderText(object newValue) =>
        _editorBox.Placeholder = newValue == null ? "" : (string)newValue;

    private void Editor_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Editor_ProcessAndSetText(e.NewTextValue);
        Field_UpdateValidationAndChangedState();
        Field_UpdateNotificationMessage();
        FieldLastValue = e.NewTextValue;
    }

    private void OnEditorDataTypeChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _editorBox.Keyboard = Editor_GetKeyboardForFieldType((EditorDataTypeEnum)newValue);
            });
        });
    }

    private void OnEditorMaxSizePropertyChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _editorBox.MaxLength = (int)newValue;
            });
        });
    }

    private void OnEditorPlaceholderPropertyChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _editorBox.Placeholder = newValue?.ToString() ?? "";
            });
        });
    }

    private void OnEditorRowCountPropertyChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                // Only apply a row-based height when no explicit FieldHeightRequest was set
                if (!(FieldHeightRequest is double h) || h <= 0)
                {
                    _editorBox.HeightRequest =
                        (int)newValue * DeviceHelper.GetImageSizeForDevice(DefaultButtonSize) * 1.1;
                }
            });
        });
    }


    private void SetEditorValue(string? value)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _editorBox.Text = value ?? String.Empty;
            });
        });
    }

    #endregion Private Methods

    #region Protected Methods

    protected override string Field_GetFormatErrorMessage() =>
        EditorDataType switch
        {
            EditorDataTypeEnum.Richtext => "Invalid text.",
            EditorDataTypeEnum.Plaintext => "Invalid text.",
            _ => throw new Exception("Invalid format, unspecified textbox field type")
        };


    protected override bool Field_HasChangedFromLast() =>
        !FieldAreValuesEqual(FieldLastValue, _editorBox.Text);

    protected override bool Field_HasChangedFromOriginal() =>
        !FieldAreValuesEqual(FieldOriginalValue, _editorBox.Text);


    protected override bool Field_HasFormatError()
    {
        if (EditorDataType == EditorDataTypeEnum.Richtext && !InputValidator.IsValidRichtext(_editorBox.Text))
            return true;
        if (EditorDataType == EditorDataTypeEnum.Plaintext && !InputValidator.IsValidPlaintext(_editorBox.Text))
            return true;
        return false;
    }

    protected override bool Field_HasRequiredError() =>
        FieldMandatory && string.IsNullOrEmpty(_editorBox.Text);

    protected override void Field_OriginalValue_SetToClear()
    {
        FieldOriginalValue = String.Empty;
        SetEditorValue(FieldOriginalValue);
    }

    protected override void Field_SetValue(string? value)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _editorBox.TextChanged -= Editor_TextChanged;
                _editorBox.Text = value ?? string.Empty;
                _editorBox.TextChanged += Editor_TextChanged;

                if (FieldAccessMode != FieldAccessModeEnum.Editing)
                    Field_ShowPlaceholders();
            });
        });
    }

    protected override void Field_RefreshLayout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            if (Content is Grid grid)
            {
                grid.InvalidateMeasure();
            }
            InvalidateMeasure();
        });
    }
    private void SyncInnerEditorHeightFromField()
    {
        if (FieldHeightRequest is double h && h > 0)
        {
            _editorBox.AutoSize = EditorAutoSizeOption.Disabled; // <- important
            _editorBox.MinimumHeightRequest = 0;
            _editorBox.HeightRequest = h;
            _editorBox.VerticalOptions = LayoutOptions.Fill;
        }
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        SyncInnerEditorHeightFromField();
    }


    #endregion Protected Methods


}