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
/// - `Field_Clear()`: Clears the field.
/// - `Field_UndoValue()`: Reverts to original value.
/// - `Field_SaveAndMarkAsReadOnly()`: Commits current value and locks the field.
/// - `Field_NotifyHasChanges()` and `Field_NotifyValidationChanges()`: Raise change/validation events.
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

    /// <summary>Whether the field is enabled or disabled.</summary>
    bool FieldEnabled { get; set; }

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
    event EventHandler<HasChangesEventArgs>? FieldHasChanges;

    /// <summary>Fired when Validation state changes.</summary>
    event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

    /// <summary>Clear to default/empty.</summary>
    void Field_Clear();

    /// <summary>Save current value as new “original” and lock.</summary>
    void Field_SaveAndMarkAsReadOnly();

    /// <summary>Revert back to the original value.</summary>
    void Field_UndoValue();

    /// <summary>Remove focus from any inner control.</summary>
    void Field_Unfocus();

    /// <summary>Adjust the label column width at runtime.</summary>
    void Field_UpdateLabelWidth(double newWidth);

    /// <summary>Adjust the control column width at runtime.</summary>
    void Field_UpdateWidth(double newWidth);
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
    #region Fields

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

    /// <summary>Flag to prevent redundant disabling calls.</summary>
    private bool _baseFieldDisabling = false;

    /// <summary>Flag to prevent redundant enabling calls.</summary>
    private bool _baseFieldEnabling = false;

    /// <summary>Flag to control internal evaluation of validation changes.</summary>
    private bool _baseFieldEvaluateToRaiseValidationChangesEventing = false;

    /// <summary>Tracks whether the original value has been set.</summary>
    protected bool FieldIsOriginalValueSet = false;

    /// <summary>Tracks the field's previous change state from original.</summary>
    private bool _baseFieldPreviousHasChangedFromOriginal = false;

    /// <summary>Flag used to skip nested property change events.</summary>
    private bool _baseFieldUpdateValidationAndChangedStating = false;

    /// <summary>Property for controlling how the field is accessed.</summary>
    public static readonly BindableProperty FieldAccessModeProperty = BindableProperty.Create(
        propertyName: nameof(FieldAccessMode),
        returnType: typeof(FieldAccessModeEnum),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: FieldAccessModeEnum.ViewOnly,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldAccessModePropertyChanged);

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

    /// <summary>Property for enabling or disabling the field.</summary>
    public static readonly BindableProperty FieldEnabledProperty = BindableProperty.Create(
        propertyName: nameof(FieldEnabled),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: false,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnBaseFieldEnabledPropertyChanged);

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

    #endregion Fields

    #region Public Constructors

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
    }

    protected void Initialize()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Initialize()");
        BaseField_WireFocusEvents(Field_ControlMain());
        BaseField_InitializeDataSource();
        ControlVisualHelper.MatchDisabledToEnabled(this);
    }

    #endregion Public Constructors

    #region Events

    /// <inheritdoc/>
    public event EventHandler<HasChangesEventArgs>? FieldHasChanges;

    /// <inheritdoc/>
    public event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

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

    /// <summary>
    /// Gets or sets the typed data source for this field.
    /// </summary>
    public T? FieldDataSource
    {
        get => (T?)GetValue(FieldDataSourceProperty);
        set
        {
            SetValue(FieldDataSourceProperty, value);
            Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : FieldDataSource set to: {value}");
        }
    }

    /// <inheritdoc/>
    public bool FieldEnabled
    {
        get => (bool)GetValue(FieldEnabledProperty);
        set => SetValue(FieldEnabledProperty, value);
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
    /// Invoked when the field enabled property changes.
    /// </summary>
    private static void OnBaseFieldEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool newEnabledState)
        {
            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"[BaseFormField] : {element.FieldLabelText} : OnBaseFieldEnabledPropertyChanged({newValue})");
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
            Debug.WriteLine($"[BaseFormField] : {element.FieldLabelText} : OnBaseFieldLabelTextPropertyChanged({newValue})");
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
                Debug.WriteLine($"[BaseFormField] : {element.FieldLabelText} : OnBaseFieldLabelVisiblePropertyChanged({newValue})");
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
                Debug.WriteLine($"[BaseFormField] : {element.FieldLabelText} : OnBaseFieldUndoButtonVisiblePropertyChanged({newValue})");
                element.FieldButtonUndo!.IsVisible = fieldUndoButtonVisible;
            }
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

    protected bool FieldAreValuesEqual(T? original, T? current)
    {
        if (typeof(T) == typeof(string))
        {
            var s1 = original as string ?? string.Empty;
            var s2 = current as string ?? string.Empty;

            Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : FieldAreValuesEqual(Comparing strings (forced cast): Original='{s1}', Current='{s2}')");
            return string.Equals(s1.Trim(), s2.Trim(), StringComparison.Ordinal);
        }

        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : FieldAreValuesEqual(Comparing objects: Original='{original}', Current='{current}')");
        return Equals(original, current);
    }

    /// <summary>
    /// Evaluates whether a change event should be raised.
    /// </summary>
    private void BaseFieldEvaluateToRaiseHasChangesEvent()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : BaseFieldEvaluateToRaiseHasChangesEvent()");

        if (_baseFieldEvaluateToRaiseHasChangesEventing)
            return;

        _baseFieldEvaluateToRaiseHasChangesEventing = true;

        bool hasChangedFromOriginal = Field_HasChangedFromOriginal();
        bool hasChangedFromLast = Field_HasChangedFromLast();

        if (_baseFieldPreviousHasChangedFromOriginal != hasChangedFromOriginal)
        {
            //Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : BaseFieldEvaluateToRaiseHasChangesEvent()");
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
            //Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Setting FieldChangeState = {(hasChangedFromOriginal ? "Changed" : "NotChanged")}");
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
            Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : BaseFieldEvaluateToRaiseValidationChangesEvent( {currentFieldHasInvalidData} )");
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
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : OnFieldButtonUndoPressed() - attempt revert");
        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            try
            {
                Field_UndoValue();
            }
            finally
            {
            }
        }
    }

    #endregion Private Methods

    #region Protected Methods

    /// <summary>
    /// Invoked when the field access mode property changes.
    /// </summary>
    protected static void OnFieldAccessModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is FieldAccessModeEnum newAccessMode)
        {
            element.Field_UpdateNotificationMessage();

            // Respect FieldReadOnly: force ViewOnly if read-only
            if (element.FieldReadOnly && newAccessMode != FieldAccessModeEnum.Hidden)
            {
                Debug.WriteLine($"[BaseFormField] : {element.FieldLabelText} : ReadOnly prevents switching to {newAccessMode}, forcing ViewOnly");
                element.Field_ConfigAccessModeViewing();
                return;
            }


            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"[BaseFormField] : {element.FieldLabelText} : OnFieldAccessModePropertyChanged({newValue})");
                switch (newAccessMode)
                {
                    case FieldAccessModeEnum.ViewOnly:
                        element.Field_ConfigAccessModeViewing();
                        return;

                    case FieldAccessModeEnum.Editing:
                        element.Field_ConfigAccessModeEditing();
                        return;

                    case FieldAccessModeEnum.Editable:
                        element.Field_ConfigAccessModeViewing();
                        return;

                    case FieldAccessModeEnum.Hidden:
                        element.BaseFieldConfigAccessModeHidden();
                        return;
                }
            }
        }
    }

    /// <summary>
    /// Enables descendant controls based on the current label visibility.
    /// </summary>
    protected void Field_ApplyEnabled()
    {
        ControlVisualHelper.EnableDescendantControls(this, FieldLabelVisible);
    }

    /// <summary>
    /// Applies a read-only state to all descendant controls.
    /// </summary>
    protected void Field_ApplyReadOnly()
    {
        ControlVisualHelper.DisableDescendantControls(this, FieldLabelVisible);
    }

    /// <summary>
    /// Sets this field to an editing mode, enabling controls and evaluating any changes.
    /// </summary>
    protected void Field_ConfigAccessModeEditing()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_ConfigAccessModeEditing()");
        Field_ApplyEnabled();
        BaseFieldEvaluateToRaiseHasChangesEvent();
    }

    /// <summary>
    /// Sets this field to a view-only mode, applying a read-only state and hiding the undo button.
    /// </summary>
    protected void Field_ConfigAccessModeViewing()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_ConfigAccessModeViewOnly()");
        Field_ApplyReadOnly();
        if (FieldButtonUndo != null) FieldButtonUndo.Hide();
    }
    protected override void OnBindingContextChanged()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : OnBindingContextChanged()");
        base.OnBindingContextChanged();
        // no more resetting FieldIsOriginalValueSet here
    }
    /// <summary>
    /// Disables user interaction on this field.
    /// </summary>
    protected void Field_ConfigDisabled()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_ConfigDisabled()");
        if (_baseFieldDisabling) return;
        _baseFieldDisabling = true;
        Field_ApplyReadOnly();
        _baseFieldDisabling = false;
    }

    /// <summary>
    /// Enables user interaction on this field.
    /// </summary>
    protected void Field_ConfigEnabled()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_ConfigEnabled()");
        if (_baseFieldEnabling) return;
        _baseFieldEnabling = true;
        Field_ApplyEnabled();
        _baseFieldEnabling = false;
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

    #region Abstract members you **must** implement in your subclass

    /// <summary>
    /// Return the raw input control(s) (Entry, DatePicker, CheckBoxOverlay, etc.)
    /// so the base can hook up Focused/Unfocused and validation logic.
    /// </summary>
    protected abstract List<View> Field_ControlMain();

    /// <summary>
    /// Build a two-row Grid:
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
    private Grid Field_CreateLayoutGrid()
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

        // 1) Label
        grid.Add(FieldLabel, 0, 0);

        // 2) Field‐specific controls (overlay, container, etc.)
        foreach (var ctl in Field_ControlMain())
            grid.Add(ctl, 1, 0);

        // 3) common undo button
        grid.Add(FieldButtonUndo, 2, 0);

        // 4) common notification row
        grid.Add(FieldNotification, 0, 1);
        Grid.SetColumnSpan(FieldNotification, 3);

        return grid;
    }

    /// <summary>
    /// Push a new value from the VM into your UI widget(s).
    /// Wrap all assignments in:
    /// UiThreadHelper.RunOnMainThread(() ⇒  
    ///     Field_PerformBatchUpdate(() ⇒ { … })  
    /// );
    /// </summary>
    protected abstract void Field_SetValue(T? value);

    /// <summary>
    /// Read the current value back out of your UI widget(s) so
    /// the base can compare it to original/last.
    /// </summary>
    protected abstract T? Field_GetCurrentValue();

    /// <summary>
    /// Return true if the current UI text/value is missing or empty
    /// while <c>FieldMandatory</c> is true.
    /// </summary>
    protected abstract bool Field_HasRequiredError();

    /// <summary>
    /// Return true if the current UI text/value is syntactically invalid.
    /// </summary>
    protected abstract bool Field_HasFormatError();

    /// <summary>
    /// Provide the format-error message to display under the field.
    /// </summary>
    protected abstract string Field_GetFormatErrorMessage();

    /// <summary>
    /// Compare <c>FieldLastValue</c> to <c>Field_GetCurrentValue()</c>
    /// to decide if the value changed since last update.
    /// </summary>
    protected abstract bool Field_HasChangedFromLast();

    /// <summary>
    /// Compare <c>FieldOriginalValue</c> to <c>Field_GetCurrentValue()</c>
    /// to decide if the value changed since initial load.
    /// </summary>
    protected abstract bool Field_HasChangedFromOriginal();

    /// <summary>
    /// After toggling LabelVisible or UndoButton, recalc your
    /// Grid.ColumnSpan/Column so your control remains centered.
    /// </summary>
    protected abstract void UpdateRow0Layout();

    #endregion


    /// <summary>
    /// Determines if the field data is valid by checking format and required errors.
    /// </summary>
    private bool BaseField_HasValidData()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : BaseField_HasValidData()");
        // Evaluate all error conditions and return false if any found.
        bool fail = Field_HasFormatError() || Field_HasRequiredError();
        return !fail;
    }

    private void BaseField_InitializeDataSource()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : BaseField_InitializeDataSource()");
        FieldOriginalValue = FieldDataSource;
        Field_SetValue(FieldDataSource);
    }

    protected void BaseField_OriginalValue_Reset()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : BaseField_OriginalValue_Reset()");
        Field_SetValue(FieldOriginalValue);
    }

    /// <summary>
    /// Clears the original value of the field.
    /// </summary>
    protected virtual void Field_OriginalValue_SetToClear()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_OriginalValue_SetToClear()");
        FieldOriginalValue = default(T);
        Field_SetValue(FieldOriginalValue);
    }

    /// <summary>
    /// Updates the original value of the field to match its current state.
    /// </summary>
    private void Field_OriginalValue_SetToCurrentValue()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_OriginalValue_SetToCurrentValue()");
        FieldOriginalValue = Field_GetCurrentValue();
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
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_SetDataSourceValue({newValue})");

        FieldSuppressDataSourceCallback = true;
        FieldDataSource = newValue;

        Field_UpdateValidationAndChangedState();
        Field_UpdateNotificationMessage();
    }


    /// <summary>
    /// Updates the field's notification label based on its validation state.
    /// </summary>
    protected virtual void Field_UpdateNotificationMessage()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_UpdateNotificationMessage()");
        bool showNotificationMessage = false;
        string notificationMessageText = String.Empty;
        if (FieldNotification == null) return;
        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            if (Field_HasRequiredError())
            {
                showNotificationMessage = true;
                notificationMessageText += String.Concat(" ", "Required");
            }
            if (Field_HasFormatError())
            {
                showNotificationMessage = true;
                notificationMessageText = Field_GetFormatErrorMessage();
            }
        }
        UiThreadHelper.RunOnMainThread(() =>
        {
            Field_PerformBatchUpdate(() =>
            {
                FieldNotification.Text = notificationMessageText.Trim();
                FieldNotification.IsVisible = showNotificationMessage;
            });
        });
    }

    /// <summary>
    /// Updates both validation and changed states, optionally forcing a validation check.
    /// </summary>
    protected void Field_UpdateValidationAndChangedState(bool forceRaiseValidationChangesEvent = false)
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_UpdateValidationAndChangedState({forceRaiseValidationChangesEvent})");
        if (_baseFieldUpdateValidationAndChangedStating)
            return;
        _baseFieldUpdateValidationAndChangedStating = true;

        BaseFieldEvaluateToRaiseHasChangesEvent();
        BaseFieldEvaluateToRaiseValidationChangesEvent(forceRaiseValidationChangesEvent);

        _baseFieldUpdateValidationAndChangedStating = false;
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
    /// Hides the field by applying a read-only state and hiding the undo button.
    /// </summary>
    private void BaseFieldConfigAccessModeHidden()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : BaseFieldConfigAccessModeHidden()");
        Field_ApplyReadOnly();
        if (FieldButtonUndo != null) FieldButtonUndo.Hide();
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
        Debug.WriteLine($"[BaseFormField] : {control.FieldLabelText} : OnBaseFieldDataSourcePropertyChangedStatic(old='{oldValue}', new='{newValue}')");

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
            control.OnBaseInitializeFieldDataSourceSet(initialValue: (T?)newValue);
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
    private void OnBaseInitializeFieldDataSourceSet(T? initialValue)
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : OnBaseInitializeFieldDataSourceSet(initialValue {initialValue})");
        FieldOriginalValue = initialValue;
    }

    /// <summary>
    /// Called on every FieldDataSource change (after initial).
    /// Default implementation synchronizes UI; override to inject custom logic but call base.Sync if needed.
    /// </summary>
    private void OnBaseFieldDataSourceChanged(T? newValue, T? oldValue)
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : OnBaseFieldDataSourceChanged(oldValue {newValue}, oldValue {oldValue})");
        Field_SetValue(newValue);
        FieldLastValue = newValue;
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : OnParentSet()");
        base.OnParentSet();

        Content = Field_CreateLayoutGrid();

        // keep the UI in sync if a design-time value is already present
        Field_SetValue(FieldDataSource);

        OnFieldAccessModePropertyChanged(
            bindable: this,
            oldValue: FieldAccessMode,  // or something else if you have an "old" value
            newValue: FieldAccessMode
        );
    }

    #endregion Protected Methods

    #region Public Methods

    /// <inheritdoc/>
    public void Field_Clear()
    {
        Field_OriginalValue_SetToClear();
        Field_UpdateValidationAndChangedState();
        Field_Unfocus();
    }

    /// <inheritdoc/>
    protected void Field_Focused(object? sender, FocusEventArgs e)
    { }

    /// <inheritdoc/>
    protected void Field_NotifyHasChanges(bool hasChanged) =>
        FieldHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));

    /// <inheritdoc/>
    protected void Field_NotifyValidationChanges(bool isValid) =>
        FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));

    /// <inheritdoc/>Field_SaveAndMarkAsReadOnly
    public void Field_SaveAndMarkAsReadOnly()
    {
        Field_OriginalValue_SetToCurrentValue();
        Field_ConfigDisabled();
        Field_UpdateValidationAndChangedState();
        FieldAccessMode = FieldAccessModeEnum.ViewOnly;
    }

    public void Field_UndoValue()
    {
        // By default, restore FieldOriginalValue
        Field_PerformBatchUpdate(() =>
        {
            BaseField_OriginalValue_Reset();
            Field_UpdateValidationAndChangedState(true);
            Field_UpdateNotificationMessage();
        });
    }

    /// <inheritdoc/>
    public virtual void Field_Unfocus() => base.Unfocus();

    /// <inheritdoc/>
    protected void Field_Unfocused(object? sender, FocusEventArgs e)
    { }

    /// <inheritdoc/>
    public void Field_UpdateLabelWidth(double newWidth)
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_UpdateLabelWidth(newWidth: {newWidth})");
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
    public void Field_UpdateWidth(double newWidth)
    {
        Debug.WriteLine($"[BaseFormField] : {FieldLabelText} : Field_UpdateWidth(newWidth: {newWidth})");
        if (Content is Grid grid && grid.ColumnDefinitions.Count > 1)
            UiThreadHelper.RunOnMainThread(() =>
            {
                Field_PerformBatchUpdate(() =>
                {
                    grid.ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
                });
            });
    }

    #endregion Public Methods
}

