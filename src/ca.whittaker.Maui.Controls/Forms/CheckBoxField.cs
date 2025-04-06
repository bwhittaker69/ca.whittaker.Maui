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
        propertyChanged: (bindable, oldValue, newValue) => { ((CheckBoxField)bindable).OnFieldDataSourcePropertyChanged(newValue, oldValue); });


    public static readonly BindableProperty CheckBoxTypeSourceProperty = BindableProperty.Create(
        propertyName: nameof(CheckBoxType),
        returnType: typeof(CheckBoxTypeEnum),
        declaringType: typeof(CheckBoxField),
        defaultValue: CheckBoxTypeEnum.TriState,
        defaultBindingMode: BindingMode.OneWay);

    public CheckBoxTypeEnum CheckBoxType
    {
        get => (CheckBoxTypeEnum)GetValue(CheckBoxTypeSourceProperty);
        set => SetValue(CheckBoxTypeSourceProperty, value);
    }

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
        _checkBoxOverlay!.Tapped += OnCheckBox_Tapped;
        _checkBoxTapOverlay.GestureRecognizers.Add(_checkBoxOverlay);

        InitializeLayout();
    }

    #endregion Public Constructors

    #region Enums

    public enum CheckBoxTypeEnum
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

    #endregion Properties

    #region Private Methods

    private void CheckBoxToggleValue()
    {
        if (_checkBoxOverlay != null)
        {
            if (CheckBoxType == CheckBoxTypeEnum.Boolean)
            {
                switch (GetCheckBoxSate())
                {
                    case CheckedStateEnum.UnChecked:
                        // UnChecked => Checked
                        SetChecked();
                        break;

                    case CheckedStateEnum.Checked:
                    case CheckedStateEnum.NotSet:
                        // Checked or NotSet => UnChecked
                        SetUnChecked();
                        break;
                }
            }
            else if (CheckBoxType == CheckBoxTypeEnum.TriState)
            {
                switch (GetCheckBoxSate())
                {
                    case CheckedStateEnum.UnChecked:
                        if (FieldMandatory == true)
                        {
                            // mandatory?
                            // UnChecked => Checked
                            SetChecked();
                        }
                        else
                        {
                            // Not mandatory?
                            // unchecked => NotSet
                            SetNotSet();
                        }
                        break;

                    case CheckedStateEnum.Checked:
                        // Checked => NotSet
                        SetUnChecked();
                        break;

                    case CheckedStateEnum.NotSet:
                        // NotSet => UnChecked
                        SetChecked();
                        break;
                }
            }
        }
    }

    private CheckedStateEnum GetCheckBoxSate()
    {
        if (_checkBox!.IsEnabled == false
            && FieldMandatory == false)
            return CheckedStateEnum.NotSet;
        else
            return _checkBox.IsChecked ? CheckedStateEnum.Checked : CheckedStateEnum.UnChecked;
    }

    private void OnCheckBox_Tapped(object? sender, TappedEventArgs e)
    {
        Debug.WriteLine($"OnCheckBox_Tapped");
        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            CheckBoxToggleValue();
            FieldUpdateValidationAndChangedState();
            FieldUpdateNotificationMessage();
        }
    }

    private void SetCheckBoxValue(CheckedStateEnum value)
    {
        if (CheckBoxType == CheckBoxTypeEnum.Boolean)
        {
            switch (value)
            {
                case CheckedStateEnum.Checked:
                    SetChecked();
                    break;

                case CheckedStateEnum.NotSet:
                case CheckedStateEnum.UnChecked:
                    SetUnChecked();
                    break;
            }
        }
        else if (CheckBoxType == CheckBoxTypeEnum.TriState)
        {
            switch (value)
            {
                case CheckedStateEnum.NotSet:
                    SetNotSet();
                    break;

                case CheckedStateEnum.Checked:
                    SetChecked();
                    break;

                case CheckedStateEnum.UnChecked:
                    SetUnChecked();
                    break;
            }
        }
    }

    private void SetChecked()
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

    private void SetNotSet()
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

    private void SetUnChecked()
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

    protected override string FieldGetFormatErrorMessage()
    {
        throw new Exception("checkbox should not trigger format error");
    }

    protected override bool FieldHasChangedFromLast()
    {
        return _lastValue != GetCheckBoxSate();
    }

    protected override bool FieldHasChangedFromOriginal()
    {
        return _originalValue != GetCheckBoxSate();
    }

    protected override bool FieldHasFormatError()
    {
        return false;
    }

    protected override bool FieldHasRequiredError()
    {
        if (FieldMandatory && GetCheckBoxSate() == CheckedStateEnum.NotSet)
            return true;
        else if (CheckBoxType == CheckBoxTypeEnum.Boolean && GetCheckBoxSate() == CheckedStateEnum.NotSet)
            return true;
        else
            return false;
    }

    protected override void FieldOriginalValue_Reset()
    {
        SetCheckBoxValue(_originalValue);
    }
    protected override void FieldOriginalValue_SetToCurrentValue()
    {
        _originalValue = GetCurrentValue().FromNullableBoolean();
    }
    protected override void FieldOriginalValue_SetToClear()
    {
        _originalValue = FieldMandatory ? CheckedStateEnum.UnChecked : CheckedStateEnum.NotSet;
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
                _checkBox!.WidthRequest = -1;
                _checkBox!.HeightRequest = HeightRequest;
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

    //void FieldSetOriginalValueToDataSourceValue()
    //protected override void FieldSetOriginalValue(object? originalValue = null)
    //{
    //    _fieldIsOriginalValueSet = true;
    //    _originalValue = (bool?)originalValue == true ? CheckedStateEnum.Checked
    //                    : (bool?)originalValue == false ? CheckedStateEnum.UnChecked
    //                    : CheckedStateEnum.NotSet;
    //    SetCheckBoxValue(_originalValue);
    //}
    //protected override void FieldUnhide()
    //{
    //    if (FieldAccessMode != FieldAccessModeEnum.Hidden)
    //    {
    //        void _updateUI()
    //        {
    //            BatchBegin();
    //            Content.IsVisible = true;
    //            BatchCommit();
    //        }

    //        if (MainThread.IsMainThread)
    //            _updateUI();
    //        else
    //            MainThread.BeginInvokeOnMainThread(_updateUI);
    //    }
    //}

    protected override void OnFieldDataSourcePropertyChanged(object newValue, object oldValue)
    {
        //if (!_fieldIsOriginalValueSet)
        //{
        //    // prevent loop back
        //    if (!_onFieldDataSourcePropertyChanging)
        //    {
        //        _onFieldDataSourcePropertyChanging = true;
        //        if (_checkBox != null)
        //        {
        //            _checkBox.CheckedChanged -= CheckBox_CheckedChanged;
        //            _checkBox.IsChecked = (bool)newValue;
        //            _checkBox.CheckedChanged += CheckBox_CheckedChanged;
        //        }
        //        _onFieldDataSourcePropertyChanging = false;
        //    }
        //}
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        SetCheckBoxValue(CheckBoxDataSource.FromNullableBoolean());
        FieldRefreshUI();
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
        FieldRefreshUI();
    }

    #endregion Protected Methods

    #region Public Methods

    public override void FieldUnfocus()
    {
        base.FieldUnfocus();
        _checkBox?.Unfocus();
    }

    public bool? GetCurrentValue() => _checkBox?.IsChecked == true ? true : _checkBox?.IsEnabled == false ? null : false;

    #endregion Public Methods
}