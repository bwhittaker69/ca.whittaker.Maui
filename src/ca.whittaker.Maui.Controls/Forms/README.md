# Forms Overview

This folder contains custom MAUI form controls that simplify the creation of composite, data-driven forms.  
These controls share common functionality — such as state management, undo support, change tracking, and validation — while providing flexible UI composition.

# Forms Architecture Overview


```plaintext
                   +----------------+
                   |     Form        |
                   | (Composite)     |
                   +----------------+
                          |
          +---------------+---------------+
          |                               |
  (Wired at runtime)               (Wired at runtime)
          |                               |
+----------------+               +----------------+
| IBaseFormField |               | IBaseFormField |
| (Interface)    |               | (Interface)    |
+----------------+               +----------------+
          |                               |
          |                               |
+----------------------+     +----------------------+
| BaseFormField<T>      |     | BaseFormField<T>      |
| (Abstract Class)      |     | (Abstract Class)      |
+----------------------+     +----------------------+
          |                               |
  +------------------+           +------------------+
  | TextBoxField      |           | CheckBoxField    |
  | (Concrete Field)  |           | (Concrete Field) |
  +------------------+           +------------------+
```

---

# Key Relationships
- `Form` automatically discovers and wires all `IBaseFormField` descendants inside its visual tree.
- `IBaseFormField` defines **what fields must implement** (access mode, change tracking, validation).
- `BaseFormField<T>` provides a **standard reusable base** for all typed input controls.
- Concrete fields (e.g., `TextBoxField`, `CheckBoxField`) **inherit** from `BaseFormField<T>` and implement field-specific logic.

# Design Principles
- **Loose coupling:** Form doesn’t hardcode any field types — it only depends on the interface.
- **Code reuse:** `BaseFormField<T>` centralizes undo, validation, and event raising logic.
- **Flexibility:** You can add new field types (e.g., `DatePickerField`, `DropdownField`) easily by inheriting `BaseFormField<T>`.
- 
## Key Components

- **IBaseFormField.cs**  
  *Standardized interface for all form fields.*  
  - Defines field state properties like `FieldAccessMode`, `FieldChangeState`, and `FieldValidationState`.
  - Provides methods for clear, undo, save, and label layout management.
  - Raises events (`FieldHasChanges`, `FieldHasValidationChanges`) to notify parent containers like `Form`.

- **BaseFormField&lt;T&gt;.cs**  
  *Generic abstract base class for building custom fields.*  
  - Inherits from `ContentView` and implements `IBaseFormField`.
  - Adds two-way binding to a typed data source (`FieldDataSource` of type `T`).
  - Handles original value tracking, undo functionality, validation error management, and undo button wiring.
  - Provides flexible layout templates for labels, controls, notifications, and undo buttons.

- **Concrete Field Controls (e.g., TextBoxField, CheckBoxField)**  
  *Specific input controls for different data types.*  
  - Inherit from `BaseFormField&lt;T&gt;`.
  - Implement field-specific value handling, validation, and control layouts.
  - Automatically support undo/reset, validation messages, and change tracking.

- **Form.cs**  
  *Composite control that aggregates multiple form fields.*  
  - Scans visual tree for child `IBaseFormField` instances.
  - Wires up change tracking and validation event handlers automatically.
  - Dynamically manages form-wide states: `FormHasChanges`, `FormHasErrors`, and `FormAccessMode`.
  - Provides built-in Save, Edit, and Cancel button functionality.
  - Supports undoing unsaved changes or saving field values in batch.

## How They Work Together

- **Standardization:**  
  `IBaseFormField` defines a common contract for all field types.  
  `BaseFormField&lt;T&gt;` centralizes shared behaviors (undo, validation, change tracking) for consistent field behavior.

- **Field Controls:**  
  Specific controls like `TextBoxField` or `CheckBoxField` extend `BaseFormField&lt;T&gt;`, providing typed data entry, undo, and validation out of the box.

- **Form Aggregation:**  
  The `Form` control automatically wires up any child implementing `IBaseFormField`.  
  It monitors change and validation events, updates overall state, and controls button visibility and enabled states accordingly.

## Use Case Examples

### User Profile Form
- **Components:**  
  - `TextBoxField` for capturing the user's name and email.
  - `CheckBoxField` for options like "Make Profile Public."
- **Workflow:**  
  - The form monitors changes and validates each field.
  - The Save button is enabled only if at least one field is changed and all inputs are valid.
  - Clicking Cancel triggers the form’s undo logic across all fields.

### Settings Form
- **Components:**  
  - A mix of `TextBoxField` (for settings requiring text input) and `CheckBoxField` (for toggle options).
- **Workflow:**  
  - The form aggregates all settings inputs.
  - It updates overall state to show error notifications if any control’s data is invalid.
  - It uses the Save/Cancel buttons to either commit the changes or revert all modifications.

## Summary

These custom form controls allow you to build complex, interactive forms in a consistent and reusable manner.  
By leveraging shared behaviors from `BaseFormField&lt;T&gt;` and composing specific input controls (`TextBoxField`, `CheckBoxField`),  
the `Form` control offers a robust framework for managing form state, validation, undo support, and user interactions.
