

using ca.whittaker.Maui.Controls.Buttons;

namespace ca.whittaker.Maui.Controls.Forms;



public class Form : ContentView
{

    // ChangeState Bindable Property
    public static readonly BindableProperty FormStateProperty = BindableProperty.Create(
        propertyName: nameof(FormState),
        returnType: typeof(FormStateEnum),
        declaringType: typeof(TextBox),
        defaultValue: FormStateEnum.Disabled,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnPropertyChanged);

    //
    // used to update the UI when the property changes
    //
    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
        { 
            if ((FormStateEnum)newValue == FormStateEnum.Reset)
            {
                ((Form)bindable).ResetChangeState();
                ((Form)bindable).FormState = FormStateEnum.Enabled;
            }
            ((Form)bindable).CalcSubmitButtonState();
        }
    }

    public FormStateEnum FormState {
        get => (FormStateEnum)GetValue(FormStateProperty);
        set => SetValue(FormStateProperty, value);
    }

    private bool HasNoErrors = false;
    private bool HasNoChanges = false;

    public Form()
    {
    }
    protected override void OnParentSet()
    {
        base.OnParentSet();
        WireUpControls();
    }

    private void CalcSubmitButtonState()
    {
        var submitButton = GetSubmitButton();
        switch (FormState)
        {
            case FormStateEnum.Enabled:
                IsEnabled = true;
                IsVisible = true;
                if (HasNoChanges)
                    submitButton.SetButtonState(ButtonStateEnum.Disabled);
                else
                {
                    if (HasNoErrors)
                        submitButton.SetButtonState(ButtonStateEnum.Enabled);
                    else
                        submitButton.SetButtonState(ButtonStateEnum.Disabled);
                }
                break;
            case FormStateEnum.Disabled:
                IsEnabled = false;
                submitButton.SetButtonState(ButtonStateEnum.Hidden);
                break;
            case FormStateEnum.Hidden:
                IsVisible = false;
                submitButton.SetButtonState(ButtonStateEnum.Hidden);
                break;
        }
    }

    private FormStateEnum CalcFormState()
    {
        if (!IsVisible)
        {
            return FormStateEnum.Hidden;
        }
        else if (!IsEnabled)
        {
            return FormStateEnum.Disabled;
        }
        else
        {
            return FormStateEnum.Enabled;
        }
    }

    private void WireUpControls()
    {
        var controlCount = this.Content.GetVisualTreeDescendants().Count;
        if (controlCount == 0) throw new InvalidOperationException("Form missing controls");

        foreach (var customTextBox in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            customTextBox.HasChanges += CustomTextBox_HasChanges;
            customTextBox.HasValidationChanges += CustomTextBox_HasValidationChanges;
        }
        return;
    }

    private void CustomTextBox_HasValidationChanges(object? sender, ValidationDataChangesEventArgs e) => EvaluateForm();
    private void CustomTextBox_HasChanges(object? sender, HasChangesEventArgs e) => EvaluateForm();

    private void EvaluateForm()
    {
        HasNoErrors = IsFormDataValid();
        HasNoChanges = HasFormNotChanged();
        FormState = CalcFormState();
        CalcSubmitButtonState();
    }

    private SaveButton GetSubmitButton() =>
        this.GetVisualTreeDescendants().OfType<SaveButton>().FirstOrDefault()
        ?? throw new InvalidOperationException("Form missing a submit button");

    private bool IsFormDataValid() =>
        this.GetVisualTreeDescendants().OfType<TextBox>().All(ctb => ctb.ValidationState == ValidationStateEnum.Valid);
    private void ResetChangeState()
    {
        foreach (TextBox t in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            t.ResetChangeState();
        }
    }
    private void ClearForm()
    {
        foreach (TextBox t in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            t.Clear();
        }
    }
    private bool HasFormNotChanged() 
    {
        foreach (TextBox t in this.GetVisualTreeDescendants().OfType<TextBox>())
        {
            Console.WriteLine(t.ChangeState.ToString());
            if (t.ChangeState == ChangeStateEnum.Changed)
                return false;
        }
        return true;
    }
}
