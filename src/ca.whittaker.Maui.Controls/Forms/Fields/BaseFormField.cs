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

    private bool _fieldEvaluateToRaiseHasChangesEventing = false;
    private bool _previousFieldHasInvalidData = false;
    private ValidationStateEnum _previousFieldValidationState;
    protected const SizeEnum DefaultButtonSize = SizeEnum.XXSmall;
    protected bool _fieldDisabling = false;
    protected bool _fieldEnabling = false;
    protected bool _fieldEvaluateToRaiseValidationChangesEventing = false;
    protected bool _fieldIsOriginalValueSet = false;
    protected bool _fieldPreviousHasChangedFromOriginal = false;
    protected bool _fieldUpdateValidationAndChangedStating = false;
    protected bool _fieldUpdatingUI = false;
    protected bool _onFieldDataSourcePropertyChanging = false;

    public static readonly BindableProperty FieldAccessModeProperty = BindableProperty.Create(
        propertyName: nameof(FieldAccessMode),
        returnType: typeof(FieldAccessModeEnum),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: FieldAccessModeEnum.ViewOnly,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldAccessModePropertyChanged);

    public static readonly BindableProperty FieldChangeStateProperty = BindableProperty.Create(
        propertyName: nameof(FieldChangeState),
        returnType: typeof(ChangeStateEnum),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: ChangeStateEnum.NotChanged,
        BindingMode.TwoWay);

    public static readonly BindableProperty FieldCommandParameterProperty = BindableProperty.Create(
        propertyName: nameof(FieldCommandParameter),
        returnType: typeof(object),
        declaringType: typeof(Form));

    public static readonly BindableProperty FieldCommandProperty = BindableProperty.Create(
        propertyName: nameof(FieldCommand),
        returnType: typeof(ICommand),
        declaringType: typeof(Form));

    public static readonly BindableProperty FieldDataSourceProperty = BindableProperty.Create(
        propertyName: nameof(FieldDataSource),
        returnType: typeof(T?),
        declaringType: typeof(BaseFormField<T?>),
        defaultValue: default(T?),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => { ((BaseFormField<T?>)bindable).OnFieldDataSourcePropertyChanged(newValue, oldValue); });

    public static readonly BindableProperty FieldEnabledProperty = BindableProperty.Create(
        propertyName: nameof(FieldEnabled),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: false,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldEnabledPropertyChanged);

    public static readonly BindableProperty FieldLabelTextProperty = BindableProperty.Create(
                propertyName: nameof(FieldLabelText),
        returnType: typeof(string),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: string.Empty,
        propertyChanged: OnFieldLabelTextPropertyChanged);

    public static readonly BindableProperty FieldLabelVisibleProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelVisible),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: true,
        propertyChanged: OnFieldLabelVisiblePropertyChanged);

    public static readonly BindableProperty FieldLabelWidthProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: 100d,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldLabelWidthPropertyChanged);

    public static readonly BindableProperty FieldMandatoryProperty = BindableProperty.Create(
        propertyName: nameof(FieldMandatory),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: false);

    public static readonly BindableProperty FieldReadOnlyProperty = BindableProperty.Create(
                                        propertyName: nameof(FieldReadOnly),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty FieldUndoButtonVisibleProperty = BindableProperty.Create(
                                        propertyName: nameof(FieldUndoButtonVisible),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: true,
        propertyChanged: OnFieldUndoButtonVisiblePropertyChanged);

    public static readonly BindableProperty FieldValidationStateProperty = BindableProperty.Create(
        propertyName: nameof(FieldValidationState),
        returnType: typeof(ValidationStateEnum),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: ValidationStateEnum.Valid,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty FieldWidthProperty = BindableProperty.Create(
        propertyName: nameof(FieldWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormField<T>),
        defaultValue: 100d,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldWidthPropertyChanged);

    public UndoButton? ButtonUndo;
    public Label? FieldLabel;
    public T? FieldLastValue = default(T);
    public Label? FieldNotification;
    public T? FieldOriginalValue = default(T);

    #endregion Fields

    #region Public Constructors

    public BaseFormField()
    {
        FieldLabel = Field_CreateLabel(fieldLabelVisible: FieldLabelVisible);
        FieldNotification = Field_CreateNotificationLabel();
        ButtonUndo = Field_CreateUndoButton(fieldHasUndo: FieldUndoButtonVisible, fieldAccessMode: FieldAccessMode);
        ButtonUndo.Pressed += OnFieldButtonUndoPressed;
    }

    #endregion Public Constructors

    #region Events

    public event EventHandler<HasChangesEventArgs>? FieldHasChanges;

    public event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

    #endregion Events

    #region Properties

    public FieldAccessModeEnum FieldAccessMode
    {
        get => (FieldAccessModeEnum)GetValue(FieldAccessModeProperty);
        set => SetValue(FieldAccessModeProperty, value);
    }

    public ChangeStateEnum FieldChangeState
    {
        get => (ChangeStateEnum)GetValue(FieldChangeStateProperty);
        set => SetValue(FieldChangeStateProperty, value);
    }

    public ICommand FieldCommand
    {
        get => (ICommand)GetValue(FieldCommandProperty);
        set => SetValue(FieldCommandProperty, value);
    }

    public object FieldCommandParameter
    {
        get => GetValue(FieldCommandParameterProperty);
        set => SetValue(FieldCommandParameterProperty, value);
    }

    public T? FieldDataSource
    {
        get => (T?)GetValue(FieldDataSourceProperty);
        set => SetValue(FieldDataSourceProperty, value);
    }
    public bool FieldEnabled
    {
        get => (bool)GetValue(FieldEnabledProperty);
        set => SetValue(FieldEnabledProperty, value);
    }

    public string FieldLabelText
    {
        get => (string)GetValue(FieldLabelTextProperty);
        set => SetValue(FieldLabelTextProperty, value);
    }

    public bool FieldLabelVisible
    {
        get => (bool)GetValue(FieldLabelVisibleProperty);
        set => SetValue(FieldLabelVisibleProperty, value);
    }

    public double FieldLabelWidth
    {
        get => (double)GetValue(FieldLabelWidthProperty);
        set => SetValue(FieldLabelWidthProperty, value);
    }

    public bool FieldMandatory
    {
        get => (bool)GetValue(FieldMandatoryProperty);
        set => SetValue(FieldMandatoryProperty, value);
    }

    public bool FieldReadOnly
    {
        get => (bool)GetValue(FieldReadOnlyProperty);
        set => SetValue(FieldReadOnlyProperty, value);
    }

    public bool FieldUndoButtonVisible
    {
        get => (bool)GetValue(FieldUndoButtonVisibleProperty);
        set => SetValue(FieldUndoButtonVisibleProperty, value);
    }

    public ValidationStateEnum FieldValidationState
    {
        get => (ValidationStateEnum)GetValue(FieldValidationStateProperty);
        set => SetValue(FieldValidationStateProperty, value);
    }

    public double FieldWidth
    {
        get => (double)GetValue(FieldWidthProperty);
        set => SetValue(FieldWidthProperty, value);
    }

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

    private static void OnFieldAccessModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
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

    private static void OnFieldLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.FieldLabel != null)
            element.FieldLabel.Text = newValue?.ToString() ?? "";
    }

    private static void OnFieldLabelVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool fieldLabelVisible)
        {
            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"OnFieldLabelVisiblePropertyChanged({newValue})");
                if (fieldLabelVisible)
                    element.FieldLabel!.IsVisible = true;
                else
                    element.FieldLabel!.IsVisible = false;
            }
        }
    }

    private static void OnFieldLabelWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.Content is Grid)
            element.Field_UpdateLabelWidth((double)newValue);
    }

    private static void OnFieldUndoButtonVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && newValue is bool fieldUndoButtonVisible)
        {
            if (!oldValue.Equals(newValue))
            {
                Debug.WriteLine($"OnFieldUndoButtonVisiblePropertyChanged({newValue})");
                if (fieldUndoButtonVisible)
                    element.ButtonUndo!.IsVisible = true;
                else
                    element.ButtonUndo!.IsVisible = false;
            }
        }
    }

    private static void OnFieldWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField<T> element && element.Content is Grid)
            element.Field_UpdateWidth((double)newValue);
    }

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
            if (ButtonUndo != null)
            {
                if (FieldUndoButtonVisible)
                {
                    if (FieldAccessMode == FieldAccessModeEnum.Editing)
                    {
                        if (hasChangedFromOriginal)
                        {
                            Debug.WriteLine("ButtonUndo.Enabled()");
                            ButtonUndo.Enabled();
                        }
                        else
                        {
                            Debug.WriteLine("ButtonUndo.Disabled()");
                            ButtonUndo.Disabled();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ButtonUndo.Hide()");
                        ButtonUndo.Hide();
                    }
                }
            }
            _fieldPreviousHasChangedFromOriginal = hasChangedFromOriginal;
            FieldChangeState = hasChangedFromOriginal ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
            Field_NotifyHasChanges(hasChangedFromOriginal);
        }
        _fieldEvaluateToRaiseHasChangesEventing = false;
    }

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

    private void OnFieldButtonUndoPressed(object? sender, EventArgs e)
    {
        Debug.WriteLine($"{FieldLabelText} : OnFieldButtonUndoPressed() - attempt revert");
        if (FieldAccessMode == FieldAccessModeEnum.Editing)
        {
            try
            {
                Field_OriginalValue_Reset();
                Field_UpdateValidationAndChangedState(true);
                Field_UpdateNotificationMessage();
            }
            finally
            {
            }
        }
    }

    #endregion Private Methods

    #region Protected Methods

    protected virtual void Field_ApplyEnabled()
    {
        ControlVisualHelper.EnableDescendantControls(this, FieldLabelVisible);
    }

    protected virtual void Field_ApplyReadOnly()
    {
        ControlVisualHelper.DisableDescendantControls(this, FieldLabelVisible);
    }

    protected virtual void Field_ConfigAccessModeEditable()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigAccessModeEditable()");
        Field_ApplyReadOnly();
        if (ButtonUndo != null) ButtonUndo.Hide();
    }

    protected virtual void Field_ConfigAccessModeEditing()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigAccessModeEditing()");
        Field_ApplyEnabled();
        EvaluateToRaiseHasChangesEvent();
    }

    protected virtual void Field_ConfigAccessModeViewOnly()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigAccessModeViewOnly()");
        Field_ApplyReadOnly();
        if (ButtonUndo != null) ButtonUndo.Hide();
    }

    protected virtual void Field_ConfigDisabled()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigDisabled()");
        if (_fieldDisabling) return;
        _fieldDisabling = true;
        Field_ApplyReadOnly();
        _fieldDisabling = false;
    }

    protected virtual void Field_ConfigEnabled()
    {
        Debug.WriteLine($"{FieldLabelText} : Field_ConfigEnabled()");
        if (_fieldEnabling) return;
        _fieldEnabling = true;
        Field_ApplyEnabled();
        _fieldEnabling = false;
    }

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
    /// <returns></returns>
    protected abstract Grid Field_CreateLayoutGrid();

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

    protected virtual UndoButton Field_CreateUndoButton(bool fieldHasUndo, FieldAccessModeEnum fieldAccessMode)
    {
        var btn = new UndoButton
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };
        btn.IsVisible = fieldHasUndo;
        return btn;
    }

    protected abstract string Field_GetFormatErrorMessage();

    protected abstract bool Field_HasChangedFromLast();

    protected abstract bool Field_HasChangedFromOriginal();

    protected abstract bool Field_HasFormatError();

    protected abstract bool Field_HasRequiredError();

    protected virtual bool Field_HasValidData()
    {
        // Evaluate all error conditions and return false if any found.
        bool fail = Field_HasFormatError() || Field_HasRequiredError();
        return !fail;
    }

    protected abstract void Field_OriginalValue_Reset();

    protected abstract void Field_OriginalValue_SetToClear();

    protected abstract void Field_OriginalValue_SetToCurrentValue();

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

    protected void Field_UpdateValidationAndChangedState(bool forceRaiseValidationChangesEvent = false)
    {
        if (_fieldUpdateValidationAndChangedStating)
            return;
        _fieldUpdateValidationAndChangedStating = true;

        EvaluateToRaiseHasChangesEvent();
        EvaluateToRaiseValidationChangesEvent(forceRaiseValidationChangesEvent);

        _fieldUpdateValidationAndChangedStating = false;
    }

    protected virtual void FieldConfigAccessModeHidden()
    {
        Debug.WriteLine($"{FieldLabelText} : FieldConfigAccessModeHidden()");
        Field_ApplyReadOnly();
        if (ButtonUndo != null) ButtonUndo.Hide();
    }

    protected void InitializeLayout() { }
    /// <summary>
    /// Called whenever the data source for a derived field changes. Derived classes typically handle type-specific logic here.
    /// </summary>
    /// <param name="newValue"></param>
    /// <param name="oldValue"></param>
    protected abstract void OnFieldDataSourcePropertyChanged(object newValue, object oldValue);

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (!_fieldIsOriginalValueSet)
        {
            _fieldIsOriginalValueSet = true;
            Field_OriginalValue_SetToCurrentValue();
        }
        Content = Field_CreateLayoutGrid();
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

    public void Field_Clear()
    {
        Field_OriginalValue_SetToClear();
        Field_UpdateValidationAndChangedState();
        Field_Unfocus();
    }

    public void Field_Focused(object? sender, FocusEventArgs e) { }

    public void Field_NotifyHasChanges(bool hasChanged) =>
        FieldHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));

    public void Field_NotifyValidationChanges(bool isValid) =>
        FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));

    public void Field_SaveAndMarkAsReadOnly()
    {
        FieldAccessMode = FieldAccessModeEnum.ViewOnly;
        Field_OriginalValue_SetToCurrentValue();
        Field_ConfigDisabled();
        Field_UpdateValidationAndChangedState();
    }

    public virtual void Field_Unfocus() => base.Unfocus();

    public void Field_Unfocused(object? sender, FocusEventArgs e) { }
    public void Field_UpdateLabelWidth(double newWidth)
    {
        if (Content is Grid grid && grid.ColumnDefinitions.Count > 0)
        {
            grid.ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
            FieldLabel!.WidthRequest = newWidth;
        }
    }

    public void Field_UpdateWidth(double newWidth)
    {
        if (Content is Grid grid && grid.ColumnDefinitions.Count > 1)
            grid.ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
    }

    #endregion Public Methods

}