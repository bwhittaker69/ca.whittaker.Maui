using System;

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
        if (bindable is ButtonBase buttonBase && newValue is ButtonStateEnum newState)
        {
            // Check if the new value is different from the old value
            if (!Equals(oldValue, newValue))
            {
                buttonBase.ButtonState = newState;
            }
        }
    }

    public ButtonStateEnum SaveButtonState
    {
        get => (ButtonStateEnum)GetValue(SaveButtonStateProperty);
        set => SetValue(SaveButtonStateProperty, value);
    }

    private void ParentForm_HasChanges(object? sender, HasFormChangesEventArgs e)
    {
        if (e == null)
        {
            // Handle null argument appropriately
            return;
        }

        try
        {
            switch (e.FormState)
            {
                case FormStateEnum.Enabled:
                    ButtonState = ButtonStateEnum.Enabled;
                    break;
                case FormStateEnum.Disabled:
                    ButtonState = ButtonStateEnum.Disabled;
                    break;
                case FormStateEnum.Hidden:
                    ButtonState = ButtonStateEnum.Hidden;
                    break;
            }
        }
        catch
        {
            // Handle exceptions
            // Log the exception or take appropriate actions
        }
    }

    public SaveButton() : base(BaseButtonTypeEnum.Save)
    {
    }
}
