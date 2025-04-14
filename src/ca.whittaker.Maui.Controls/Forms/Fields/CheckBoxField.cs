using System.Diagnostics;
using System.Runtime.CompilerServices;
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
public partial class CheckBoxField : BaseFormField<bool?>
{

    #region Fields

    private Microsoft.Maui.Controls.CheckBox? _checkBox;
    private TapGestureRecognizer _checkBoxOverlay;
    private ContentView _checkBoxTapOverlay;


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
        Field_WireFocusEvents(_checkBox);

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

        Field_InitializeDataSource();

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
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _checkBox!.IsEnabled = true;
                _checkBox!.IsChecked = true;
            });
        });
        if (FieldDataSource != true)
            Field_SetDataSourceValue(true);
    }

    private void CheckBox_SetNotSet()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _checkBox!.IsEnabled = false;
                _checkBox!.IsChecked = false;
            });
        });
        if (FieldDataSource != null)
            Field_SetDataSourceValue(null);
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
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _checkBox!.IsEnabled = true;
                _checkBox!.IsChecked = false;
            });
        });
        if (FieldDataSource != false)
            Field_SetDataSourceValue(false);
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
        grid.Add(FieldButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        Grid.SetColumnSpan(FieldNotification, 3);

        // Layer the overlay on top of the _checkBox (same row, same column)
        // Add the overlay and set its grid position
        grid.Children.Add(_checkBoxTapOverlay);
        Grid.SetColumn(_checkBoxTapOverlay, 1);
        Grid.SetRow(_checkBoxTapOverlay, 0);

        return grid;
    }

    protected override bool? Field_GetCurrentValue() => _checkBox?.IsChecked == true ? true : _checkBox?.IsEnabled == false ? null : false;

    protected override string Field_GetFormatErrorMessage()
    {
        throw new Exception("checkbox should not trigger format error");
    }

    protected override bool Field_HasChangedFromLast()
    {
        return FieldLastValue.FromNullableBoolean() != CheckBox_GetState();
    }

    protected override bool Field_HasChangedFromOriginal()
    {
        return FieldOriginalValue.FromNullableBoolean() != CheckBox_GetState();
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

    protected override void Field_OriginalValue_SetToClear()
    {
        FieldOriginalValue = (FieldMandatory ? CheckedStateEnum.UnChecked : CheckedStateEnum.NotSet).ToNullableBoolean();
    }


    protected override void Field_SetValue(bool? newValue)
    {
        CheckBox_SetState(newValue.FromNullableBoolean());
    }
    protected override void OnParentSet()
    {
        base.OnParentSet();
        CheckBox_SetState(FieldDataSource.FromNullableBoolean());
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
    }

    // Update the _editorBox layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
    protected override void UpdateRow0Layout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
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
                _checkBox?.Unfocus();
            });
        });
    }

    #endregion Public Methods
}