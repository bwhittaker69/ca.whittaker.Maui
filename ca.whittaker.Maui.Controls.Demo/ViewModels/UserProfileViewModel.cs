using CommunityToolkit.Mvvm.ComponentModel;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {

        [ObservableProperty]
        private FormStateEnum formState;

        [ObservableProperty]
        private string userprofile_username, userprofile_email, userprofile_nickname, userprofile_website, userprofile_phonenumber, userprofile_bio;

        private string? currentUser_username = null;
        private string? currentUser_nickname = null;
        private string? currentUser_website = null;
        private string? currentUser_phonenumber = null;
        private string? currentUser_bio = null;
        private string? currentUser_email = null;

        public UserProfileViewModel()
        {
            FormSaveCommand = new Command(Save);
        }

        public Command FormSaveCommand { get; }

        public void Initialize()
        {
            ConfigureUserProfileForm();
        }


        private void Save()
        {
            if (currentUser_username == null) return;
            if (true) // savedSuccess
                FormState = FormStateEnum.Saved;
            else
                FormState = FormStateEnum.Undo;
        }

        #region USER PROFILE FORM
        private void ConfigureUserProfileForm()
        {
            if (currentUser_username == null)
            {
                ClearUserProfileForm();
                FormState = FormStateEnum.Enabled;
            }
            else
            {
                PopulateUserProfileForm(currentUser_username);
                FormState = FormStateEnum.Undo;
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

    }
}
