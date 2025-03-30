# Forms Folder Overview

This folder contains custom MAUI form controls that simplify the creation of composite, data-driven forms. These controls share common functionality—such as state management, change tracking, and validation—while providing flexible UI composition.

## Key Classes

- **BaseFormElement.cs**  
  *Foundation for custom form controls.*  
  - Inherits from `ContentView` and provides common bindable properties such as `ChangeState`, `Label`, `LabelWidth`, and `ValidationState`.
  - Offers helper methods to create standard UI sub-elements (e.g., notification labels and undo buttons).
  - Raises events for data changes and validation state updates to enable reactive behavior in child controls.

- **TextBox.cs**  
  *A customizable text box control for user input.*  
  - Combines a field label, an `Entry` control, an undo button, and a notification label arranged in a Grid.
  - Supports text manipulation, filtering (e.g., lowercase conversion, whitespace control), and validation.
  - Uses a simple bindable property named `Text` for input value exposure, suitable for straightforward binding scenarios.

- **TextBoxElement.cs**  
  *An advanced text box control with robust two-way binding.*  
  - Similar to `TextBox` in layout and validation.
  - Exposes its value via a dedicated two-way bindable property called `TextBoxSource`.
  - Implements loopback prevention logic to better manage binding synchronization and complex text processing requirements.

- **CheckBoxElement.cs**  
  *A customizable checkbox control for boolean input.*  
  - Arranges a field label, a `CheckBox` control, an undo button, and a notification label in a Grid.
  - Tracks the original state of the checkbox to determine if changes have occurred.
  - Provides undo functionality to revert changes and supports change tracking through events.

- **Form.cs**  
  *A composite form control that aggregates multiple form elements.*  
  - Acts as a container for child controls (derived from `BaseFormElement`), such as `TextBoxElement` and `CheckBoxElement`.
  - Manages overall form state including whether the form has changes or errors.
  - Provides built-in Save and Cancel buttons with configurable texts.
  - Wires up events from child controls to automatically update properties like `FormHasChanges`, `FormHasErrors`, and `FormState`.
  - Handles high-level state transitions (Initialize, Saved, Undo, Clear, Hidden) to deliver a responsive and consistent form experience.

## How They Work Together

- **Common Foundation:**  
  `BaseFormElement` centralizes common behavior (layout helpers, event raising) so that every derived control behaves consistently.

- **Input Controls:**  
  `TextBox` and `TextBoxElement` provide text input capabilities with filtering and validation, but differ in how they expose their value (simple `Text` vs. robust `TextBoxSource`).  
  `CheckBoxElement` offers a similar model for boolean input, ensuring that changes are tracked and undoable.

- **Form Aggregation:**  
  The `Form` control aggregates these input controls and monitors their state via events. It updates its own state (e.g., enabling the Save button only when changes are detected and inputs are valid) and provides methods for actions like canceling changes or saving form data.

## Use Case Examples

### User Profile Form
- **Components:**  
  - `TextBoxElement` for capturing the user's name and email.
  - `CheckBoxElement` for options like "Make Profile Public."
- **Workflow:**  
  - The form monitors changes and validates each field.
  - The Save button is enabled only if at least one field is changed and all inputs are valid.
  - Clicking Cancel triggers the form’s `CancelForm` method, which calls the `Undo` method on each child control.

### Settings Form
- **Components:**  
  - A mix of `TextBox` (for settings requiring text input) and `CheckBoxElement` (for toggle options).
- **Workflow:**  
  - The form aggregates all settings inputs.
  - It updates overall state to show error notifications if any control’s data is invalid.
  - It uses the Save/Cancel buttons to either commit the changes or revert all modifications.

## Summary

These custom form controls allow you to build complex, interactive forms in a consistent and reusable manner. By leveraging shared behaviors from `BaseFormElement` and composing specific input controls (`TextBox`, `TextBoxElement`, and `CheckBoxElement`), the `Form` control offers a robust framework for managing form state, validation, and user interactions.


