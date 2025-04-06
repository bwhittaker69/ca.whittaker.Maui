using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels;

public class SampleTextBoxViewModel : ObservableObject
{
    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private string _userprofile_nickname = string.Empty;
    public string Userprofile_nickname
    {
        get => _userprofile_nickname;
        set => SetProperty(ref _userprofile_nickname, value);
    }

    public SampleTextBoxViewModel()
    {
        // Initialization logic if needed.
    }
}