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

    private Microsoft.Maui.Controls.CheckBox _checkBox;
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

        Initialize();
    }
    protected override List<View> Field_ControlView() // ← was returning only the CheckBox
    {
        return new List<View> { _checkBox, _checkBoxTapOverlay };
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

    protected void Field_SetValue(CheckedStateEnum value)
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


    protected override void Field_SetDataSourceValue(bool? newValue)
    {
        FieldSuppressDataSourceCallback = true;           // ← prevent original-capture loop
        SetValue(FieldDataSourceProperty, newValue);
        Field_SetValue(newValue);
        Field_UpdateValidationAndChangedState();          // ← tell base to re-check
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

    private CheckedStateEnum CalcNextCheckedStateEnum()
    {
        var current = CheckBox_GetState();

        // Boolean mode: only two states (NotSet treated as “on”)
        if (CheckBoxDataType == CheckBoxDataTypeEnum.Boolean)
        {
            // UnChecked ⇒ Checked, everything else ⇒ UnChecked
            return current == CheckedStateEnum.UnChecked
                ? CheckedStateEnum.Checked
                : CheckedStateEnum.UnChecked;
        }

        // TriState mode
        if (FieldMandatory)
        {
            // Mandatory: UnChecked ⇒ Checked ⇒ NotSet ⇒ UnChecked …
            return current switch
            {
                CheckedStateEnum.UnChecked => CheckedStateEnum.Checked,
                CheckedStateEnum.Checked   => CheckedStateEnum.UnChecked,
                CheckedStateEnum.NotSet    => CheckedStateEnum.Checked,
                _                          => CheckedStateEnum.UnChecked
            };
        }
        else
        {
            // Optional: UnChecked ⇒ NotSet ⇒ Checked ⇒ UnChecked …
            return current switch
            {
                CheckedStateEnum.UnChecked => CheckedStateEnum.NotSet,
                CheckedStateEnum.NotSet    => CheckedStateEnum.Checked,
                CheckedStateEnum.Checked   => CheckedStateEnum.UnChecked,
                _                          => CheckedStateEnum.NotSet
            };
        }
    }



    private void OnCheckBoxTapped(object? sender, TappedEventArgs e)
    {
        Debug.WriteLine($"OnCheckBox_Tapped");
        if (FieldAccessMode != FieldAccessModeEnum.Editing) return;
        // cycle state…
        var next = CalcNextCheckedStateEnum(); /* compute next CheckedStateEnum */;
        var nv = next.ToNullableBoolean();
        Field_SetDataSourceValue(nv);
        FieldLastValue = nv;           // ← record “last” for change-tracking
    }

    #endregion Private Methods

    #region Protected Methods
    //CheckBoxField
    //protected override Grid Field_CreateLayoutGrid()
    //{
    //    var grid = new Grid
    //    {
    //        ColumnDefinitions =
    //        {
    //            new ColumnDefinition { Width = new GridLength(FieldLabelWidth, GridUnitType.Absolute) },
    //            new ColumnDefinition { Width = GridLength.Star },
    //            new ColumnDefinition { Width = new GridLength(DeviceHelper.GetImageSizeForDevice(DefaultButtonSize) * 2, GridUnitType.Absolute) },
    //        },
    //        RowDefinitions =
    //        {
    //            new RowDefinition { Height = GridLength.Auto },
    //            new RowDefinition { Height = GridLength.Auto }
    //        }
    //    };

    //    grid.Add(FieldLabel, 0, 0);
    //    grid.Add(_checkBox, 1, 0);
    //    grid.Add(FieldButtonUndo, 2, 0);
    //    grid.Add(FieldNotification, 0, 1);
    //    Grid.SetColumnSpan(FieldNotification, 3);

    //    // Layer the overlay on top of the _checkBox (same row, same column)
    //    // Add the overlay and set its grid position
    //    grid.Children.Add(_checkBoxTapOverlay);
    //    Grid.SetColumn(_checkBoxTapOverlay, 1);
    //    Grid.SetRow(_checkBoxTapOverlay, 0);

    //    return grid;
    //}

    protected override bool? Field_GetCurrentValue() // ← add this override
    {
        if (_checkBox.IsChecked) return true;
        if (!_checkBox.IsEnabled) return null;
        return false;
    }

    protected override string Field_GetFormatErrorMessage() => string.Empty;


    protected override bool Field_HasChangedFromLast() =>
        !Nullable.Equals(FieldLastValue, Field_GetCurrentValue());

    protected override bool Field_HasChangedFromOriginal() =>
        !Nullable.Equals(FieldOriginalValue, Field_GetCurrentValue());

    protected override bool Field_HasFormatError()
    {
        return false;
    }

    protected override bool Field_HasRequiredError()
    {
        var state = CheckBox_GetState();
        if (state != CheckedStateEnum.NotSet)
            return false;

        // Tri-state: only error when the field is marked mandatory
        if (CheckBoxDataType == CheckBoxDataTypeEnum.TriState && FieldMandatory)
            return true;

        // Boolean: always required (NotSet is not allowed)
        if (CheckBoxDataType == CheckBoxDataTypeEnum.Boolean)
            return true;

        return false;
    }


    protected override void Field_OriginalValue_SetToClear()
    {
        FieldOriginalValue = (FieldMandatory ? CheckedStateEnum.UnChecked : CheckedStateEnum.NotSet).ToNullableBoolean();
    }

    protected override void Field_SetValue(bool? newValue)
    {
        Field_SetValue(newValue.FromNullableBoolean());
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        //Content = Field_CreateLayoutGrid();               // ← ensure grid is assigned
        //Field_SetValue(FieldDataSource);
        FieldOriginalValue = FieldDataSource;             // ← capture original here
        Field_UpdateValidationAndChangedState();
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
                    bool isButtonUndoVisible = FieldUndoButton;

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


