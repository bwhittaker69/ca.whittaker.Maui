using ca.whittaker.Maui.Controls.Buttons;
using System.Diagnostics;
using System.Windows.Input;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a standardized contract for form field controls used inside a Form.
///
/// Core Responsibilities:
/// - Exposes field-specific state (Access Mode, Validation, Change Tracking).
/// - Supports undo/reset behavior.
/// - Supports dynamic layout updates (label width, control width).
/// - Raises events (`FieldHasChanges`, `FieldHasValidationChanges`) to notify the parent Form.
/// - Allows the parent Form to control field focus/unfocus, clear, undo, and save operations.
///
/// Key Properties:
/// - `FieldAccessMode`: Determines if the field is editable, editing, view-only, or hidden.
/// - `FieldChangeState`: Tracks whether the field has unsaved changes.
/// - `FieldValidationState`: Tracks whether the field's current value is valid.
///
/// Key Methods:
/// - `Clear()`: Clears the field.
/// - `Undo()`: Reverts to original value.
/// - `Save()`: Commits current value and locks the field.
/// - `NotifyHasChanges()` and `NotifyValidationChanges()`: Raise change/validation events.
///
/// Implementation Note:
/// - Typically implemented by a base control such as `BaseFormField<T/>`.
/// </summary>
///

/// <summary>
/// Contract for a strongly-typed form field whose value is of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The CLR type of the data this field edits (e.g. string, bool?, DateTimeOffset?).</typeparam>
public interface IBaseFormFieldTyped<T> : IBaseFormField
{
    /// <summary>
    /// The two-way bound value of this field.
    /// </summary>
    T? FieldDataSource { get; set; }
}

/// <summary>
/// Core, untyped contract for any single form field.
/// </summary>
public interface IBaseFormField
{
    /// <summary>How the user can interact with this field.</summary>
    FieldAccessModeEnum FieldAccessMode { get; set; }

    /// <summary>Tracks whether the field’s current value differs from its original value.</summary>
    ChangeStateEnum FieldChangeState { get; set; }

    /// <summary>An optional command to fire (e.g. pressing Enter in a textbox).</summary>
    ICommand FieldCommand { get; set; }

    /// <summary>Parameter to pass to the command when it executes.</summary>
    object FieldCommandParameter { get; set; }

    /// <summary>The text label shown next to the control.</summary>
    string FieldLabelText { get; set; }

    /// <summary>Show or hide the text label.</summary>
    bool FieldLabelVisible { get; set; }

    /// <summary>Width (in device-independent units) of the text label column.</summary>
    double FieldLabelWidth { get; set; }

    /// <summary>Whether this field is marked as required.</summary>
    bool FieldMandatory { get; set; }

    /// <summary>Whether this field is read-only.</summary>
    bool FieldReadOnly { get; set; }

    /// <summary>Show or hide the built-in Undo button.</summary>
    bool FieldUndoButton { get; set; }

    /// <summary>Overall validity state (Valid, FormatError, RequiredError).</summary>
    ValidationStateEnum FieldValidationState { get; set; }

    /// <summary>Width of the input control column.</summary>
    double FieldWidth { get; set; }

    /// <summary>LayoutOptions for the outer grid container.</summary>
    LayoutOptions HorizontalOptions { get; set; }

    LayoutOptions VerticalOptions { get; set; }

    /// <summary>Fired when HasChanges toggles on/off.</summary>
    event EventHandler<HasChangesEventArgs>? OnHasChanges;

    /// <summary>Fired when Validation state changes.</summary>
    event EventHandler<ValidationDataChangesEventArgs>? OnHasValidationChanges;

    /// <summary>Remove focus from any inner control.</summary>
    void Unfocus();
    void Save();
    void Undo();
    void Clear();

}

/// <summary>
/// Base class that implements <see cref="IBaseFormFieldTyped{T}"/> and wires up:
/// - Two-way bindable <c>FieldDataSource</c>,
/// - Original/Last/Current value tracking for undo and change detection,
/// - Format and required validation,
/// - Built-in Undo button,
/// - Access-mode handling (ViewOnly, Editable, Editing, Hidden),
/// - Notification label for errors,
/// - Batched UI updates to minimize flicker.
/// </summary>
/// <remarks>
/// <para>When you derive your own field (e.g. TextBoxField, DateField, CheckBoxField):
/// 1. **Field_ControlView()**
///    Return your raw input element(s) so the base can wire focus events.<br/>
/// 2. **Field_CreateLayoutGrid()**
///    Arrange: Label | your control | UndoButton in row 0
///    and NotificationLabel spanning all columns in row 1.<br/>
/// 3. **OnParentSet()** (override if your control has a non-default initial UI state)
///    Call `base.OnParentSet()`, then align `FieldOriginalValue = FieldDataSource`
///    so first change-detection runs correctly.<br/>
/// 4. **Field_SetValue(T? value)**
///    Push the VM value into your control (inside `Field_PerformBatchUpdate` + main thread).<br/>
/// 5. **Field_GetCurrentValue()**
///    Read your control’s current value so the base can compare to original and last.<br/>
/// 6. **Field_HasFormatError()** / **Field_HasRequiredError()**
///    Return true when the current UI value is invalid or missing (for mandatory).<br/>
/// 7. **Field_GetFormatErrorMessage()**
///    Provide a user-friendly message for format errors (shown under the control).<br/>
/// 8. **Field_HasChangedFromOriginal()** / **Field_HasChangedFromLast()**
///    Compare `FieldOriginalValue` or `FieldLastValue` against `Field_GetCurrentValue()`.<br/>
///
/// The base already:
/// - Captures the very first real VM assignment as the “original” value,
/// - Suppresses its own callbacks during programmatic updates,
/// - Raises events and enables/disables the Undo button automatically.
/// </para>
/// </remarks>
public abstract class BaseFormField<T> : ContentView, IBaseFormFieldTyped<T>
{
    #region Private Fields
    private double _borderWidth = -1;
    private Color? _textColor = null;
    private Microsoft.Maui.Controls.SolidColorBrush? _backgroundColor = null;
    private Guid _placeholderEntryId;
    private Guid _placeholderEntrySpacerId;
    /// <summary>grid for layout</summary>
    private Grid? _layoutGrid;

    /// <summary>Flag to control internal evaluation of changes.</summary>
    private bool _baseFieldEvaluateToRaiseHasChangesEventing = false;

    /// <summary>Tracks whether the field previously had invalid data.</summary>
    private bool _baseFieldPreviouslyHasInvalidData = false;

    /// <summary>Tracks the previous validation state for this field.</summary>
    private ValidationStateEnum _baseFieldPreviouslyHasValidationState;

    /// <summary>Defines the default button size.</summary>
    protected const SizeEnum DefaultButtonSize = SizeEnum.Normal;

    /// <summary>Re-entrancy guard for programmatic DataSource → UI updates.</summary>
    protected bool FieldSuppressDataSourceCallback = false;

    /// <summary>Flag to control internal evaluation of validation changes.</summary>
    private bool _baseFieldEvaluateToRaiseValidationChangesEventing = false;

    /// <summary>Tracks whether the original value has been set.</summary>
    protected bool FieldIsOriginalValueSet = false;

    /// <summary>Tracks the field's previous change state from original.</summary>
    private bool _baseFieldPreviousHasChangedFromOriginal = false;

    /// <summary>Flag used to skip nested property change events.</summary>
    private bool _baseFieldUpdateValidationAndChangedStating = false;
    #endregion  Private Fields

    #region Protected Fields
    protected Label _placeholderEntry;
    protected UiEntry _placeholderEntrySpacer;

    /// <summary>Reference to the undo button.</summary>
    protected UndoButton? FieldButtonUndo;

    /// <summary>Reference to the label for this field.</summary>
    protected Label? FieldLabel;

    /// <summary>Stores the last known value of this field.</summary>
    protected T? FieldLastValue = default(T);

    /// <summary>Reference to the notification label.</summary>
    protected Label? FieldNotification;

    /// <summary>Stores the original value for this field.</summary>
    protected T? FieldOriginalValue = default(T);

    #endregion Protected Fields

    #region Bindable Properties
    /// <summary>Property for controlling how the field is accessed.</summary>
    public static readonly BindableProperty FieldAccessModeProperty = BindableProperty.Create(
        propertyName: nameof(FieldAccessMode),
        returnType: typeof(FieldAccessModeEnum),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: FieldAccessModeEnum.ViewOnly,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnBaseFieldAccessModePropertyChanged);

    /// <summary>Property for tracking the field's change state.</summary>
    public static readonly BindableProperty FieldChangeStateProperty = BindableProperty.Create(
        propertyName: nameof(FieldChangeState),
        returnType: typeof(ChangeStateEnum),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: ChangeStateEnum.NotChanged,
        BindingMode.TwoWay);

    /// <summary>Property for passing a command parameter.</summary>
    public static readonly BindableProperty FieldCommandParameterProperty = BindableProperty.Create(
        propertyName: nameof(FieldCommandParameter),
        returnType: typeof(object),
        declaringType: typeof(Form));

    /// <summary>Property for storing a command reference.</summary>
    public static readonly BindableProperty FieldCommandProperty = BindableProperty.Create(
        propertyName: nameof(FieldCommand),
        returnType: typeof(ICommand),
        declaringType: typeof(Form));

    /// <summary>
    /// Property for holding the data source of this field and capturing its original value for undo.
    /// </summary>
    public static readonly BindableProperty FieldDataSourceProperty = BindableProperty.Create(
        propertyName: nameof(FieldDataSource),
        returnType: typeof(T?),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: default(T?),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnBaseFieldDataSourcePropertyChangedStatic);

    /// <summary>Property for the field's label text.</summary>
    public static readonly BindableProperty FieldLabelTextProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelText),
        returnType: typeof(string),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: string.Empty,
        propertyChanged: OnBaseFieldLabelTextPropertyChanged);

    /// <summary>Property for showing or hiding the field's label.</summary>
    public static readonly BindableProperty FieldLabelVisibleProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelVisible),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: true,
        propertyChanged: OnBaseFieldLabelVisiblePropertyChanged);

    /// <summary>Property for the width of the field's label.</summary>
    public static readonly BindableProperty FieldLabelWidthProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: 100d,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnBaseFieldLabelWidthPropertyChanged);

    /// <summary>Property indicating if the field is mandatory.</summary>
    public static readonly BindableProperty FieldMandatoryProperty = BindableProperty.Create(
        propertyName: nameof(FieldMandatory),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: false);

    /// <summary>Property for making the field read-only.</summary>
    public static readonly BindableProperty FieldReadOnlyProperty = BindableProperty.Create(
        propertyName: nameof(FieldReadOnly),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWay);

    /// <summary>Property for showing or hiding the undo button.</summary>
    public static readonly BindableProperty FieldUndoButtonProperty = BindableProperty.Create(
        propertyName: nameof(FieldUndoButton),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: true,
        propertyChanged: OnBaseFieldUndoButtonVisiblePropertyChanged);

    /// <summary>Property for storing the field's validation state.</summary>
    public static readonly BindableProperty FieldValidationStateProperty = BindableProperty.Create(
        propertyName: nameof(FieldValidationState),
        returnType: typeof(ValidationStateEnum),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: ValidationStateEnum.Valid,
        defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Property for controlling the overall width of the field.</summary>
    public static readonly BindableProperty FieldWidthProperty = BindableProperty.Create(
        propertyName: nameof(FieldWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: 100d,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnBaseFieldWidthPropertyChanged);

    public static readonly BindableProperty FieldLabelLayoutProperty =
        BindableProperty.Create(
            nameof(FieldLabelLayout),
            typeof(FieldLabelLayoutEnum),
            typeof(BaseFormField<T>),
            FieldLabelLayoutEnum.Left,
            propertyChanged: (b, o, n) => ((BaseFormField<T>)b).ApplyFieldLabelLayout()
        );
    #endregion

    #region Public Constructors & Initializer

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseFormField{T}"/> class.
    /// </summary>
    public BaseFormField()
    {


        FieldLabel = Field_CreateLabel(fieldLabelVisible: FieldLabelVisible);
        FieldNotification = Field_CreateNotificationLabel();
        FieldButtonUndo = Field_CreateUndoButton(fieldHasUndo: FieldUndoButton, fieldAccessMode: FieldAccessMode);

        // Subscribe to the undo button's Pressed event
        FieldButtonUndo.Pressed += OnBaseFieldButtonUndoPressed;

        // Explicitly configure the undo button's initial state
        if (FieldAccessMode == FieldAccessModeEnum.Editing && FieldUndoButton)
        {
            if (Field_HasChangedFromOriginal())
                FieldButtonUndo.Enabled();
            else
                FieldButtonUndo.Disabled();
        }
        else
        {
            FieldButtonUndo.Hide();
        }
        // build our one-and-only layout grid now  
        _layoutGrid = null;// Field_CreateLayoutGrid();
                           //ApplyFieldLabelLayout();

        _placeholderEntryId = Guid.NewGuid();
        _placeholderEntrySpacerId = Guid.NewGuid();

        _placeholderEntrySpacer = new UiEntry
        {
            AutomationId = _placeholderEntrySpacerId.ToString(),
            VerticalOptions = new LayoutOptions(LayoutAlignment.Fill, true),
            HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true),
            BackgroundColor = Colors.Transparent,
            BorderColor = Colors.Transparent,
            InputTransparent = true,
            IsReadOnly = true,
            IsEnabled = false,
            IsVisible = true,
            Text = String.Empty,
            ZIndex = 0
        };

        // 2) Placeholder entry on top
        _placeholderEntry = new Label
        {
            AutomationId = _placeholderEntryId.ToString(),
            VerticalOptions = new LayoutOptions(LayoutAlignment.Fill, true),
            HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true),
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Center,
            IsVisible = FieldAccessMode == FieldAccessModeEnum.Editable,
            Text = String.Empty,
            IsEnabled = true,
            BackgroundColor = Colors.Transparent,
            Margin = new Thickness(8, 0, 0, 0)
        };
    }

    protected void Initialize()
    {
        BaseField_WireFocusEvents(Field_GetControlsFromGrid());
        BaseField_InitializeDataSource();

        UiThreadHelper.RunOnMainThread(() =>
        {
            _placeholderEntrySpacer.MoveToBack();
        });

    }

    #endregion Public Constructors

    #region Events

    /// <inheritdoc/>
    public event EventHandler<HasChangesEventArgs>? OnHasChanges;

    /// <inheritdoc/>
    public event EventHandler<ValidationDataChangesEventArgs>? OnHasValidationChanges;

    #endregion Events

    #region Properties

    /// <inheritdoc/>
    public FieldAccessModeEnum FieldAccessMode
    {
        get => (FieldAccessModeEnum)GetValue(FieldAccessModeProperty);
        set => SetValue(FieldAccessModeProperty, value);
    }

    /// <inheritdoc/>
    public ChangeStateEnum FieldChangeState
    {
        get => (ChangeStateEnum)GetValue(FieldChangeStateProperty);
        set => SetValue(FieldChangeStateProperty, value);
    }

    /// <inheritdoc/>
    public ICommand FieldCommand
    {
        get => (ICommand)GetValue(FieldCommandProperty);
        set => SetValue(FieldCommandProperty, value);
    }

    /// <inheritdoc/>
    public object FieldCommandParameter
    {
        get => GetValue(FieldCommandParameterProperty);
        set => SetValue(FieldCommandParameterProperty, value);
    }

    public FieldLabelLayoutEnum FieldLabelLayout
    {
        get => (FieldLabelLayoutEnum)GetValue(FieldLabelLayoutProperty);
        set => SetValue(FieldLabelLayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the typed data source for this field.
    /// </summary>
    public T? FieldDataSource
    {
        get => (T?)GetValue(FieldDataSourceProperty);
        set
        {
            SetValue(FieldDataSourceProperty, value);
        }
    }

    /// <inheritdoc/>
    public string FieldLabelText
    {
        get => (string)GetValue(FieldLabelTextProperty);
        set => SetValue(FieldLabelTextProperty, value);
    }

    /// <inheritdoc/>
    public bool FieldLabelVisible
    {
        get => (bool)GetValue(FieldLabelVisibleProperty);
        set => SetValue(FieldLabelVisibleProperty, value);
    }

    /// <inheritdoc/>
    public double FieldLabelWidth
    {
        get => (double)GetValue(FieldLabelWidthProperty);
        set => SetValue(FieldLabelWidthProperty, value);
    }

    /// <inheritdoc/>
    public bool FieldMandatory
    {
        get => (bool)GetValue(FieldMandatoryProperty);
        set => SetValue(FieldMandatoryProperty, value);
    }

    /// <inheritdoc/>
    public bool FieldReadOnly
    {
        get => (bool)GetValue(FieldReadOnlyProperty);
        set => SetValue(FieldReadOnlyProperty, value);
    }

    /// <inheritdoc/>
    public bool FieldUndoButton
    {
        get => (bool)GetValue(FieldUndoButtonProperty);
        set => SetValue(FieldUndoButtonProperty, value);
    }

    /// <inheritdoc/>
    public ValidationStateEnum FieldValidationState
    {
        get => (ValidationStateEnum)GetValue(FieldValidationStateProperty);
        set => SetValue(FieldValidationStateProperty, value);
    }

    /// <inheritdoc/>
    public double FieldWidth
    {
        get => (double)GetValue(FieldWidthProperty);
        set => SetValue(FieldWidthProperty, value);
    }

    /// <inheritdoc/>
    public new LayoutOptions HorizontalOptions
    {
        get => base.HorizontalOptions;
        set
        {
            if (Children.FirstOrDefault() is Grid grid)
                grid.HorizontalOptions = value;
            base.HorizontalOptions = value;
        }
    }

    /// <inheritdoc/>
    public new LayoutOptions VerticalOptions
    {
        get => base.VerticalOptions;
        set
        {
            if (Content is Grid grid)
                grid.VerticalOptions = value;
            base.VerticalOptions = value;
        }
    }

    #endregion Properties

    #region Private Methods

    /// <summary>
    /// Build a two-row Grid (default layout for label left mode)
    /// Row 0: Label | control(s) | UndoButton
    /// Row 1: NotificationLabel spanning all columns
    /// and return it.
    /// </summary>
    /// <summary>
    /// Shared grid‐template for *all* fields:
    ///   • Col 0: fixed label width
    ///   • Col 1: star (*) for your controls
    ///   • Col 2: fixed undo-button width
    ///   • Row 0: auto for label/control/undo
    ///   • Row 1: auto for notification
    /// </summary>
    private Grid BaseField_CreateLayoutGrid()
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
            new RowDefinition { Height = 0 },              // row1 – label‑top (collapsed by default)
            new RowDefinition { Height = GridLength.Auto },// row2 – main controls
            new RowDefinition { Height = 0 }               // row3 – notification (start collapsed)
        },
            VerticalOptions = LayoutOptions.Fill
        };

        // label + controls + undo
        grid.Add(FieldLabel, col1, row2);
        foreach (var ctl in Field_GetControls())
            grid.Add(ctl, col2, row2);
        grid.Add(_placeholderEntry, col2, row2);
        grid.Add(_placeholderEntrySpacer, col2, row2);
        grid.Add(FieldButtonUndo, col3, row2);

        // notification now lives in its own row (row3)
        grid.Add(FieldNotification, col1, row3);
        Grid.SetColumnSpan(FieldNotification, 3);

        return grid;
    }

    private const int col1 = 0, col2 = 1, col3 = 2;
    private const int row1 = 0, row2 = 1, row3 = 2;

    private void ApplyFieldLabelLayout()
    {
        if (_layoutGrid == null) return;

        // Label left layout
        if (FieldLabelLayout == FieldLabelLayoutEnum.Left)
        {
            // row1 is hidden, col1 width is FieldLabelWidth
            _layoutGrid.RowDefinitions[row1].Height = 0;
            _layoutGrid.ColumnDefinitions[col1].Width = FieldLabelWidth;

            // label in position 1st row and column spanning 1 column
            Grid.SetRow(FieldLabel, row2);
            Grid.SetColumn(FieldLabel, col1);
            Grid.SetColumnSpan(FieldLabel, 1);

        }
        else  // Label top layout
        {
            // row1 has auto height and col1 is hidden
            _layoutGrid.RowDefinitions[row1].Height = GridLength.Auto;
            _layoutGrid.ColumnDefinitions[col1].Width = 0;

            // label in 1st row and column spanning all columns
            Grid.SetRow(FieldLabel, row1);
            Grid.SetColumn(FieldLabel, col1);
            Grid.SetColumnSpan(FieldLabel, 3);

        }
    }

    /// <summary>
    /// Invoked when the field enabled property changes.
    /// </summary>
    private static void OnBaseFieldEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool newEnabledState)
        {
            if (!oldValue.Equals(newValue))
            {
                if (newEnabledState)
                    element.Field_ConfigEnabled();
                else
                    element.Field_ConfigDisabled();
            }
        }
    }

    /// <summary>
    /// Invoked when the field label text property changes.
    /// </summary>
    private static void OnBaseFieldLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.FieldLabel != null)
        {
            element.FieldLabel.Text = newValue?.ToString() ?? "";
        }
    }

    /// <summary>
    /// Invoked when the field label visibility changes.
    /// </summary>
    private static void OnBaseFieldLabelVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool fieldLabelVisible)
        {
            if (!oldValue.Equals(newValue))
            {
                element.FieldLabel!.IsVisible = fieldLabelVisible;
            }
        }
    }

    /// <summary>
    /// Invoked when the field label width changes.
    /// </summary>
    private static void OnBaseFieldLabelWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.Content is Grid)
            element.Field_UpdateLabelWidth((double)newValue);
    }

    /// <summary>
    /// Invoked when the undo button visibility changes.
    /// </summary>
    private static void OnBaseFieldUndoButtonVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool fieldUndoButtonVisible)
        {
            if (!oldValue.Equals(newValue))
            {
                element.FieldButtonUndo!.IsVisible = fieldUndoButtonVisible;
            }
        }
    }

    /// <summary>
    /// Invoked when the field access mode property changes.
    /// </summary>
    private static void OnBaseFieldAccessModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Property Changed: FieldAccessModeEnum
        if (bindable is BaseFormField<T> elementAccessModeChanged && newValue is FieldAccessModeEnum newAccessMode)
        {
            #region 
            elementAccessModeChanged.Field_UpdateNotificationMessage();

            // Respect FieldReadOnly: force ViewOnly if read-only
            if (elementAccessModeChanged.FieldReadOnly && newAccessMode != FieldAccessModeEnum.Hidden)
            {
                elementAccessModeChanged.Field_ConfigAccessModeViewing();
                return;
            }

            if (!oldValue.Equals(newValue))
            {
                switch (newAccessMode)
                {
                    case FieldAccessModeEnum.ViewOnly:
                        elementAccessModeChanged.Field_ConfigAccessModeViewing();
                        return;

                    case FieldAccessModeEnum.Editing:
                        elementAccessModeChanged.Field_ConfigAccessModeEditing();
                        return;

                    case FieldAccessModeEnum.Editable:
                        elementAccessModeChanged.Field_ConfigAccessModeViewing();
                        return;

                    case FieldAccessModeEnum.Hidden:
                        elementAccessModeChanged.BaseConfigAccessModeHidden();
                        return;
                }
            }
            #endregion 
        }
    }

    /// <summary>
    /// Invoked when the field width changes.
    /// </summary>
    private static void OnBaseFieldWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.Content is Grid)
            element.Field_UpdateWidth((double)newValue);
    }

    /// <summary>
    /// Evaluates whether a change event should be raised.
    /// </summary>
    private void BaseFieldEvaluateToRaiseHasChangesEvent()
    {

        if (_baseFieldEvaluateToRaiseHasChangesEventing)
            return;

        _baseFieldEvaluateToRaiseHasChangesEventing = true;

        bool hasChangedFromOriginal = Field_HasChangedFromOriginal();
        bool hasChangedFromLast = Field_HasChangedFromLast();

        if (_baseFieldPreviousHasChangedFromOriginal != hasChangedFromOriginal)
        {
            if (FieldButtonUndo != null && FieldUndoButton)
            {
                if (FieldAccessMode == FieldAccessModeEnum.Editing)
                {
                    if (hasChangedFromOriginal)
                        FieldButtonUndo.Enabled();
                    else
                        FieldButtonUndo.Disabled();
                }
                else
                {
                    FieldButtonUndo.Hide();
                }
            }
            _baseFieldPreviousHasChangedFromOriginal = hasChangedFromOriginal;
            FieldChangeState = hasChangedFromOriginal ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
            Field_NotifyHasChanges(hasChangedFromOriginal);
        }
        _baseFieldEvaluateToRaiseHasChangesEventing = false;
    }

    /// <summary>
    /// Evaluates whether a validation change event should be raised.
    /// </summary>
    private void BaseFieldEvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
    {
        if (_baseFieldEvaluateToRaiseValidationChangesEventing)
            return;

        _baseFieldEvaluateToRaiseValidationChangesEventing = true;
        bool currentFieldHasInvalidData = BaseField_HasValidData() == false;
        var currentFieldValidationState = currentFieldHasInvalidData ? ValidationStateEnum.FormatError : ValidationStateEnum.Valid;
        if (_baseFieldPreviouslyHasInvalidData != currentFieldHasInvalidData || forceRaise || _baseFieldPreviouslyHasValidationState != currentFieldValidationState)
        {
            _baseFieldPreviouslyHasInvalidData = currentFieldHasInvalidData;
            _baseFieldPreviouslyHasValidationState = currentFieldValidationState;
            FieldValidationState = currentFieldValidationState;
            Field_NotifyValidationChanges(!currentFieldHasInvalidData);
        }
        _baseFieldEvaluateToRaiseValidationChangesEventing = false;
    }

    /// <summary>
    /// Handles the logic when the Undo button is pressed.
    /// </summary>
    private void OnBaseFieldButtonUndoPressed(object? sender, EventArgs e)
    {
        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            try
            {
                Undo();
            }
            finally
            {
            }
        }
    }


    /// <summary>
    /// Hides the field by applying a read-only state and hiding the undo button.
    /// </summary>
    private void BaseConfigAccessModeHidden()
    {
        this.HideAllDescendantControls();
        if (FieldButtonUndo != null) FieldButtonUndo.Hide();
    }


    #endregion Private Methods

    #region Protected Methods

    protected void SaveBorderWidth(double width)
    {
        if (_borderWidth <= 0 && width > 0)
            _borderWidth = width;
    }

    protected double GetBorderWidth()
    {
        return (_borderWidth > 0 ? _borderWidth : 0);
    }

    protected Color? GetTextColor()
    {
        return _textColor;
    }

    protected void SaveTextColor(Color? color)
    {
        if (color != null && _textColor == null)
            _textColor = color;
    }

    protected Microsoft.Maui.Controls.SolidColorBrush? GetBackgroundColor()
    {
        return _backgroundColor;
    }

    protected void SaveBackgroundColor(Microsoft.Maui.Controls.SolidColorBrush? color)
    {
        if (color != null && _backgroundColor == null && color != Brush.Transparent)
            _backgroundColor = color;
    }
    protected bool FieldAreValuesEqual(T? original, T? current)
    {
        if (typeof(T) == typeof(string))
        {
            var s1 = original as string ?? string.Empty;
            var s2 = current as string ?? string.Empty;

            return string.Equals(s1.Trim(), s2.Trim(), StringComparison.Ordinal);
        }

        return Equals(original, current);
    }

    /// <summary>
    /// Disables user interaction on this field.
    /// </summary>
    protected void Field_ConfigDisabled()
    {
        FieldAccessMode = FieldAccessModeEnum.ViewOnly;
        //this.ViewingModeDescendantControls(FieldLabelVisible);
    }

    /// <summary>
    /// Enables user interaction on this field.
    /// </summary>
    protected void Field_ConfigEnabled()
    {
        FieldAccessMode = FieldAccessModeEnum.ViewOnly;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        // no more resetting FieldIsOriginalValueSet here
    }

    /// <summary>
    /// Creates the primary label for the field, respecting visibility.
    /// </summary>
    protected Label Field_CreateLabel(bool fieldLabelVisible)
    {
        var label = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            IsVisible = fieldLabelVisible
        };
        return label;
    }

    /// <summary>
    /// Creates a label used for notification messages such as errors or required fields.
    /// </summary>
    protected Label Field_CreateNotificationLabel()
    {
        var label = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            TextColor = Colors.Red,
            IsVisible = false
        };
        return label;
    }

    /// <summary>
    /// Creates the undo button for reverting field changes.
    /// </summary>
    protected UndoButton Field_CreateUndoButton(bool fieldHasUndo, FieldAccessModeEnum fieldAccessMode)
    {
        var btn = new UndoButton
        {
            Text = "",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
            ButtonSize = DefaultButtonSize,
            WidthRequest = -1,
            ButtonState = ButtonStateEnum.Disabled,
            ButtonIcon = ButtonIconEnum.Undo,
            BorderWidth = 0,
            Margin = new Thickness(0),
            Padding = new Thickness(5, 0, 0, 0)
        };
        btn.IsVisible = fieldHasUndo;
        return btn;
    }

    protected (int row, int col) Field_GetControlLocation()
    {
        if (FieldLabelLayout == FieldLabelLayoutEnum.Left)
            return (row1, col2);
        else
            return (row2, col2);
    }

    protected List<View> Field_GetControlsFromGrid()
    {
        var siblings = _layoutGrid?.Children
                   .Where(
                            v => _layoutGrid?.GetRow(v) == Field_GetControlLocation().row &&
                                 _layoutGrid?.GetColumn(v) == Field_GetControlLocation().col &&
                                 v.AutomationId != _placeholderEntryId.ToString() &&
                                 v.AutomationId != _placeholderEntrySpacerId.ToString())
                   .Cast<View>()
                   .ToList() ?? new();
        return siblings;
    }

    /// <summary>
    /// Clears the original value of the field.
    /// </summary>
    protected virtual void Field_OriginalValue_SetToClear()
    {
        FieldOriginalValue = default(T);
        Field_SetValue(FieldOriginalValue);
    }


    protected void Field_PerformBatchUpdate(Action updateAction)
    {
        this.BatchBegin();
        try
        {
            updateAction();
        }
        finally
        {
            this.BatchCommit();
        }
    }

    protected virtual void Field_SetDataSourceValue(T? newValue)
    {

        FieldSuppressDataSourceCallback = true;
        FieldDataSource = newValue;

        Field_UpdateValidationAndChangedState();
        Field_UpdateNotificationMessage();
    }

    /// <summary>
    /// Updates the field's notification label based on its validation state.
    /// </summary>
    protected void Field_UpdateNotificationMessage()
    {
        bool show = false;
        string msg = string.Empty;

        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            if (Field_HasRequiredError())
            {
                show = true;
                msg = "Required";
            }
            if (Field_HasFormatError())
            {
                show = true;
                msg = Field_GetFormatErrorMessage();
            }
        }

        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                if (FieldNotification != null)
                {
                    FieldNotification.Text = msg;
                    FieldNotification.IsVisible = show;
                }

                // Collapse / expand the dedicated notification row
                if (_layoutGrid != null)
                    _layoutGrid.RowDefinitions[row3].Height = show ? GridLength.Auto : 0;
            });

            Field_RefreshLayout();   // force re‑measure so overall height updates
        });
    }

    /// <summary>
    /// Updates both validation and changed states, optionally forcing a validation check.
    /// </summary>
    protected void Field_UpdateValidationAndChangedState(bool forceRaiseValidationChangesEvent = false)
    {
        if (_baseFieldUpdateValidationAndChangedStating)
            return;
        _baseFieldUpdateValidationAndChangedStating = true;

        BaseFieldEvaluateToRaiseHasChangesEvent();
        BaseFieldEvaluateToRaiseValidationChangesEvent(forceRaiseValidationChangesEvent);

        _baseFieldUpdateValidationAndChangedStating = false;
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        // only build once, after derived ctor has created its controls
        if (_layoutGrid == null)
        {
            Content = _layoutGrid = BaseField_CreateLayoutGrid();
        }
        ApplyFieldLabelLayout();

        // keep the UI in sync if a design-time value is already present
        Field_SetValue(FieldDataSource);

        OnBaseFieldAccessModePropertyChanged(
            bindable: this,
            oldValue: FieldAccessMode,  // or something else if you have an "old" value
            newValue: FieldAccessMode
        );
    }

    /// <inheritdoc/>
    protected void Field_Unfocused(object? sender, FocusEventArgs e)
    { }


    /// <inheritdoc/>
    protected void Field_Focused(object? sender, FocusEventArgs e)
    { }

    /// <inheritdoc/>
    protected void Field_NotifyHasChanges(bool hasChanged) =>
        OnHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));

    /// <inheritdoc/>
    protected void Field_NotifyValidationChanges(bool isValid) =>
        OnHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));

    protected void Field_ShowPlaceholders()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            _placeholderEntry.IsVisible = true;
            _placeholderEntry.Text = Field_GetDisplayText();
            _placeholderEntry.BringToFront();
            _placeholderEntrySpacer.MoveToBack();
        });
    }

    protected void Field_HidePlaceholders()
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
        _placeholderEntry.IsVisible = false;
        _placeholderEntry.MoveToBack();
        _placeholderEntrySpacer.MoveToBack();
        });
    }


    /// <inheritdoc/>
    protected void Field_UpdateLabelWidth(double newWidth)
    {
        if (Content is Grid grid && grid.ColumnDefinitions.Count > 0)
        {
            UiThreadHelper.RunOnMainThread(() =>
            {
                Field_PerformBatchUpdate(() =>
                {
                    grid.ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
                    if (FieldLabel != null)
                        FieldLabel.WidthRequest = newWidth;
                });
            });
        }
    }

    /// <inheritdoc/>
    protected void Field_UpdateWidth(double newWidth)
    {
        if (Content is Grid grid && grid.ColumnDefinitions.Count > 1)
            UiThreadHelper.RunOnMainThread(() =>
            {
                Field_PerformBatchUpdate(() =>
                {
                    grid.ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
                });
            });
    }


    #endregion Protected methods

    #region Abstract members you **must** implement in your subclass

    /// <summary>
    /// Return every interactive child view that represents the “field body”.
    /// The base class wires Focused / Unfocused, change‑tracking, and validation
    /// to these controls, so exclude the label, undo button, placeholder overlay,
    /// and notification label that BaseFormField creates.
    /// </summary>
    protected abstract List<View> Field_GetControls();

    /// <summary>
    /// Switch the field into editing mode.<br/>
    /// • Enable or reveal your input controls and hide any read‑only overlay.<br/>
    /// • Call <c>Field_RefreshLayout()</c> (or <c>RefreshLayout()</c>) so column spans
    ///   adjust after you show/hide elements.<br/>
    /// • Call <c>Field_HidePlaceholders()</c> when real controls are visible.
    /// </summary>
    protected abstract void Field_ConfigAccessModeEditing();

    /// <summary>
    /// Switch the field into viewing mode.<br/>
    /// • Remove focus from inner controls and disable or hide them.<br/>
    /// • Call <c>Field_ShowPlaceholders()</c> so read‑only text appears.<br/>
    /// • Ensure the undo button is hidden when <c>FieldAccessMode</c> ≠ Editing.
    /// </summary>
    protected abstract void Field_ConfigAccessModeViewing();

    /// <summary>
    /// Push a new VM value into the UI.<br/>
    /// • Always wrap UI writes in
    ///   <c>UiThreadHelper.RunOnMainThread(() => Field_PerformBatchUpdate(...))</c>.<br/>
    /// • Detach value‑changed handlers before you assign, then re‑attach afterward
    ///   to avoid recursion.<br/>
    /// • Show or hide a blank placeholder row when the value represents
    ///   “no selection”.  Do NOT modify <c>FieldDataSource</c> here.
    /// </summary>
    protected abstract void Field_SetValue(T? value);

    /// <summary>
    /// Read the current value from the UI so the base class can compare it to
    /// <c>FieldOriginalValue</c> and <c>FieldLastValue</c>.
    /// Never perform additional side‑effects here.
    /// </summary>
    protected abstract T? Field_GetCurrentValue();

    /// <summary>
    /// Return <c>true</c> when the user has supplied no value and
    /// <c>FieldMandatory</c> is <c>true</c>.  Use the same “blank” rules that
    /// <c>Field_SetValue</c> applies when showing placeholders.
    /// </summary>
    protected abstract bool Field_HasRequiredError();

    /// <summary>
    /// Return <c>true</c> when the raw UI text/value is syntactically invalid
    /// (e.g., bad date, number out of range).  Do NOT combine required checks here.
    /// </summary>
    protected abstract bool Field_HasFormatError();

    /// <summary>
    /// Human‑readable message that will be displayed in the notification label
    /// when <c>Field_HasFormatError()</c> is <c>true</c>.
    /// </summary>
    protected abstract string Field_GetFormatErrorMessage();

    /// <summary>
    /// Compare <c>FieldLastValue</c> with <c>Field_GetCurrentValue()</c>.
    /// Use <c>FieldAreValuesEqual</c> for consistent string trimming &amp; null logic.
    /// </summary>
    protected abstract bool Field_HasChangedFromLast();

    /// <summary>
    /// Compare <c>FieldOriginalValue</c> with <c>Field_GetCurrentValue()</c>.
    /// Use <c>FieldAreValuesEqual</c> so the base class gets reliable change events.
    /// </summary>
    protected abstract bool Field_HasChangedFromOriginal();

    /// <summary>
    /// Re‑apply layout after items or visibility change (e.g., after repopulating
    /// a Picker).  Usually refreshes <c>ItemsSource</c>, reapplies selection,
    /// then calls <c>Field_UpdateValidationAndChangedState()</c>.
    /// </summary>
    protected abstract void Field_RefreshLayout();

    /// <summary>
    /// Return the user‑visible text that should appear in the read‑only placeholder
    /// when the field is in viewing mode.  Must mirror exactly what the live
    /// control would show for the same selection.
    /// </summary>
    protected abstract string Field_GetDisplayText();




    #endregion Abstract members you **must** implement in your subclass

    #region Private Methods

    /// <summary>
    /// Determines if the field data is valid by checking format and required errors.
    /// </summary>
    private bool BaseField_HasValidData()
    {
        // Evaluate all error conditions and return false if any found.
        bool fail = Field_HasFormatError() || Field_HasRequiredError();
        return !fail;
    }

    private void BaseField_InitializeDataSource()
    {
        FieldOriginalValue = FieldDataSource;
        Field_SetValue(FieldDataSource);
    }

    private void BaseField_OriginalValue_Reset()
    {
        FieldDataSource = FieldOriginalValue;
        Field_SetValue(FieldOriginalValue);
        Field_SetDataSourceValue(FieldOriginalValue);
    }


    /// <summary>
    /// Updates the original value of the field to match its current state.
    /// </summary>
    private void BaseField_OriginalValue_SetToCurrentValue()
    {
        FieldOriginalValue = Field_GetCurrentValue();
    }


    private void BaseField_WireFocusEvents(List<View> inputList)
    {
        foreach (var control in inputList)
        {
            control.Focused += Field_Focused;
            control.Unfocused += Field_Unfocused;
        }
    }

    /// <summary>
    /// Static callback invoked on every FieldDataSource change.
    /// Captures the very first value via <see cref="OnBaseFieldDataSourcePropertyChangedStatic"/>
    /// then forwards to <see cref="OnBaseFieldDataSourcePropertyChangedStatic"/> (overrideable).
    /// </summary>
    /// <summary>
    /// Static callback invoked on every FieldDataSource change.
    /// Captures the very first “real” value (i.e. after the VM has been set)
    /// as the original, and thereafter only updates the UI or raises change
    /// events.
    /// </summary>
    private static void OnBaseFieldDataSourcePropertyChangedStatic(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (BaseFormField<T>)bindable;

        // 1) ignore any updates that fire before the VM is bound
        if (control.BindingContext == null)
            return;

        if (Equals(oldValue, newValue))
            return;

        // 2) first “real” assignment: capture original + sync UI
        if (!control.FieldIsOriginalValueSet)
        {
            control.FieldIsOriginalValueSet = true;
            // record this as the original value
            control.OnBaseFieldInitializeFieldDataSourceSet(initialValue: (T?)newValue);
            // now sync the UI
            control.OnBaseFieldDataSourceChanged(oldValue: (T?)oldValue, newValue: (T?)newValue);
            return;
        }

        // 3) skip if this is our own programmatic SetDataSourceValue call
        if (control.FieldSuppressDataSourceCallback)
        {
            control.FieldSuppressDataSourceCallback = false;
            return;
        }

        // 4) normal subsequent change: update UI and raise change events
        control.OnBaseFieldDataSourceChanged(oldValue: (T?)oldValue, newValue: (T?)newValue);
    }

    /// <summary>
    /// Called once, on the very first assignment of FieldDataSource.
    /// Default implementation sets <see cref="FieldOriginalValue"/>; override for custom setup but call base.
    /// </summary>
    private void OnBaseFieldInitializeFieldDataSourceSet(T? initialValue)
    {
        FieldOriginalValue = initialValue;
    }

    /// <summary>
    /// Called on every FieldDataSource change (after initial).
    /// Default implementation synchronizes UI; override to inject custom logic but call base.Sync if needed.
    /// </summary>
    private void OnBaseFieldDataSourceChanged(T? newValue, T? oldValue)
    {
        Field_SetValue(newValue);
        FieldLastValue = newValue;
    }

    #endregion Private Methods

    #region Public Methods
    /// <inheritdoc/>
    public void Clear()
    {
        Field_OriginalValue_SetToClear();
        Field_UpdateValidationAndChangedState();
        Unfocus();
    }

    /// <inheritdoc/>
    public void Save()
    {
        BaseField_OriginalValue_SetToCurrentValue();
        Field_UpdateValidationAndChangedState();
        FieldAccessMode = FieldAccessModeEnum.ViewOnly;
    }

    public void Undo()
    {
        // By default, restore FieldOriginalValue
        BaseField_OriginalValue_Reset();
        Field_UpdateValidationAndChangedState(true);
        Field_UpdateNotificationMessage();
        FieldButtonUndo!.ButtonState = ButtonStateEnum.Disabled;
        BaseFieldEvaluateToRaiseHasChangesEvent();
    }


    #endregion Public Methods
}

public static class ViewGridExtensions
{

    private static bool IsInGrid(this View? control)
    {
        if (control == null)
            return false;
        Element? parent = control.Parent;
        while (parent != null)
        {
            if (parent is Grid)
                return true;
            parent = (parent as VisualElement)?.Parent;
        }
        return false;
    }

    private static (int row, int col, int colSpan) GetPositionInGrid(this View? control)
    {
        if (control == null)
            return (-1, -1, 1);
        Element? parent = control.Parent;
        while (parent != null)
        {
            if (parent is Grid grid)
            {
                int row = grid.GetRow(control);
                int col = grid.GetColumn(control);
                int span = grid.GetColumnSpan(control);
                return (row, col, span);
            }
            parent = (parent as VisualElement)?.Parent;
        }
        return (-1, -1, 1);
    }

    public static void BringToFront(this View? control)
    {
        if (control == null || !control.IsInGrid())
            return;
        var (row, col, span) = control.GetPositionInGrid();
        if (row < 0)
            return;

        var grid = control.GetParentGrid();
        if (grid == null)
            return;

        var siblings = grid.Children
                           .Where(v => grid.GetRow(v) == row && grid.GetColumn(v) == col)
                           .ToList();

        if (!siblings.Any())
            return;

        int minIndex = siblings.Min(v => v.ZIndex);
        for (int i = 0; i < siblings.Count; i++)
            ((View)siblings[i]).ZIndex -= minIndex;

        var ordered = siblings.OrderBy(v => v.ZIndex).ToList();
        for (int i = 0; i < ordered.Count; i++)
            ((View)siblings[i]).ZIndex = i;

        control.ZIndex = ordered.Count;
    }

    public static void MoveToBack(this View? control)
    {
        if (control == null || !control.IsInGrid())
            return;
        var (row, col, span) = control.GetPositionInGrid();
        if (row < 0)
            return;

        var grid = control.GetParentGrid();
        if (grid == null)
            return;

        var siblings = grid.Children
                           .Where(v => grid.GetRow(v) == row && grid.GetColumn(v) == col)
                           .ToList();
        if (!siblings.Any())
            return;

        int minIndex = siblings.Min(v => v.ZIndex);
        for (int i = 0; i < siblings.Count; i++)
        {
            ((View)siblings[i]).ZIndex -= minIndex;

        }

        var ordered = siblings.OrderBy(v => v.ZIndex).ToList();
        int idx = 1;
        foreach (var v in ordered)
        {
            if (v != control)
                ((View)v).ZIndex = idx++;
        }

        control.ZIndex = 0;
    }

    private static Grid? GetParentGrid(this View? control)
    {
        if (control == null)
            return null;
        Element? parent = control.Parent;
        while (parent != null)
        {
            if (parent is Grid grid)
                return grid;
            parent = (parent as VisualElement)?.Parent;
        }
        return null;
    }
}



///// <summary>
///// After toggling LabelVisible or UndoButton, recalc your
///// Grid.ColumnSpan/Column so your control remains centered.
///// </summary>
//protected void BaseRefreshLayout()
//{
//    UiThreadHelper.RunOnMainThread(() =>
//    {
//        Field_PerformBatchUpdate(() =>
//        {
//            foreach (var control in Field_GetControls())
//                if (control!.Parent is Grid grid)
//                {
//                    bool isFieldLabelVisible = FieldLabelVisible;
//                    bool isButtonUndoVisible = FieldUndoButton;

//                    if (isFieldLabelVisible && isButtonUndoVisible)
//                    {
//                        Grid.SetColumn(control!, 1);
//                        Grid.SetColumnSpan(control!, 1);
//                    }
//                    else if (isFieldLabelVisible && !isButtonUndoVisible)
//                    {
//                        Grid.SetColumn(control!, 1);
//                        Grid.SetColumnSpan(control!, 2);
//                    }
//                    else if (!isFieldLabelVisible && isButtonUndoVisible)
//                    {
//                        Grid.SetColumn(control!, 0);
//                        Grid.SetColumnSpan(control!, 2);
//                    }
//                    else // both not visible
//                    {
//                        Grid.SetColumn(control!, 0);
//                        Grid.SetColumnSpan(control!, 3);
//                    }
//                }
//        });
//    });

//    Field_RefreshLayout();

//}