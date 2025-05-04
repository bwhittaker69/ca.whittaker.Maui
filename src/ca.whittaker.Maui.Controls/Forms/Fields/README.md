# Custom `BaseFormField<T>` Field Development Guide

When you derive a new control from `BaseFormField<T>`, follow these guidelines to ensure two-way binding, change-tracking, validation, undo/reset and access-mode support.

---

## 1. Define the Input View  
Override the core UI element:  
```csharp
protected override View Field_ControlView()
{
    // return your Entry, DatePicker, Switch, etc.
    return _myControl;
}
```

---

## 2. Build the Layout Grid  
Compose the label, control, Undo button and notification row:  
```csharp
protected override Grid Field_CreateLayoutGrid()
{
    var grid = new Grid
    {
        RowDefinitions    = { /* … */ },
        ColumnDefinitions = { /* … */ }
    };
    grid.Add(Label,              0, 0);
    grid.Add(_myControl,         1, 0);
    grid.Add(UndoButton,         2, 0);
    grid.Add(NotificationLabel,  0, 1, 3, 1);
    return grid;
}
```

---

## 3. Initialize Early  
In your constructor, set the VM value **before** calling `Initialize()` so the “original” capture is accurate:  
```csharp
public MyField()
{
    FieldDataSource = defaultValueFromVM;
    Initialize();
}
```

---

## 4. Implement `Field_SetValue(...)`  
Update the control’s UI on the main thread inside a batch update:  
```csharp
protected override void Field_SetValue(T? value)
{
    UiThreadHelper.RunOnMainThread(() =>
        Field_PerformBatchUpdate(() =>
            _myControl.Value = value
        )
    );
}
```

---

## 5. Suppress Static Callbacks in `Field_SetDataSourceValue(...)`  
Prevent one-time original capture when pushing back to the VM:  
```csharp
protected override void Field_SetDataSourceValue(T? newValue)
{
    FieldSuppressDataSourceCallback = true;
    SetValue(FieldDataSourceProperty, newValue);
    // update your UI widget…
    Field_UpdateValidationAndChangedState();
}
```

---

## 6. Wire UI Events  
In your control’s event handler, push changes through the base logic and update `FieldLastValue`:  
```csharp
private void OnControlValueChanged(object sender, EventArgs e)
{
    var newValue = _myControl.GetValue();
    Field_SetDataSourceValue(newValue);
    FieldLastValue = newValue;
}
```

---

## 7. Override Change-Detection Methods  
Compare `FieldOriginalValue`/`FieldLastValue` against the control’s current value:  
```csharp
protected override bool Field_HasChangedFromOriginal()
    => !EqualityComparer<T?>.Default.Equals(
        FieldOriginalValue,
        CurrentControlValue
    );
```

---

## 8. Override Validation Methods  
Let the base detect and display errors:  
```csharp
protected override bool Field_HasRequiredError()
    => FieldMandatory && CurrentControlValue == null;

protected override string Field_GetFormatErrorMessage()
    => "Please enter a valid value.";
```

---

## 9. Avoid Resetting on Context Change  
Do **not** reset `FieldIsOriginalValueSet` inside `OnBindingContextChanged()`—the static handler already captures the one-time original.

---

## 10. Prime Defaults in `OnParentSet()` (If Needed)  
For controls whose UI default ≠ `default(T)`, align the original right after layout:  
```csharp
protected override void OnParentSet()
{
    base.OnParentSet();
    FieldOriginalValue = CurrentControlValue;
    Field_UpdateValidationAndChangedState();
}
```

---

## 11. Batch UI Updates  
Always wrap UI changes in `Field_PerformBatchUpdate(...)` to prevent flicker and duplicate events.

---

## 12. Preserve Undo/Reset Support  
Do not modify the base’s undo/reset methods:  
- `Field_OriginalValue_SetToCurrentValue()`  
- `Field_OriginalValue_SetToClear()`  
- `Field_UndoValue()`  

They provide consistent undo behavior out of the box.

---

## 13. Leverage Access-Mode Configuration  
Use the built-in methods to handle enabling/disabling and Undo visibility:  
```csharp
Field_ConfigAccessModeEditing();
Field_ConfigAccessModeViewOnly();
```