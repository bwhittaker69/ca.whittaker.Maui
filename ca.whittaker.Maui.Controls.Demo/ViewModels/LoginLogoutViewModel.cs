﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;


namespace ca.whittaker.Maui.Controls.Demo.ViewModels
{
    public partial class LoginLogoutViewModel : ObservableObject
    {

        [ObservableProperty]
        private string loginLogoutHeaderText = Resources.Strings.AppResources.Page_LoginLogout_Title_SignIn;

        [ObservableProperty]
        private FormFieldsStateEnum formState;

        [ObservableProperty]
        private bool isVisible = false;

        [ObservableProperty]
        private string lastlogin_device, lastlogin_ipaddress, lastlogin_date = "";

        [ObservableProperty]
        private ButtonStateEnum loginButtonState, logoutButtonState;

        [ObservableProperty]
        private string userprofile_username, userprofile_email, userprofile_nickname, userprofile_website, userprofile_phonenumber, userprofile_bio = String.Empty;

        [ObservableProperty]
        private bool userprofile_ispublic = false;

        public LoginLogoutViewModel()
        {

            IsVisible = false;

            LoginCommand = new Command<string>(Login);
            LogoutCommand = new Command<string>(Logout);

            FormSaveCommand = new Command(Save);

            LoginButtonState = ButtonStateEnum.Enabled;
            LogoutButtonState = ButtonStateEnum.Hidden;


        }

        public Command FormSaveCommand { get; }

        public Command<string> LoginCommand { get; }

        public Command<string> LogoutCommand { get; }

        private void Login(string loginscheme)
        {
            IsVisible = true;

            LoginLogoutHeaderText = Resources.Strings.AppResources.Page_LoginLogout_Title_SignOut;

            void UpdateUI()
            {
                ProcessLoginLogout(true);
            }

            // Check if on the main thread and update UI accordingly
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

            void UpdateUI()
            {
                ProcessLoginLogout(false);
            }

            // Check if on the main thread and update UI accordingly
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
            if (true) // savedSuccess
                //FormFieldsState = 
                FormState = FormFieldsStateEnum.Editing;
        }

        #region USER PROFILE FORM
        private void ClearUserProfileForm()
        {
            Userprofile_nickname = Userprofile_username = Userprofile_website = Userprofile_phonenumber = Userprofile_bio = string.Empty;
        }

        public void InitializeForm()
        {
            ClearUserProfileForm();
            FormState = FormFieldsStateEnum.Editing;
        }
        #endregion

        #region LOGIN LOGOUT BUTTONS
        private void ProcessLoginLogout(bool isUserLoggedIn)
        {
            if (isUserLoggedIn)
            {
                // we are logged in, show logout button
                LoginButtonState = ButtonStateEnum.Hidden;
                LogoutButtonState = ButtonStateEnum.Enabled;
                FormState = FormFieldsStateEnum.Editing;
            }
            else
            {
                // we are logged out, show all login buttons
                LoginButtonState = ButtonStateEnum.Enabled;
                LogoutButtonState = ButtonStateEnum.Hidden;
                FormState = FormFieldsStateEnum.Hidden;
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

                if (ip != null) return ip.Address.ToString();
            }

            return String.Empty;

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
}
