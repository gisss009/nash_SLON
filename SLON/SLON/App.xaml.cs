using Microsoft.Maui.Controls;
using SLON.Services;

namespace SLON
{
    public partial class App : Application
    {

        public static event Action ThemeChanged;

        public App()
        {
            InitializeComponent();

            // Стартовая страница на основе состояния авторизации
            MainPage = AuthService.IsAuthenticated()
                ? new AppShell()
                : new NavigationPage(new AuthPage());
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        // Этот метод можно вызвать при смене темы
        public static void NotifyThemeChanged()
        {
            ThemeChanged?.Invoke();
        }
    }
}