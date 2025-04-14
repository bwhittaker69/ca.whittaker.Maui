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
    #region Fields

    public Picker MonthPicker { get; private set; }
    public Picker YearPicker { get; private set; }

    #endregion Fields

    #region Public Constructors

    public DateMonthYearField()
    {
        MonthPicker = new Picker
        {
            Title = "Month",
            VerticalOptions = LayoutOptions.Center
        };
        YearPicker = new Picker
        {
            Title = "Year",
            VerticalOptions = LayoutOptions.Center
        };

        // Populate MonthPicker with month names.
        for (int i = 1; i <= 12; i++)
        {
            MonthPicker.Items.Add(new DateTime(2000, i, 1).ToString("MMMM"));
        }

        // Populate YearPicker with a range of years (e.g., last 100 years up to current year).
        int currentYear = DateTime.Today.Year;
        for (int year = currentYear - 100; year <= currentYear; year++)
        {
            YearPicker.Items.Add(year.ToString());
        }

        MonthPicker.SelectedIndexChanged += OnPickerSelectionChanged;
        YearPicker.SelectedIndexChanged += OnPickerSelectionChanged;

        InitializeLayout();
    }

    #endregion Public Constructors


    #region Private Methods


    private void OnPickerSelectionChanged(object? sender, EventArgs e)
    {
        if (MonthPicker.SelectedIndex < 0 || YearPicker.SelectedIndex < 0)
            return;

        int month = MonthPicker.SelectedIndex + 1;
        int year = int.Parse(YearPicker?.SelectedItem?.ToString() ?? String.Empty);
        try
        {
            // Create a new DateTime with the day fixed to 1.
            DateTime newDate = new DateTime(year, month, 1);
            FieldDataSource = new DateTimeOffset(newDate, TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting date: {ex.Message}");
        }
    }

    private void Date_ProcessAndSet(DateTimeOffset newDateOffset)
    {
        DateTime newDate = new DateTime(newDateOffset.Year, newDateOffset.Month, 1);
        FieldDataSource = new DateTimeOffset(newDate, TimeSpan.Zero);
    }

    private void Date_SetValue(DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            int month = value.Value.Month;
            int year = value.Value.Year;
            MonthPicker.SelectedIndex = month - 1;

            // Set the YearPicker based on the year.
            string yearStr = year.ToString();
            for (int i = 0; i < YearPicker.Items.Count; i++)
            {
                if (YearPicker.Items[i] == yearStr)
                {
                    YearPicker.SelectedIndex = i;
                    break;
                }
            }
        }
        else
        {
            MonthPicker.SelectedIndex = -1;
            YearPicker.SelectedIndex = -1;
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
            },
            VerticalOptions = LayoutOptions.Fill
        };

        grid.Add(FieldLabel, 0, 0);

        // Create a horizontal layout for the Month and Year pickers.
        var pickerLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            VerticalOptions = LayoutOptions.Center,
            Spacing = 10,
            Children = { MonthPicker, YearPicker }
        };

        grid.Add(pickerLayout, 1, 0);
        grid.Add(ButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        Grid.SetColumnSpan(FieldNotification, 3);
        return grid;
    }

    protected override string Field_GetFormatErrorMessage() => String.Empty;

    protected override bool Field_HasChangedFromLast() =>
        FieldLastValue != FieldDataSource;

    protected override bool Field_HasChangedFromOriginal() =>
        FieldOriginalValue != FieldDataSource;

    protected override bool Field_HasFormatError() => false;

    protected override bool Field_HasRequiredError() =>
        FieldMandatory && FieldDataSource == null;

    protected override void Field_OriginalValue_Reset() => Date_SetValue(FieldOriginalValue);

    protected override void Field_OriginalValue_SetToClear()
    {
        FieldOriginalValue = null;
        Date_SetValue(FieldOriginalValue);
    }

    protected override void Field_OriginalValue_SetToCurrentValue()
    {
        FieldOriginalValue = GetCurrentValue();
    }

    protected override void OnFieldDataSourcePropertyChanged(object newValue, object oldValue)
    {
        if (newValue is DateTimeOffset dto)
        {
            Date_SetValue(dto);
            FieldLastValue = dto;
        }
        else
        {
            Date_SetValue(null);
        }
    }

    protected override void UpdateRow0Layout()
    {
        void _updateRow0Layout()
        {
            
            BatchBegin();
#pragma warning disable CS0618 // Type or member is obsolete
            if (MonthPicker.Parent is Layout<View> pickerLayout)
            {
                bool isFieldLabelVisible = FieldLabelVisible;
                bool isButtonUndoVisible = FieldUndoButtonVisible;

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
        MonthPicker?.Unfocus();
        YearPicker?.Unfocus();
    }

    public DateTimeOffset? GetCurrentValue() => FieldDataSource;

    #endregion Public Methods
}
