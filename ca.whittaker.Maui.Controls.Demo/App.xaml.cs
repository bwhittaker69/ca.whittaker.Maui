using Microsoft.Maui.Controls;

namespace ca.whittaker.Maui.Controls.Demo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
