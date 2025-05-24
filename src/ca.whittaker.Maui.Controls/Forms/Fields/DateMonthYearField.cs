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

    protected override void Field_ConfigAccessModeEditing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_HidePlaceholders();

            Field_PerformBatchUpdate(() =>
            {
                _monthPicker.IsVisible = true;
                _yearPicker.IsVisible = true;
                _monthPicker.IsEnabled = true;
                _yearPicker.IsEnabled = true;
                _monthPicker.InputTransparent = false;
                _yearPicker.InputTransparent = false;
            });

            Field_RefreshLayout();
        });
    }


    protected override void Field_ConfigAccessModeViewing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            _monthPicker.Unfocus();
            _yearPicker.Unfocus();

            Field_PerformBatchUpdate(() =>
            {
                _monthPicker.IsVisible = false;
                _yearPicker.IsVisible = false;
                _monthPicker.IsEnabled = false;
                _yearPicker.IsEnabled = false;
            });

            Field_ShowPlaceholders();
            Field_RefreshLayout();
        });
    }


    protected override string Field_GetDisplayText()
    {
        string monthName = string.Empty;
        if (_monthPicker.SelectedIndex >= 0 && _monthPicker.SelectedIndex < _monthPicker.Items.Count)
            monthName = _monthPicker.Items[_monthPicker.SelectedIndex];

        string yearName = string.Empty;
        if (_yearPicker.SelectedIndex >= 0 && _yearPicker.SelectedIndex < _yearPicker.Items.Count)
            yearName = _yearPicker.Items[_yearPicker.SelectedIndex];

        return (!string.IsNullOrEmpty(monthName) && !string.IsNullOrEmpty(yearName))
            ? $"{monthName} {yearName}"  // combined display
            : string.Empty;
    }

    #region Public Constructors

    // Placeholder text shown when no selection in view mode
    public static readonly BindableProperty DateMonthYearPlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(Placeholder),
        returnType: typeof(string),
        declaringType: typeof(DateMonthYearField),
        defaultValue: string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((DateMonthYearField)bindable).OnDateMonthYearPlaceholderChanged(newValue?.ToString() ?? String.Empty));

    public string Placeholder
    {
        get => (string)GetValue(DateMonthYearPlaceholderProperty);
        set => SetValue(DateMonthYearPlaceholderProperty, value);
    }

    public DateMonthYearField()
    {
        // Container grid for layering
        _dateMonthYearContainer = new Grid();
        _dateMonthYearContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        _dateMonthYearContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        // StackLayout containing the month and year pickers
        var stack = new StackLayout { Orientation = StackOrientation.Horizontal };

        _monthPicker = new Picker { VerticalOptions = LayoutOptions.Center };
        for (int i = 1; i <= 12; i++)
            _monthPicker.Items.Add(new DateTime(2000, i, 1).ToString("MMMM"));
        _monthPicker.IsEnabled = false;
        stack.Children.Add(_monthPicker);

        _yearPicker = new Picker { VerticalOptions = LayoutOptions.Center };
        int currentYear = DateTime.Today.Year;
        for (int year = currentYear - 100; year <= currentYear; year++)
            _yearPicker.Items.Add(year.ToString());
        _yearPicker.IsEnabled = false;
        stack.Children.Add(_yearPicker);

        _monthPicker.SelectedIndexChanged += OnPickerSelectionChanged;
        _yearPicker.SelectedIndexChanged += OnPickerSelectionChanged;

        Grid.SetRow(stack, 0);
        Grid.SetColumn(stack, 0);
        _dateMonthYearContainer.Children.Add(stack);


        Initialize();
    }

    private void OnDateMonthYearPlaceholderChanged(string newPlaceholder)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            _placeholderEntry.Text = newPlaceholder;
        });
    }

    protected override List<View> Field_GetControls() => new List<View> { _dateMonthYearContainer };

    #endregion Public Constructors

    #region Properties

    private Grid _dateMonthYearContainer;
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

        DateTimeOffset? newDate; 
        if (FieldDataSource != null)
            newDate = ChangeYearMonth((DateTimeOffset)FieldDataSource, year, month);
        else
            newDate = new DateTime(year, month, FieldDataSource == null ? 1 : FieldDataSource.Value.Day);
        Field_SetDataSourceValue(newDate);
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
    protected override string Field_GetFormatErrorMessage() => string.Empty;

    protected override bool Field_HasChangedFromLast() =>
        !FieldAreValuesEqual(FieldLastValue, Field_GetCurrentValue());

    protected override bool Field_HasChangedFromOriginal() =>
        !FieldAreValuesEqual(FieldOriginalValue, Field_GetCurrentValue());
    
    protected override bool Field_HasFormatError() => false;
    protected override bool Field_HasRequiredError() => FieldMandatory && FieldDataSource == null;


    protected override void Field_SetValue(DateTimeOffset? value)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _monthPicker.SelectedIndexChanged -= OnPickerSelectionChanged;
                _yearPicker.SelectedIndexChanged -= OnPickerSelectionChanged;

                if (value.HasValue)
                {
                    _monthPicker.SelectedIndex = value.Value.Month - 1;

                    string yearStr = value.Value.Year.ToString();
                    _yearPicker.SelectedIndex = _yearPicker.Items.IndexOf(yearStr);
                }
                else
                {
                    _monthPicker.SelectedIndex = -1;
                    _yearPicker.SelectedIndex = -1;
                }

                _monthPicker.SelectedIndexChanged += OnPickerSelectionChanged;
                _yearPicker.SelectedIndexChanged += OnPickerSelectionChanged;

                if (FieldAccessMode != FieldAccessModeEnum.Editing)
                    Field_ShowPlaceholders();
            });
        });
    }


    protected override void Field_RefreshLayout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            _dateMonthYearContainer.InvalidateMeasure();
            InvalidateMeasure();
        });

    }


    #endregion Protected Methods


}