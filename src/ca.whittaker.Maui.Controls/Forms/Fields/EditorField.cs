using Editor = Microsoft.Maui.Controls.Editor;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control that combines several UI elements.
/// </summary>
public partial class EditorField : BaseFormField<string>
{
    #region Fields

    private Editor _editorBox;

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
        _editorBox = new Editor
        {
            VerticalOptions = LayoutOptions.Center,
        };
        _editorBox.IsEnabled = false;
        _editorBox.TextChanged += Editor_TextChanged;

        Field_WireFocusEvents(_editorBox);

        Editor_SetPlaceholderText(EditorPlaceholder);
        Editor_SetMaxSize(EditorMaxSize);

        Field_InitializeDataSource();

        InitializeLayout();
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
                _editorBox.HeightRequest = (int)newValue * DeviceHelper.GetImageSizeForDevice(DefaultButtonSize) * 1.1;
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

    protected override Grid Field_CreateLayoutGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(FieldLabelWidth, GridUnitType.Absolute) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = new GridLength(DeviceHelper.GetImageSizeForDevice(DefaultButtonSize) * 2, GridUnitType.Absolute) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            VerticalOptions = LayoutOptions.Fill
        };
        grid.Add(FieldLabel, 0, 0);
        grid.Add(_editorBox, 1, 0);
        grid.Add(FieldButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        grid.SetColumnSpan(FieldNotification, 3);
        return grid;
    }

    protected override string Field_GetCurrentValue() => _editorBox.Text;

    protected override string Field_GetFormatErrorMessage() =>
        EditorDataType switch
        {
            EditorDataTypeEnum.Richtext => "Invalid text.",
            EditorDataTypeEnum.Plaintext => "Invalid text.",
            _ => throw new Exception("Invalid format, unspecified textbox field type")
        };

    protected override bool Field_HasChangedFromLast() =>
        FieldLastValue != (_editorBox.Text ?? String.Empty);

    protected override bool Field_HasChangedFromOriginal() =>
                FieldOriginalValue != (_editorBox.Text ?? String.Empty);

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
                _editorBox.Text = value ?? String.Empty;
            });
        });
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
    }

    // Update the _editorBox layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
    protected override void UpdateRow0Layout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                if (_editorBox!.Parent is Grid grid)
                {
                    bool isFieldLabelVisible = FieldLabelVisible;
                    bool isButtonUndoVisible = FieldUndoButton;

                    if (isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(_editorBox!, 1);
                        Grid.SetColumnSpan(_editorBox!, 1);
                    }
                    else if (isFieldLabelVisible && !isButtonUndoVisible)
                    {
                        Grid.SetColumn(_editorBox!, 1);
                        Grid.SetColumnSpan(_editorBox!, 2);
                    }
                    else if (!isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(_editorBox!, 0);
                        Grid.SetColumnSpan(_editorBox!, 2);
                    }
                    else // both not visible
                    {
                        Grid.SetColumn(_editorBox!, 0);
                        Grid.SetColumnSpan(_editorBox!, 3);
                    }
                }
            });
        });
    }

    #endregion Protected Methods

    #region Public Methods

    public override void Field_Unfocus()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                base.Field_Unfocus();
                _editorBox?.Unfocus();
            });
        });
    }

    #endregion Public Methods
}