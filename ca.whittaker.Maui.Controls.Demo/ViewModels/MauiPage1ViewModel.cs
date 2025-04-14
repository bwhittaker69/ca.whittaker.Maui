using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels;

public class TestUser
{
    public Guid UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
}


public class MauiPage1ViewModel : ObservableObject
{

    private List<TestUser>? GenerateTestUsers(int count)
    {
        var testUsers = new List<TestUser>();

        for (int i = 1; i <= count; i++)
        {
            var user = new TestUser
            {
                UserId = Guid.NewGuid(),
                Nickname = $"TestUser{i}",
                Email = $"testuser{i}@example.com",
                Bio = $"This is the bio for TestUser{i}.",
                IsPublic = (i % 2 == 0)
            };

            testUsers.Add(user);
        }

        return testUsers;
    }



    private List<TestUser>? _userprofile_users; 
    public List<TestUser>? Userprofile_users
    {
        get => _userprofile_users;
        set => SetProperty(ref _userprofile_users, value);
    }

    private Object? _userprofile_userid;
    public Object? Userprofile_userid
    {
        get =>
            _userprofile_userid;
        set
        {
            if (SetProperty(ref _userprofile_userid, value))
            {
                System.Diagnostics.Debug.WriteLine($"Userprofile_userid changed to: {value}");
            }
        }
    }
    //
    private DateTimeOffset? _userprofile_date = new DateTimeOffset(new DateTime(1969, 7, 25));
    public DateTimeOffset? Userprofile_date
    {
        get => _userprofile_date;
        set => SetProperty(ref _userprofile_date, value);
    }


    private string _userprofile_nickname = "Nickname value";
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

    private string _userprofile_country = String.Empty;
    public string Userprofile_country
    {
        get => _userprofile_country;
        set
        {
            if (SetProperty(ref _userprofile_country, value))
            {
                System.Diagnostics.Debug.WriteLine($"Userprofile_country changed to: {value}");
            }
        }
    }

    private List<string> _userprofile_country_items = [];
    public List<string> Userprofile_country_items
    {
        get => _userprofile_country_items;
        set => SetProperty(ref _userprofile_country_items, value);
    }

    private bool _userprofile_ispublic = false;
    public bool Userprofile_ispublic
    {
        get => _userprofile_ispublic;
        set => SetProperty(ref _userprofile_ispublic, value);
    }

    public Command FormSaveCommand { get; }

    public MauiPage1ViewModel()
    {
        FormSaveCommand = new Command(OnFormSave);

        // Populate Userprofile_users with TestUser objects.
        Userprofile_users = GenerateTestUsers(6);
    }

    private void OnFormSave()
    {
        // Implement save logic here.
    }
}
