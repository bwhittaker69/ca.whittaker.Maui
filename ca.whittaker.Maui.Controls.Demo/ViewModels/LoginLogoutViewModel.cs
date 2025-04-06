using CommunityToolkit.Mvvm.ComponentModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels;

public class LoginLogoutViewModel : ObservableObject
{
    private string _loginLogoutHeaderText = Resources.Strings.AppResources.Page_LoginLogout_Title_SignIn;
    public string LoginLogoutHeaderText
    {
        get => _loginLogoutHeaderText;
        set => SetProperty(ref _loginLogoutHeaderText, value);
    }

    private FormAccessModeEnum _formState;
    public FormAccessModeEnum FormState
    {
        get => _formState;
        set => SetProperty(ref _formState, value);
    }

    private bool _isVisible = false;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private string _lastlogin_device = string.Empty;
    public string Lastlogin_device
    {
        get => _lastlogin_device;
        set => SetProperty(ref _lastlogin_device, value);
    }

    private string _lastlogin_ipaddress = string.Empty;
    public string Lastlogin_ipaddress
    {
        get => _lastlogin_ipaddress;
        set => SetProperty(ref _lastlogin_ipaddress, value);
    }

    private string _lastlogin_date = string.Empty;
    public string Lastlogin_date
    {
        get => _lastlogin_date;
        set => SetProperty(ref _lastlogin_date, value);
    }

    private ButtonStateEnum _loginButtonState;
    public ButtonStateEnum LoginButtonState
    {
        get => _loginButtonState;
        set => SetProperty(ref _loginButtonState, value);
    }

    private ButtonStateEnum _logoutButtonState;
    public ButtonStateEnum LogoutButtonState
    {
        get => _logoutButtonState;
        set => SetProperty(ref _logoutButtonState, value);
    }

    private string _userprofile_username = string.Empty;
    public string Userprofile_username
    {
        get => _userprofile_username;
        set => SetProperty(ref _userprofile_username, value);
    }

    private string _userprofile_email = string.Empty;
    public string Userprofile_email
    {
        get => _userprofile_email;
        set => SetProperty(ref _userprofile_email, value);
    }

    private string _userprofile_nickname = string.Empty;
    public string Userprofile_nickname
    {
        get => _userprofile_nickname;
        set => SetProperty(ref _userprofile_nickname, value);
    }

    private string _userprofile_website = string.Empty;
    public string Userprofile_website
    {
        get => _userprofile_website;
        set => SetProperty(ref _userprofile_website, value);
    }

    private string _userprofile_phonenumber = string.Empty;
    public string Userprofile_phonenumber
    {
        get => _userprofile_phonenumber;
        set => SetProperty(ref _userprofile_phonenumber, value);
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
    public Command<string> LoginCommand { get; }
    public Command<string> LogoutCommand { get; }

    public LoginLogoutViewModel()
    {
        IsVisible = false;

        LoginCommand = new Command<string>(Login);
        LogoutCommand = new Command<string>(Logout);
        FormSaveCommand = new Command(Save);

        LoginButtonState = ButtonStateEnum.Enabled;
        LogoutButtonState = ButtonStateEnum.Hidden;
    }

    private void Login(string loginscheme)
    {
        IsVisible = true;
        LoginLogoutHeaderText = Resources.Strings.AppResources.Page_LoginLogout_Title_SignOut;

        void UpdateUI() => ProcessLoginLogout(true);

        if (MainThread.IsMainThread)
        {
            UpdateUI();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
    }

    private void Logout(string loginscheme)
    {
        IsVisible = false;
        LoginLogoutHeaderText = Resources.Strings.AppResources.Page_LoginLogout_Title_SignIn;

        void UpdateUI() => ProcessLoginLogout(false);

        if (MainThread.IsMainThread)
        {
            UpdateUI();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateUI());
        }
    }

    private void Save()
    {
        // Assume save is successful
        FormState = FormAccessModeEnum.Editable;
    }

    #region USER PROFILE FORM
    private void ClearUserProfileForm()
    {
        Userprofile_nickname = Userprofile_username = Userprofile_website = Userprofile_phonenumber = Userprofile_bio = string.Empty;
    }

    public void InitializeForm()
    {
        ClearUserProfileForm();
        FormState = FormAccessModeEnum.Editable;
    }
    #endregion

    #region LOGIN LOGOUT BUTTONS
    private void ProcessLoginLogout(bool isUserLoggedIn)
    {
        if (isUserLoggedIn)
        {
            // User is logged in: show logout button and enable editing.
            LoginButtonState = ButtonStateEnum.Hidden;
            LogoutButtonState = ButtonStateEnum.Enabled;
            FormState = FormAccessModeEnum.Editable;
        }
        else
        {
            // User is logged out: show login button and hide form.
            LoginButtonState = ButtonStateEnum.Enabled;
            LogoutButtonState = ButtonStateEnum.Hidden;
            FormState = FormAccessModeEnum.Hidden;
        }

        if (isUserLoggedIn)
        {
            PopulateLoginDetails();
        }
        else
        {
            ClearLoginDetails();
        }
    }
    #endregion

    #region LOGIN DETAILS TABLE
    public static string GetLocalIPAddress()
    {
        var host = NetworkInterface.GetAllNetworkInterfaces()
            .OrderByDescending(c => c.Speed)
            .FirstOrDefault(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);

        if (host != null)
        {
            var ip = host.GetIPProperties().UnicastAddresses
                .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);

            if (ip != null)
                return ip.Address.ToString();
        }

        return string.Empty;
    }

    private void ClearLoginDetails()
    {
        Lastlogin_device = Lastlogin_ipaddress = Lastlogin_date = string.Empty;
    }

    private void PopulateLoginDetails()
    {
        Lastlogin_device = DeviceInfo.Platform.ToString();
        Lastlogin_ipaddress = GetLocalIPAddress();
        Lastlogin_date = DateTime.Now.ToString();
    }
    #endregion
}
