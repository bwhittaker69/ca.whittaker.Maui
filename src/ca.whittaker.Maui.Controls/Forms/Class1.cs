//// File: IBaseFormField.cs
//using ca.whittaker.Maui.Controls.Buttons;
//using System;
//using System.Windows.Input;

//namespace ca.whittaker.Maui.Controls.Forms;

///// <summary>
///// Core contract for form field controls inside a Form.
///// </summary>
//public interface IBaseFormField
//{
//    // Access & state
//    FieldAccessModeEnum FieldAccessMode { get; set; }
//    ChangeStateEnum FieldChangeState { get; set; }
//    ValidationStateEnum FieldValidationState { get; set; }
//    bool FieldEnabled { get; set; }
//    bool FieldReadOnly { get; set; }
//    bool FieldUndoButton { get; set; }

//    // Layout
//    string FieldLabelText { get; set; }
//    bool FieldLabelVisible { get; set; }
//    double FieldLabelWidth { get; set; }
//    double FieldWidth { get; set; }
//    LayoutOptions HorizontalOptions { get; set; }
//    LayoutOptions VerticalOptions { get; set; }

//    // Commands
//    ICommand FieldCommand { get; set; }
//    object FieldCommandParameter { get; set; }

//    // Events
//    event EventHandler<HasChangesEventArgs>? FieldHasChanges;
//    event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

//    // Actions
//    void Field_Clear();
//    void Field_SaveAndMarkAsReadOnly();
//    void Field_UndoValue();
//    void Field_Unfocus();
//    void Field_UpdateLabelWidth(double newWidth);
//    void Field_UpdateWidth(double newWidth);
//}

///// <summary>
///// Typed extension for form fields.
///// </summary>
//public interface IBaseFormFieldTyped<T> : IBaseFormField
//{
//    T? FieldDataSource { get; set; }
//}

///// <summary>
///// Abstract base implementing core behavior for form fields.
///// </summary>
//public abstract class BaseFormField<T> : ContentView, IBaseFormFieldTyped<T>
//{
//    #region Bindable Properties Setup
//    private static BindableProperty CreateProp<U>(string name, U defaultValue, BindingMode mode = BindingMode.TwoWay, BindableProperty.BindingPropertyChangedDelegate? changed = null)
//        => BindableProperty.Create(name, typeof(U), typeof(BaseFormField<T>), defaultValue, mode, propertyChanged: changed);

//    public static readonly BindableProperty FieldAccessModeProperty = CreateProp(nameof(FieldAccessMode), FieldAccessModeEnum.ViewOnly, BindingMode.TwoWay, OnAccessModeChanged);
//    public static readonly BindableProperty FieldChangeStateProperty = CreateProp(nameof(FieldChangeState), ChangeStateEnum.NotChanged);
//    public static readonly BindableProperty FieldValidationStateProperty = CreateProp(nameof(FieldValidationState), ValidationStateEnum.Valid, BindingMode.TwoWay);
//    public static readonly BindableProperty FieldEnabledProperty = CreateProp(nameof(FieldEnabled), false, BindingMode.TwoWay, OnEnabledChanged);
//    public static readonly BindableProperty FieldReadOnlyProperty = CreateProp(nameof(FieldReadOnly), false, BindingMode.OneWay);
//    public static readonly BindableProperty FieldUndoButtonProperty = CreateProp(nameof(FieldUndoButton), true, changed: OnUndoVisibilityChanged);
//    public static readonly BindableProperty FieldLabelTextProperty = CreateProp(nameof(FieldLabelText), string.Empty, changed: OnLabelTextChanged);
//    public static readonly BindableProperty FieldLabelVisibleProperty = CreateProp(nameof(FieldLabelVisible), true, changed: OnLabelVisibilityChanged);
//    public static readonly BindableProperty FieldLabelWidthProperty = CreateProp(nameof(FieldLabelWidth), 100d, BindingMode.TwoWay, OnLabelWidthChanged);
//    public static readonly BindableProperty FieldWidthProperty = CreateProp(nameof(FieldWidth), 100d, BindingMode.TwoWay, OnWidthChanged);
//    public static readonly BindableProperty FieldCommandProperty = CreateProp(nameof(FieldCommand), default(ICommand)!);
//    public static readonly BindableProperty FieldCommandParameterProperty = CreateProp(nameof(FieldCommandParameter), default(object)!);
//    public static readonly BindableProperty FieldDataSourceProperty = CreateProp(nameof(FieldDataSource), default(T)!, BindingMode.TwoWay, OnDataSourceChangedStatic);
//    #endregion

//    #region Fields
//    protected const SizeEnum DefaultButtonSize = SizeEnum.Normal;
//    private bool _suppressCallback;
//    private bool _updating;
//    private bool _originalCaptured;
//    private T? _originalValue;
//    private T? _lastValue;

//    public UndoButton FieldButtonUndo { get; }
//    public Label FieldLabel { get; }
//    public Label FieldNotification { get; }
//    #endregion

//    #region Constructors
//    protected BaseFormField()
//    {
//        FieldLabel = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
//        FieldNotification = new Label { VerticalOptions = LayoutOptions.Center, TextColor = Colors.Red, IsVisible = false };
//        FieldButtonUndo = new UndoButton { ButtonIcon = ButtonIconEnum.Undo, ButtonSize = DefaultButtonSize };
//        FieldButtonUndo.Pressed += (_, __) => { if (FieldAccessMode == FieldAccessModeEnum.Editing) Field_UndoValue(); };
//        BaseFieldInitialize();
//    }
//    #endregion

//    #region Public Properties & Events
//    public FieldAccessModeEnum FieldAccessMode { get => (FieldAccessModeEnum)GetValue(FieldAccessModeProperty); set => SetValue(FieldAccessModeProperty, value); }
//    public ChangeStateEnum FieldChangeState { get => (ChangeStateEnum)GetValue(FieldChangeStateProperty); set => SetValue(FieldChangeStateProperty, value); }
//    public ValidationStateEnum FieldValidationState { get => (ValidationStateEnum)GetValue(FieldValidationStateProperty); set => SetValue(FieldValidationStateProperty, value); }
//    public bool FieldEnabled { get => (bool)GetValue(FieldEnabledProperty); set => SetValue(FieldEnabledProperty, value); }
//    public bool FieldReadOnly { get => (bool)GetValue(FieldReadOnlyProperty); set => SetValue(FieldReadOnlyProperty, value); }
//    public bool FieldUndoButton { get => (bool)GetValue(FieldUndoButtonProperty); set => SetValue(FieldUndoButtonProperty, value); }
//    public string FieldLabelText { get => (string)GetValue(FieldLabelTextProperty); set => SetValue(FieldLabelTextProperty, value); }
//    public bool FieldLabelVisible { get => (bool)GetValue(FieldLabelVisibleProperty); set => SetValue(FieldLabelVisibleProperty, value); }
//    public double FieldLabelWidth { get => (double)GetValue(FieldLabelWidthProperty); set => SetValue(FieldLabelWidthProperty, value); }
//    public double FieldWidth { get => (double)GetValue(FieldWidthProperty); set => SetValue(FieldWidthProperty, value); }
//    public ICommand FieldCommand { get => (ICommand)GetValue(FieldCommandProperty); set => SetValue(FieldCommandProperty, value); }
//    public object FieldCommandParameter { get => GetValue(FieldCommandParameterProperty)!; set => SetValue(FieldCommandParameterProperty, value); }
//    public T? FieldDataSource { get => (T?)GetValue(FieldDataSourceProperty); set => SetValue(FieldDataSourceProperty, value); }

//    public event EventHandler<HasChangesEventArgs>? FieldHasChanges;
//    public event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;
//    #endregion

//    #region Abstract Members
//    protected abstract List<View> Field_ControlView();
//    protected abstract Grid Field_CreateLayoutGrid();
//    protected abstract T? Field_GetCurrentValue();
//    protected abstract void Field_SetValue(T? value);
//    protected abstract bool Field_HasChangedFromOriginal();
//    protected abstract bool Field_HasFormatError();
//    protected abstract bool Field_HasRequiredError();
//    protected abstract string Field_GetFormatErrorMessage();
//    protected abstract void UpdateRow0Layout();
//    #endregion

//    #region Lifecycle & Initialization
//    protected override void OnParentSet()
//    {
//        base.OnParentSet();
//        Content = Field_CreateLayoutGrid();
//        Field_SetValue(FieldDataSource);
//        ApplyAccessMode(FieldAccessMode);
//    }

//    private void BaseFieldInitialize()
//    {
//        foreach (var v in Field_ControlView())
//        {
//            v.Focused += (_, e) => { };
//            v.Unfocused += (_, e) => { };
//        }
//        _originalValue = FieldDataSource;
//        Field_SetValue(FieldDataSource);
//    }
//    #endregion

//    #region Bindable Callbacks
//    private static void OnAccessModeChanged(BindableObject b, object o, object n) => ((BaseFormField<T>)b).ApplyAccessMode((FieldAccessModeEnum)n);
//    private static void OnEnabledChanged(BindableObject b, object o, object n) => ((BaseFormField<T>)b).ApplyEnabled((bool)n);
//    private static void OnLabelTextChanged(BindableObject b, object o, object n) => ((BaseFormField<T>)b).FieldLabel.Text = n?.ToString();
//    private static void OnLabelVisibilityChanged(BindableObject b, object o, object n) => ((BaseFormField<T>)b).FieldLabel.IsVisible = (bool)n;
//    private static void OnLabelWidthChanged(BindableObject b, object o, object n) => ((BaseFormField<T>)b).Field_UpdateLabelWidth((double)n);
//    private static void OnWidthChanged(BindableObject b, object o, object n) => ((BaseFormField<T>)b).Field_UpdateWidth((double)n);
//    private static void OnUndoVisibilityChanged(BindableObject b, object o, object n) => ((BaseFormField<T>)b).FieldButtonUndo.IsVisible = (bool)n;
//    private static void OnDataSourceChangedStatic(BindableObject b, object o, object n)
//    {
//        var f = (BaseFormField<T>)b;
//        if (!f._originalCaptured)
//        {
//            f._originalCaptured = true;
//            f._originalValue = (T?)n;
//        }
//        else if (!f._suppressCallback)
//        {
//            f.Field_SetValue((T?)n);
//            f._lastValue = (T?)n;
//            f.UpdateState();
//        }
//        f._suppressCallback = false;
//    }
//    #endregion

//    #region State Management
//    private void UpdateState()
//    {
//        if (_updating) return;
//        _updating = true;

//        bool changed = Field_HasChangedFromOriginal();
//        FieldChangeState = changed ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
//        FieldHasChanges?.Invoke(this, new HasChangesEventArgs(changed));

//        bool valid = !Field_HasFormatError() && !Field_HasRequiredError();
//        FieldValidationState = valid ? ValidationStateEnum.Valid : ValidationStateEnum.FormatError;
//        FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!valid));

//        FieldButtonUndo.IsVisible = FieldAccessMode == FieldAccessModeEnum.Editing && changed;
//        _updating = false;
//    }

//    private void ApplyAccessMode(FieldAccessModeEnum mode)
//    {
//        FieldNotification.IsVisible = false;
//        switch (mode)
//        {
//            case FieldAccessModeEnum.ViewOnly:
//            case FieldAccessModeEnum.Hidden:
//                DisableControls();
//                FieldButtonUndo.Hide();
//                break;
//            case FieldAccessModeEnum.Editable:
//                DisableControls();
//                FieldButtonUndo.Hide();
//                break;
//            case FieldAccessModeEnum.Editing:
//                EnableControls();
//                break;
//        }
//    }

//    private void ApplyEnabled(bool enabled) { if (enabled) EnableControls(); else DisableControls(); }
//    private void EnableControls() => ControlVisualHelper.EnableDescendantControls(this, FieldLabelVisible);
//    private void DisableControls() => ControlVisualHelper.DisableDescendantControls(this, FieldLabelVisible);
//    #endregion

//    #region Public Methods
//    public void Field_Clear() => Field_SetValue(default(T));
//    public void Field_SaveAndMarkAsReadOnly()
//    {
//        _originalValue = Field_GetCurrentValue();
//        ApplyAccessMode(FieldAccessModeEnum.ViewOnly);
//    }
//    public void Field_UndoValue() => Field_SetValue(_originalValue);
//    public void Field_Unfocus() => base.Unfocus();
//    public void Field_UpdateLabelWidth(double w) => Field_UpdateLabelWidth(w);
//    public void Field_UpdateWidth(double w) => Field_UpdateWidth(w);
//    #endregion

//    #region Notification Helpers
//    public void Field_NotifyHasChanges(bool hasChanged) => FieldHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
//    public void Field_NotifyValidationChanges(bool isValid) => FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));
//    #endregion
//}
