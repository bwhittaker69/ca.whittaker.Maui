using ca.whittaker.Maui.Controls.Buttons;
using System.Diagnostics;
using System.Windows.Input;

namespace ca.whittaker.Maui.Controls.Forms;

public interface IBaseFormField
{
    #region Events

    event EventHandler<HasChangesEventArgs>? FieldHasChanges;

    event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

    #endregion Events

    #region Properties

    FieldAccessModeEnum FieldAccessMode { get; set; }
    ChangeStateEnum FieldChangeState { get; set; }
    ICommand FieldCommand { get; set; }
    object FieldCommandParameter { get; set; }
    bool FieldEnabled { get; set; }
    string FieldLabelText { get; set; }
    bool FieldLabelVisible { get; set; }
    double FieldLabelWidth { get; set; }
    bool FieldMandatory { get; set; }
    bool FieldReadOnly { get; set; }
    bool FieldUndoButtonVisible { get; set; }
    ValidationStateEnum FieldValidationState { get; set; }
    double FieldWidth { get; set; }
    LayoutOptions HorizontalOptions { get; set; }
    LayoutOptions VerticalOptions { get; set; }

    #endregion Properties

    #region Public Methods

    void Field_Clear();

    void Field_Focused(object? sender, FocusEventArgs e);

    void Field_NotifyHasChanges(bool hasChanged);

    void Field_NotifyValidationChanges(bool isValid);

    void Field_SaveAndMarkAsReadOnly();

    void Field_Unfocus();

    void Field_Unfocused(object? sender, FocusEventArgs e);

    void Field_UpdateLabelWidth(double newWidth);

    void Field_UpdateWidth(double newWidth);

    #endregion Public Methods
}

/// <summary>
/// Represents the base class for customizable data capture controls.
/// </summary>
public abstract class BaseFormField<T> : ContentView, IBaseFormField
{
    #region Fields

    /// <summary>Flag to control internal evaluation of changes.</summary>
    private bool _fieldEvaluateToRaiseHasChangesEventing = false;

    /// <summary>Tracks whether the field previously had invalid data.</summary>
    private bool _previousFieldHasInvalidData = false;

    /// <summary>Tracks the previous validation state for this field.</summary>
    private ValidationStateEnum _previousFieldValidationState;

    /// <summary>Defines the default button size.</summary>
    protected const SizeEnum DefaultButtonSize = SizeEnum.XXSmall;

    /// <summary>Flag to prevent redundant disabling calls.</summary>
    protected bool _fieldDisabling = false;

    /// <summary>Flag to prevent redundant enabling calls.</summary>
    protected bool _fieldEnabling = false;

    /// <summary>Flag to control internal evaluation of validation changes.</summary>
    protected bool _fieldEvaluateToRaiseValidationChangesEventing = false;

    /// <summary>Tracks whether the original value has been set.</summary>
    protected bool _fieldIsOriginalValueSet = false;

    /// <summary>Tracks the field's previous change state from original.</summary>
    protected bool _fieldPreviousHasChangedFromOriginal = false;

    /// <summary>Flag used to skip nested property change events.</summary>
    protected bool _fieldUpdateValidationAndChangedStating = false;

    /// <summary>Flag to prevent nested UI changes from firing multiple updates.</summary>
    protected bool _fieldUpdatingUI = false;

    /// <summary>Flag indicating if data source property change is in progress.</summary>
    protected bool _onFieldDataSourcePropertyChanging = false;

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

    /// <summary>Property for holding the data source of this field.</summary>
    public static readonly BindableProperty FieldDataSourceProperty = BindableProperty.Create(
        propertyName: nameof(FieldDataSource),
        returnType: typeof(T?),
        declaringType: typeof(BaseFormField<T?>),
        defaultValue: default(T?),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => { ((BaseFormField<T?>)bindable).OnFieldDataSourcePropertyChanged(newValue, oldValue); });

    /// <summary>Property for enabling or disabling the field.</summary>
    public static readonly BindableProperty FieldEnabledProperty = BindableProperty.Create(
        propertyName: nameof(FieldEnabled),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: false,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldEnabledPropertyChanged);

    /// <summary>Property for the field's label text.</summary>
    public static readonly BindableProperty FieldLabelTextProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelText),
        returnType: typeof(string),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: string.Empty,
        propertyChanged: OnFieldLabelTextPropertyChanged);

    /// <summary>Property for showing or hiding the field's label.</summary>
    public static readonly BindableProperty FieldLabelVisibleProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelVisible),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: true,
        propertyChanged: OnFieldLabelVisiblePropertyChanged);

    /// <summary>Property for the width of the field's label.</summary>
    public static readonly BindableProperty FieldLabelWidthProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: 100d,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldLabelWidthPropertyChanged);

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
    public static readonly BindableProperty FieldUndoButtonVisibleProperty = BindableProperty.Create(
        propertyName: nameof(FieldUndoButtonVisible),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: true,
        propertyChanged: OnFieldUndoButtonVisiblePropertyChanged);

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
        propertyChanged: OnFieldWidthPropertyChanged);

    /// <summary>Reference to the undo button.</summary>
    public UndoButton? FieldButtonUndo;

    /// <summary>Reference to the label for this field.</summary>
    public Label? FieldLabel;

    /// <summary>Stores the last known value of this field.</summary>
    public T? FieldLastValue = default(T);

    /// <summary>Reference to the notification label.</summary>
    public Label? FieldNotification;

    /// <summary>Stores the original value for this field.</summary>
    public T? FieldOriginalValue = default(T);

    #endregion Fields

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseFormField{T}"/> class.
    /// </summary>
    public BaseFormField()
    {
        FieldLabel = Field_CreateLabel(fieldLabelVisible: FieldLabelVisible);
        FieldNotification = Field_CreateNotificationLabel();
        FieldButtonUndo = Field_CreateUndoButton(fieldHasUndo: FieldUndoButtonVisible, fieldAccessMode: FieldAccessMode);

        // Subscribe to the undo button's Pressed event
        FieldButtonUndo.Pressed += OnFieldButtonUndoPressed;

        // Explicitly configure the undo button's initial state
        if (FieldAccessMode == FieldAccessModeEnum.Editing && FieldUndoButtonVisible)
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
        set => SetValue(FieldDataSourceProperty, value);
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
    public bool FieldUndoButtonVisible
    {
        get => (bool)GetValue(FieldUndoButtonVisibleProperty);
        set => SetValue(FieldUndoButtonVisibleProperty, value);
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
    /// Invoked when the field access mode property changes.
    /// </summary>
    protected static void OnFieldAccessModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is FieldAccessModeEnum newAccessMode)
        {
            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"{element.FieldLabelText} : OnFieldAccessModePropertyChanged({newValue})");
                switch (newAccessMode)
                {
                    case FieldAccessModeEnum.ViewOnly:
                        element.Field_ConfigAccessModeViewOnly();
                        return;

                    case FieldAccessModeEnum.Editing:
                        element.Field_ConfigAccessModeEditing();
                        return;

                    case FieldAccessModeEnum.Editable:
                        element.Field_ConfigAccessModeEditable();
                        return;

                    case FieldAccessModeEnum.Hidden:
                        element.FieldConfigAccessModeHidden();
                        return;
                }
            }
        }
    }

    /// <summary>
    /// Invoked when the field enabled property changes.
    /// </summary>
    private static void OnFieldEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool newEnabledState)
        {
            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"{element.FieldLabelText} : OnFieldEnabledPropertyChanged({newValue})");
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
    private static void OnFieldLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.FieldLabel != null)
        {
            Debug.WriteLine($"{element.FieldLabelText} : OnFieldAccessModePropertyChanged({newValue})");
            element.FieldLabel.Text = newValue?.ToString() ?? "";
        }
    }

    /// <summary>
    /// Invoked when the field label visibility changes.
    /// </summary>
    private static void OnFieldLabelVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool fieldLabelVisible)
        {
            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"OnFieldLabelVisiblePropertyChanged({newValue})");
                element.FieldLabel!.IsVisible = fieldLabelVisible;
            }
        }
    }

    /// <summary>
    /// Invoked when the field label width changes.
    /// </summary>
    private static void OnFieldLabelWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.Content is Grid)
            element.Field_UpdateLabelWidth((double)newValue);
    }

    /// <summary>
    /// Invoked when the undo button visibility changes.
    /// </summary>
    private static void OnFieldUndoButtonVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool fieldUndoButtonVisible)
        {
            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"OnFieldUndoButtonVisiblePropertyChanged({newValue})");
                element.FieldButtonUndo!.IsVisible = fieldUndoButtonVisible;
            }
        }
    }

    /// <summary>
    /// Invoked when the field width changes.
    /// </summary>
    private static void OnFieldWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.Content is Grid)
            element.Field_UpdateWidth((double)newValue);
    }

    /// <summary>
    /// Evaluates whether a change event should be raised.
    /// </summary>
    private void EvaluateToRaiseHasChangesEvent()
    {
        if (_fieldEvaluateToRaiseHasChangesEventing)
            return;

        _fieldEvaluateToRaiseHasChangesEventing = true;
        bool hasChangedFromOriginal = Field_HasChangedFromOriginal();
        bool hasChangedFromLast = Field_HasChangedFromLast();
        if (_fieldPreviousHasChangedFromOriginal != hasChangedFromOriginal)
        {
            Debug.WriteLine($"{FieldLabelText} : EvaluateToRaiseHasChangesEvent()");
            if (FieldButtonUndo != null && FieldUndoButtonVisible)
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
            _fieldPreviousHasChangedFromOriginal = hasChangedFromOriginal;
            FieldChangeState = hasChangedFromOriginal ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
            Field_NotifyHasChanges(hasChangedFromOriginal);
        }
        _fieldEvaluateToRaiseHasChangesEventing = false;
    }

    /// <summary>
    /// Evaluates whether a validation change event should be raised.
    /// </summary>
    private void EvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
    {
        if (_fieldEvaluateToRaiseValidationChangesEventing)
            return;

        _fieldEvaluateToRaiseValidationChangesEventing = true;
        bool currentFieldHasInvalidData = Field_HasValidData() == false;
        var currentFieldValidationState = currentFieldHasInvalidData ? ValidationStateEnum.FormatError : ValidationStateEnum.Valid;
        if (_previousFieldHasInvalidData != currentFieldHasInvalidData || forceRaise || _previousFieldValidationState != currentFieldValidationState)
        {
            Debug.WriteLine($"{FieldLabelText} : EvaluateToRaiseValidationChangesEvent( {currentFieldHasInvalidData} )");
            _previousFieldHasInvalidData = currentFieldHasInvalidData;
            _previousFieldValidationState = currentFieldValidationState;
            FieldValidationState = currentFieldValidationState;
            Field_NotifyValidationChanges(!currentFieldHasInvalidData);
        }
        _fieldEvaluateToRaiseValidationChangesEventing = false;
    }

    /// <summary>
    /// Handles the logic when the Undo button is pressed.
    /// </summary>
    private void OnFieldButtonUndoPressed(object? sender, EventArgs e)
    {
        Debug.WriteLine($"{FieldLabelText} : OnFieldButtonUndoPressed() - attempt revert");
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
    /// Enables descendant controls based on the current label visibility.
    /// </summary>
    protected virtual void Field_ApplyEnabled()
    {
        ControlVisualHelper.EnableDescendantControls(this, FieldLabelVisible);
    }

    /// <summary>
    /// Applies a read-only state to all descendant controls.
    /// </summary>
    protected virtual void Field_ApplyReadOnly()
    {
        ControlVisualHelper.DisableDescendantControls(this, FieldLabelVisible);
    }

    /// <summary>
    /// Sets this field to an editable mode by applying a read-only state and hiding the undo button.
    /// </summary>
    protected virtual void Field_ConfigAccessModeEditable()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigAccessModeEditable()");
        Field_ApplyReadOnly();
        if (FieldButtonUndo != null) FieldButtonUndo.Hide();
    }

    /// <summary>
    /// Sets this field to an editing mode, enabling controls and evaluating any changes.
    /// </summary>
    protected virtual void Field_ConfigAccessModeEditing()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigAccessModeEditing()");
        Field_ApplyEnabled();
        EvaluateToRaiseHasChangesEvent();
    }

    /// <summary>
    /// Sets this field to a view-only mode, applying a read-only state and hiding the undo button.
    /// </summary>
    protected virtual void Field_ConfigAccessModeViewOnly()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigAccessModeViewOnly()");
        Field_ApplyReadOnly();
        if (FieldButtonUndo != null) FieldButtonUndo.Hide();
    }

    /// <summary>
    /// Disables user interaction on this field.
    /// </summary>
    protected virtual void Field_ConfigDisabled()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigDisabled()");
        if (_fieldDisabling) return;
        _fieldDisabling = true;
        Field_ApplyReadOnly();
        _fieldDisabling = false;
    }

    /// <summary>
    /// Enables user interaction on this field.
    /// </summary>
    protected virtual void Field_ConfigEnabled()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigEnabled()");
        if (_fieldEnabling) return;
        _fieldEnabling = true;
        Field_ApplyEnabled();
        _fieldEnabling = false;
    }

    /// <summary>
    /// Creates the primary label for the field, respecting visibility.
    /// </summary>
    protected virtual Label Field_CreateLabel(bool fieldLabelVisible)
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
    /// Deriving classes must implement a layout in row 0 for label, data-input control, and undo button
    /// plus row 1 for notifications. Some fields place multiple controls in row 0.
    /// </summary>
    /// <returns>The configured grid layout for this field.</returns>
    protected abstract Grid Field_CreateLayoutGrid();

    /// <summary>
    /// Creates a label used for notification messages such as errors or required fields.
    /// </summary>
    protected virtual Label Field_CreateNotificationLabel()
    {
        var label = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            IsVisible = false
        };
        return label;
    }

    /// <summary>
    /// Creates the undo button for reverting field changes.
    /// </summary>
    protected virtual UndoButton Field_CreateUndoButton(bool fieldHasUndo, FieldAccessModeEnum fieldAccessMode)
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

    protected abstract T? Field_GetCurrentValue();

    /// <summary>
    /// Returns the format error message of the field.
    /// </summary>
    protected abstract string Field_GetFormatErrorMessage();

    /// <summary>
    /// Checks if the field's current value differs from its last known value.
    /// </summary>
    protected abstract bool Field_HasChangedFromLast();

    /// <summary>
    /// Checks if the field's current value differs from its original value.
    /// </summary>
    protected abstract bool Field_HasChangedFromOriginal();

    /// <summary>
    /// Indicates whether the field data contains any format errors.
    /// </summary>
    protected abstract bool Field_HasFormatError();

    /// <summary>
    /// Indicates whether the field is missing required data.
    /// </summary>
    protected abstract bool Field_HasRequiredError();

    /// <summary>
    /// Determines if the field data is valid by checking format and required errors.
    /// </summary>
    protected virtual bool Field_HasValidData()
    {
        // Evaluate all error conditions and return false if any found.
        bool fail = Field_HasFormatError() || Field_HasRequiredError();
        return !fail;
    }

    protected virtual void Field_InitializeDataSource()
    {
        Field_SyncUIFromDataSource(FieldDataSource);
    }


    /// <summary>
    /// Clears the original value of the field.
    /// </summary>
    protected virtual void Field_OriginalValue_SetToClear()
    {
        FieldOriginalValue = default(T);
        Field_SetValue(FieldOriginalValue);
    }

    protected void Field_OriginalValue_Reset()
    {
        Field_SetValue(FieldOriginalValue);
    }


    /// <summary>
    /// Updates the original value of the field to match its current state.
    /// </summary>
    protected void Field_OriginalValue_SetToCurrentValue()
    {
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
        Debug.WriteLine($"{FieldLabelText} : Field_SetDataSourceValue({newValue})");
        FieldDataSource = newValue;
        Field_UpdateValidationAndChangedState();
        Field_UpdateNotificationMessage();
        FieldLastValue = newValue;
    }

    protected abstract void Field_SetValue(T? newValue);

    protected void Field_SyncUIFromDataSource(T? newValue)
    {
        Field_SetValue(newValue);
    }

    protected virtual void Field_UndoValue()
    {
        // By default, restore FieldOriginalValue
        Field_PerformBatchUpdate(() =>
        {
            Field_OriginalValue_Reset();
            Field_UpdateValidationAndChangedState(true);
            Field_UpdateNotificationMessage();
        });
    }

    /// <summary>
    /// Updates the field's notification label based on its validation state.
    /// </summary>
    protected virtual void Field_UpdateNotificationMessage()
    {
        if (FieldNotification == null) return;
        if (Field_HasFormatError())
        {
            FieldNotification.Text = Field_GetFormatErrorMessage();
            FieldNotification.IsVisible = true;
        }
        else if (Field_HasRequiredError())
        {
            FieldNotification.Text = "Required.";
            FieldNotification.IsVisible = true;
        }
        else
        {
            FieldNotification.Text = string.Empty;
            FieldNotification.IsVisible = false;
        }
    }

    /// <summary>
    /// Updates both validation and changed states, optionally forcing a validation check.
    /// </summary>
    protected void Field_UpdateValidationAndChangedState(bool forceRaiseValidationChangesEvent = false)
    {
        if (_fieldUpdateValidationAndChangedStating)
            return;
        _fieldUpdateValidationAndChangedStating = true;

        EvaluateToRaiseHasChangesEvent();
        EvaluateToRaiseValidationChangesEvent(forceRaiseValidationChangesEvent);

        _fieldUpdateValidationAndChangedStating = false;
    }

    protected void Field_WireFocusEvents(View input)
    {
        input.Focused += Field_Focused;
        input.Unfocused += Field_Unfocused;
    }

    /// <summary>
    /// Hides the field by applying a read-only state and hiding the undo button.
    /// </summary>
    protected virtual void FieldConfigAccessModeHidden()
    {
        Debug.WriteLine($"{FieldLabelText} : FieldConfigAccessModeHidden()");
        Field_ApplyReadOnly();
        if (FieldButtonUndo != null) FieldButtonUndo.Hide();
    }

    /// <summary>
    /// Initializes the layout container if necessary.
    /// </summary>
    protected void InitializeLayout()
    { }

    protected virtual void OnFieldDataSourcePropertyChanged(object newValue, object oldValue)
    {
        Field_SyncUIFromDataSource((T)newValue);
    }

    //protected override void OnParentSet()
    //{
    //    Debug.WriteLine($"OnParentSet()");
    //    base.OnParentSet();
    //    Field_SetValue(FieldDataSource);
    //}


    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (!_fieldIsOriginalValueSet)
        {
            _fieldIsOriginalValueSet = true;
            Field_OriginalValue_SetToCurrentValue();
        }

        Content = Field_CreateLayoutGrid();

        OnFieldAccessModePropertyChanged(
            bindable: this,
            oldValue: FieldAccessMode,  // or something else if you have an "old" value
            newValue: FieldAccessMode
        );
    }

    /// <summary>
    /// Called by derived classes to update the layout constraints of row 0
    /// whenever the label or button might become hidden or shown. This method typically calls
    /// Grid.SetColumnSpan on the data-input control.
    /// </summary>
    protected virtual void UpdateRow0Layout()
    {
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
    public void Field_Focused(object? sender, FocusEventArgs e)
    { }

    /// <inheritdoc/>
    public void Field_NotifyHasChanges(bool hasChanged) =>
        FieldHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));

    /// <inheritdoc/>
    public void Field_NotifyValidationChanges(bool isValid) =>
        FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));

    /// <inheritdoc/>
    public void Field_SaveAndMarkAsReadOnly()
    {
        FieldAccessMode = FieldAccessModeEnum.ViewOnly;
        Field_OriginalValue_SetToCurrentValue();
        Field_ConfigDisabled();
        Field_UpdateValidationAndChangedState();
    }

    /// <inheritdoc/>
    public virtual void Field_Unfocus() => base.Unfocus();

    /// <inheritdoc/>
    public void Field_Unfocused(object? sender, FocusEventArgs e)
    { }

    /// <inheritdoc/>
    public void Field_UpdateLabelWidth(double newWidth)
    {
        if (Content is Grid grid && grid.ColumnDefinitions.Count > 0)
        {
            grid.ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
            FieldLabel!.WidthRequest = newWidth;
        }
    }

    /// <inheritdoc/>
    public void Field_UpdateWidth(double newWidth)
    {
        if (Content is Grid grid && grid.ColumnDefinitions.Count > 1)
            grid.ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
    }

    #endregion Public Methods
}