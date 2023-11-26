# Custom MAUI Button Controls

## Overview
This project defines a custom button control system for a .NET MAUI application. It includes an abstract `ButtonBase` class and several derived classes, enabling dynamic and reusable button configurations across the application.

## Features
- **Abstract ButtonBase Class**: Extends the standard `Button` from `Microsoft.Maui.Controls`. Manages image sources and state-based configurations.
- **Derived Button Classes**: Customized button implementations (`CustomCancelButton`, `LoginButton`, `LogoutButton`, `SubmitButton`) inheriting from `ButtonBase`.
- **Dynamic State Management**: Utilizes `ButtonStateEnum` to handle different button states (`Enabled`, `Disabled`, `Hidden`).
- **Bindable Properties**: Support for data binding in button state management.
- **Image Source Handling**: Dynamic image source path construction based on button state.

## Usage

### ButtonBase Class
```csharp
public abstract class ButtonBase : Button
{
    // Properties and methods
}
```

### Derived Classes
Implement specific button behaviors by extending `ButtonBase`.
```csharp
public class CustomCancelButton : ButtonBase { /*...*/ }
public class LoginButton : ButtonBase { /*...*/ }
public class LogoutButton : ButtonBase { /*...*/ }
public class SubmitButton : ButtonBase { /*...*/ }
```

## Installation
Include these classes in your MAUI project to enhance button controls.

## Contributing
Contributions to enhance or extend the functionality are welcome.

## License
[Specify License Here]
