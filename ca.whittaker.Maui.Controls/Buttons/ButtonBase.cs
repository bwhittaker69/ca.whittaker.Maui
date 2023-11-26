using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace ca.whittaker.Maui.Controls.Buttons;

public abstract class ButtonBase : Button
{

    private string _baseImageSource;

    public ButtonBase() : base()
    {
        // Constructor logic if needed
    }

    public string BaseImageSource
    {
        get => _baseImageSource;
        set
        {
            _baseImageSource = value;
            ConfigureButton();
        }
    }
   
    public void ConfigureButton(ButtonStateEnum buttonState = ButtonStateEnum.Enabled)
    {
        Debug.Print(this.GetType().ToString());
        switch (buttonState)
        {
            case ButtonStateEnum.Enabled:
                IsVisible = true;
                IsEnabled = true;
                ImageSource = GetImageSource("");
                break;
            case ButtonStateEnum.Disabled:
                IsVisible = true;
                IsEnabled = false;
                ImageSource = GetImageSource("_disabled");
                break;
            case ButtonStateEnum.Hidden:
                IsEnabled = false;
                IsVisible = false;
                break;
        }
    }

    private ImageSource GetImageSource(string suffix)
    {
        if (!String.IsNullOrEmpty(BaseImageSource))
            return ImageSource.FromFile($"{BaseImageSource.Replace(".png", "")}{suffix}.png");
        else
            return "";
    }
}
