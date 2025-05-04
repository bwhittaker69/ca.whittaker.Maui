using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels;

public class TestForm2ViewModel : ObservableObject
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

    private string _userprofile_email = string.Empty;
    public string Userprofile_email
    {
        get => _userprofile_email;
        set => SetProperty(ref _userprofile_email, value);
    }

    private string _userprofile_bio = string.Empty;
    public string Userprofile_bio
    {
        get => _userprofile_bio;
        set => SetProperty(ref _userprofile_bio, value);
    }

    private bool _userprofile_ispublic = false;
    public bool Userprofile_ispublic
    {
        get => _userprofile_ispublic;
        set => SetProperty(ref _userprofile_ispublic, value);
    }

    public Command FormSaveCommand { get; }

    public TestForm2ViewModel()
    {
        FormSaveCommand = new Command(OnFormSave);
    }

    private void OnFormSave()
    {
        // Implement your save logic here.
    }
}
