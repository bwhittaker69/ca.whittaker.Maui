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
    private Grid _pickerContainer;
    private Picker _pickerControl;
    private Entry _pickerControlPlaceholderEntry;
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

        // 2) Placeholder on top
        _pickerControlPlaceholderEntry = new Entry
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            Placeholder = DropdownPlaceholder,
            InputTransparent = true,
            IsReadOnly = true,
            IsEnabled = false,
            BackgroundColor = Colors.Transparent
            
        };

        Grid.SetRow(_pickerControlPlaceholderEntry, 0);
        Grid.SetColumn(_pickerControlPlaceholderEntry, 0);
        _pickerContainer.Children.Add(_pickerControlPlaceholderEntry);

        // 3) Picker behind
        _pickerControl = new Picker
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };
        Grid.SetRow(_pickerControl, 0);
        Grid.SetColumn(_pickerControl, 0);
        _pickerContainer.Children.Add(_pickerControl);

        _pickerControl.IsEnabled = false;
        _pickerControl.SelectedIndexChanged += OnDropdownSelectedIndexChanged;

        // 4) Keep your binding logic exactly as before
        _pickerControl.SetBinding(
            Picker.ItemsSourceProperty,
            new Binding(nameof(DropdownItemsSource), source: this)
        );
        if (!string.IsNullOrEmpty(DropdownItemsSourceDisplayPath))
            _pickerControl.ItemDisplayBinding = new Binding(DropdownItemsSourceDisplayPath);

        // 5) Finish initialization
        Initialize();
    }

    protected void Field_ConfigEditing()
    {

    }
    protected void Field_ConfigViewing()
    {

    }


    protected override List<View> Field_ControlMain()
        => new List<View> { _pickerContainer };

    protected override void OnParentSet()
    {
        base.OnParentSet();

        Field_UpdateValidationAndChangedState();
    }

    private enum DataSourceTypeEnum
    {
        delimitedstring,
        complexobject
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

    private void OnDropdownSelectedIndexChanged(object? sender, EventArgs e)
    {
        Debug.WriteLine($"[DropdownField] : {FieldLabelText} : OnDropdownSelectedIndexChanged");

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

            case DataSourceTypeEnum.complexobject:
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
    }
 
    protected override object? Field_GetCurrentValue() => FieldDataSource;

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
        Debug.WriteLine($"[DropdownField] : {FieldLabelText} : Field_SetValue(selectedItem: {selectedItem})");

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

                    if (dataSourceType == DataSourceTypeEnum.complexobject && DropdownItemsSource is IEnumerable items)
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
                else if (dataSourceType == DataSourceTypeEnum.complexobject && DropdownItemsSource is IEnumerable items)
                {
                    // ItemsSource is IList<object?> with first element == null
                    var src = (IList<object?>)_pickerControl.ItemsSource!;
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
        SetValue(FieldDataSourceProperty, newValue);
        Field_SetValue(newValue);
        Field_UpdateValidationAndChangedState();          // ← tell base to re-check
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
                    bool isButtonUndoVisible = FieldUndoButton;

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
