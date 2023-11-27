namespace ca.whittaker.Maui.Controls.Buttons;

public class SaveButton : ButtonBase
{
    public static readonly BindableProperty SaveButtonStateProperty = BindableProperty.Create(
        propertyName: nameof(SaveButtonState),
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
                buttonBase.SetButtonState((ButtonStateEnum)newValue);
            }
    }

    public ButtonStateEnum SaveButtonState
    {
        get => (ButtonStateEnum)GetValue(SaveButtonStateProperty);
        set => SetValue(SaveButtonStateProperty, value);
    }

    private void ParentForm_HasChanges(object? sender, HasFormChangesEventArgs e)
    {
        switch (e.FormState)
        {
            case FormStateEnum.Enabled:
                SetButtonState(ButtonStateEnum.Enabled);
                break;
            case FormStateEnum.Disabled:
                SetButtonState(ButtonStateEnum.Disabled);
                break;
            case FormStateEnum.Hidden:
                SaveButtonState = ButtonStateEnum.Hidden;
                break;
        }
    }

    public SaveButton() : base(BaseButtonTypeEnum.Save)
    {
    }
}
