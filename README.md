# MAUI Form Controls

## Overview
The `Form` class is a sophisticated component designed for .NET MAUI applications, extending the `StackLayout` to provide an enhanced form management experience. This class is tailored to facilitate various aspects of form handling, including state management, validation, and dynamic user interface adjustments. Key features include:

- **Bindable Properties**: Incorporates a range of bindable properties such as `Command`, `CommandParameter`, `FormCancelButtonText`, `FormName`, and more. These properties enable seamless interaction and integration with other parts of a MAUI application.

- **User Interface Elements**: Features essential UI components like save and cancel buttons, a form label, and a notification label, which collectively enhance the user interaction experience.

- **Event Handling and State Management**: Efficiently handles user interactions (e.g., button clicks) and internal state changes. It ensures the form's state (enabled, disabled, hidden) is managed effectively, reflecting the current context of the application.

- **Validation and Change Detection**: Employs mechanisms to track changes and validate the input in text boxes. It intelligently enables or disables buttons based on the validity of the changes made to the form.

- **Dynamic UI Updates**: Capable of updating UI elements dynamically in response to property changes, ensuring a responsive and interactive user experience.

- **Initialization and Setup**: Utilizes the `OnParentSet` method to initialize and configure form controls, ensuring a robust setup.

- **Error Handling and Event Wiring**: Includes methods for wiring up custom text box events and handling potential errors, such as missing controls, thereby enhancing reliability and stability.

This `Form` class is an all-encompassing solution for creating and managing forms within .NET MAUI applications, offering a wide array of features for comprehensive state management, validation, and UI responsiveness.

## Features
- **Abstract ButtonBase Class**: Extends the standard `Button` from `Microsoft.Maui.Controls`. Manages image sources and state-based configurations.
- **Derived Button Classes**: Customized button implementations (`CustomCancelButton`, `LoginButton`, `LogoutButton`, `SubmitButton`) inheriting from `ButtonBase`.
- **Dynamic State Management**: Utilizes `ButtonStateEnum` to handle different button states (`Enabled`, `Disabled`, `Hidden`).
- **Bindable Properties**: Support for data binding in button state management.
- **Image Source Handling**: Dynamic image source path construction based on button state.

## Usage
To integrate the Form class into your .NET MAUI application, follow these steps:
Usage

To integrate the Form class into your .NET MAUI application, follow these steps:

1. Adding the Form to Your View

Incorporate the Form class in your XAML where you need a form. Make sure to reference the namespace ca.whittaker.Maui.Controls.Forms.

XAML Example:
```xml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:forms="ca.whittaker.Maui.Controls.Forms"
             x:Class="YourNamespace.YourPage">
    <forms:Form x:Name="myForm"/>
</ContentPage>
```

2. Configuring the Form

You can configure various aspects of the form directly in XAML. Set properties such as FormName, FormSaveButtonText, and FormCancelButtonText to customize the form's appearance and behavior.

Example of Setting Properties:

```xml
<forms:Form x:Name="myForm"
            FormName="User Details"
            FormSaveButtonText="Submit"
            FormCancelButtonText="Reset" />
```

3. Binding Commands and Parameters

Bind commands and parameters to handle actions like saving data or canceling the form operation. Use the Command and CommandParameter properties for this purpose.

Example of Command Binding:
```xml
<forms:Form x:Name="myForm"
            Command="{Binding SaveCommand}"
            CommandParameter="{Binding FormData}" />
```

4. Customizing Appearance

Customize the appearance of the form by setting properties like FormNameTextColor and FormSize.

Example of Customizing Appearance:
```xml
<forms:Form x:Name="myForm"
            FormNameTextColor="Blue"
            FormSize="Large" />
```

By following these steps, you can effectively incorporate and customize the Form class in your .NET MAUI application using XAML.

1. Adding the Form to Your View
Incorporate the Form class in your XAML where you need a form. Make sure to reference the namespace ca.whittaker.Maui.Controls.Forms.

<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:forms="ca.whittaker.Maui.Controls.Forms"
             x:Class="YourNamespace.YourPage">
    <forms:Form x:Name="myForm"/>
</ContentPage>