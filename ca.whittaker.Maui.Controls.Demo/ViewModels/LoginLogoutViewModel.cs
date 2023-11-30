using CommunityToolkit.Mvvm.ComponentModel;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;
using System.Net.Sockets;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels
{
    public partial class LoginLogoutViewModel : ObservableObject
    {

        [ObservableProperty]
        private FormStateEnum formState;

        [ObservableProperty]
        private string lastlogin_device, lastlogin_ipaddress, lastlogin_date = "";

        [ObservableProperty]
        private ButtonStateEnum loginButtonState, logoutButtonState = ButtonStateEnum.Disabled;

        [ObservableProperty]
        private bool isUserLoggedIn;

        public LoginLogoutViewModel()
        {
            IsUserLoggedIn = false;
            LoginCommand = new Command<string>(Login);
            LogoutCommand = new Command<string>(Logout);
        }

        public Command<string> LoginCommand { get; }
        public Command<string> LogoutCommand { get; }

        public void Initialize()
        {
            ConfigurePage(IsUserLoggedIn);
        }


        private void ConfigurePage(bool isUserLoggedIn)
        {
            ConfigureLoginLogoutButtons(isUserLoggedIn);
            ConfigureLoginDetails(isUserLoggedIn);
        }

        private void Login(string loginscheme)
        {
            IsUserLoggedIn = true;

            void UpdateUI()
            {
                ConfigureLoginLogoutButtons(true);
                ConfigureLoginDetails(true);
                if (App.Current != null && App.Current.MainPage is AppShell appShell) appShell.OnLogin();
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
            IsUserLoggedIn = false;
            void UpdateUI()
            {
                ConfigureLoginLogoutButtons(false);
                ConfigureLoginDetails(false);
                if (App.Current != null && App.Current.MainPage is AppShell appShell) appShell.OnLogout();
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

        #region LOGIN LOGOUT BUTTONS
        private void ConfigureLoginLogoutButtons(bool isUserLoggedIn)
        {
            if (isUserLoggedIn)
            {
                // we are logged in, show logout button
                LoginButtonState = ButtonStateEnum.Hidden;
                LogoutButtonState = ButtonStateEnum.Enabled;
            }
            else
            {
                // we are logged out, show all login buttons
                LoginButtonState = ButtonStateEnum.Enabled;
                LogoutButtonState = ButtonStateEnum.Hidden;
            }
        }
        #endregion


        #region LOGIN DETAILS TABLE
        private void ConfigureLoginDetails(bool isUserLoggedIn)
        {
            if (isUserLoggedIn)
            {
                PopulateLoginDetails();
            }
            else
            {
                ClearLoginDetails();
            }
        }
        private void PopulateLoginDetails()
        {
            Lastlogin_device = DeviceInfo.Platform.ToString(); 
            Lastlogin_ipaddress = GetLocalIPAddress();
            Lastlogin_date = DateTime.Now.ToString();
        }
        private void ClearLoginDetails()
        {
            Lastlogin_device = Lastlogin_ipaddress = Lastlogin_date = string.Empty;
        }
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
        #endregion
    }
}
