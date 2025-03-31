using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using ca.whittaker.Maui.Controls.Buttons;
using System.Text.RegularExpressions;
using Entry = Microsoft.Maui.Controls.Entry;
using Label = Microsoft.Maui.Controls.Label;
using System.Windows.Input;

namespace ca.whittaker.Maui.Controls.Forms;

public interface ICheckBoxField : IBaseFormField
{
    #region Public Properties

    bool FieldDataSource { get; set; }

    #endregion Public Properties
}



/// <summary>
/// Represents a customizable checkbox control that combines several UI elements:
/// 
/// - A Grid layout that organizes:
/// 
///   • A FieldLabel (Label) for displaying the field's title.
///   • A CheckBox control for boolean input.
///   • A ButtonUndo for reverting changes.
///   • A FieldNotification label for showing validation or error messages.
///   
/// <para>
/// Differences:
/// - Uses a CheckBox control instead of an Entry for data input.
/// - Exposes its value via a dedicated two-way bindable property named <c>CheckBoxSource</c>.
/// - Contains logic to track changes against an original value and update an undo button accordingly.
/// 
/// Grid Layout Overview:
/// 
/// +-------------------+-------------------+----------------------------------+
/// | FieldLabel        | _checkBox         | ButtonUndo                       |
/// +-------------------+-------------------+----------------------------------+
/// | FieldNotification (spans all three columns)                              |
/// +--------------------------------------------------------------------------+
/// 
/// This composite control supports boolean value capture, change tracking, and validation.
/// </para>
/// </summary>
public partial class CheckBoxField : BaseFormField, ICheckBoxField
{

    #region Private Fields

    private Microsoft.Maui.Controls.CheckBox? _checkBox;

    private bool _originalValue = false;

    #endregion Private Fields

    #region Public Fields

    public static readonly BindableProperty FieldDataSourceProperty = BindableProperty.Create(
        propertyName: nameof(FieldDataSource),
        returnType: typeof(bool),
        declaringType: typeof(CheckBoxField),
        defaultValue: false,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => { ((CheckBoxField)bindable).OnFieldDataSourcePropertyChanged(newValue, oldValue); });

    #endregion Public Fields

    #region Public Constructors

    public CheckBoxField()
    {
        // ************
        //   CHECKBOX
        // ************
        _checkBox = new Microsoft.Maui.Controls.CheckBox
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };
        _checkBox.CheckedChanged += CheckBox_CheckedChanged;
        _checkBox.Focused += Field_Focused;
        _checkBox.Unfocused += Field_Unfocused;

        FieldLabel = FieldCreateLabel();
        FieldNotification = FieldCreateNotificationLabel();
        ButtonUndo = FieldCreateUndoButton(fieldAccessMode: FieldAccessMode);
        Content = FieldCreateLayoutGrid();
        ButtonUndo.Pressed += OnFieldButtonUndoPressed;



    }

    #endregion Public Constructors

    #region Public Properties

    public bool FieldDataSource
    {
        get => (bool)GetValue(FieldDataSourceProperty);
        set
        {
            SetValue(FieldDataSourceProperty, value);
            FieldSetOriginalValue(value);
        }
    }

    #endregion Public Properties

    #region Private Methods

    private void CheckBox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        CheckBoxProcessAndSetValue(e.Value);
        FieldUpdateValidationAndChangedState();
        FieldUpdateNotificationMessage();
    }

    private void CheckBoxProcessAndSetValue(bool? value)
    {
        if (_checkBox != null)
        {
            _checkBox.CheckedChanged -= CheckBox_CheckedChanged;
            FieldDataSource = value == true;
            _checkBox.CheckedChanged += CheckBox_CheckedChanged;
        }
    }

    #endregion Private Methods

    #region Protected Methods

    protected override Grid FieldCreateLayoutGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(FieldLabelWidth, GridUnitType.Absolute) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = new GridLength(DeviceHelper.GetImageSizeForDevice(cUndoButtonSize) * 2, GridUnitType.Absolute) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        grid.Add(FieldLabel, 0, 0);
        grid.Add(_checkBox, 1, 0);
        grid.Add(ButtonUndo, 2, 0);
        grid.Add(FieldNotification, 0, 1);
        Grid.SetColumnSpan(FieldNotification, 3);

        return grid;
    }

    // Disable the field and hide the undo button
    protected override void FieldDisable()
    {
        if (_fieldDisabling || _checkBox != null) return;
        _fieldDisabling = true;
        FieldSetOriginalValue(_originalValue);
        ButtonUndo?.Hide();
        _checkBox!.IsEnabled = false;
        _checkBox!.Unfocus();
        _fieldDisabling = false;
    }

    // Enable the field and disable the undo button
    protected override void FieldEnable()
    {
        if (_fieldEnabling || _checkBox != null) return;
        _fieldEnabling = true;
        FieldSetOriginalValue(_originalValue);
        ButtonUndo?.Disabled();
        _checkBox!.IsEnabled = false;
        FieldUnfocus();
        _fieldEnabling = false;
    }

    protected override string FieldGetFormatErrorMessage()
    {
        throw new Exception("checkbox should not trigger format error");
    }

    protected override bool FieldHasChanged()
    {
        return _originalValue != _checkBox?.IsChecked;
    }

    protected override bool FieldHasFormatError()
    {
        return false;
    }

    protected override bool FieldHasRequiredError()
    {
        if (FieldMandatory && _checkBox == null)
            return true;
        else
            return false;
    }

    protected override bool FieldHasValidData()
    {

        return !FieldHasFormatError() && !FieldHasRequiredError();
    }

    protected void FieldSetOriginalValue(bool? originalValue)
    {
        _fieldIsOriginalValueSet = true;
        _originalValue = originalValue == true;
        if (_checkBox != null)
        {
            _checkBox.CheckedChanged -= CheckBox_CheckedChanged;
            _checkBox.IsChecked = originalValue == true;
            _checkBox.CheckedChanged += CheckBox_CheckedChanged;
        }
    }

    protected override void OnFieldButtonUndoPressed(object? sender, EventArgs e)
    {
        if (!FieldHasChanged()) return;
        if (_undoing) return;
        _undoing = true;
        FieldSetOriginalValue(_originalValue);
        if (_checkBox != null)
            _checkBox.Unfocus();
        _undoing = false;
    }

    protected override void OnFieldDataSourcePropertyChanged(object newValue, object oldValue)
    {
        if (!_fieldIsOriginalValueSet)
        {
            // prevent loop back
            if (!_onFieldDataSourcePropertyChanging)
            {
                _onFieldDataSourcePropertyChanging = true;
                if (_checkBox != null)
                {
                    _checkBox.CheckedChanged -= CheckBox_CheckedChanged;
                    _checkBox.IsChecked = (bool)newValue;
                    _checkBox.CheckedChanged += CheckBox_CheckedChanged;
                }
                _onFieldDataSourcePropertyChanging = false;
            }
        }
    }

    #endregion Protected Methods

    #region Public Methods

    public override void FieldClear()
    {
        FieldSetOriginalValue(false);
        FieldUpdateValidationAndChangedState();
        _checkBox?.Unfocus();
    }
    // make field editable, set original value from data source
    public override void FieldMarkAsEditable()
    {
        FieldUnhide();
        FieldAccessMode = FieldAccessModeEnum.Editable;
        FieldSetOriginalValue(FieldDataSource);
        FieldEnable();
        FieldUpdateValidationAndChangedState();
    }
    // make field readonly, and revert back to original value
    public override void FieldMarkAsReadOnly()
    {
        FieldUnhide();
        FieldAccessMode = FieldAccessModeEnum.ReadOnly;
        if (_checkBox != null)
        {
            FieldSetOriginalValue(FieldDataSource);
            FieldDisable();
            FieldUpdateValidationAndChangedState();
        }
    }
    // make field readonly, and save the current value as original value
    public override void FieldSavedAndMarkAsReadOnly()
    {
        FieldUnhide();
        FieldAccessMode = FieldAccessModeEnum.ReadOnly;
        FieldSetOriginalValue(_checkBox?.IsChecked);
        FieldDisable();
        FieldUpdateValidationAndChangedState();
    }
    // make field hidden
    public override void FieldHide()
    {
        FieldAccessMode = FieldAccessModeEnum.Hidden;
        _checkBox!.IsVisible = false;
        FieldLabel!.IsVisible = false;
        FieldNotification!.IsVisible = false;
        ButtonUndo!.IsVisible = false;
        Content.IsVisible = false;
    }
    protected override void FieldUnhide()
    {
        _checkBox!.IsVisible = true;
        FieldLabel!.IsVisible = true;
        Content.IsVisible = true;
    }


    public override void FieldUnfocus()
    {
        base.FieldUnfocus();
        _checkBox?.Unfocus();
    }

    #endregion Public Methods

}
