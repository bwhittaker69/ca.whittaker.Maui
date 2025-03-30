namespace ca.whittaker.Maui.Controls;


public class HasChangesEventArgs : EventArgs
{
    public HasChangesEventArgs(bool hasChanged) => HasChanged = hasChanged;
    public bool HasChanged { get; }
}
public class HasFormChangesEventArgs : EventArgs
{
    public HasFormChangesEventArgs(FormStateEnum formState) => FormState = formState;
    public FormStateEnum FormState { get; }
}

public class ValidationDataChangesEventArgs : EventArgs
{
    public ValidationDataChangesEventArgs(bool isInvalid) => Invalid = isInvalid;
    public bool Invalid { get; }
}
