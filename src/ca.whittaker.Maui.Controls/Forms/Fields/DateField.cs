namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control that combines several UI elements.
/// </summary>
public partial class DateField : BaseFormField<DateTimeOffset?>
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
        };
        _datePicker.IsEnabled = false;
        _datePicker.DateSelected += DatePicker_DateSelected;
        Field_WireFocusEvents(_datePicker);

        Field_InitializeDataSource();

        InitializeLayout();
    }

    #endregion Public Constructors

    #region Private Methods

    private void Date_ProcessAndSet(DateTimeOffset? newDate)
    {
        if (_datePicker.Date != newDate)
        {
            UiThreadHelper.RunOnMainThread(() =>
            {
                Field_PerformBatchUpdate(() =>
                {
                    _datePicker.Date = newDate.HasValue ? newDate.Value.DateTime : DateTime.MinValue;
                });
            });
        }
        Field_SetDataSourceValue(new DateTimeOffset(_datePicker.Date, TimeSpan.Zero));
    }

    private void DatePicker_DateSelected(object? sender, DateChangedEventArgs e)
    {
        Date_ProcessAndSet(e.NewDate);
        Field_UpdateValidationAndChangedState();
        Field_UpdateNotificationMessage();
        FieldLastValue = e.NewDate;
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
        grid.Add(_datePicker, 1, 0);
        grid.Add(FieldButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        grid.SetColumnSpan(FieldNotification, 3);
        return grid;
    }

    protected override DateTimeOffset? Field_GetCurrentValue() => _datePicker.Date;

    protected override string Field_GetFormatErrorMessage() => String.Empty;

    protected override bool Field_HasChangedFromLast() =>
                FieldLastValue != (_datePicker.Date);

    protected override bool Field_HasChangedFromOriginal() =>
                        FieldOriginalValue != (_datePicker.Date);

    protected override bool Field_HasFormatError()
    {
        return false;
    }

    protected override bool Field_HasRequiredError() =>
                FieldMandatory && _datePicker == null;

    protected override void Field_SetValue(DateTimeOffset? value)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                if (_datePicker.Date != value)
                    _datePicker.Date = value.HasValue ? value.Value.DateTime : DateTime.MinValue;
            });
        });
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