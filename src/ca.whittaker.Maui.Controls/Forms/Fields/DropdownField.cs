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

    protected override void Field_ConfigAccessModeEditing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            _pickerContainer.IsVisible = true;
        });
        Field_RefreshLayout();
        Field_HidePlaceholders();
    }
    protected override void Field_ConfigAccessModeViewing()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            ControlVisualHelper.UnfocusDescendantControls(this);
            _pickerContainer.IsVisible = false;
        });
        Field_ShowPlaceholders();
    }
    protected override List<View> Field_GetControls() => new List<View>() { _pickerContainer };
    protected override object? Field_GetCurrentValue() => FieldDataSource;
    protected override void Field_RefreshLayout()
    {
        if (DropdownItemsSource != null)
        {
            if (_pickerControl.SelectedIndex < 0)
                Dropdown_SetItems(DropdownItemsSource);
        }
    }


    private Grid _pickerContainer;
    private UiPicker _pickerControl;
    private UiEntry _pickerControlPlaceholderEntry;
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

    public DropdownField()
    {
        // 1) Prepare an overlay container
        _pickerContainer = new Grid();
        _pickerContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        _pickerContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        // 2) Picker behind
        _pickerControl = new UiPicker
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };
        Grid.SetRow(_pickerControl, 0);
        Grid.SetColumn(_pickerControl, 0);
        _pickerContainer.Children.Add(_pickerControl);
        _pickerControl.SelectedIndexChanged += OnDropdownSelectedIndexChanged;
        _pickerControl.SetBinding(
            Picker.ItemsSourceProperty,
            new Binding(nameof(DropdownItemsSource), source: this)
        );

        _pickerControlPlaceholderEntry = new UiEntry
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
        };
        Grid.SetRow(_pickerControlPlaceholderEntry, 0);
        Grid.SetColumn(_pickerControlPlaceholderEntry, 0);
        _pickerContainer.Children.Add(_pickerControlPlaceholderEntry);

        Initialize();
    }



    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (!string.IsNullOrEmpty(DropdownItemsSourceDisplayPath))
            _pickerControl.ItemDisplayBinding = new Binding(DropdownItemsSourceDisplayPath);

        Field_UpdateValidationAndChangedState();
    }

    protected override string Field_GetDisplayText()
    {
        if (_pickerControl?.SelectedItem == null)
            return string.Empty;

        var selected = _pickerControl.SelectedItem;

        switch (dataSourceType)
        {
            case DataSourceTypeEnum.delimitedstring:
                if (_pickerControl.SelectedIndex >= 0
                    && _pickerControl.Items.Count > _pickerControl.SelectedIndex)
                {
                    return _pickerControl.Items[_pickerControl.SelectedIndex];
                }
                else
                {
                    return string.Empty;
                }

            case DataSourceTypeEnum.listOfString:
                if (_pickerControl.SelectedItem != null)
                {
                    return _pickerControl.Items[_pickerControl.SelectedIndex];
                }
                else
                {
                    return string.Empty;
                }

            case DataSourceTypeEnum.listOfComplexObject:
                if (_pickerControl.SelectedItem != null)
                {
                    return GetProperty(_pickerControl.SelectedItem, DropdownItemsSourceDisplayPath);
                }
                else
                {
                    return string.Empty;
                }
            default:
                return string.Empty;
        }
    }
    private enum DataSourceTypeEnum
    {
        delimitedstring,
        listOfComplexObject,
        listOfString
    }

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

    private void Dropdown_SetItems(object newValue)
    {

        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                // Update the Picker's Items collection based on the new dropdown items.

                if (newValue is IList itemSource && newValue is not IList<string>)
                {
                    // ***************************
                    //   list of complex objects
                    // ***************************
                    dataSourceType = DataSourceTypeEnum.listOfComplexObject;

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
                } 
                else if (newValue is IList<string> listOfStrings)
                {
                    // ***************************
                    //   list of string
                    // ***************************
                    dataSourceType = DataSourceTypeEnum.listOfString;

                    // If not mandatory, create a modifiable list with a blank item at the beginning.
                    if (!FieldMandatory)
                    {
                        var modifiableList = new List<string>();
                        // Add a blank item; null works if your display binding returns an empty string.
                        modifiableList.Add(String.Empty);
                        foreach (var item in listOfStrings)
                        {
                            modifiableList.Add(item);
                        }
                        _pickerControl.ItemsSource = modifiableList;
                    }
                    else
                    {
                        _pickerControl.ItemsSource = (IList)listOfStrings;
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
                    //throw new ArgumentException("Invalid type for DropdownItems. Expected IEnumerable<object>.");
                }

                // Re-apply the VM value now that the list exists
                Field_SetValue(FieldDataSource);

                // And make sure change-tracking / undo state are refreshed
                Field_UpdateValidationAndChangedState();

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
                _pickerControlPlaceholderEntry.Text = newValue?.ToString() ?? "";
                _pickerControlPlaceholderEntry.InputTransparent = true;
                _pickerControlPlaceholderEntry.VerticalOptions = LayoutOptions.Center;
                _pickerControlPlaceholderEntry.HorizontalOptions = LayoutOptions.Fill;
                _pickerControlPlaceholderEntry.IsEnabled = false;
            });
        });
    }

    private void ProcessDropdownSelectedIndexChanged()
    {
        if (FieldAccessMode != FieldAccessModeEnum.Editing)
            return;

        object? newValue;

        switch (dataSourceType)
        {
            case DataSourceTypeEnum.delimitedstring:
                if (_pickerControl.SelectedIndex >= 0
                    && _pickerControl.Items.Count > _pickerControl.SelectedIndex)
                {
                    newValue = _pickerControl.Items[_pickerControl.SelectedIndex];
                }
                else
                {
                    newValue = string.Empty;
                }
                break;

            case DataSourceTypeEnum.listOfString:
                if (_pickerControl.SelectedItem != null)
                {
                    newValue = _pickerControl.SelectedItem;
                }
                else
                {
                    newValue = string.Empty;
                }
                break;


            case DataSourceTypeEnum.listOfComplexObject:
                if (_pickerControl.SelectedItem != null)
                {
                    newValue = GetProperty(_pickerControl.SelectedItem, DropdownItemsSourcePrimaryKey);
                }
                else
                {
                    newValue = string.Empty;
                }
                break;

            default:
                newValue = null;
                break;
        }

        // push into VM, then record as “last”
        Field_SetDataSourceValue(newValue);
        FieldLastValue = newValue;

        if (base.FieldAccessMode == FieldAccessModeEnum.Editing)
            base.FieldButtonUndo!.ButtonState = Field_HasChangedFromOriginal() ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled;
        else
            base.FieldButtonUndo!.ButtonState = ButtonStateEnum.Hidden;


    }

    private void OnDropdownSelectedIndexChanged(object? sender, EventArgs e)
    {
        ProcessDropdownSelectedIndexChanged();
    }

    protected override string Field_GetFormatErrorMessage() =>
        "Invalid selection.";

    protected override bool Field_HasChangedFromLast() =>
        !object.Equals(FieldLastValue, FieldDataSource);

    protected override bool Field_HasChangedFromOriginal() =>
        !object.Equals(FieldOriginalValue, FieldDataSource);

    protected override bool Field_HasFormatError()
    { return false; }

    //protected override bool Field_HasRequiredError() => FieldMandatory && FieldDataSource == null;
    protected override bool Field_HasRequiredError()
    {
        // 1) Not mandatory → no error
        if (!FieldMandatory)
            return false;

        // 2) Null → definitely no selection → error
        if (FieldDataSource == null)
            return true;

        // 3) Empty-string → treated as no selection → error
        if (FieldDataSource is string s && string.IsNullOrWhiteSpace(s))
            return true;

        // 4) Otherwise, a valid selection exists → no error
        return false;
    }
    protected override void Field_SetValue(object? selectedItem)
    {

        if (DropdownItemsSource == null) 
            return;

        UiThreadHelper.RunOnMainThread(() =>
            Field_PerformBatchUpdate(() =>
            {
                // Detach handler to prevent recursive SelectedIndexChanged calls
                _pickerControl.SelectedIndexChanged -= OnDropdownSelectedIndexChanged;

                // Show placeholder when there’s no real selection
                bool noSelection = selectedItem == null
                    || (selectedItem is string s && string.IsNullOrWhiteSpace(s));

                _pickerControlPlaceholderEntry.IsVisible = noSelection;

                if (noSelection)
                {

                    // clear selection
                    if (dataSourceType == DataSourceTypeEnum.delimitedstring)
                        if (_pickerControl.SelectedIndex > 0)
                            _pickerControl.SelectedIndex = -1;

                    if (dataSourceType == DataSourceTypeEnum.listOfComplexObject && DropdownItemsSource is IEnumerable)
                        if (_pickerControl.SelectedItem != null)
                            _pickerControl.SelectedItem = null;

                    if (dataSourceType == DataSourceTypeEnum.listOfString && DropdownItemsSource is IEnumerable)
                        if (_pickerControl.SelectedItem != null)
                            _pickerControl.SelectedItem = null;

                }
                else if (dataSourceType == DataSourceTypeEnum.delimitedstring)
                {
                    // Items is a string list: find the index of that value
                    var text = selectedItem!.ToString()!;
                    // Items[0] is the blank placeholder, so this will push up to the correct slot
                    _pickerControl.SelectedIndex = _pickerControl.Items.IndexOf(text);
                }
                else if (dataSourceType == DataSourceTypeEnum.listOfString && DropdownItemsSource is IEnumerable itemsListofString)
                {
                    // ItemsSource is IList<object?> with first element == null
                    var src = (IList<string?>)_pickerControl.ItemsSource!;
                    // scan from 1…N
                    for (int i = 1; i < src.Count; i++)
                    {
                        var key = src[i]!.ToString();
                        if (key == selectedItem!.ToString())
                        {
                            _pickerControl.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else if (dataSourceType == DataSourceTypeEnum.listOfComplexObject && DropdownItemsSource is IEnumerable itemsListOfComplexObjects)
                {
                    // ItemsSource is IList<object?> with first element == null
                    var src = (IList)_pickerControl.ItemsSource!;
                    // scan from 1…N
                    for (int i = 1; i < src.Count; i++)
                    {
                        var obj = src[i]!;
                        var key = GetProperty(obj, DropdownItemsSourcePrimaryKey)?.ToString();
                        if (key == selectedItem!.ToString())
                        {
                            // select it by index (this also sets SelectedItem under the hood)
                            _pickerControl.SelectedIndex = i;
                            break;
                        }
                    }
                }
                _pickerControl.SelectedIndexChanged += OnDropdownSelectedIndexChanged;
            })
        );
    }

    protected override void OnBindingContextChanged()
    {
        // 1) grab the current datasource (VM→Field) before base runs
        var initial = FieldDataSource;

        // 2) call up so MAUI wires everything up (but don't let this fire our change logic)
        bool wasSuppress = FieldSuppressDataSourceCallback;
        FieldSuppressDataSourceCallback = true;
        base.OnBindingContextChanged();
        FieldSuppressDataSourceCallback = wasSuppress;

        // 3) if we haven’t yet recorded an original, do it now—without touching the UI
        if (!FieldIsOriginalValueSet)
        {
            FieldOriginalValue = initial;
            FieldIsOriginalValueSet = true;
        }
    }


    protected override void Field_SetDataSourceValue(object? newValue)
    {
        FieldSuppressDataSourceCallback = true;           // ← prevent original-capture loop
        SetValue(FieldDataSourceProperty, newValue);      // ← set data source property
        Field_SetValue(newValue);                         // ← force update of picker control
        Field_UpdateValidationAndChangedState();          // ← tell base to re-check
        Field_UpdateNotificationMessage();

    }


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
        catch
        {
            return null;
        }
    }

    // Convert back from the primary key to the corresponding object in the ItemsSource.
    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value == null || parameter == null || string.IsNullOrEmpty(PrimaryKeyName))
        {
            return null;
        }

        if (!(parameter is IEnumerable items))
        {
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
