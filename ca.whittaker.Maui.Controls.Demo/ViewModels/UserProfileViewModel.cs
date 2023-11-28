using CommunityToolkit.Mvvm.ComponentModel;

namespace ca.whittaker.Maui.Controls.Demo.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {

        [ObservableProperty]
        private FormStateEnum formState;

        [ObservableProperty]
        private string userprofile_username, userprofile_email, userprofile_nickname, userprofile_website, userprofile_phonenumber, userprofile_bio = String.Empty;

        private string currentUser_username = "";
        private string currentUser_nickname = "";
        private string currentUser_website = "";
        private string currentUser_phonenumber = "";
        private string currentUser_bio = "";
        private string currentUser_email = "";

        public UserProfileViewModel()
        {
            FormSaveCommand = new Command(Save);
        }

        public Command FormSaveCommand { get; }

        public void Initialize()
        {
            ResetUserProfileForm();
        }


        private void Save()
        {
            if (true) // savedSuccess
                FormState = FormStateEnum.Saved;
        }

        #region USER PROFILE FORM
        private void ResetUserProfileForm()
        {
            ClearUserProfileForm();
            FormState = FormStateEnum.Enabled;
        }

        private void ClearUserProfileForm()
        {
            Userprofile_nickname = Userprofile_username = Userprofile_website = Userprofile_phonenumber = Userprofile_bio = string.Empty;
        }
        #endregion

    }
}
