using Microsoft.Maui.Controls.Compatibility;
using System.Diagnostics;
using Grid = Microsoft.Maui.Controls.Grid;
using StackLayout = Microsoft.Maui.Controls.StackLayout;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Custom control for selecting a Year and Month (day is fixed to 1).
/// </summary>
public partial class DateMonthYearField : BaseFormField<DateTimeOffset?>
{
    #region Public Constructors

    public DateMonthYearField()
    {
        _monthPicker = new Picker
        {
            Title = "Month",
            VerticalOptions = LayoutOptions.Center
        };
        // Populate MonthPicker with month names.
        for (int i = 1; i <= 12; i++)
        {
            _monthPicker.Items.Add(new DateTime(2000, i, 1).ToString("MMMM"));
        }
        _monthPicker.IsEnabled = false;

        _yearPicker = new Picker
        {
            Title = "Year",
            VerticalOptions = LayoutOptions.Center
        };
        // Populate YearPicker with a range of years (e.g., last 100 years up to current year).
        int currentYear = DateTime.Today.Year;
        for (int year = currentYear - 100; year <= currentYear; year++)
        {
            _yearPicker.Items.Add(year.ToString());
        }
        _yearPicker.IsEnabled = false;

        _monthPicker.SelectedIndexChanged += OnPickerSelectionChanged;
        _yearPicker.SelectedIndexChanged += OnPickerSelectionChanged;

        Initialize();
    }

    protected override List<View> Field_ControlView()
    {
        var stack = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { _monthPicker, _yearPicker } };
        return new List<View> { stack };
    }

    #endregion Public Constructors

    #region Properties

    public Picker _monthPicker { get; private set; }
    public Picker _yearPicker { get; private set; }

    #endregion Properties

    #region Private Methods

    private void OnPickerSelectionChanged(object? sender, EventArgs e)
    {
        if (_monthPicker.SelectedIndex < 0 || _yearPicker.SelectedIndex < 0)
            return;

        int month = _monthPicker.SelectedIndex + 1;
        int year = int.Parse(_yearPicker?.SelectedItem?.ToString() ?? String.Empty);
        try
        {
            DateTimeOffset? newDate; 
            if (FieldDataSource != null)
                newDate = ChangeYearMonth((DateTimeOffset)FieldDataSource, year, month);
            else
                newDate = new DateTime(year, month, FieldDataSource == null ? 1 : FieldDataSource.Value.Day);
            Field_SetDataSourceValue(newDate);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting date: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns a new DateTimeOffset with its year and month replaced, preserving day (clamped), time, and offset.
    /// </summary>
    private static DateTimeOffset? ChangeYearMonth(DateTimeOffset original, int year, int month)
    {
        int day = Math.Min(original.Day, DateTime.DaysInMonth(year, month));
        // rebuild a DateTime preserving the time‐of‐day ticks
        var baseDate = new DateTime(year, month, day).AddTicks(original.TimeOfDay.Ticks);
        return new DateTimeOffset(baseDate, original.Offset);
    }
    #endregion Private Methods

    #region Protected Methods

    protected override DateTimeOffset? Field_GetCurrentValue() => FieldDataSource;

    protected override string Field_GetFormatErrorMessage() => String.Empty;

    protected override bool Field_HasChangedFromLast() =>
        FieldLastValue != FieldDataSource;

    protected override bool Field_HasChangedFromOriginal() =>
        FieldOriginalValue != FieldDataSource;

    protected override bool Field_HasFormatError() => false;

    protected override bool Field_HasRequiredError() =>
        FieldMandatory && FieldDataSource == null;

    protected override void Field_SetValue(DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            int month = value.Value.Month;
            int year = value.Value.Year;
            _monthPicker.SelectedIndex = month - 1;

            // Set the YearPicker based on the year.
            string yearStr = year.ToString();
            for (int i = 0; i < _yearPicker.Items.Count; i++)
            {
                if (_yearPicker.Items[i] == yearStr)
                {
                    _yearPicker.SelectedIndex = i;
                    break;
                }
            }
        }
        else
        {
            _monthPicker.SelectedIndex = -1;
            _yearPicker.SelectedIndex = -1;
        }
    }

    protected override void UpdateRow0Layout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (_monthPicker.Parent is Layout<View> pickerLayout)
                {
                    bool isFieldLabelVisible = FieldLabelVisible;
                    bool isButtonUndoVisible = FieldUndoButton;

                    if (isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(pickerLayout, 1);
                        Grid.SetColumnSpan(pickerLayout, 1);
                    }
                    else if (isFieldLabelVisible && !isButtonUndoVisible)
                    {
                        Grid.SetColumn(pickerLayout, 1);
                        Grid.SetColumnSpan(pickerLayout, 2);
                    }
                    else if (!isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(pickerLayout, 0);
                        Grid.SetColumnSpan(pickerLayout, 2);
                    }
                    else
                    {
                        Grid.SetColumn(pickerLayout, 0);
                        Grid.SetColumnSpan(pickerLayout, 3);
                    }
                }
#pragma warning restore CS0618 // Type or member is obsolete
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
                _monthPicker?.Unfocus();
                _yearPicker?.Unfocus();
            });
        });
    }

    #endregion Public Methods
}