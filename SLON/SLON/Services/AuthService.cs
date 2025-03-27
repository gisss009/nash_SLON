using Microsoft.Maui.Storage;
using System.Text;
using System.Text.Json;

namespace SLON.Services
{
    public static class AuthService
    {
        private const string AuthKey = "IsAuthenticated";
        private const string UsernameKey = "Username";
        private const string PasswordKey = "Password";
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) // Таймаут, чтобы избежать зависания
        };
        private static readonly SemaphoreSlim _requestSemaphore = new SemaphoreSlim(1, 1); // Контроль одновременных запросов
        private static bool _isRequestInProgress = false; // Флаг для блокировки повторных запросов

        // Установка статуса авторизации
        public static void SetAuthenticated(bool isAuthenticated)
        {
            Preferences.Set(AuthKey, isAuthenticated);
        }

        // Проверка статуса авторизации
        public static bool IsAuthenticated()
        {
            ClearCredentials();
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

        public class JsonResponse
        {
            public bool ok { get; set; }
            public JsonElement? response { get; set; }
        }

        public static async Task<bool> IsUsernameAndPasswordCorrect(string username, string password)
        {
            string url = "http://139.28.223.134:5000/is_username_and_password_correct?username=" + username + "&password=" + password;

            //using (var _httpClient = new HttpClient())
            //{
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                if (jsonResponse != null)
                {
                    Console.WriteLine($"ok: {jsonResponse.ok}");

                    if (jsonResponse.response.HasValue)
                    {
                        if (jsonResponse.response.Value.ValueKind == JsonValueKind.True ||
                            jsonResponse.response.Value.ValueKind == JsonValueKind.False)
                        {
                            Console.WriteLine($"response: {jsonResponse.response.Value}");
                            return jsonResponse.ok && jsonResponse.response.Value.GetBoolean(); // Возвращаем true только если оба значения true
                        }

                        Console.WriteLine($"response: {jsonResponse.response.Value}");
                        return false; // Возвращаем true только если оба значения true
                    }

                    Console.WriteLine("response is missing or null.");
                    return false; // Если response null, возвращаем false
                }
                return false;
            }
            catch (HttpRequestException)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Please wait.", "OK");
                return false;
            }
            //}
        }

        public static async Task<bool> Register(string username, string password)
        {
            if (_isRequestInProgress)
                return false;

            _isRequestInProgress = true;

            string url = "http://139.28.223.134:5000/add_username_and_password?username=" + username + "&password=" + password;

            //using (var _httpClient = new HttpClient())
            //{
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                return jsonResponse.ok;
            }
            catch (HttpRequestException)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Please wait.", "OK");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неизвестная ошибка: {ex}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                _isRequestInProgress = false;
            }
                //}
        }

    }
}