using Microsoft.Maui.Controls.Compatibility;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Grid = Microsoft.Maui.Controls.Grid;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable dropdown field control using the MAUI Picker.
/// </summary>
public partial class DropdownField : BaseFormField<object>
{
    #region Fields

    private Grid _pickerContainer;
    private Picker _pickerControl;
    private Entry _pickerControlPlaceholder;
    private DataSourceTypeEnum dataSourceType;

    // Bindable property for DropdownItemsSourceDisplayPath
    public static readonly BindableProperty DropdownItemsSourceDisplayPathProperty = BindableProperty.Create(
        propertyName: nameof(DropdownItemsSourceDisplayPath),
        returnType: typeof(string),
        declaringType: typeof(DropdownField),
        defaultValue: string.Empty);

    // Bindable property for DropdownItemsSourcePrimaryKey
    public static readonly BindableProperty DropdownItemsSourcePrimaryKeyProperty = BindableProperty.Create(
        propertyName: nameof(DropdownItemsSourcePrimaryKey),
        returnType: typeof(string),
        declaringType: typeof(DropdownField),
        defaultValue: string.Empty);

    /// <summary>Bindable property for <see cref="DropdownItemsSource"/>.</summary>
    public static readonly BindableProperty DropdownItemsSourceProperty =
        BindableProperty.Create(
            propertyName: nameof(DropdownItemsSource),
            returnType: typeof(object),
            declaringType: typeof(DropdownField),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: (bindable, oldValue, newValue) =>
                 ((DropdownField)bindable).OnDropdownItemsSourceChanged(newValue));

    // Bindable property for the dropdown placeholder (title).
    public static readonly BindableProperty DropdownPlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(DropdownPlaceholder),
        returnType: typeof(string),
        declaringType: typeof(DropdownField),
        defaultValue: string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((DropdownField)bindable).OnDropdownPlaceholderPropertyChanged(newValue));

    #endregion Fields

    #region Public Constructors

    public DropdownField()
    {
        _pickerContainer = new Grid
        {
        };
        _pickerControlPlaceholder = new Entry
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            Placeholder = DropdownPlaceholder,
            InputTransparent = true,
            IsEnabled = false,
            BackgroundColor = Colors.Transparent
        };
        _pickerControl = new Picker
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };
        _pickerControl.IsEnabled = false;
        _pickerControl.SelectedIndexChanged += OnDropdownSelectedIndexChanged;

        Field_WireFocusEvents(_pickerControl);

        // Use ItemDisplayBinding instead of custom DisplayMemberPath
        _pickerControl.SetBinding(Picker.ItemsSourceProperty, new Binding(nameof(DropdownItemsSource), source: this));

        if (!String.IsNullOrEmpty(DropdownItemsSourceDisplayPath))
            _pickerControl.ItemDisplayBinding = new Binding(DropdownItemsSourceDisplayPath);

        Field_InitializeDataSource();

        // Initialize layout of this control.
        InitializeLayout();
    }

    #endregion Public Constructors

    #region Enums

    private enum DataSourceTypeEnum
    {
        delimitedstring,
        complexobject
    }

    #endregion Enums

    #region Properties

    /// <summary>
    /// Gets or sets dropdown items.
    /// </summary>
    public object DropdownItemsSource
    {
        get => (object)GetValue(DropdownItemsSourceProperty);
        set => SetValue(DropdownItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the property name to display in the dropdown when binding to complex objects.
    /// </summary>
    public string DropdownItemsSourceDisplayPath
    {
        get => (string)GetValue(DropdownItemsSourceDisplayPathProperty);
        set => SetValue(DropdownItemsSourceDisplayPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the primary key field name to use when binding to complex objects.
    /// </summary>
    public string DropdownItemsSourcePrimaryKey
    {
        get => (string)GetValue(DropdownItemsSourcePrimaryKeyProperty);
        set => SetValue(DropdownItemsSourcePrimaryKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder (title) of the dropdown.
    /// </summary>
    public string DropdownPlaceholder
    {
        get => (string)GetValue(DropdownPlaceholderProperty);
        set => SetValue(DropdownPlaceholderProperty, value);
    }

    #endregion Properties

    #region Private Methods

    private void Dropdown_SetItems(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                // Update the Picker's Items collection based on the new dropdown items.

                if (newValue is System.Collections.IList itemSource)
                {
                    // ***************************
                    //   list of complex objects
                    // ***************************
                    dataSourceType = DataSourceTypeEnum.complexobject;

                    // If not mandatory, create a modifiable list with a blank item at the beginning.
                    if (!FieldMandatory)
                    {
                        var modifiableList = new List<object?>();
                        // Add a blank item; null works if your display binding returns an empty string.
                        modifiableList.Add(null);
                        foreach (var item in itemSource)
                        {
                            modifiableList.Add(item);
                        }
                        _pickerControl.ItemsSource = modifiableList;
                    }
                    else
                    {
                        _pickerControl.ItemsSource = itemSource;
                    }

                    if (!string.IsNullOrEmpty(DropdownItemsSourceDisplayPath))
                    {
                        _pickerControl.ItemDisplayBinding = new Binding(DropdownItemsSourceDisplayPath);
                    }
                }
                else if (newValue is string str)
                {
                    // *************************************
                    //   string of delimited list of items
                    // *************************************
                    dataSourceType = DataSourceTypeEnum.delimitedstring;
                    _pickerControl.Items.Clear();
                    var itemsFromString = str.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (FieldMandatory == false)
                        _pickerControl.Items.Add(string.Empty);
                    foreach (var s in itemsFromString)
                    {
                        _pickerControl.Items.Add(s.Trim());
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid type for DropdownItems. Expected IEnumerable<object>.");
                }
            });
        });
    }

    private string GetProperty(object instance, string propertyName)
    {
        if (instance == null) return String.Empty;
        PropertyInfo? property = instance!.GetType().GetProperty(propertyName);
        if (property == null) return String.Empty;
        var valueAsObject = property.GetValue(instance) ?? throw new Exception($"property {propertyName} does not exist");
        return valueAsObject.ToString() ?? String.Empty;
    }

    private void OnDropdownItemsSourceChanged(object newValue)
        => Dropdown_SetItems(newValue);

    private void OnDropdownPlaceholderPropertyChanged(object newValue)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                _pickerControlPlaceholder.Text = newValue?.ToString() ?? "";
                _pickerControlPlaceholder.InputTransparent = true;
                _pickerControlPlaceholder.VerticalOptions = LayoutOptions.Center;
                _pickerControlPlaceholder.HorizontalOptions = LayoutOptions.Fill;
                _pickerControlPlaceholder.IsEnabled = false;
            });
        });
    }

    private void OnDropdownSelectedIndexChanged(object? sender, EventArgs e)
    {
        Debug.WriteLine($"OnDropdownSelectedIndexChanged");

        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            // When a new selection is made, update the binding property.
            switch (dataSourceType)
            {
                case DataSourceTypeEnum.delimitedstring:
                    if (_pickerControl.SelectedIndex >= 0 && _pickerControl.Items.Count > _pickerControl.SelectedIndex)
                        Field_SetDataSourceValue(_pickerControl.Items[_pickerControl.SelectedIndex]);
                    else
                        Field_SetDataSourceValue(String.Empty);
                    break;

                case DataSourceTypeEnum.complexobject:
                    if (_pickerControl.SelectedItem != null)
                    {
                        var key = GetProperty(_pickerControl.SelectedItem, DropdownItemsSourcePrimaryKey);
                        Field_SetDataSourceValue(key);
                    }
                    else
                        Field_SetDataSourceValue(String.Empty);
                    break;
            }
        }
    }
    #endregion Private Methods

    #region Protected Methods
    protected override Grid Field_CreateLayoutGrid()
    {
        // Explicitly add one row and one column.
        _pickerContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        _pickerContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        // Set the Picker to row 0, column 0.
        Grid.SetRow(_pickerControl, 0);
        Grid.SetColumn(_pickerControl, 0);
        _pickerContainer.Children.Add(_pickerControl);

        // Set the placeholder Label to row 0, column 0 as well.
        Grid.SetRow(_pickerControlPlaceholder, 0);
        Grid.SetColumn(_pickerControlPlaceholder, 0);

        // Make sure the Label is transparent to input and background.
        _pickerControlPlaceholder.InputTransparent = true;
        _pickerControlPlaceholder.BackgroundColor = Colors.Transparent;
        _pickerControlPlaceholder.HorizontalOptions = LayoutOptions.Fill;
        _pickerControlPlaceholder.VerticalOptions = LayoutOptions.Center;
        _pickerContainer.Children.Add(_pickerControlPlaceholder);

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
        grid.Add(_pickerContainer, 1, 0);
        grid.Add(FieldButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        grid.SetColumnSpan(FieldNotification, 3);
        return grid;
    }

    protected override object? Field_GetCurrentValue() => FieldDataSource;

    protected override string Field_GetFormatErrorMessage() =>
        "Invalid selection.";

    protected override bool Field_HasChangedFromLast() =>
        !object.Equals(FieldLastValue, FieldDataSource);

    protected override bool Field_HasChangedFromOriginal() =>
        !object.Equals(FieldOriginalValue, FieldDataSource);

    protected override bool Field_HasFormatError() { return false; }
    protected override bool Field_HasRequiredError() => FieldMandatory && FieldDataSource == null;

    protected override void Field_SetValue(object? selectedItem)
    {
        Debug.WriteLine($"Dropdown_SetSelectedItem ({selectedItem})");

        if (_pickerControl != null)
            UiThreadHelper.RunOnMainThread(() =>
            {
                Field_PerformBatchUpdate(() =>
                {
                    if ((selectedItem is not string && selectedItem is not null)
                         || !String.IsNullOrEmpty(selectedItem?.ToString()))
                    {
                        switch (dataSourceType)
                        {
                            case DataSourceTypeEnum.delimitedstring:
                                _pickerControl.SelectedIndex = _pickerControl.Items.IndexOf(selectedItem.ToString());
                                _pickerControlPlaceholder.IsVisible = _pickerControl.SelectedIndex < 0;
                                if (_pickerControl!.SelectedIndex > -1 && FieldDataSource != selectedItem)
                                    Field_SetDataSourceValue(selectedItem?.ToString() ?? String.Empty);
                                break;

                            case DataSourceTypeEnum.complexobject:
                                _pickerControlPlaceholder.IsVisible = false;
                                Field_SetDataSourceValue(selectedItem?.ToString() ?? String.Empty);
                                break;
                        }
                    }
                    else
                    {
                        _pickerControlPlaceholder.IsVisible = true;
                        _pickerControlPlaceholder.InputTransparent = true;
                        _pickerControlPlaceholder.VerticalOptions = LayoutOptions.Center;
                        _pickerControlPlaceholder.HorizontalOptions = LayoutOptions.Fill;
                        Field_SetDataSourceValue(null);
                        switch (dataSourceType)
                        {
                            case DataSourceTypeEnum.delimitedstring:
                                _pickerControl.SelectedIndex = -1;
                                break;

                            case DataSourceTypeEnum.complexobject:
                                _pickerControl.SelectedItem = null;
                                break;
                        }
                    }
                });
            });
    }


    // Update the _pickerControl layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
    protected override void UpdateRow0Layout()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                if (_pickerContainer?.Parent is Grid grid)
                {
                    bool isFieldLabelVisible = FieldLabelVisible;
                    bool isButtonUndoVisible = FieldUndoButtonVisible;

                    if (isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(_pickerContainer, 1);
                        Grid.SetColumnSpan(_pickerContainer, 1);
                    }
                    else if (isFieldLabelVisible && !isButtonUndoVisible)
                    {
                        Grid.SetColumn(_pickerContainer, 1);
                        Grid.SetColumnSpan(_pickerContainer, 2);
                    }
                    else if (!isFieldLabelVisible && isButtonUndoVisible)
                    {
                        Grid.SetColumn(_pickerContainer, 0);
                        Grid.SetColumnSpan(_pickerContainer, 2);
                    }
                    else // both not visible
                    {
                        Grid.SetColumn(_pickerContainer, 0);
                        Grid.SetColumnSpan(_pickerContainer, 3);
                    }
                }
            });
        });

        if (DropdownItemsSource != null)
        {
            if (_pickerControl.SelectedIndex < 0)
                Dropdown_SetItems(DropdownItemsSource);
        }
    }

    #endregion Protected Methods

    #region Public Methods

    public static List<string> GetListFromDelimitedString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        int commaCount = input.Count(c => c == ',');
        int semicolonCount = input.Count(c => c == ';');
        // Choose the delimiter with the greater count. If equal, default to comma.
        char delimiter = commaCount >= semicolonCount ? ',' : ';';

        return input
            .Split(delimiter)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    public override void Field_Unfocus()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                base.Field_Unfocus();
                _pickerControl?.Unfocus();
            });
        });
    }

    #endregion Public Methods
}

// Converter to extract the primary key from the selected item.
public class SelectedItemPrimaryKeyConverter : IValueConverter
{
    #region Properties

    public string? PrimaryKeyName { get; set; }

    #endregion Properties

    #region Public Methods

    // Convert from SelectedItem (the whole object) to its primary key.
    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value == null || string.IsNullOrEmpty(PrimaryKeyName))
            return null;

        try
        {
            PropertyInfo? prop = value.GetType().GetProperty(PrimaryKeyName);
            return prop?.GetValue(value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SelectedItemPrimaryKeyConverter.Convert: {ex.Message}");
            return null;
        }
    }

    // Convert back from the primary key to the corresponding object in the ItemsSource.
    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value == null || parameter == null || string.IsNullOrEmpty(PrimaryKeyName))
        {
            Debug.WriteLine("ConvertBack: value, parameter, or PrimaryKeyName is null.");
            return null;
        }

        if (!(parameter is IEnumerable items))
        {
            Debug.WriteLine($"ConvertBack: parameter is not IEnumerable. Actual type: {parameter.GetType()}");
            return null;
        }

        try
        {
            foreach (var item in items)
            {
                PropertyInfo? prop = item.GetType().GetProperty(PrimaryKeyName);
                if (prop != null && Equals(prop.GetValue(item), value))
                    return item;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SelectedItemPrimaryKeyConverter.ConvertBack: {ex.Message}");
        }
        return null;
    }

    #endregion Public Methods
}