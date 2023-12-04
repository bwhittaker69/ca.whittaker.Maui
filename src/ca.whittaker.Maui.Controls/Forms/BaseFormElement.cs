//using ca.whittaker.Maui.Controls.Buttons;
//using System.Text.RegularExpressions;
//using Entry = Microsoft.Maui.Controls.Entry;
//using Label = Microsoft.Maui.Controls.Label;

//namespace ca.whittaker.Maui.Controls.Forms;

///// <summary>
///// Represents a customizable text box control with various properties for text manipulation and validation.
///// </summary>
//public class BaseFormElement : ContentView
//{
//    public static readonly BindableProperty ChangeStateProperty = BindableProperty.Create(
//        propertyName: nameof(ChangeState),
//        returnType: typeof(ChangeStateEnum),
//        declaringType: typeof(BaseFormElement),
//        defaultValue: ChangeStateEnum.NotChanged,
//        defaultBindingMode: BindingMode.TwoWay);

//    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
//        propertyName: nameof(Label),
//        returnType: typeof(string),
//        declaringType: typeof(BaseFormElement),
//        defaultValue: string.Empty,
//        propertyChanged: (bindable, oldValue, newValue) =>
//        {
//            ((BaseFormElement)bindable).OnLabelPropertyChanged(newValue);
//        });

//    public static readonly BindableProperty CheckBoxSourceProperty = BindableProperty.Create(
//        propertyName: nameof(CheckBoxSource),
//        returnType: typeof(bool),
//        declaringType: typeof(BaseFormElement),
//        defaultValue: false,
//        defaultBindingMode: BindingMode.TwoWay,
//        propertyChanged: (bindable, oldValue, newValue) =>
//        {
//            ((BaseFormElement)bindable).OnCheckBoxSourcePropertyChanged(newValue);
//        });

//    public static readonly BindableProperty LabelWidthProperty = BindableProperty.Create(
//        propertyName: nameof(LabelWidth),
//        returnType: typeof(double?),
//        declaringType: typeof(BaseFormElement),
//        defaultValue: (double)100,
//        defaultBindingMode: BindingMode.TwoWay,
//        propertyChanged: (bindable, oldValue, newValue) =>
//        {
//            ((BaseFormElement)bindable).OnLabelWidthPropertyChanged(newValue);
//        });

//    public static readonly BindableProperty ValidationStateProperty = BindableProperty.Create(
//        propertyName: nameof(ValidationState),
//        returnType: typeof(ValidationStateEnum),
//        declaringType: typeof(BaseFormElement),
//        defaultValue: ValidationStateEnum.Valid,
//        defaultBindingMode: BindingMode.TwoWay);

//    // Fields, constants, and regex
//    public UndoButton _buttonUndo;
//    public Label _label;
//    public Label _labelNotification;
//    private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;

//    public BaseFormElement()
//    {
//        InitializeUI();
//    }
//    private void InitializeUI()
//    {
//        _label = CreateLabel();
//        _labelNotification = CreateNotificationLabel();
//        _buttonUndo = CreateUndoButton();
//        _buttonUndo.Pressed += (s, e) => Undo();
//    }

//    public event EventHandler<HasChangesEventArgs>? HasChanges;
//    public event EventHandler<ValidationDataChangesEventArgs>? HasValidationChanges;
//    public ChangeStateEnum ChangeState { get => (ChangeStateEnum)GetValue(ChangeStateProperty); set => SetValue(ChangeStateProperty, value); }
//    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
//    public bool Mandatory { get => (bool)GetValue(MandatoryProperty); set => SetValue(MandatoryProperty, value); }
//    public double LabelWidth { get => (double)GetValue(LabelWidthProperty); set => SetValue(LabelWidthProperty, value); }

//    public bool CheckBoxSource
//    {
//        get => (bool)GetValue(CheckBoxSourceProperty);
//        set
//        {
//            SetValue(CheckBoxSourceProperty, value);
//            SetOriginalValue(value);
//        }
//    }

//    public ValidationStateEnum ValidationState { get => (ValidationStateEnum)GetValue(ValidationStateProperty); set => SetValue(ValidationStateProperty, value); }

//    public void Clear()
//    {
//        SetOriginalValue(false);
//    }

//    public void Saved()
//    {
//        SetOriginalValue(_checkBox.IsChecked);
//    }

//    public void SetOriginalValue(bool originalValue)
//    {
//        _originalValue = originalValue;
//        _checkBox.IsChecked = originalValue;
//    }

//    public void Undo()
//    {
//        _checkBox.IsChecked = _originalValue;
//    }

//    private static Label CreateNotificationLabel()
//    {
//        return new Label
//        {
//            HorizontalOptions = LayoutOptions.Center,
//            VerticalOptions = LayoutOptions.Center,
//            IsVisible = false,
//            TextColor = Colors.Red
//        };
//    }

//    private static UndoButton CreateUndoButton()
//    {
//        return new UndoButton
//        {
//            Text = "",
//            HorizontalOptions = LayoutOptions.Center,
//            VerticalOptions = LayoutOptions.Center,
//            BackgroundColor = Colors.Transparent,
//            ButtonSize = cUndoButtonSize,
//            WidthRequest = -1,
//            ButtonState = ButtonStateEnum.Disabled,
//            ButtonType = BaseButtonTypeEnum.Undo,
//            BorderWidth = 0,
//            Margin = new Thickness(0),
//            Padding = new Thickness(5, 0, 0, 0)
//        };
//    }

//    private void OnLabelPropertyChanged(object newValue)
//    {
//        _label.Text = newValue?.ToString() ?? "";
//    }

//    private void OnCheckBoxSourcePropertyChanged(object newValue)
//    {
//        _checkBox.IsChecked = (bool)newValue;
//    }

//    private void OnLabelWidthPropertyChanged(object newValue)
//    {
//        if (Content is Grid grid)
//        {
//            grid.ColumnDefinitions[0].Width = new GridLength((double)newValue, GridUnitType.Absolute);
//        }
//    }

//    private Microsoft.Maui.Controls.CheckBox CreateCheckBox()
//    {
//        Microsoft.Maui.Controls.CheckBox box = new Microsoft.Maui.Controls.CheckBox
//        {
//            HorizontalOptions = LayoutOptions.Fill,
//            VerticalOptions = LayoutOptions.Center
//        };
//        box.CheckedChanged += CheckBox_ValueChanged;
//        return box;
//    }

//    private Label CreateLabel()
//    {
//        return new Label
//        {
//            Text = Label,
//            HorizontalOptions = LayoutOptions.Start,
//            VerticalOptions = LayoutOptions.Center
//        };
//    }

//    //private Grid CreateLayoutGrid(double fieldLabelWidth)
//    private Grid CreateLayoutGrid()
//    {
//        var grid = new Grid
//        {
//            ColumnDefinitions =
//                {
//                    new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Absolute) },
//                    new ColumnDefinition { Width = GridLength.Star },
//                    new ColumnDefinition { Width = new GridLength(DeviceHelper.GetImageSizeForDevice(cUndoButtonSize) * 2, GridUnitType.Absolute)  },
//                },
//            RowDefinitions =
//                {
//                    new RowDefinition { Height = GridLength.Auto },
//                    new RowDefinition { Height = GridLength.Auto }
//                }
//        };

//        grid.Add(_label, 0, 0);
//        grid.Add(_checkBox, 1, 0);
//        grid.Add(_buttonUndo, 2, 0);
//        grid.Add(_labelNotification, 0, 1);
//        Grid.SetColumnSpan(_labelNotification, 3);

//        return grid;
//    }


//    private void CheckBox_ValueChanged(object? sender, CheckedChangedEventArgs e)
//    {
//        ProcessAndSetValue(e.Value);
//    }

//    private void EvaluateToRaiseHasChangesEvent()
//    {
//        bool hasChanged = _originalValue != _checkBox.IsChecked;
//        if (_previousHasChangedState != hasChanged)
//        {
//            void UpdateUI()
//            {
//                using (ResourceHelper resourceHelper = new())
//                {
//                    _buttonUndo.ImageSource = resourceHelper.GetImageSource(hasChanged ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled, BaseButtonTypeEnum.Undo, cUndoButtonSize);
//                }
//                _previousHasChangedState = hasChanged;
//                ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
//                HasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
//            }

//            // Check if on the main thread and update UI accordingly
//            if (MainThread.IsMainThread)
//            {
//                UpdateUI();
//            }
//            else
//            {
//                MainThread.BeginInvokeOnMainThread(() => UpdateUI());
//            }
//        }
//    }


//    private void ProcessAndSetValue(bool value)
//    {
//        _checkBox.IsChecked = value;
//    }

//    private void SetLabelText(object newValue)
//    {
//        _label.Text = newValue == null ? "" : newValue.ToString();
//    }

//}