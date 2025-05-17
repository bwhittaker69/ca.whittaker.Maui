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
    /// <summary>
    /// Put the control into **Editing** mode:
    /// – make the live <see cref="_datePicker"/> visible and  enabled
    /// – hide the read‑only placeholder
    /// – notify the layout system so the grid resizes correctly
    /// – use <c>Field_HidePlaceholders()</c> for consistency
    /// </summary>
    protected override void Field_ConfigAccessModeEditing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_HidePlaceholders();

            _datePicker.IsEnabled = true;
            _datePicker.IsVisible = true;
            _datePicker.InputTransparent = false;

            Field_RefreshLayout();
        });
    }

    /// <summary>
    /// Put the control into **Viewing** mode:
    /// – disable / hide the live picker
    /// – surface the placeholder with the display text
    /// – ensure focus is cleared so keyboards are dismissed
    /// – call <c>Field_RefreshLayout()</c> to recompute the grid
    /// </summary>
    protected override void Field_ConfigAccessModeViewing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            if (_datePicker.IsFocused)
                _datePicker.Unfocus();

            _datePicker.IsEnabled = false;
            _datePicker.IsVisible = false;

            Field_ShowPlaceholders();   // sets text & brings to front
            Field_RefreshLayout();
        });
    }

    protected override string Field_GetDisplayText()
    {
        if (FieldDataSource.HasValue)
            return FieldDataSource.Value.Date.ToShortDateString();
        return string.Empty;
    }

    #region Fields

    private Grid _datePickerContainer;
    private DatePicker _datePicker;

    #endregion Fields

    // Bindable property for placeholder text
    public static readonly BindableProperty DatePlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(DatePlaceholder),
        returnType: typeof(string),
        declaringType: typeof(DateField),
        defaultValue: string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((DateField)bindable).OnDatePlaceholderChanged(newValue?.ToString() ?? String.Empty)
    );

    private void OnDatePlaceholderChanged(string newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            _placeholderEntry.Text = newValue;
        });
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

    public string DatePlaceholder
    {
        get => (string)GetValue(DatePlaceholderProperty);
        set => SetValue(DatePlaceholderProperty, value);
    }

    #region Public Constructors

    public DateField()
    {
        // 1) Container overlay
        _datePickerContainer = new Grid();
        _datePickerContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        _datePickerContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        // 3) DatePicker behind
        _datePicker = new DatePicker
        {
            VerticalOptions = LayoutOptions.Center,
            MinimumDate = new DateTime(1900, 1, 1),
            MaximumDate = new DateTime(2100, 12, 31),
            Date = FieldDataSource?.DateTime ?? DateTime.Today
        };
        _datePicker.IsEnabled = false;
        _datePicker.DateSelected += DatePicker_DateSet;

        Grid.SetRow(_datePicker, 0);
        Grid.SetColumn(_datePicker, 0);
        _datePickerContainer.Children.Add(_datePicker);

        Initialize();
    }

    protected override List<View> Field_GetControls() => new List<View> { _datePickerContainer };

    #endregion Public Constructors

    #region Private Methods

    protected override void Field_SetDataSourceValue(DateTimeOffset? newValue)
    {

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

    #endregion Private Methods

    #region Protected Methods

    protected override DateTimeOffset? Field_GetCurrentValue() => _datePicker.Date;

    protected override string Field_GetFormatErrorMessage() => String.Empty;


    /// <summary>Detects a change from the last value using the helper.</summary>
    protected override bool Field_HasChangedFromLast() =>
            !FieldAreValuesEqual(FieldLastValue, Field_GetCurrentValue());


    /// <summary>Detects a change from the original value using the helper.</summary>
    protected override bool Field_HasChangedFromOriginal() =>
            !FieldAreValuesEqual(FieldOriginalValue, Field_GetCurrentValue());

    protected override bool Field_HasFormatError()
    {
        return false;
    }

    /// <summary>
    /// Required‑field validation: true when mandatory **and** no value chosen.
    /// </summary>
    protected override bool Field_HasRequiredError() =>
            FieldMandatory && Field_GetCurrentValue() == null;

    /// <summary>
    /// Pushes a new value from the VM into the picker without
    /// triggering recursive <c>DateSelected</c> events and keeps the
    /// placeholder in sync when in viewing mode.
    /// </summary>
    protected override void Field_SetValue(DateTimeOffset? newDate)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _datePicker.DateSelected -= DatePicker_DateSet;   // 1) prevent recursion
                try
                {
                    if (_datePicker.Date != newDate.ToDateTime())
                        _datePicker.Date = newDate.ToDateTime();
                }
                finally
                {
                    _datePicker.DateSelected += DatePicker_DateSet; // 2) re‑attach
                }

                if (FieldAccessMode != FieldAccessModeEnum.Editing)
                    Field_ShowPlaceholders();
            });
        });
    }

    /// <summary>
    /// Forces a re‑layout after any visibility or value change so column
    /// widths and row heights recalculate correctly.
    /// </summary>
    protected override void Field_RefreshLayout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            _datePickerContainer.InvalidateMeasure();
            this.InvalidateMeasure();
        });
    }

    #endregion Protected Methods

    protected override void OnParentSet()
    {
        base.OnParentSet();


        // 1) Take whatever the DatePicker is showing now as the ORIGINAL
        //    so that initial change-detector sees no difference.
        FieldOriginalValue = _datePicker.Date.ToDateTimeOffset();

        // 2) Re-evaluate change state now that original==current
        Field_UpdateValidationAndChangedState();
    }
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