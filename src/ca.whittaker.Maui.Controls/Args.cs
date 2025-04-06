namespace ca.whittaker.Maui.Controls;


public class FormSavedEventArgs : EventArgs
{
    public FormSavedEventArgs(bool hasChanges) => HasChanges = hasChanges;
    public bool HasChanges { get; }
}

public class HasChangesEventArgs : EventArgs
{
    public HasChangesEventArgs(bool hasChanged) => HasChanged = hasChanged;
    public bool HasChanged { get; }
}
public class HasFormChangesEventArgs : EventArgs
{
    public HasFormChangesEventArgs(FormAccessModeEnum formState) => FormState = formState;
    public FormAccessModeEnum FormState { get; }
}

public class ValidationDataChangesEventArgs : EventArgs
{
    public ValidationDataChangesEventArgs(bool isInvalid) => Invalid = isInvalid;
    public bool Invalid { get; }
}
