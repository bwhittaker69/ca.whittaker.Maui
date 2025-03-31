using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

using ca.whittaker.Maui.Controls.Buttons;
using Label = Microsoft.Maui.Controls.Label;
using System.Windows.Input;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Forms;


public interface IBaseFormField
{
    FieldAccessModeEnum FieldAccessMode { get; set; }
    ChangeStateEnum FieldChangeState { get; set; }
    ICommand FieldCommand { get; set; }
    object FieldCommandParameter { get; set; }
    string FieldLabelText { get; set; }
    double FieldLabelWidth { get; set; }
    bool FieldMandatory { get; set; }
    ValidationStateEnum FieldValidationState { get; set; }
    double FieldWidth { get; set; }
    LayoutOptions HorizontalOptions { get; set; }
    LayoutOptions VerticalOptions { get; set; }

    event EventHandler<HasChangesEventArgs>? FieldHasChanges;
    event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

    void FieldClear();
    void FieldMarkAsEditable();
    void FieldMarkAsReadOnly();
    void FieldNotifyHasChanges(bool hasChanged);
    void FieldNotifyValidationChanges(bool isValid);
    void FieldSavedAndMarkAsReadOnly();
    void FieldUnfocus();
    void FieldUpdateLabelWidth(double newWidth);
    void FieldUpdateWidth(double newWidth);
}

/// <summary>
/// Represents the base class for customizable data capture controls. This abstract class provides common properties,
/// events, and helper methods used by derived form elements to simplify their implementation.
/// 
/// Core Features:
/// - Inherits from ContentView to support UI composition.
/// - Defines bindable properties for:
///   • ChangeState: Tracks whether the control's data has changed.
///   • Label: The text label for the control.
///   • LabelWidth: The width allocated for the label.
///   • ValidationState: Indicates the result of data validation.
/// - Provides methods to raise events when data changes (HasChanges) or validation errors occur (HasValidationChanges).
/// - Includes helper methods for creating standard UI sub-elements:
///   • CreateNotificationLabel: Generates a centered, red notification label for validation messages.
///   • CreateUndoButton: Creates a button (UndoButton) used to revert changes.
///   • CreateLabel: Generates a label configured for field titles.
///   • UpdateLabelWidth: Adjusts the label’s width in the Grid layout.
/// 
/// Intended Usage:
/// Derived classes combine these common elements (often arranged in a Grid layout) to form composite controls.
/// For example, a typical layout might include a field label, an input element, an undo button, and a notification label,
/// all of which are managed by the functionality provided in this base class.
/// 
/// Grid Layout Considerations:
/// While BaseFormElement does not enforce a specific layout, many derived controls use a Grid layout to organize elements:
/// +-------------------+
/// | FieldLabel        |
/// +-------------------+
/// | (Derived UI)      |
/// +-------------------+
/// | FieldNotification |
/// +-------------------+
/// 
/// This class centralizes common behavior to promote consistency and reduce duplication across custom form controls.
/// </summary>
public abstract class BaseFormField : ContentView, IBaseFormField
{

    #region Private Fields

    private const SizeEnum FieldUndoButtonSize = SizeEnum.XXSmall;
    private bool _fieldEvaluateToRaiseHasChangesEventing = false;
    private bool _fieldPreviousInvalidDataState = false;

    #endregion Private Fields

    #region Protected Fields

    protected const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;
    protected bool _fieldDisabling = false;
    protected bool _fieldEnabling = false;
    protected bool _fieldEvaluateToRaiseValidationChangesEventing = false;
    protected bool _fieldIsOriginalValueSet = false;
    protected bool _fieldUpdateValidationAndChangedStating = false;
    protected bool _fieldUpdatingUI = false;
    protected bool _onFieldDataSourcePropertyChanging = false;
    protected bool _previousHasChangedState = false;
    protected bool _undoing = false;

    #endregion Protected Fields

    #region Public Fields

    public static readonly BindableProperty FieldAccessModeProperty = BindableProperty.Create(
        propertyName: nameof(FieldAccessMode),
        returnType: typeof(FieldAccessModeEnum),
        declaringType: typeof(BaseFormField),
        defaultValue: FieldAccessModeEnum.ReadOnly,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldAccessModePropertyChanged);

    public static readonly BindableProperty FieldChangeStateProperty = BindableProperty.Create(
            propertyName: nameof(FieldChangeState),
        returnType: typeof(ChangeStateEnum),
        declaringType: typeof(BaseFormField),
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

    public static readonly BindableProperty FieldLabelTextProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelText),
        returnType: typeof(string),
        declaringType: typeof(BaseFormField),
        defaultValue: string.Empty,
        propertyChanged: OnFieldLabelTextPropertyChanged);

    public static readonly BindableProperty FieldLabelWidthProperty = BindableProperty.Create(
        propertyName: nameof(FieldLabelWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormField),
        defaultValue: 100d,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldLabelWidthPropertyChanged);

    public static readonly BindableProperty FieldMandatoryProperty = BindableProperty.Create(
        propertyName: nameof(FieldMandatory),
        returnType: typeof(bool),
        declaringType: typeof(BaseFormField),
        defaultValue: false);

    public static readonly BindableProperty FieldValidationStateProperty = BindableProperty.Create(
        propertyName: nameof(FieldValidationState),
        returnType: typeof(ValidationStateEnum),
        declaringType: typeof(BaseFormField),
        defaultValue: ValidationStateEnum.Valid,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty FieldWidthProperty = BindableProperty.Create(
                        propertyName: nameof(FieldWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormField),
        defaultValue: 100d,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnFieldWidthPropertyChanged);

    public UndoButton? ButtonUndo;

    public Label? FieldLabel;

    public Label? FieldNotification;

    #endregion Public Fields

    #region Public Constructors

    // Constructor
    public BaseFormField()
    {
    }

    #endregion Public Constructors

    #region Public Events

    public event EventHandler<HasChangesEventArgs>? FieldHasChanges;

    public event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

    #endregion Public Events

    #region Public Properties

    public FieldAccessModeEnum FieldAccessMode
    {
        get => (FieldAccessModeEnum)GetValue(FieldAccessModeProperty);
        set => SetValue(FieldAccessModeProperty, value);
    }

    // Properties
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

    public string FieldLabelText
    {
        get => (string)GetValue(FieldLabelTextProperty);
        set => SetValue(FieldLabelTextProperty, value);
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
            ((Grid)this.Children[0]).HorizontalOptions = value;
            base.HorizontalOptions = value;
        }
    }

    public new LayoutOptions VerticalOptions
    {
        get => base.VerticalOptions;
        set
        {
            ((Grid)Content).VerticalOptions = value;
            base.VerticalOptions = value;
        }
    }

    #endregion Public Properties

    #region Private Methods

    private static void OnFieldAccessModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField element && newValue is FieldAccessModeEnum newAccessMode)
        {
            void _onFieldAccessModePropertyChanged_UpdateUI()
            {
                if (oldValue != newValue)
                {
                    Debug.WriteLine($"{oldValue}-{newValue} : OnFieldAccessModePropertyChanged()");
                    if (newAccessMode == FieldAccessModeEnum.ReadOnly)
                    {
                        if (element.ButtonUndo != null)
                        {
                            element.ButtonUndo.Disabled();
                        }
                    }
                    else if (newAccessMode == FieldAccessModeEnum.Editable)
                    {
                        if (element.ButtonUndo != null)
                        {
                            element.ButtonUndo.Enabled();
                        }
                    }
                    else if (newAccessMode == FieldAccessModeEnum.Hidden)
                    {
                        if (element.ButtonUndo != null)
                        {
                            element.ButtonUndo.Hide();
                        }
                    }
                    //element.RefreshFormState();
                }
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _onFieldAccessModePropertyChanged_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _onFieldAccessModePropertyChanged_UpdateUI());
            }
        }
    }

    private static void OnFieldLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField element)
        {
            if (element != null && element.FieldLabel != null)
                element.FieldLabel.Text = newValue?.ToString() ?? "";
        }
    }

    private static void OnFieldLabelWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField element && element.Content is Grid grid)
        {
            element.FieldUpdateLabelWidth((double)newValue);
        }
    }

    private static void OnFieldWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormField element && element.Content is Grid grid)
        {
            element.FieldUpdateWidth((double)newValue);
        }
    }

    #endregion Private Methods

    #region Protected Methods

    // Methods
    protected static Label FieldCreateNotificationLabel()
    {
        return new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false,
            TextColor = Colors.Red
        };
    }

    protected static UndoButton FieldCreateUndoButton(FieldAccessModeEnum fieldAccessMode)
    {
        return new UndoButton
        {
            Text = "",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
            ButtonSize = FieldUndoButtonSize,
            WidthRequest = -1,
            ButtonState = ButtonStateEnum.Disabled,
            ButtonType = ImageResourceEnum.Undo,
            BorderWidth = 0,
            Margin = new Thickness(0),
            Padding = new Thickness(5, 0, 0, 0),
            IsVisible = fieldAccessMode == FieldAccessModeEnum.Editable
        };
    }

    protected void Field_Focused(object? sender, FocusEventArgs e)
    {
    }

    protected void Field_Unfocused(object? sender, FocusEventArgs e)
    {
        FieldUpdateNotificationMessage();
    }

    protected ValidationStateEnum FieldCalculateValidationState()
    {
        if (FieldHasRequiredError())
        {
            return ValidationStateEnum.RequiredFieldError;
        }
        else if (FieldHasFormatError())
        {
            return ValidationStateEnum.FormatError;
        }
        return ValidationStateEnum.Valid;
    }

    protected Label FieldCreateLabel()
    {
        return new Label
        {
            Text = FieldLabelText,
            HorizontalOptions = LayoutOptions.Start,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center, // Ensure vertical centering
            HeightRequest = -1
        };
    }

    protected abstract Grid FieldCreateLayoutGrid();

    protected abstract void FieldDisable();

    protected abstract void FieldEnable();

    protected void FieldEvaluateToRaiseHasChangesEvent()
    {
        if (_fieldEvaluateToRaiseHasChangesEventing) return;
        _fieldEvaluateToRaiseHasChangesEventing = true;
        Debug.WriteLine($"{FieldLabel!.Text}: FieldEvaluateToRaiseHasChangesEvent()");
        bool hasChanged = FieldHasChanged();
        if (_previousHasChangedState != hasChanged)
        {
            void _fieldEvaluateToRaiseHasChangesEvent_UpdateUI()
            {
                if (_fieldUpdatingUI) return;
                _fieldUpdatingUI = true;
                using (ResourceHelper resourceHelper = new())
                {
                    if (ButtonUndo != null)
                    {
                        if (FieldAccessMode == FieldAccessModeEnum.Editable)
                        {
                            //ButtonUndo.ImageSource = resourceHelper.GetImageSource(
                            //                                            buttonState: hasChanged ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled,
                            //                                            baseButtonType: ImageResourceEnum.Undo,
                            //                                            sizeEnum: cUndoButtonSize);
                            if (hasChanged)
                                ButtonUndo.Enabled();
                            else
                                ButtonUndo.Disabled();
                        }
                        else
                        {
                            ButtonUndo.Hide();
                        }
                    }
                }
                _previousHasChangedState = hasChanged;
                FieldChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
                FieldNotifyHasChanges(hasChanged);
                _fieldUpdatingUI = false;
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _fieldEvaluateToRaiseHasChangesEvent_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _fieldEvaluateToRaiseHasChangesEvent_UpdateUI());
            }
        }
        _fieldEvaluateToRaiseHasChangesEventing = false;
    }

    protected void FieldEvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
    {
        if (_fieldEvaluateToRaiseValidationChangesEventing) return;
        _fieldEvaluateToRaiseValidationChangesEventing = true;
        Debug.WriteLine($"{FieldLabel!.Text}: FieldEvaluateToRaiseValidationChangesEvent(forceRaise: {forceRaise})");
        bool isValid = FieldHasValidData();
        if (_fieldPreviousInvalidDataState != isValid || forceRaise)
        {
            void _evaluateToRaiseValidationChangesEvent_UpdateUI()
            {
                _fieldPreviousInvalidDataState = isValid;
                FieldValidationState = isValid ? ValidationStateEnum.Valid : ValidationStateEnum.FormatError;
                FieldNotifyValidationChanges(isValid);
            }

            // Check if on the main thread and update UI accordingly
            if (MainThread.IsMainThread)
            {
                _evaluateToRaiseValidationChangesEvent_UpdateUI();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _evaluateToRaiseValidationChangesEvent_UpdateUI());
            }
        }
        _fieldEvaluateToRaiseValidationChangesEventing = false;
    }

    protected abstract string FieldGetFormatErrorMessage();

    protected abstract bool FieldHasChanged();

    protected abstract bool FieldHasFormatError();

    protected abstract bool FieldHasRequiredError();

    protected abstract bool FieldHasValidData();

    protected void FieldUpdateNotificationMessage()
    {
        var validationState = FieldCalculateValidationState();
        string notificationMessage = "";
        bool isNotificationVisible = false;

        switch (validationState)
        {
            case ValidationStateEnum.RequiredFieldError:
                notificationMessage = "Field is required.";
                isNotificationVisible = true;
                break;

            case ValidationStateEnum.FormatError:
                notificationMessage = FieldGetFormatErrorMessage();
                isNotificationVisible = true;
                break;
        }
        if (FieldNotification != null)
        {
            FieldNotification.Text = notificationMessage;
            FieldNotification.IsVisible = isNotificationVisible;
        }

        FieldValidationState = validationState; // Set the validation state based on calculated value
    }

    protected void FieldUpdateValidationAndChangedState()
    {
        if (_fieldUpdateValidationAndChangedStating) return;
        _fieldUpdateValidationAndChangedStating = true;
        Debug.WriteLine($"{FieldLabel!.Text}: FieldUpdateValidationAndChangedState()");
        FieldEvaluateToRaiseHasChangesEvent();
        FieldEvaluateToRaiseValidationChangesEvent();
        _fieldUpdateValidationAndChangedStating = false;
    }

    // Events
    protected abstract void OnFieldButtonUndoPressed(object? sender, EventArgs e);
    protected abstract void OnFieldDataSourcePropertyChanged(object newValue, object oldValue);
    protected void OnFieldSizeRequestChanged(double newValue)
    {
        HeightRequest = newValue;
        WidthRequest = newValue;
    }

    #endregion Protected Methods

    #region Public Methods

    public abstract void FieldClear();
    public abstract void FieldMarkAsEditable();
    public abstract void FieldMarkAsReadOnly();
    public abstract void FieldHide();
    protected abstract void FieldUnhide();

    public void FieldNotifyHasChanges(bool hasChanged)
    {
        FieldHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
    }

    public void FieldNotifyValidationChanges(bool isValid)
    {
        FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));
    }

    public abstract void FieldSavedAndMarkAsReadOnly();
    public virtual void FieldUnfocus()
    {
        base.Unfocus();
    }
    public void FieldUpdateLabelWidth(double newWidth)
    {
        ((Grid)Content).ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
        if (FieldLabel != null)
            FieldLabel.WidthRequest = newWidth;
    }

    public void FieldUpdateWidth(double newWidth)
    {
        ((Grid)Content).ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
    }

    #endregion Public Methods

}
