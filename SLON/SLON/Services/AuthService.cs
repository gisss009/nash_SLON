using Microsoft.Maui.Storage;

namespace SLON.Services
{
    public static class AuthService
    {
        private const string AuthKey = "IsAuthenticated";
        private const string UsernameKey = "Username";
        private const string PasswordKey = "Password";

        // Установка статуса авторизации
        public static void SetAuthenticated(bool isAuthenticated)
        {
            Preferences.Set(AuthKey, isAuthenticated);
        }

        // Проверка статуса авторизации
        public static bool IsAuthenticated()
        {
            //return false;
            return Preferences.Get(AuthKey, false);
        }

        // Сохранение данных пользователя
        public static async Task SaveCredentialsAsync(string username, string password)
        {
            await SecureStorage.SetAsync(UsernameKey, username);
            await SecureStorage.SetAsync(PasswordKey, password);
        }

        // Получение имени пользователя
        public static async Task<string> GetUsernameAsync()
        {
            return await SecureStorage.GetAsync(UsernameKey) ?? string.Empty;
        }

        // Получение пароля
        public static async Task<string> GetPasswordAsync()
        {
            return await SecureStorage.GetAsync(PasswordKey) ?? string.Empty;
        }

        // Очистка данных при выходе
        public static void ClearCredentials()
        {
            SecureStorage.Remove(UsernameKey);
            SecureStorage.Remove(PasswordKey);
            Preferences.Clear();
        }
    }
}