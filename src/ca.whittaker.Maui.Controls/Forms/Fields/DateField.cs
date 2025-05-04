using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.ComponentModel;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control that combines several UI elements.
/// </summary>
public partial class DateField : BaseFormField<DateTimeOffset?>, IBaseFormFieldTyped<DateTimeOffset?>
{

    #region Fields

    private DatePicker _datePicker;

    #endregion Fields

    #region Public Constructors

    public DateField()
    {
        _datePicker = new DatePicker
        {
            VerticalOptions = LayoutOptions.Center,
            MinimumDate = new DateTime(1900, 1, 1),
            MaximumDate = new DateTime(2100, 12, 31),
            Date = FieldDataSource?.DateTime ?? DateTime.Today // force valid date at creation
        };

        _datePicker.IsEnabled = false;
        _datePicker.DateSelected += DatePicker_DateSet;

        _datePicker.DateSelected += (s, e) =>
        {
            Debug.WriteLine($"[DateField] : {FieldLabelText} : _datePicker.DateSelected(e.NewDate={e.NewDate})");
        };

        Initialize();
    }

    protected override List<View> Field_ControlView() => new List<View>() { _datePicker };

    #endregion Public Constructors

    #region Private Methods


    protected override void Field_SetDataSourceValue(DateTimeOffset? newValue)
    {
        Debug.WriteLine($"[DateField] : {FieldLabelText} : Field_SetDataSourceValue(DateTimeOffset? {newValue})");

        // 1) Suppress the static change-callback
        FieldSuppressDataSourceCallback = true;

        // 2) Set binding value to newValue
        SetValue(FieldDataSourceProperty, newValue);

        // 4) Push the date into the picker
        Field_SetValue(newValue);

        // 5) Now re-enable change detection
        FieldSuppressDataSourceCallback = false;

        // 6) Run your usual validation / change-state logic
        Field_UpdateValidationAndChangedState();
        Field_UpdateNotificationMessage();
    }

    private void DatePicker_DateSet(object? sender, DateChangedEventArgs e)
    {
        Debug.WriteLine($"[DateField] : {FieldLabelText} : DatePicker_DateSet(e.NewDate={e.NewDate})");

        // 1) Push the new date back into the bindable so we get the same
        //    suppression + change-detection logic you use in TextBoxField
        var newOffset = e.NewDate.ToDateTimeOffset();
        Field_SetDataSourceValue(newOffset);

        // 2) Mirror the TextBoxField pattern:
        //    update the “last value” so undo/last-change logic stays in sync
        FieldLastValue = newOffset;
    }


    #endregion Private Methods

    #region Protected Methods
    //DateField
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
    //        },
    //        VerticalOptions = LayoutOptions.Fill
    //    };
    //    grid.Add(FieldLabel, 0, 0);
    //    grid.Add(_datePicker, 1, 0);
    //    grid.Add(FieldButtonUndo, 2, 0);
    //    grid.Add(FieldNotification, 0, 1);
    //    grid.SetColumnSpan(FieldNotification, 3);
    //    return grid;
    //}

    protected override DateTimeOffset? Field_GetCurrentValue() => _datePicker.Date;
    protected override string Field_GetFormatErrorMessage() => String.Empty;

    protected override bool Field_HasChangedFromLast() =>
                FieldLastValue != (_datePicker.Date);

    protected override bool Field_HasChangedFromOriginal()
    {

        if (FieldOriginalValue == null)
            return _datePicker?.Date != DateTime.MinValue;

        var originalDate = FieldOriginalValue.Value.Date;
        var currentDate = _datePicker?.Date.Date;

        Debug.WriteLine($"[DateField] : {FieldLabelText} : Field_HasChangedFromOriginal() = {originalDate != currentDate}");

        return originalDate != currentDate;
    }



    protected override bool Field_HasFormatError()
    {
        return false;
    }

    protected override bool Field_HasRequiredError() =>
                FieldMandatory && _datePicker == null;



    protected override void Field_SetValue(DateTimeOffset? newDate)
    {

        if (_datePicker.Date != newDate.ToDateTime())
        {
            UiThreadHelper.RunOnMainThread(() =>
            {
                Field_PerformBatchUpdate(() =>
                {
                    Debug.WriteLine($"[DateField] : {FieldLabelText} : Field_SetValue(DateTimeOffset? {newDate})");
                    _datePicker.Date = newDate.ToDateTime();
                });
            });
        }
    }


    // Update the _editorBox layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
    protected override void UpdateRow0Layout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                if (_datePicker!.Parent is Grid grid)
                {
                    bool isFieldLabelVisible = FieldLabelVisible;
                    bool isButtonUndoVisible = FieldUndoButton;

                    if (isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(_datePicker!, 1);
                        Grid.SetColumnSpan(_datePicker!, 1);
                    }
                    else if (isFieldLabelVisible && !isButtonUndoVisible)
                    {
                        Grid.SetColumn(_datePicker!, 1);
                        Grid.SetColumnSpan(_datePicker!, 2);
                    }
                    else if (!isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(_datePicker!, 0);
                        Grid.SetColumnSpan(_datePicker!, 2);
                    }
                    else // both not visible
                    {
                        Grid.SetColumn(_datePicker!, 0);
                        Grid.SetColumnSpan(_datePicker!, 3);
                    }
                }
            });
        });
    }

    #endregion Protected Methods
    protected override void OnParentSet()
    {
        base.OnParentSet();

        Debug.WriteLine($"[DateField] : {FieldLabelText} : OnParentSet() – aligning original to current picker value");

        // 1) Take whatever the DatePicker is showing now as the ORIGINAL
        //    so that initial change-detector sees no difference.
        FieldOriginalValue = _datePicker.Date.ToDateTimeOffset();

        // 2) Re-evaluate change state now that original==current
        Field_UpdateValidationAndChangedState();
    }
    #region Public Methods

    public override void Field_Unfocus()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                base.Field_Unfocus();
                _datePicker?.Unfocus();
            });
        });
    }

    #endregion Public Methods

}

public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Returns the DateTime part of the nullable DateTimeOffset, or DateTime.Now if null.
    /// </summary>
    public static DateTime ToDateTime(this DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset?.DateTime ?? DateTime.Now;
    }
    public static DateTimeOffset? ToDateTimeOffset(this DateTime dateTime)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
        return new DateTimeOffset(dateTime, offset);
    }

}