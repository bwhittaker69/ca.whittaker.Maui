﻿namespace ca.whittaker.Maui.Controls.Forms;

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
        _datePicker.Focused += Field_Focused;   
        _datePicker.Unfocused += Field_Unfocused;

        InitializeLayout();
    }

    #endregion Public Constructors


    #region Private Methods


    private void Date_ProcessAndSet(DateTimeOffset? newDate)
    {
        if (_datePicker.Date != newDate)
            _datePicker.Date = newDate.HasValue ? newDate.Value.DateTime : DateTime.MinValue;
        FieldDataSource = new DateTimeOffset(_datePicker.Date, TimeSpan.Zero); 
    }

    private void Date_SetValue(DateTimeOffset? value)
    {
        if (_datePicker.Date != value)
            _datePicker.Date = value.HasValue ? value.Value.DateTime : DateTime.MinValue;
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
        grid.Add(ButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        grid.SetColumnSpan(FieldNotification, 3);
        return grid;
    }

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

    protected override void Field_OriginalValue_Reset()
    {
        Date_SetValue(FieldOriginalValue);
    }

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
    { }

    protected override void OnParentSet()
    {
        base.OnParentSet();
    }

    // Update the _editorBox layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
    protected override void UpdateRow0Layout()
    {
        void _updateRow0Layout()
        {
            BatchBegin();
            if (_datePicker!.Parent is Grid grid)
            {
                bool isFieldLabelVisible = FieldLabelVisible;
                bool isButtonUndoVisible = FieldUndoButtonVisible;

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
        _datePicker?.Unfocus();
    }

    public DateTimeOffset? GetCurrentValue() => _datePicker.Date;

    #endregion Public Methods
}