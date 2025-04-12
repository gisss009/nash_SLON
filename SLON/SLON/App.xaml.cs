using Microsoft.Maui.Controls;
using SLON.Services;

namespace SLON
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Стартовая страница на основе состояния авторизации
            MainPage = ResponseService.IsAuthenticated()
                ? new AppShell()
                : new NavigationPage(new AuthPage());
        }

        protected override void OnStart()
        {
            base.OnStart();
        }
    }
}