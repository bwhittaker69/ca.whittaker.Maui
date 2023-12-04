using ca.whittaker.Maui.Controls.Buttons;
using System.Text.RegularExpressions;
using Entry = Microsoft.Maui.Controls.Entry;
using Label = Microsoft.Maui.Controls.Label;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control with various properties for text manipulation and validation.
/// </summary>
public class CheckBoxElement : BaseFormElement
{

    public static readonly BindableProperty MandatoryProperty = BindableProperty.Create(
        propertyName: nameof(Mandatory),
        returnType: typeof(bool),
        declaringType: typeof(CheckBoxElement),
        defaultValue: false);

    public static readonly BindableProperty CheckBoxSourceProperty = BindableProperty.Create(
        propertyName: nameof(CheckBoxSource),
        returnType: typeof(bool),
        declaringType: typeof(CheckBoxElement),
        defaultValue: false,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            ((CheckBoxElement)bindable).OnCheckBoxSourcePropertyChanged(newValue);
        });

    // Fields, constants, and regex
    public Microsoft.Maui.Controls.CheckBox _checkBox;
    private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;

    private bool _isOriginalValueSet = false;
    private bool _originalValue = false;
    private bool _previousHasChangedState = false;
    private bool _previousInvalidDataState = false;

    public CheckBoxElement()
    {
        InitializeUI();
    }
    private void InitializeUI()
    {
        _checkBox = CreateCheckBox();
        _label = CreateLabel();
        _labelNotification = CreateNotificationLabel();
        _buttonUndo = CreateUndoButton();
        Content = CreateLayoutGrid();
        _buttonUndo.Pressed += (s, e) => Undo();
    }

    public bool Mandatory { get => (bool)GetValue(MandatoryProperty); set => SetValue(MandatoryProperty, value); }

    public bool CheckBoxSource
    {
        get => (bool)GetValue(CheckBoxSourceProperty);
        set
        {
            SetValue(CheckBoxSourceProperty, value);
            SetOriginalValue(value);
        }
    }

    public void Clear()
    {
        SetOriginalValue(false);
        UpdateValidationState();
    }

    public void Saved()
    {
        SetOriginalValue(_checkBox.IsChecked);
        UpdateValidationState();
    }

    public void SetOriginalValue(bool originalValue)
    {
        _originalValue = originalValue;
        _checkBox.IsChecked = originalValue;
    }

    public void Undo()
    {
        _checkBox.IsChecked = _originalValue;
        UpdateValidationState();
    }


    private void OnCheckBoxSourcePropertyChanged(object newValue)
    {
        _checkBox.IsChecked = (bool)newValue;
    }


    private Microsoft.Maui.Controls.CheckBox CreateCheckBox()
    {
        Microsoft.Maui.Controls.CheckBox box = new Microsoft.Maui.Controls.CheckBox
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };
        box.CheckedChanged += CheckBox_ValueChanged;
        return box;
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

        grid.Add(_label, 0, 0);
        grid.Add(_checkBox, 1, 0);
        grid.Add(_buttonUndo, 2, 0);
        grid.Add(_labelNotification, 0, 1);
        Grid.SetColumnSpan(_labelNotification, 3);

        return grid;
    }


    private void CheckBox_ValueChanged(object? sender, CheckedChangedEventArgs e)
    {
        ProcessAndSetValue(e.Value);
        UpdateValidationState();
    }

    private void EvaluateToRaiseHasChangesEvent()
    {
        bool hasChanged = _originalValue != _checkBox.IsChecked;
        if (_previousHasChangedState != hasChanged)
        {
            void UpdateUI()
            {
                using (ResourceHelper resourceHelper = new())
                {
                    _buttonUndo.ImageSource = resourceHelper.GetImageSource(hasChanged ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled, BaseButtonTypeEnum.Undo, cUndoButtonSize);
                }
                _previousHasChangedState = hasChanged;
                ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
                RaiseHasChanges(hasChanged);
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


    private void ProcessAndSetValue(bool value)
    {
        _checkBox.IsChecked = value;
    }


    private void UpdateValidationState()
    {
        EvaluateToRaiseHasChangesEvent();
    }
}