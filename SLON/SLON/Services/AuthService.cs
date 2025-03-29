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

        // Установка статуса авторизации
        public static void SetAuthenticated(bool isAuthenticated)
        {
            Preferences.Set(AuthKey, isAuthenticated);
        }

        // Проверка статуса авторизации
        public static bool IsAuthenticated()
        {
            //ClearCredentials();
            return Preferences.Get(AuthKey, false);
        }

        // Сохранение данных пользователя
        public static void SaveCredentialsAsync(string username, string password)
        {
            SecureStorage.SetAsync(UsernameKey, username);
            SecureStorage.SetAsync(PasswordKey, password);
        }

        // Получение имени пользователя
        public static string GetUsernameAsync()
        {
            return SecureStorage.GetAsync(UsernameKey).Result ?? string.Empty;
        }

        // Получение пароля
        public static string GetPasswordAsync()
        {
            return SecureStorage.GetAsync(PasswordKey).Result ?? string.Empty;
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

                HttpResponseMessage response = await _httpClient.GetAsync(url);
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

        public static async Task<bool> Register(string username, string password)
        {
            string url = "http://139.28.223.134:5000/add_username_and_password?username=" + username + "&password=" + password;

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
        }

        public class UserProfile
        {
            public string username { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
            public List<string> categories { get; set; }
            public List<string> events { get; set; }
            public string description { get; set; }
            public List<int> swipedUsers { get; set; }
            public string mail { get; set; }
            public string vocation { get; set; }
            public Dictionary<string, string> tags { get; set; } // Изменено с List<string> на string
            public Dictionary<string, string> skills { get; set; } // Аналогично для skills, если нужно
        }

        public static async Task<bool> UpdateProfileCategory(string username, string category,
    string tags, string skills)
        {
            try
            {
                string url = $"http://139.28.223.134:5000/users/add_category_with_tags?" +
                           $"username={username}&category={Uri.EscapeDataString(category)}" +
                           $"&tags={Uri.EscapeDataString(tags)}" +
                           $"&skills={Uri.EscapeDataString(skills)}";

                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                return false;
            }
        }

        // Получение профиля пользователя
        public static async Task<UserProfile> GetUserProfileAsync(string username)
        {
            string url = $"http://139.28.223.134:5000//users/get_profile?username={username}";

            Stepper:
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);

                    // Десериализация JSON-ответа в объект UserProfile
                    var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                    if (jsonResponse != null && jsonResponse.ok && jsonResponse.response.HasValue)
                    {
                        // Преобразование JSON-данных в UserProfile
                        var userProfile = JsonSerializer.Deserialize<UserProfile>(jsonResponse.response.Value.ToString());
                        Console.WriteLine($"Received JSON: {responseBody}");
                        return userProfile;
                    }
                    else
                    {
                        Console.WriteLine("Ошибка в ответе API: неверный формат данных.");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                goto Stepper;
                return null;
            }
        }

        public class EventData
        {
            public string hash { get; set; }
            public string owner { get; set; }
            public List<string> members { get; set; }
            public string name { get; set; }
            public List<string> categories { get; set; }
            public string description { get; set; }
            public string location { get; set; }
            public string date_from { get; set; }
            public string date_to { get; set; }
            public int @public { get; set; }
            public int online { get; set; }
        }
        public static async Task<List<EventData>> GetAllUserEventsAsync(string username)
        {
            string url = $"http://139.28.223.134:5000/users/get_all_user_events?username={username}";

            Step:
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Server Response: {responseBody}");

                    var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                    if (jsonResponse != null && jsonResponse.ok && jsonResponse.response.HasValue)
                    {
                        // Преобразование JSON-данных в список EventData
                        var events = JsonSerializer.Deserialize<List<EventData>>(jsonResponse.response.Value.ToString());
                        Console.WriteLine(events.Count());
                        return events;
                    }
                    else
                    {
                        Console.WriteLine("Ошибка в ответе API: неверный формат данных.");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Content: {errorContent}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                goto Step;
                return null;
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Ошибка десериализации JSON: {jsonEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла неизвестная ошибка: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> AddEventAsync(EventData eventData)
        {
            try
            {
                string url = $"http://139.28.223.134:5000/events/add_event?" +
                            $"name={Uri.EscapeDataString(eventData.name)}" +
                            $"&owner={Uri.EscapeDataString(eventData.owner)}" +
                            $"&categories={Uri.EscapeDataString(string.Join(",", eventData.categories))}" +
                            $"&description={Uri.EscapeDataString(eventData.description)}" +
                            $"&location={Uri.EscapeDataString(eventData.location)}" +
                            $"&date_from={Uri.EscapeDataString(eventData.date_from)}" +
                            $"&date_to={Uri.EscapeDataString(eventData.date_to)}" +
                            $"&public={eventData.@public}" +
                            $"&online={eventData.online}";

                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding event: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> DeleteEventAsync(string eventHash)
        {
            try
            {
                string url = $"http://139.28.223.134:5000/events/delete_event?hash={eventHash}";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting event: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateUserProfileAsync(string username, Dictionary<string, string> updates)
        {
            try
            {
                var url = $"http://139.28.223.134:5000/users/update_profile?username={username}";

                // Явно указываем какие поля мы можем обновлять
                if (updates.ContainsKey("name"))
                    url += $"&name={Uri.EscapeDataString(updates["name"])}";

                if (updates.ContainsKey("surname"))
                    url += $"&surname={Uri.EscapeDataString(updates["surname"])}";

                if (updates.ContainsKey("vocation"))
                    url += $"&vocation={Uri.EscapeDataString(updates["vocation"])}";

                if (updates.ContainsKey("description"))
                    url += $"&description={Uri.EscapeDataString(updates["description"])}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);
                    return jsonResponse?.ok ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateEventAsync(EventData eventData)
        {
            try
            {
                string url = $"http://139.28.223.134:5000/events/edit_event?" +
                            $"hash={eventData.hash}" +
                            $"&name={Uri.EscapeDataString(eventData.name)}" +
                            $"&categories={Uri.EscapeDataString(string.Join(",", eventData.categories))}" +
                            $"&description={Uri.EscapeDataString(eventData.description)}" +
                            $"&location={Uri.EscapeDataString(eventData.location)}" +
                            $"&date_from={Uri.EscapeDataString(eventData.date_from)}" +
                            $"&date_to={Uri.EscapeDataString(eventData.date_to)}" +
                            $"&public={eventData.@public}" +
                            $"&online={eventData.online}";

                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating event: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> RemoveProfileCategory(string username, string category)
        {
            try
            {
                string url = $"http://139.28.223.134:5000/users/delete_profile_category?" +
                           $"username={username}&category={Uri.EscapeDataString(category)}";

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                return jsonResponse?.ok ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing category: {ex.Message}");
                return false;
            }
        }
    }
}