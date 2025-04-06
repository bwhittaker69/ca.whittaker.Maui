using System.Diagnostics;
using System.Text.RegularExpressions;
using Entry = Microsoft.Maui.Controls.Entry;

namespace ca.whittaker.Maui.Controls.Forms
{
    /// <summary>
    /// Represents a customizable text box control that combines several UI elements.
    /// </summary>
    public partial class TextBoxField : BaseFormField
    {
        #region Fields

        private static readonly Regex emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private string _lastValue = string.Empty;
        private string _originalValue = string.Empty;
        private Entry _textBox;

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

        public static readonly BindableProperty TextBoxDataSourceProperty = BindableProperty.Create(
                                            propertyName: nameof(TextBoxDataSource),
            returnType: typeof(string),
            declaringType: typeof(TextBoxField),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
                ((TextBoxField)bindable).OnFieldDataSourcePropertyChanged(newValue, oldValue));

        public static readonly BindableProperty TextBoxFieldTypeProperty = BindableProperty.Create(
            propertyName: nameof(TextBoxFieldType),
            returnType: typeof(TextBoxFieldTypeEnum),
            declaringType: typeof(TextBoxField),
            defaultValue: TextBoxFieldTypeEnum.Text,
            propertyChanged: (bindable, oldValue, newValue) =>
                ((TextBoxField)bindable).OnTextBoxFieldTypeChanged(newValue));

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

        public TextBoxField()
        {
            _textBox = new Entry
            {
                VerticalOptions = LayoutOptions.Center,
            };
            _textBox.IsEnabled = false;
            _textBox.ReturnCommand = new Command(TextBoxReturn_PressedCommand);
            _textBox.TextChanged += TextBox_TextChanged;
            _textBox.Focused += Field_Focused;
            _textBox.Unfocused += Field_Unfocused;

            TextBoxSetPlaceholderText(TextBoxPlaceholder);
            TextBoxSetMaxLength(TextBoxMaxLength);

            InitializeLayout();
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

        public string TextBoxDataSource
        {
            get => (string)GetValue(TextBoxDataSourceProperty);
            set => SetValue(TextBoxDataSourceProperty, value);
        }

        public TextBoxFieldTypeEnum TextBoxFieldType
        {
            get => (TextBoxFieldTypeEnum)GetValue(TextBoxFieldTypeProperty);
            set
            {
                SetValue(TextBoxFieldTypeProperty, value);
                TextBoxConfigureKeyboard();
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

        private static bool TextBoxIsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) && email.Length >= 5 && emailRegex.IsMatch(email);

        private static bool TextBoxIsValidUrl(string urlString)
        {
            if (string.IsNullOrEmpty(urlString) || urlString.Length < 5)
                return false;
            return Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void OnTextBoxFieldTypeChanged(object newValue) =>
            _textBox.Keyboard = TextBoxGetKeyboardForFieldType((TextBoxFieldTypeEnum)newValue);

        private void OnTextBoxMaxLengthPropertyChanged(object newValue) =>
            _textBox.MaxLength = (int)newValue;

        private void OnTextBoxPlaceholderPropertyChanged(object newValue) =>
            _textBox.Placeholder = newValue?.ToString() ?? "";

        private void SetTextBoxValue(string value)
        {
            _textBox.Text = value;
        }

        private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            TextBoxProcessAndSetText(e.NewTextValue);
            FieldUpdateValidationAndChangedState();
            FieldUpdateNotificationMessage();
            _lastValue = e.NewTextValue;
        }

        private void TextBoxConfigureKeyboard() =>
            _textBox.Keyboard = TextBoxGetKeyboardForFieldType(TextBoxFieldType);

        private Keyboard TextBoxGetKeyboardForFieldType(TextBoxFieldTypeEnum fieldType) =>
            fieldType switch
            {
                TextBoxFieldTypeEnum.Email => Keyboard.Email,
                TextBoxFieldTypeEnum.Url => Keyboard.Url,
                TextBoxFieldTypeEnum.Chat => Keyboard.Chat,
                _ => Keyboard.Default,
            };

        private string TextBoxProcessAllLowercase(string text) =>
            TextBoxAllLowerCase ? text.ToLower() : text;

        private string TextBoxProcessAllowWhiteSpace(string text) =>
            TextBoxAllowWhiteSpace ? text : text.Replace(" ", "");

        private void TextBoxProcessAndSetText(string newText)
        {
            string filteredValue = TextBoxProcessUsernameFilter(
                                       TextBoxProcessEmailFilter(
                                           TextBoxProcessAllLowercase(
                                               TextBoxProcessAllowWhiteSpace(newText ?? ""))));
            if (_textBox.Text != filteredValue)
                _textBox.Text = filteredValue;
            TextBoxDataSource = filteredValue;
        }

        private string TextBoxProcessEmailFilter(string text) =>
            TextBoxFieldType == TextBoxFieldTypeEnum.Email ? Regex.Replace(text, @"[^a-zA-Z0-9!#$%&'*+/=?^_`{|}~.@-]", "") : text;

        private string TextBoxProcessUsernameFilter(string text) =>
            TextBoxFieldType == TextBoxFieldTypeEnum.Username ? Regex.Replace(text, @"[^a-zA-Z0-9-._]", "") : text;

        private void TextBoxReturn_PressedCommand(object obj)
        {
            if (FieldCommand?.CanExecute(FieldCommandParameter) == true)
                FieldCommand.Execute(FieldCommandParameter);
        }

        private void TextBoxSetMaxLength(object newValue)
        {
            if (newValue != null)
                _textBox.MaxLength = (int)newValue;
        }

        private void TextBoxSetPlaceholderText(object newValue) =>
            _textBox.Placeholder = newValue == null ? "" : (string)newValue;

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
            grid.Add(_textBox, 1, 0);
            grid.Add(ButtonUndo, 2, 0);
            grid.Add(FieldNotification, 0, 1);
            grid.SetColumnSpan(FieldNotification, 3);
            return grid;
        }

        protected override string FieldGetFormatErrorMessage() =>
            TextBoxFieldType switch
            {
                TextBoxFieldTypeEnum.Email => "Invalid email format.",
                TextBoxFieldTypeEnum.Url => "Invalid URL.",
                TextBoxFieldTypeEnum.Username => "Invalid Username.",
                TextBoxFieldTypeEnum.Chat => "Invalid message.",
                TextBoxFieldTypeEnum.Text => "Invalid text.",
                _ => throw new Exception("Invalid format, unspecified textbox field type")
            };

        protected override bool FieldHasChangedFromLast() =>
            _lastValue != (_textBox.Text ?? String.Empty);

        protected override bool FieldHasChangedFromOriginal() =>
                    _originalValue != (_textBox.Text ?? String.Empty);

        protected override bool FieldHasFormatError()
        {
            if (TextBoxFieldType == TextBoxFieldTypeEnum.Email && !TextBoxIsValidEmail(_textBox.Text))
                return true;
            if (TextBoxFieldType == TextBoxFieldTypeEnum.Url && !TextBoxIsValidUrl(_textBox.Text))
                return true;
            return false;
        }

        protected override bool FieldHasRequiredError() =>
            FieldMandatory && string.IsNullOrEmpty(_textBox.Text);

        protected override void FieldOriginalValue_Reset()
        {
            SetTextBoxValue(_originalValue);
        }
        protected override void FieldOriginalValue_SetToCurrentValue()
        {
            _originalValue = GetCurrentValue();
        }
        protected override void FieldOriginalValue_SetToClear()
        {
            _originalValue = String.Empty;
            SetTextBoxValue(_originalValue);
        }

        protected override void FieldRefreshUI()
        {
            void _fieldRefreshUI()
            {
                BatchBegin();
                if (Content is Grid grid)
                {
                    if (grid.ColumnDefinitions?.Count == 3)
                    {
                        grid.ColumnDefinitions[0].Width = new GridLength(FieldLabelWidth, GridUnitType.Absolute);
                        grid.ColumnDefinitions[1].Width = GridLength.Star;
                        grid.ColumnDefinitions[2].Width = new GridLength(DeviceHelper.GetImageSizeForDevice(DefaultButtonSize) * 2, GridUnitType.Absolute);
                    }
                    grid.HeightRequest = HeightRequest;
                    grid.WidthRequest = WidthRequest;
                    FieldLabel!.HeightRequest = HeightRequest;
                    FieldLabel.WidthRequest = FieldLabelWidth;
                    _textBox.WidthRequest = -1;
                    _textBox.HeightRequest = HeightRequest;
                    if (ButtonUndo != null)
                    {
                        ButtonUndo.WidthRequest = DeviceHelper.GetImageSizeForDevice(DefaultButtonSize) * 2;
                        ButtonUndo.HeightRequest = HeightRequest;
                    }
                }
                BatchCommit();
            }

            if (MainThread.IsMainThread)
                _fieldRefreshUI();
            else
                MainThread.BeginInvokeOnMainThread(_fieldRefreshUI);
        }

        protected override void OnFieldDataSourcePropertyChanged(object newValue, object oldValue)
        {
            //if (!_fieldIsOriginalValueSet && !_onFieldDataSourcePropertyChanging)
            //{
            //    _onFieldDataSourcePropertyChanging = true;
            //    _textBox.TextChanged -= TextBox_TextChanged;
            //    FieldSetOriginalValue((string)newValue);
            //    _textBox.Text = (string)newValue;
            //    _textBox.TextChanged += TextBox_TextChanged;
            //    _onFieldDataSourcePropertyChanging = false;
            //}
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            FieldRefreshUI();
        }

        #endregion Protected Methods

        #region Public Methods

        public override void FieldUnfocus()
        {
            base.FieldUnfocus();
            _textBox?.Unfocus();
        }

        public string GetCurrentValue() => _textBox.Text;

        #endregion Public Methods
    }
}