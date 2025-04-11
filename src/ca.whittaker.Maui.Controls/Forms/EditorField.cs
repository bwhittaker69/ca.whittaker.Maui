using System.Diagnostics;
using System.Text.RegularExpressions;
using Editor = Microsoft.Maui.Controls.Editor;

namespace ca.whittaker.Maui.Controls.Forms
{
    /// <summary>
    /// Represents a customizable text box control that combines several UI elements.
    /// </summary>
    public partial class EditorField : BaseFormField
    {
        #region Fields

        private string _lastValue = string.Empty;
        private string _originalValue = string.Empty;
        private Editor _editorBox;

        public static readonly BindableProperty EditorDataSourceProperty = BindableProperty.Create(
            propertyName: nameof(EditorDataSource),
            returnType: typeof(string),
            declaringType: typeof(EditorField),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
                ((EditorField)bindable).OnDataSourcePropertyChanged(newValue, oldValue));

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

        public static readonly BindableProperty EditorRowCountProperty = BindableProperty.Create(
            propertyName: nameof(EditorRowCount),
            returnType: typeof(int),
            declaringType: typeof(EditorField),
            defaultValue: 5,
            propertyChanged: (bindable, oldValue, newValue) =>
                ((EditorField)bindable).OnEditorRowCountPropertyChanged(newValue));

        public static readonly BindableProperty EditorPlaceholderProperty = BindableProperty.Create(
            propertyName: nameof(EditorPlaceholder),
            returnType: typeof(string),
            declaringType: typeof(EditorField),
            defaultValue: string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
                ((EditorField)bindable).OnEditorPlaceholderPropertyChanged(newValue));

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
            _editorBox.Focused += Field_Focused;
            _editorBox.Unfocused += Field_Unfocused;

            Editor_SetPlaceholderText(EditorPlaceholder);
            Editor_SetMaxSize(EditorMaxSize);

            InitializeLayout();
        }

        #endregion Public Constructors

        #region Properties


        public string EditorDataSource
        {
            get => (string)GetValue(EditorDataSourceProperty);
            set => SetValue(EditorDataSourceProperty, value);
        }

        public EditorDataTypeEnum EditorDataType
        {
            get => (EditorDataTypeEnum)GetValue(EditorDataTypeProperty);
            set
            {
                SetValue(EditorDataTypeProperty, value);
                Editor_ConfigureKeyboard();
            }
        }

        public int EditorRowCount
        {
            get => (int)GetValue(EditorRowCountProperty);
            set => SetValue(EditorRowCountProperty, value);
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

        #endregion Properties

        #region Private Methods

        private void OnEditorDataTypeChanged(object newValue) =>
            _editorBox.Keyboard = Editor_GetKeyboardForFieldType((EditorDataTypeEnum)newValue);

        private void OnEditorRowCountPropertyChanged(object newValue) =>
            _editorBox.HeightRequest = (int)newValue * DeviceHelper.GetImageSizeForDevice(DefaultButtonSize) * 1.1;

        private void OnEditorMaxSizePropertyChanged(object newValue) =>
            _editorBox.MaxLength = (int)newValue;

        private void OnEditorPlaceholderPropertyChanged(object newValue) =>
            _editorBox.Placeholder = newValue?.ToString() ?? "";

        private void SetEditorValue(string value)
        {
            _editorBox.Text = value;
        }

        private void Editor_TextChanged(object? sender, TextChangedEventArgs e)
        {
            Editor_ProcessAndSetText(e.NewTextValue);
            Field_UpdateValidationAndChangedState();
            Field_UpdateNotificationMessage();
            _lastValue = e.NewTextValue;
        }

        private void Editor_ConfigureKeyboard() =>
            _editorBox.Keyboard = Editor_GetKeyboardForFieldType(EditorDataType);

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
            EditorDataSource = filteredValue;
        }

        private void Editor_SetMaxSize(object newValue)
        {
            if (newValue != null)
                _editorBox.MaxLength = (int)newValue;
        }

        private void Editor_SetPlaceholderText(object newValue) =>
            _editorBox.Placeholder = newValue == null ? "" : (string)newValue;

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
            grid.Add(ButtonUndo, 2, 0);
            grid.Add(FieldNotification, 0, 1);
            grid.SetColumnSpan(FieldNotification, 3);
            return grid;
        }

        protected override string Field_GetFormatErrorMessage() =>
            EditorDataType switch
            {
                EditorDataTypeEnum.Richtext => "Invalid text.",
                EditorDataTypeEnum.Plaintext => "Invalid text.",
                _ => throw new Exception("Invalid format, unspecified textbox field type")
            };

        protected override bool Field_HasChangedFromLast() =>
            _lastValue != (_editorBox.Text ?? String.Empty);

        protected override bool Field_HasChangedFromOriginal() =>
                    _originalValue != (_editorBox.Text ?? String.Empty);

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

        protected override void Field_OriginalValue_Reset()
        {
            SetEditorValue(_originalValue);
        }
        protected override void Field_OriginalValue_SetToCurrentValue()
        {
            _originalValue = GetCurrentValue();
        }
        protected override void Field_OriginalValue_SetToClear()
        {
            _originalValue = String.Empty;
            SetEditorValue(_originalValue);
        }

        protected override void OnDataSourcePropertyChanged(object newValue, object oldValue) { }

        // Update the _editorBox layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
        protected override void UpdateRow0Layout()
        {
            void _updateRow0Layout()
            {
                BatchBegin();
                if (_editorBox!.Parent is Grid grid)
                {
                    bool isFieldLabelVisible = FieldLabelVisible;
                    bool isButtonUndoVisible = FieldUndoButtonVisible;

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
                BatchCommit();
            }

            if (MainThread.IsMainThread)
                _updateRow0Layout();
            else
                MainThread.BeginInvokeOnMainThread(_updateRow0Layout);

        }



        protected override void OnParentSet()
        {
            base.OnParentSet();
        }

        #endregion Protected Methods

        #region Public Methods

        public override void Field_Unfocus()
        {
            base.Field_Unfocus();
            _editorBox?.Unfocus();
        }

        public string GetCurrentValue() => _editorBox.Text;

        #endregion Public Methods
    }
}