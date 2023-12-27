using ca.whittaker.Maui.Controls.Buttons;
using Label = Microsoft.Maui.Controls.Label;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents the base class for customizable controls with various properties for data capture.
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

    public static readonly BindableProperty ValidationStateProperty =
        BindableProperty.Create(nameof(ValidationState), typeof(ValidationStateEnum), typeof(BaseFormElement), ValidationStateEnum.Valid, BindingMode.TwoWay);

    public UndoButton ButtonUndo;
    public Label FieldLabel;
    public Label FieldNotification;
    private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;
    // Constructor
    public BaseFormElement()
    {
    }

    // Events
    public event EventHandler<HasChangesEventArgs> HasChanges;

    public event EventHandler<ValidationDataChangesEventArgs> HasValidationChanges;

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

    public void UpdateLabelWidth(double newWidth)
    {
        ((Grid)Content).ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
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
            VerticalOptions = LayoutOptions.CenterAndExpand, // Ensure vertical centering
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

}
