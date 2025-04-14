using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using ca.whittaker.Maui.Controls.Buttons;
using System.Text.RegularExpressions;
using Entry = Microsoft.Maui.Controls.Entry;
using Label = Microsoft.Maui.Controls.Label;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using static ca.whittaker.Maui.Controls.Forms.CheckBoxField;

namespace ca.whittaker.Maui.Controls.Forms;

public static class CheckedStateEnumExtensions
{
    #region Public Methods

    public static CheckedStateEnum FromNullableBoolean(this bool? value) =>
            value switch
            {
                true => CheckedStateEnum.Checked,
                false => CheckedStateEnum.UnChecked,
                _ => CheckedStateEnum.NotSet,
            };

    public static bool? ToNullableBoolean(this CheckedStateEnum state) =>
        state switch
        {
            CheckedStateEnum.Checked => true,
            CheckedStateEnum.UnChecked => false,
            _ => null,
        };

    #endregion Public Methods
}

/// <summary>
/// Represents a customizable checkbox control that combines several UI elements:
///
/// - A Grid layout that organizes:
///
///   • A FieldLabel (Label) for displaying the field's title.
///   • A CheckBox control for boolean input.
///   • A ButtonUndo for reverting changes.
///   • A FieldNotification label for showing validation or error messages.
///
/// <para>
/// Differences:
/// - Uses a CheckBox control instead of an Entry for data input.
/// - Exposes its value via a dedicated two-way bindable property named <c>CheckBoxSource</c>.
/// - Contains logic to track changes against an original value and update an undo button accordingly.
///
/// Grid Layout Overview:
///
/// +-------------------+-------------------+----------------------------------+
/// | FieldLabel        | _checkBox         | ButtonUndo                       |
/// +-------------------+-------------------+----------------------------------+
/// | FieldNotification (spans all three columns)                              |
/// +--------------------------------------------------------------------------+
///
/// This composite control supports boolean value capture, change tracking, and validation.
/// </para>
/// </summary>
public partial class CheckBoxField : BaseFormField
{
    #region Fields

    private Microsoft.Maui.Controls.CheckBox? _checkBox;
    private TapGestureRecognizer _checkBoxOverlay;
    private ContentView _checkBoxTapOverlay;
    private CheckedStateEnum _lastValue = CheckedStateEnum.NotSet;
    private CheckedStateEnum _originalValue = CheckedStateEnum.NotSet;

    public static readonly BindableProperty CheckBoxDataSourceProperty = BindableProperty.Create(
        propertyName: nameof(CheckBoxDataSource),
        returnType: typeof(bool?),
        declaringType: typeof(CheckBoxField),
        defaultValue: null,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => { ((CheckBoxField)bindable).OnDataSourcePropertyChanged(newValue, oldValue); });

    public static readonly BindableProperty CheckBoxDataTypeSourceProperty = BindableProperty.Create(
        propertyName: nameof(CheckBoxDataType),
        returnType: typeof(CheckBoxDataTypeEnum),
        declaringType: typeof(CheckBoxField),
        defaultValue: CheckBoxDataTypeEnum.TriState,
        defaultBindingMode: BindingMode.OneWay);

    #endregion Fields

    #region Public Constructors

    public CheckBoxField()
    {
        // ************
        //   CHECKBOX
        // ************
        _checkBox = new Microsoft.Maui.Controls.CheckBox
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };
        _checkBox.Focused += Field_Focused;
        _checkBox.Unfocused += Field_Unfocused;

        // ***************
        //   TAP OVERLAY
        // ***************
        _checkBoxTapOverlay = new ContentView
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        _checkBoxOverlay = new TapGestureRecognizer();
        _checkBoxOverlay!.Tapped += OnCheckBoxTapped;
        _checkBoxTapOverlay.GestureRecognizers.Add(_checkBoxOverlay);

        InitializeLayout();
    }


    #endregion Public Constructors

    #region Enums

    public enum CheckBoxDataTypeEnum
    {
        Boolean,
        TriState,
    }

    public enum CheckedStateEnum
    {
        NotSet,
        Checked,
        UnChecked
    }

    #endregion Enums

    #region Properties

    public bool? CheckBoxDataSource
    {
        get => (bool?)GetValue(CheckBoxDataSourceProperty);
        set => SetValue(CheckBoxDataSourceProperty, value);
    }

    public CheckBoxDataTypeEnum CheckBoxDataType
    {
        get => (CheckBoxDataTypeEnum)GetValue(CheckBoxDataTypeSourceProperty);
        set => SetValue(CheckBoxDataTypeSourceProperty, value);
    }

    #endregion Properties

    #region Private Methods

    private CheckedStateEnum CheckBox_GetState()
    {
        if (_checkBox!.IsEnabled == false
            && FieldMandatory == false)
            return CheckedStateEnum.NotSet;
        else
            return _checkBox.IsChecked ? CheckedStateEnum.Checked : CheckedStateEnum.UnChecked;
    }

    private void CheckBox_SetChecked()
    {
        void _updateUI()
        {
            BatchBegin();
            _checkBox!.IsEnabled = true;
            _checkBox!.IsChecked = true;
            if (CheckBoxDataSource != true) CheckBoxDataSource = true;
            BatchCommit();
        }

        if (MainThread.IsMainThread)
            _updateUI();
        else
            MainThread.BeginInvokeOnMainThread(_updateUI);
    }

    private void CheckBox_SetNotSet()
    {
        void _updateUI()
        {
            BatchBegin();
            _checkBox!.IsEnabled = false;
            _checkBox!.IsChecked = false;
            if (CheckBoxDataSource != null) CheckBoxDataSource = null;
            BatchCommit();
        }

        if (MainThread.IsMainThread)
            _updateUI();
        else
            MainThread.BeginInvokeOnMainThread(_updateUI);
    }

    private void CheckBox_SetState(CheckedStateEnum value)
    {
        if (CheckBoxDataType == CheckBoxDataTypeEnum.Boolean)
        {
            switch (value)
            {
                case CheckedStateEnum.Checked:
                    CheckBox_SetChecked();
                    break;

                case CheckedStateEnum.NotSet:
                case CheckedStateEnum.UnChecked:
                    CheckBox_SetUnChecked();
                    break;
            }
        }
        else if (CheckBoxDataType == CheckBoxDataTypeEnum.TriState)
        {
            switch (value)
            {
                case CheckedStateEnum.NotSet:
                    CheckBox_SetNotSet();
                    break;

                case CheckedStateEnum.Checked:
                    CheckBox_SetChecked();
                    break;

                case CheckedStateEnum.UnChecked:
                    CheckBox_SetUnChecked();
                    break;
            }
        }
    }

    private void CheckBox_SetUnChecked()
    {
        void _updateUI()
        {
            BatchBegin();
            _checkBox!.IsEnabled = true;
            _checkBox!.IsChecked = false;
            if (CheckBoxDataSource != false) CheckBoxDataSource = false;
            BatchCommit();
        }

        if (MainThread.IsMainThread)
            _updateUI();
        else
            MainThread.BeginInvokeOnMainThread(_updateUI);
    }

    private void CheckBox_Toggle()
    {
        if (_checkBoxOverlay != null)
        {
            if (CheckBoxDataType == CheckBoxDataTypeEnum.Boolean)
            {
                switch (CheckBox_GetState())
                {
                    case CheckedStateEnum.UnChecked:
                        // UnChecked => Checked
                        CheckBox_SetChecked();
                        break;

                    case CheckedStateEnum.Checked:
                    case CheckedStateEnum.NotSet:
                        // Checked or NotSet => UnChecked
                        CheckBox_SetUnChecked();
                        break;
                }
            }
            else if (CheckBoxDataType == CheckBoxDataTypeEnum.TriState)
            {
                switch (CheckBox_GetState())
                {
                    case CheckedStateEnum.UnChecked:
                        if (FieldMandatory == true)
                        {
                            // mandatory?
                            // UnChecked => Checked
                            CheckBox_SetChecked();
                        }
                        else
                        {
                            // Not mandatory?
                            // unchecked => NotSet
                            CheckBox_SetNotSet();
                        }
                        break;

                    case CheckedStateEnum.Checked:
                        // Checked => NotSet
                        CheckBox_SetUnChecked();
                        break;

                    case CheckedStateEnum.NotSet:
                        // NotSet => UnChecked
                        CheckBox_SetChecked();
                        break;
                }
            }
        }
    }

    private void OnCheckBoxTapped(object? sender, TappedEventArgs e)
    {
        Debug.WriteLine($"OnCheckBox_Tapped");
        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            CheckBox_Toggle();
            Field_UpdateValidationAndChangedState();
            Field_UpdateNotificationMessage();
        }
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
        }
        };

        grid.Add(FieldLabel, 0, 0);
        grid.Add(_checkBox, 1, 0);
        grid.Add(ButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        Grid.SetColumnSpan(FieldNotification, 3);

        // Layer the overlay on top of the _checkBox (same row, same column)
        // Add the overlay and set its grid position
        grid.Children.Add(_checkBoxTapOverlay);
        Grid.SetColumn(_checkBoxTapOverlay, 1);
        Grid.SetRow(_checkBoxTapOverlay, 0);

        return grid;
    }

    protected override string Field_GetFormatErrorMessage()
    {
        throw new Exception("checkbox should not trigger format error");
    }

    protected override bool Field_HasChangedFromLast()
    {
        return _lastValue != CheckBox_GetState();
    }

    protected override bool Field_HasChangedFromOriginal()
    {
        return _originalValue != CheckBox_GetState();
    }

    protected override bool Field_HasFormatError()
    {
        return false;
    }

    protected override bool Field_HasRequiredError()
    {
        if (FieldMandatory && CheckBox_GetState() == CheckedStateEnum.NotSet)
            return true;
        else if (CheckBoxDataType == CheckBoxDataTypeEnum.Boolean && CheckBox_GetState() == CheckedStateEnum.NotSet)
            return true;
        else
            return false;
    }

    protected override void Field_OriginalValue_Reset()
    {
        CheckBox_SetState(_originalValue);
    }

    protected override void Field_OriginalValue_SetToClear()
    {
        _originalValue = FieldMandatory ? CheckedStateEnum.UnChecked : CheckedStateEnum.NotSet;
    }

    protected override void Field_OriginalValue_SetToCurrentValue()
    {
        _originalValue = GetCurrentValue().FromNullableBoolean();
    }

    protected override void OnDataSourcePropertyChanged(object newValue, object oldValue)
    {
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        CheckBox_SetState(CheckBoxDataSource.FromNullableBoolean());
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
    }

    // Update the _editorBox layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
    protected override void UpdateRow0Layout()
    {
        void _updateRow0Layout()
        {
            BatchBegin();
            if (_checkBoxTapOverlay!.Parent is Grid grid)
            {
                bool isFieldLabelVisible = FieldLabelVisible;
                bool isButtonUndoVisible = FieldUndoButtonVisible;

                if (isFieldLabelVisible && isButtonUndoVisible)
                {
                    Grid.SetColumn(_checkBoxTapOverlay!, 1);
                    Grid.SetColumnSpan(_checkBoxTapOverlay!, 1);
                }
                else if (isFieldLabelVisible && !isButtonUndoVisible)
                {
                    Grid.SetColumn(_checkBoxTapOverlay!, 1);
                    Grid.SetColumnSpan(_checkBoxTapOverlay!, 2);
                }
                else if (!isFieldLabelVisible && isButtonUndoVisible)
                {
                    Grid.SetColumn(_checkBoxTapOverlay!, 0);
                    Grid.SetColumnSpan(_checkBoxTapOverlay!, 2);
                }
                else // both not visible
                {
                    Grid.SetColumn(_checkBoxTapOverlay!, 0);
                    Grid.SetColumnSpan(_checkBoxTapOverlay!, 3);
                }
            }
            BatchCommit();
        }

        if (MainThread.IsMainThread)
            _updateRow0Layout();
        else
            MainThread.BeginInvokeOnMainThread(_updateRow0Layout);
    }

    #endregion Protected Methods

    #region Public Methods

    public override void Field_Unfocus()
    {
        base.Field_Unfocus();
        _checkBox?.Unfocus();
    }

    public bool? GetCurrentValue() => _checkBox?.IsChecked == true ? true : _checkBox?.IsEnabled == false ? null : false;

    #endregion Public Methods
}