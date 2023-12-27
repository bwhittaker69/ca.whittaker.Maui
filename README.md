# Custom Controls for .NET MAUI

- [Buttons](#ca.whittaker.maui.controls.buttons)
  - *Delete Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/delete_12_light.png) 
  - *Camera Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/camera_12_light.png) 
  - *Cancel Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/cancel_12_light.png) 
  - *Undo Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/undo_12_light.png) 
  - *Save Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/save_12_light.png) 
  - *Refresh Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/refresh_12_light.png) 
  - *Zoom In Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/zoomin_12_light.png) 
  - *Zoom Out Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/zoomout_12_light.png) 
  - *Signout Button* ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/signout_12_light.png) 
- [Forms](#ca.whittaker.maui.controls.forms)
- [Enumerations](#enumerations)

# ca.whittaker.Maui.Controls.Buttons

The customizable controls in the `ca.whittaker.Maui.Controls.Buttons` namespace is a versatile and adaptive component designed for a wide range of functionalities within a user interface. This control, building upon the standard Button features in Maui, offers enhanced capabilities and customization options to suit various application needs.   Derived from the ButtonBase class (see [github](https://github.com/bwhittaker69/ca.whittaker.Maui) for source code), buttons are tailored for various functionalities like sign-in, edit, save, cancel, and more. They offer a high degree of customization and are responsive to different states and types.

- [Customizable Properties](#customizable-properties)
- [Dynamic UI Updates](#dynamic-ui-updates)
- [SigninButton Specialization](#signinbutton-specialization)
- [Button Types and States Enumeration](#button-types-and-states-enumeration)
- [Sizing](#sizing)
- [Button Usage Example](#button-usage-example)

Regular button types include:

| Button Class         | enabled | disabled |
|----------------------|:-------:|:--------:|
| AlarmClockButton     | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/alarmclock_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui.Controls/Resources/Images/alarmclock_24_light_disabled.png) |
| AlarmClockStopButton | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/alarmclock_stop_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/alarmclock_stop_24_light_disabled.png) |
| BookmarkButton       | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/bookmark_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/bookmark_24_light_disabled.png) |
| BookmarkDeleteButton | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/bookmark_delete_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/bookmark_delete_24_light_disabled.png) |
| CameraFlipButton     | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/phone_flip_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/phone_flip_24_light_disabled.png) |
| CameraRecordButton   | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/record_circle_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/record_circle_24_light_disabled.png) |
| EditButton           | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/edit_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/edit_24_light_disabled.png) |
| MediaPause           | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/mediapause_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/mediapause_24_light_disabled.png) |
| MediaPlay            | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/mediaplay_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/mediaplay_24_light_disabled.png) |
| MediaRecord          | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/mediarecord_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/mediarecord_24_light_disabled.png) |
| MediaStop            | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/mediastop_

Signin button exposes additional social media images:

```csharp
    public enum SiginButtonTypeEnum { Generic, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }
```

| *SiginButtonTypeEnum* | enabled | disabled |
|------------------|:-------:|:--------:|
| Generic          | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/signin_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/signin_24_light_disabled.png) |
| Tiktok           | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/tiktok_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/tiktok_24_light_disabled.png) |
| Facebook         | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/facebook_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/facebook_24_light_disabled.png) |
| Google           | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/google_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/google_24_light_disabled.png) |
| Microsoft        | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/microsoft_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/microsoft_24_light_disabled.png) |
| Apple            | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/apple_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/apple_24_light_disabled.png) |
| Linkedin         | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/linkedin_24_light.png) | ![alt text](https://raw.github.com/bwhittaker69/ca.whittaker.Maui/master/src/ca.whittaker.Maui.Controls/Resources/Images/linkedin_24_light_disabled.png) |

> **Note:** Scaling is based on the devices screen by taking the screen density of the device and factoring the density by the specified scale factor enumeration:


#### Customizable Properties:

| Property                      | Description                                                         |
|-------------------------------|---------------------------------------------------------------------|
| `ButtonSize`                  | Controls the size of the button.                                    |
| `ButtonState`                 | Determines the state of the button (Enabled, Disabled, Pressed, Hidden). |
| `ButtonType`                  | Indicates the specific function or identity of the button.          |
| `Text`                        | The default text displayed on the button.                           |
| `DisabledText`                | Text displayed when the button is in a disabled state.              |
| `PressedText`                 | Text displayed when the button is in a pressed state.               |
| `SigninButtonType`            | Specific to `SigninButton`, determines the type of sign-in service. |


#### Dynamic UI Updates:

The `ConfigureButton` method in `ButtonBase` ensures dynamic updating of the UI based on changes in button properties.  Adjustments to properties like `ButtonState`, `ButtonType`, and `ButtonSize` trigger UI updates to reflect these changes in real-time.

#### SigninButton Specialization:

`SigninButton` is a specialized version of `ButtonBase`, tailored for various sign-in options (e.g., Facebook, Google, Apple). It incorporates a SigninButtonType property, which determines the specific type of sign-in button (e.g., Signin with Google). The appearance and functionality of these buttons are customized according to the selected sign-in type.

#### Button Types and States Enumeration:

Enumerations such as `SiginButtonTypeEnum`, `BaseButtonTypeEnum`, and `ButtonStateEnum` provide a structured way to define and manage the various button types and states.

#### Sizing:

The SizeEnum enumeration facilitates size adaptability, allowing for a range of sizes from XXXSmall to XXLarge. This adaptability ensures that buttons can be styled appropriately across different devices and screen sizes.

```csharp
    public enum SizeEnum
    {
        XXXSmall = -20,
        XXSmall = -15,
        XSmall = -10,
        Small = -5,
        Normal = 0,
        Large = 5,
        XLarge= 10,
        XXLarge = 15
    }
```

#### Button Usage Example

The button controls are designed for ease of use and can be easily integrated into XAML layouts. Customization is straightforward, allowing developers to set properties in XAML or bind them to a ViewModel for dynamic changes. The button's appearance and behavior adjust automatically based on the set properties, providing a seamless user experience.

```xaml
           <Buttons:SigninButton Text="{x:Static strings:AppResources.Button_SignInWithApple}" 
                                  SigninButtonType="Apple"
                                  x:Name="SigninAppleButton"  
                                  ButtonSize="Normal"
                                  ButtonState="{Binding LoginButtonState}" 
                                  Command="{Binding LoginCommand}"  
                                  CommandParameter="signinapple"  />
```

# ca.whittaker.Maui.Controls.Forms

### Form, TextBoxElement and CheckBoxElement Controls

The Form control acts as the container for user input, with TextBoxElement controls for textual input and CheckBoxElement controls for boolean choices, creating a structured and user-friendly form interface.

#### Control: `Form`

The `Form` control serves as the container for the entire user input interface. It defines the overall layout and behavior of the form, including the form's name, save and cancel button texts, and the command to be executed when the form is saved.  In the provided example, UserProfileForm is a Form control that uses data binding to interact with the ViewModel for properties like FormState and FormSaveCommand.

The `Form` class is a comprehensive form control providing state management and validation capabilities. Key features of this class include:

1. Bindable Properties: The class includes properties like `Command`, `CommandParameter`, `FormCancelButtonText`, `FormName`, `FormSaveButtonText`, and `FormState`. These properties facilitate the binding and interaction of the form within the MAUI app.
2. UI Components: It creates and manages UI components such as save and cancel buttons, labels for form title and notifications, and a layout to organize these components.
3. Form State Management: The form can be in different states like enabled, disabled, hidden, etc., and this class manages these states, including visibility and enablement of the form and its elements.
4. Event Handling and Command Execution: The class handles various events like button clicks and text changes. It executes commands based on the form's state and user interactions.
5. Dynamic UI Update: It dynamically updates UI components based on property changes, such as text on buttons and labels.
6. Form Initialization and Control Wiring: It initializes form controls and wires up events during the parent setting process.
7. Validation and State Evaluation: The form evaluates its overall state, considering if there are changes and whether the data is valid. It updates the state based on these evaluations.
8. Form Actions: Implements methods to handle actions like saving, cancelling, clearing the form, and managing visibility of form elements.

#### Control: `TextBoxElement`

`TextBoxElement` controls are used within the Form control to accept textual input from the user. Each TextBoxElement is typically associated with a specific field of the form. Attributes such as Label, MaxLength, Placeholder, and FieldType can be set for each TextBoxElement to customize its appearance and functionality. In this example, there are three TextBoxElement controls for entering nickname, email, and bio, each bound to their respective ViewModel properties (Userprofile_nickname, Userprofile_email, and Userprofile_bio).

The `TextBoxElement` class in the ca.whittaker.Maui.Controls.Forms namespace is a customizable text box control for MAUI applications. It extends BaseFormElement and provides features for text manipulation and validation. Key functionalities include:

1. Bindable Properties: It has properties like `AllLowerCase`, `AllowWhiteSpace`, `FieldType`, `Mandatory`, `MaxLength`, `Placeholder`, and `TextBoxSource` for various text control and validation scenarios.
2. Email and URL Validation: It includes methods to validate email and URL formats.
3. UI Components: The control integrates a MAUI Entry for user input, a label for field description, a notification label for validation messages, and an undo button.
4. Dynamic Behavior: It reacts to text changes, focusing and unfocusing events, and alters UI based on validation state, text length, and placeholders.
5. Custom Text Processing: It processes text for case sensitivity, white space allowance, and filters text based on the field type (like email or URL).
6. Validation State Management: It manages and displays validation states, including required field errors and format errors.
7. Undo Functionality: Allows reverting to the original text.

#### Control: `CheckBoxElement` 

The `CheckBoxElement` control is used for boolean input, allowing users to select or unselect an option. It can be bound to a ViewModel property which reflects whether the checkbox is checked or not.  This control is also customizable with attributes like Label and LabelWidth.  It provides a specialized checkbox with added functionalities. Key features include:

1. Bindable Properties: It has Mandatory and CheckBoxSource properties. Mandatory indicates if the checkbox is required, and CheckBoxSource is used for two-way data binding.
2. UI Components: The control integrates a MAUI CheckBox, a label for description, a notification label for messages, and an undo button.
3. Customizable Behavior: It reacts to checkbox value changes and updates UI accordingly.
4. Undo Functionality: Includes an undo button to revert to the original value.
5. Validation State Management: The control manages its state based on whether its value has changed, reflecting this in the UI.
6. Initialization and Layout: Initializes UI components and sets up a layout grid for arranging these components.
7. Event Handling: Implements event handlers for checkbox value changes, focusing and unfocusing actions, and undo button press.

### Form Control Example

These controls are placed within a VerticalStackLayout inside the Form, which arranges them vertically with a consistent spacing and padding.  The TextBoxElement controls provide fields for text input, while the CheckBoxElement offers a binary choice, all within the encompassing Form. This arrangement creates an organized and user-friendly interface for data entry, where the form's overall state and actions (like save and cancel) are managed by the Form control, and individual inputs are managed by TextBoxElement and CheckBoxElement controls. This structure exemplifies a clean and modular approach to form design in XAML, ensuring each element focuses on its specific functionality while contributing to the cohesive operation of the form.

```xaml
    <Forms:Form x:Name="UserProfileForm" 
                FormName="{x:Static strings:AppResources.Form_UserProfile_TItle}"
                FormSaveButtonText="{x:Static strings:AppResources.Form_UserProfile_Button_Save}"
                FormCancelButtonText="{x:Static strings:AppResources.Form_UserProfile_Button_Cancel}"
                FormState="{Binding FormState}"
                Command="{Binding FormSaveCommand}" 
                CommandParameter=""> 
        <VerticalStackLayout Spacing="15" Padding="0" VerticalOptions="Start">
            <Forms:TextBoxElement    LabelWidth="70"
                                TextBoxSource="{Binding Userprofile_nickname}"
                                Label="{x:Static strings:AppResources.Field_Name}"
                                MaxLength="40"
                                Placeholder="{x:Static strings:AppResources.Field_Name_Placeholder}">
            </Forms:TextBoxElement>
            <Forms:TextBoxElement LabelWidth="70"
                                TextBoxSource="{Binding Userprofile_email}"
                                Label="{x:Static strings:AppResources.Field_Email}"
                                Mandatory="False"
                                MaxLength="40"
                                FieldType="Email"
                                Placeholder="{x:Static strings:AppResources.Field_Email_Placeholder}">
            </Forms:TextBoxElement>
            <Forms:TextBoxElement LabelWidth="70"
                                TextBoxSource="{Binding Userprofile_bio}"
                                FieldType="Chat"
                                Label="{x:Static strings:AppResources.Field_Bio}"
                                MaxLength="50"
                                Placeholder="{x:Static strings:AppResources.Field_Bio_Placeholder}">
            </Forms:TextBoxElement>
            <Forms:CheckBoxElement LabelWidth="70"
                                CheckBoxSource="{Binding Userprofile_ispublic}"
                                Label="{x:Static strings:AppResources.Field_IsPublic}">
            </Forms:CheckBoxElement>
        </VerticalStackLayout>
    </Forms:Form>
```

# Enumerations

```csharp
    public enum SiginButtonTypeEnum { Generic, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }
    public enum BaseButtonTypeEnum { Signin, Signout, Save, Edit, Cancel, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple, Undo }
    public enum FieldTypeEnum { Text, Email, Url, Chat, Username }
    public enum ButtonStateEnum { Enabled, Disabled, Pressed, Hidden }
    public enum ChangeStateEnum { Changed, NotChanged }
    public enum ValidationStateEnum { Valid, FormatError, RequiredFieldError }
    public enum FormStateEnum { Enabled, Disabled, Hidden, Undo, Saved, Clear }
    public enum SizeEnum
    {
        XXXSmall = -20,
        XXSmall = -15,
        XSmall = -10,
        Small = -5,
        Normal = 0,
        Large = 5,
        XLarge= 10,
        XXLarge = 15
    }
```

# Author

Brett Whittaker - brett@whittaker.ca

- https://github.com/bwhittaker69
- https://tiktok.com/@cowboycanadian
- https://www.linkedin.com/in/brettdouglaswhittaker