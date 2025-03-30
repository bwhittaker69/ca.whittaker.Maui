using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

using ca.whittaker.Maui.Controls.Buttons;
using Label = Microsoft.Maui.Controls.Label;

namespace ca.whittaker.Maui.Controls.Forms;

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
public abstract class BaseFormElement : ContentView
{
    // Bindable properties
    public static readonly BindableProperty ChangeStateProperty =
        BindableProperty.Create(nameof(ChangeState), typeof(ChangeStateEnum), typeof(BaseFormElement), ChangeStateEnum.NotChanged, BindingMode.TwoWay);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(BaseFormElement), string.Empty, propertyChanged: OnLabelPropertyChanged);

    public static readonly BindableProperty LabelWidthProperty =
        BindableProperty.Create(nameof(LabelWidth), typeof(double?), typeof(BaseFormElement), 100d, BindingMode.TwoWay, propertyChanged: OnLabelWidthPropertyChanged);

    public static readonly BindableProperty FieldWidthProperty =
        BindableProperty.Create(nameof(FieldWidth), typeof(double?), typeof(BaseFormElement), 100d, BindingMode.TwoWay, propertyChanged: OnFieldWidthPropertyChanged);

    public static readonly BindableProperty ValidationStateProperty =
        BindableProperty.Create(nameof(ValidationState), typeof(ValidationStateEnum), typeof(BaseFormElement), ValidationStateEnum.Valid, BindingMode.TwoWay);


    public new LayoutOptions HorizontalOptions
    {
        get => base.HorizontalOptions;
        set 
        {
            ((Grid)this.Children[0]).HorizontalOptions = value;
            //((Grid)Content).HorizontalOptions = value;
            base.HorizontalOptions = value; 
        }
    }

    public new LayoutOptions VerticalOptions
    {
        get => base.VerticalOptions;
        set
        {
            ((Grid)this.Children[0]).VerticalOptions = value;
            //((Grid)Content).VerticalOptions = value;
            base.VerticalOptions = value;
        }
    }


    public UndoButton? ButtonUndo;
    public Label? FieldLabel;
    public Label? FieldNotification;
    private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;
    // Constructor
    public BaseFormElement()
    {
    }

    // Events
    public event EventHandler<HasChangesEventArgs>? HasChanges;

    public event EventHandler<ValidationDataChangesEventArgs>? HasValidationChanges;

    // Properties
    public ChangeStateEnum ChangeState
    {
        get => (ChangeStateEnum)GetValue(ChangeStateProperty);
        set => SetValue(ChangeStateProperty, value);
    }
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
    public double LabelWidth
    {
        get => (double)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }
    public double FieldWidth
    {
        get => (double)GetValue(FieldWidthProperty);
        set => SetValue(FieldWidthProperty, value);
    }
    public ValidationStateEnum ValidationState
    {
        get => (ValidationStateEnum)GetValue(ValidationStateProperty);
        set => SetValue(ValidationStateProperty, value);
    }
    public void RaiseHasChanges(bool hasChanged)
    {
        HasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
    }
    public void RaiseValidationChanges(bool isValid)
    {
        HasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));
    }
    public virtual new void Unfocus()
    {
        base.Unfocus();
    }
    public void UpdateFieldWidth(double newWidth)
    {
        ((Grid)Content).ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
    }
    public void UpdateLabelWidth(double newWidth)
    {
        ((Grid)Content).ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
        if (FieldLabel != null)
            FieldLabel.WidthRequest = newWidth;
    }

    // Methods
    protected static Label CreateNotificationLabel()
    {
        return new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false,
            TextColor = Colors.Red
        };
    }

    protected static UndoButton CreateUndoButton()
    {
        return new UndoButton
        {
            Text = "",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
            ButtonSize = cUndoButtonSize,
            WidthRequest = -1,
            ButtonState = ButtonStateEnum.Disabled,
            ButtonType = ImageResourceEnum.Undo,
            BorderWidth = 0,
            Margin = new Thickness(0),
            Padding = new Thickness(5, 0, 0, 0)
        };
    }

    protected Label CreateLabel()
    {
        return new Label
        {
            Text = Label,
            HorizontalOptions = LayoutOptions.Start,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center, // Ensure vertical centering
            HeightRequest = -1
        };
    }

    protected void OnSizeRequestChanged(double newValue)
    {
        HeightRequest = newValue;
        WidthRequest = newValue;
    }

    private static void OnLabelPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormElement element)
        {
            if (element != null && element.FieldLabel != null)
                element.FieldLabel.Text = newValue?.ToString() ?? "";
        }
    }

    private static void OnLabelWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormElement element && element.Content is Grid grid)
        {
            element.UpdateLabelWidth((double)newValue);
        }
    }

    private static void OnFieldWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BaseFormElement element && element.Content is Grid grid)
        {
            element.UpdateFieldWidth((double)newValue);
        }
    }
}
