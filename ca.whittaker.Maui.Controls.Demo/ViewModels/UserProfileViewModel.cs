using CommunityToolkit.Mvvm.ComponentModel;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {

        [ObservableProperty]
        private string userprofile_email, lastlogin_device, lastlogin_location, lastlogin_date;
        [ObservableProperty]
        private ButtonStateEnum loginButtonState, logoutButtonState;
        [ObservableProperty]
        private FormStateEnum formState;
        [ObservableProperty]
        private bool loginDetailsTableVisible;
        [ObservableProperty]
        private string userprofile_username, userprofile_nickname, userprofile_website, userprofile_phonenumber, userprofile_bio;


        private string? currentUser_username = null;
        private string? currentUser_nickname = null;
        private string? currentUser_website = null;
        private string? currentUser_phonenumber = null;
        private string? currentUser_bio = null;

        public UserProfileViewModel()
        {
            LoginCommand = new Command<string>(Login);
            LogoutCommand = new Command<string>(Logout);
            UpdateCommand = new Command<string>(Update);
        }

        public Command<string> LoginCommand { get; }
        public Command<string> LogoutCommand { get; }
        public Command<string> UpdateCommand { get; }

        public async Task Initialize()
        {
            ConfigurePage(currentUser_username);
        }


        private void ConfigurePage(string? currentUsername)
        {
            ConfigureLoginLogoutButtons(currentUsername);
            ConfigureLoginDetails(currentUsername);
            ConfigureUserProfileForm(currentUsername);
        }

        private void DisabledAllControls()
        {
            ConfigureLoginLogoutButtons(null);
            ConfigureLoginDetails(null);
            ConfigureUserProfileForm(null);
        }

        private async void Login(string loginscheme)
        {
            ConfigureLoginLogoutButtons(loginscheme);
            ConfigureLoginDetails(loginscheme);
            ConfigureUserProfileForm(loginscheme);
        }

        private void Logout(string loginscheme)
        {
            DisabledAllControls();
        }

        private void Update(string? currentUser_username)
        {

            if (currentUser_username == null) return;

            currentUser_nickname = Userprofile_nickname;
            currentUser_website = Userprofile_website;
            currentUser_phonenumber = Userprofile_phonenumber;
            currentUser_bio = Userprofile_bio;
        }

        #region LOGIN LOGOUT BUTTONS
        private void ConfigureLoginLogoutButtons(string? currentUser_username)
        {
            if (currentUser_username == null)
            {
                // we are logged out, show all login buttons
                LoginButtonState = ButtonStateEnum.Enabled;
                LogoutButtonState = ButtonStateEnum.Hidden;
            }
            else
            {
                // we are logged in, show logout button
                LoginButtonState = ButtonStateEnum.Hidden;
                LogoutButtonState = ButtonStateEnum.Enabled;
            }
        }
        #endregion

        #region USER PROFILE FORM
        private void ConfigureUserProfileForm(string? currentUser_username)
        {
            if (currentUser_username == null)
            {
                ClearUserProfileForm();
                FormState = FormStateEnum.Hidden;
            }
            else
            {
                PopulateUserProfileForm(currentUser_username);
                FormState = FormStateEnum.Reset;
            }
        }

        private void PopulateUserProfileForm(string? currentUser_username)
        {
            Userprofile_nickname = "";
            Userprofile_username = currentUser_username;
            Userprofile_bio = "";
            Userprofile_website = "";
            Userprofile_email = "";
            Userprofile_phonenumber = "";
        }
        private void ClearUserProfileForm()
        {
            Userprofile_nickname = Userprofile_username = Userprofile_website = Userprofile_phonenumber = Userprofile_bio = string.Empty;
        }
        #endregion

        #region LOGIN DETAILS TABLE
        private void ConfigureLoginDetails(string? currentUser_username)
        {
            if (currentUser_username == null)
            {
                ClearLoginDetails();
                LoginDetailsTableVisible = false;
            }
            else
            {
                LoginDetailsTableVisible = true;
                PopulateLoginDetails();
            }
        }
        private void PopulateLoginDetails(string? currentUser_username = null)
        {
            if (currentUser_username == null)
            {
                ClearLoginDetails();
            }
            else
            {
                Lastlogin_device = "device type";
                Lastlogin_location = "your location";
                Lastlogin_date = DateTime.Now.ToString();
            }
        }
        private void ClearLoginDetails()
        {
            Userprofile_email = Lastlogin_device = Lastlogin_location = Lastlogin_date = string.Empty;
        }
        #endregion
    }
}
