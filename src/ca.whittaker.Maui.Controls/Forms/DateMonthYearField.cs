using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Grid = Microsoft.Maui.Controls.Grid;
using StackLayout = Microsoft.Maui.Controls.StackLayout;

namespace ca.whittaker.Maui.Controls.Forms
{
    public partial class DateMonthYearField : BaseFormField
    {
        #region Fields

        private DateTimeOffset? _lastValue = null;
        private DateTimeOffset? _originalValue = null;
        private Picker _pickerMonth;
        private Picker _pickerYear;

        public static readonly BindableProperty DateDataSourceProperty = BindableProperty.Create(
            propertyName: nameof(DateDataSource),
            returnType: typeof(DateTimeOffset?),
            declaringType: typeof(DateMonthYearField),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
                ((DateMonthYearField)bindable).OnDataSourcePropertyChanged(newValue, oldValue));

        #endregion Fields

        #region Public Constructors

        public DateMonthYearField()
        {
            _pickerMonth = new Picker
            {
                Title = "Month",
                VerticalOptions = LayoutOptions.Center
            };
            _pickerYear = new Picker
            {
                Title = "Year",
                VerticalOptions = LayoutOptions.Center
            };

            // Populate MonthPicker with month names.
            if (!FieldMandatory)
                _pickerMonth.Items.Add(String.Empty);
            for (int i = 1; i <= 12; i++)
            {
                _pickerMonth.Items.Add(new DateTime(2000, i, 1).ToString("MMMM"));
            }

            // Populate YearPicker with a range of years (e.g., last 100 years up to current year).
            int currentYear = DateTime.Today.Year;

            if (!FieldMandatory) _pickerYear.Items.Add(String.Empty);

            for (int year = currentYear - 100; year <= currentYear; year++)
                _pickerYear.Items.Add(year.ToString());

            _pickerMonth.SelectedIndexChanged += OnPickerSelectionChanged;
            _pickerYear.SelectedIndexChanged += OnPickerSelectionChanged;

            InitializeLayout();
        }

        #endregion Public Constructors

        #region Properties

        public DateTimeOffset? DateDataSource
        {
            get => (DateTimeOffset?)GetValue(DateDataSourceProperty);
            set => SetValue(DateDataSourceProperty, value);
        }

        #endregion Properties

        #region Private Methods

        private void Date_SetValue(DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                int month = value.Value.Month;
                int year = value.Value.Year;
                _pickerMonth.SelectedIndex = month - 1 + (FieldMandatory ? 1 : 0);

                // Set the YearPicker based on the year.
                string yearStr = year.ToString();
                for (int i = 0; i < _pickerYear.Items.Count; i++)
                {
                    if (_pickerYear.Items[i] == yearStr)
                    {
                        _pickerYear.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                if (FieldMandatory)
                {
                    _pickerMonth.SelectedIndex = 0;
                    _pickerYear.SelectedIndex = 0;
                }
                else
                {
                    _pickerMonth.SelectedIndex = -1;
                    _pickerYear.SelectedIndex = -1;
                }

            }
        }

        private DateTimeOffset? Date_GetValue()
        {
            DateTimeOffset? currentDateTimeOffset = null;
            bool isMonthBlank = _pickerMonth.SelectedIndex == 0 && FieldMandatory;
            bool isYearBlank = _pickerYear.SelectedIndex == 0 && FieldMandatory;

            if (isMonthBlank || isYearBlank)
                return currentDateTimeOffset;

            int month = _pickerMonth.SelectedIndex - (FieldMandatory ? 0 : 1);
            if (!int.TryParse(_pickerYear?.SelectedItem?.ToString() ?? String.Empty, out int year))
                year = 0;

            if (year > 0 && month > 0)
            {
                try
                {
                    // Create a new DateTime with the day fixed to 1.
                    DateTime newDate = new DateTime(year, month, 1);
                    currentDateTimeOffset = new DateTimeOffset(newDate, TimeSpan.Zero);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting date: {ex.Message}");
                }
            }
            return currentDateTimeOffset;
        }

        private void OnPickerSelectionChanged(object? sender, EventArgs e)
        {
            Debug.WriteLine($"OnPickerSelectionChanged");
            if (FieldAccessMode == FieldAccessModeEnum.Editing)
            {
                DateDataSource = Date_GetValue();
                Field_UpdateValidationAndChangedState();
                Field_UpdateNotificationMessage();
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
                Children = { _pickerMonth, _pickerYear }
            };

            grid.Add(pickerLayout, 1, 0);
            grid.Add(ButtonUndo, 2, 0);
            grid.Add(FieldNotification, 0, 1);
            Grid.SetColumnSpan(FieldNotification, 3);
            return grid;
        }

        protected override string Field_GetFormatErrorMessage() => String.Empty;

        protected override bool Field_HasChangedFromLast() =>
            _lastValue != DateDataSource;

        protected override bool Field_HasChangedFromOriginal() =>
            _originalValue != DateDataSource;

        protected override bool Field_HasFormatError() => false;

        protected override bool Field_HasRequiredError() =>
            FieldMandatory && DateDataSource == null;

        protected override void Field_OriginalValue_Reset() => Date_SetValue(_originalValue);

        protected override void Field_OriginalValue_SetToClear()
        {
            _originalValue = null;
            Date_SetValue(_originalValue);
        }

        protected override void Field_OriginalValue_SetToCurrentValue()
        {
            _originalValue = GetCurrentValue();
        }

        protected override void OnDataSourcePropertyChanged(object newValue, object oldValue)
        {
            //if (newValue is DateTimeOffset dto)
            //{
            //    Date_SetValue(dto);
            //    _lastValue = dto;
            //}
            //else
            //{
            //    Date_SetValue(null);
            //}
        }

        protected override void UpdateRow0Layout()
        {
            void _updateRow0Layout()
            {
                BatchBegin();
#pragma warning disable CS0618 // Type or member is obsolete
                if (_pickerMonth.Parent is Layout<View> pickerLayout)
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
            _pickerMonth?.Unfocus();
            _pickerYear?.Unfocus();
        }

        public DateTimeOffset? GetCurrentValue() => DateDataSource;

        #endregion Public Methods
    }
}