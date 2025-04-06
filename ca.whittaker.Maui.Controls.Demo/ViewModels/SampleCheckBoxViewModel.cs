using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels;

public class SampleCheckBoxViewModel : ObservableObject
{
    private bool _userprofile_ispublic;
    public bool Userprofile_ispublic
    {
        get => _userprofile_ispublic;
        set => SetProperty(ref _userprofile_ispublic, value);
    }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public SampleCheckBoxViewModel()
    {
        // Default initialization if needed.
    }
}