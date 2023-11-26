using Microsoft.Maui.Controls;

namespace ca.whittaker.Maui.Controls.Buttons;

public class SubmitButton : ButtonBase
{
    public static readonly BindableProperty SubmitButtonStateProperty = BindableProperty.Create(
        propertyName: nameof(SubmitButtonState),
        returnType: typeof(ButtonStateEnum),
        declaringType: typeof(ButtonBase),
        defaultValue: ButtonStateEnum.Disabled,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: OnPropertyChanged);

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
            if (bindable is ButtonBase buttonBase)
            {
                buttonBase.ConfigureButton((ButtonStateEnum)newValue);
            }
    }

    public ButtonStateEnum SubmitButtonState
    {
        get => (ButtonStateEnum)GetValue(SubmitButtonStateProperty);
        set => SetValue(SubmitButtonStateProperty, value);
    }

    private void ParentForm_HasChanges(object? sender, HasFormChangesEventArgs e)
    {
        switch (e.FormState)
        {
            case FormStateEnum.Enabled:
                ConfigureButton(ButtonStateEnum.Enabled);
                break;
            case FormStateEnum.Disabled:
                ConfigureButton(ButtonStateEnum.Disabled);
                break;
            case FormStateEnum.Hidden:
                SubmitButtonState = ButtonStateEnum.Hidden;
                break;
        }
    }

    public SubmitButton() : base()
    {
    }
}
