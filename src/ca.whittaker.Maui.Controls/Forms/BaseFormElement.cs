using ca.whittaker.Maui.Controls.Buttons;
using System.Text.RegularExpressions;
using Entry = Microsoft.Maui.Controls.Entry;
using Label = Microsoft.Maui.Controls.Label;

namespace ca.whittaker.Maui.Controls.Forms;

/// <summary>
/// Represents a customizable text box control with various properties for text manipulation and validation.
/// </summary>
public abstract class BaseFormElement : ContentView
{
    public static readonly BindableProperty ChangeStateProperty = BindableProperty.Create(
        propertyName: nameof(ChangeState),
        returnType: typeof(ChangeStateEnum),
        declaringType: typeof(BaseFormElement),
        defaultValue: ChangeStateEnum.NotChanged,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        propertyName: nameof(Label),
        returnType: typeof(string),
        declaringType: typeof(BaseFormElement),
        defaultValue: string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            ((BaseFormElement)bindable).OnLabelPropertyChanged(newValue);
        });


    public static readonly BindableProperty LabelWidthProperty = BindableProperty.Create(
        propertyName: nameof(LabelWidth),
        returnType: typeof(double?),
        declaringType: typeof(BaseFormElement),
        defaultValue: (double)100,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            ((BaseFormElement)bindable).OnLabelWidthPropertyChanged(newValue);
        });

    public static readonly BindableProperty ValidationStateProperty = BindableProperty.Create(
        propertyName: nameof(ValidationState),
        returnType: typeof(ValidationStateEnum),
        declaringType: typeof(BaseFormElement),
        defaultValue: ValidationStateEnum.Valid,
        defaultBindingMode: BindingMode.TwoWay);

    // Fields, constants, and regex
    public UndoButton _buttonUndo;
    public Label _label;
    public Label _labelNotification;
    private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;

    public BaseFormElement()
    {
    }

    public event EventHandler<HasChangesEventArgs>? HasChanges;
    public event EventHandler<ValidationDataChangesEventArgs>? HasValidationChanges;
    public ChangeStateEnum ChangeState { get => (ChangeStateEnum)GetValue(ChangeStateProperty); set => SetValue(ChangeStateProperty, value); }
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public double LabelWidth { get => (double)GetValue(LabelWidthProperty); set => SetValue(LabelWidthProperty, value); }


    public ValidationStateEnum ValidationState { get => (ValidationStateEnum)GetValue(ValidationStateProperty); set => SetValue(ValidationStateProperty, value); }


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

    public void RaiseValidationChanges(bool isValid)
    {
        HasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));
    }
    public void RaiseHasChanges(bool hasChanged)
    {
        HasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));
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
            ButtonType = BaseButtonTypeEnum.Undo,
            BorderWidth = 0,
            Margin = new Thickness(0),
            Padding = new Thickness(5, 0, 0, 0)
        };
    }

    protected void OnLabelPropertyChanged(object newValue)
    {
        _label.Text = newValue?.ToString() ?? "";
    }

    protected void OnLabelWidthPropertyChanged(object newValue)
    {
        if (Content is Grid grid)
        {
            grid.ColumnDefinitions[0].Width = new GridLength((double)newValue, GridUnitType.Absolute);
        }
    }

    protected Label CreateLabel()
    {
        return new Label
        {
            Text = Label,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };
    }


    protected void SetLabelText(object newValue)
    {
        _label.Text = newValue == null ? "" : newValue.ToString();
    }

}