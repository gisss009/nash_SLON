using Microsoft.Maui.Storage;
using SLON.Models;
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

        public static Task ClearCredentialsAsync()
        {
            SecureStorage.Remove(UsernameKey);
            SecureStorage.Remove(PasswordKey);
            Preferences.Clear();
            return Task.CompletedTask;
        }


        public class JsonResponse
        {
            public bool ok { get; set; }
            public JsonElement? response { get; set; }
        }

        public static async Task<bool> IsUsernameAndPasswordCorrect(string username, string password)
        {
            const int maxRetries = 3;
            int retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    string url = $"http://139.28.223.134:5000/is_username_and_password_correct?username={username}&password={password}";
                    HttpResponseMessage response = await _httpClient.GetAsync(url);
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);

                    var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);
                    if (jsonResponse != null && jsonResponse.response.HasValue)
                    {
                        // Если response имеет тип логического значения, возвращаем результат
                        if (jsonResponse.response.Value.ValueKind == JsonValueKind.True ||
                            jsonResponse.response.Value.ValueKind == JsonValueKind.False)
                        {
                            return jsonResponse.ok && jsonResponse.response.Value.GetBoolean();
                        }
                        return false;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    retries++;
                    Console.WriteLine($"IsUsernameAndPasswordCorrect: Попытка {retries} из {maxRetries} завершилась ошибкой: {ex.Message}");
                    if (retries >= maxRetries)
                        throw; // пробрасываем исключение, если попытки исчерпаны
                    await Task.Delay(1000); // ждем 1 секунду перед новой попыткой
                }
            }
            return false;
        }


        public static async Task<bool> Register(string username, string password)
        {
            string url = "http://139.28.223.134:5000/add_username_and_password?username=" + username + "&password=" + password;

            step:
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                return jsonResponse.ok;
            }
            catch (HttpRequestException)
            {
                goto step;
                //await Application.Current.MainPage.DisplayAlert("Error", $"Please wait.", "OK");
                //return false;
            }
            catch (Exception ex)
            {
                goto step;
                //Console.WriteLine($"Неизвестная ошибка: {ex}");
                //Console.WriteLine($"StackTrace: {ex.StackTrace}");
                //return false;
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
            public Dictionary<string, string> tags { get; set; }
            public Dictionary<string, string> skills { get; set; }
        }


        public static async Task<bool> UpdateProfileCategory(string username, string category,
    string tags, string skills)
        {
            step:
            try
            {
                string url = $"http://139.28.223.134:5000/users/add_category_with_tags?" +
                           $"username={username}&category={Uri.EscapeDataString(category)}" +
                           $"&tags={Uri.EscapeDataString(tags)}" +
                           $"&skills={Uri.EscapeDataString(skills)}";

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                // Явная проверка ответа сервера
                if (jsonResponse?.ok == true)
                {
                    Console.WriteLine("Category updated");
                    return true;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                goto step;
                return false;
            }
        }

        // Получение профиля пользователя
        public static async Task<UserProfile> GetUserProfileAsync(string username)
        {
            const int maxRetries = 3;
            int retries = 0;

            while (retries < maxRetries)
            {
                try
                {
                    string url = $"http://139.28.223.134:5000/users/get_profile?username={username}";
                    HttpResponseMessage response = await _httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);

                        var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                        if (jsonResponse != null && jsonResponse.ok && jsonResponse.response.HasValue)
                        {
                            var userProfile = JsonSerializer.Deserialize<UserProfile>(jsonResponse.response.Value.ToString());
                            Console.WriteLine($"Received JSON: {responseBody}");
                            return userProfile;
                        }
                        else
                        {
                            Console.WriteLine("Ошибка в ответе API: неверный формат данных.");
                            // Здесь можно либо сразу выбросить исключение, либо повторить попытку
                            throw new Exception("Неверный формат данных в ответе API.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                        throw new HttpRequestException($"Ошибка HTTP: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    retries++;
                    Console.WriteLine($"Попытка {retries} из {maxRetries} завершилась ошибкой: {ex.Message}");
                    if (retries >= maxRetries)
                    {
                        throw; // Если попытки закончились, пробрасываем исключение дальше
                    }
                    await Task.Delay(1000); // Ждём 1 секунду перед следующей попыткой
                }
            }

            // Если цикл завершился без возврата (этого формально быть не должно), возвращаем null или генерируем исключение
            return null;
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
                        goto Step;
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Content: {errorContent}");
                    goto Step;
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
                goto Step;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла неизвестная ошибка: {ex.Message}");
                goto Step;
                return null;
            }
        }

        private static string _lastSentHash = string.Empty;

        public static async Task<bool> AddEventAsync(EventData eventData)
        {
            try
            {
                // Уникальный хеш на основе ключевых данных — чтобы не отправлять повторно
                string currentHash = $"{eventData.name}_{eventData.date_from}_{eventData.owner}";
                if (_lastSentHash == currentHash)
                {
                    Console.WriteLine("[DOTNET] Duplicate create request suppressed.");
                    return false;
                }

                // Собираем URL
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

                Console.WriteLine("[DOTNET] Sending request to create event...");
                var response = await _httpClient.GetAsync(url);

                Console.WriteLine($"[DOTNET] Status: {(int)response.StatusCode} - {response.ReasonPhrase}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[DOTNET] Request failed before reading response body.");
                    return false;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DOTNET] Server Response: {responseBody}");

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                if (jsonResponse?.ok == true)
                {
                    Console.WriteLine("[DOTNET] Event successfully created.");
                    _lastSentHash = currentHash;
                    return true;
                }

                Console.WriteLine("[DOTNET] Server responded with ok = false.");
                return false;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[DOTNET] HttpRequestException: {httpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DOTNET] Unexpected error: {ex.Message}");
                return false;
            }
        }



        public static async Task<bool> DeleteEventAsync(string eventHash)
        {
            step:
            try
            {
                string url = $"http://139.28.223.134:5000/events/delete_event?hash={eventHash}";

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                // Явная проверка ответа сервера
                if (jsonResponse?.ok == true)
                {
                    Console.WriteLine("Event successfully created");
                    return true;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting event: {ex.Message}");
                goto step;
                return false;
            }
        }

        public static async Task<bool> UpdateUserProfileAsync(string username, Dictionary<string, string> updates)
        {
            step:
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

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);
                return jsonResponse?.ok ?? false;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                goto step;
                return false;
            }
        }

        public static async Task<bool> UpdateEventAsync(EventData eventData)
        {
            step:
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
                var responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                // Явная проверка ответа сервера
                if (jsonResponse?.ok == true)
                {
                    Console.WriteLine("Event successfully created");
                    return true;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating event: {ex.Message}");
                goto step;
                return false;
            }
        }

        public static async Task<bool> RemoveProfileCategory(string username, string category)
        {
            step:
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
                goto step;
                return false;
            }
        }

        public static async Task<bool> AddSwipedUser(string username, string username_to)
        {
            step:
            try
            {
                string url = $"http://139.28.223.134:5000/users/add_profile_swiped_user?" +
                           $"username={username}" +
                           $"&username_to={username_to}";

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);
                Console.WriteLine(responseBody);

                return jsonResponse?.ok ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding swiped user: {ex.Message}");
                goto step;
                return false;
            }
        }

        public static async Task<bool> AddSwipedEvent(string username, string eventHash)
        {
            step:
            try
            {
                string url = $"http://139.28.223.134:5000/users/add_swiped_event?" +
                           $"username={Uri.EscapeDataString(username)}" +
                           $"&event_hash={Uri.EscapeDataString(eventHash)}";

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                return jsonResponse?.ok ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding swiped event: {ex.Message}");
                goto step;
                return false;
            }
        }

        public static async Task<List<UserProfile>> GetAllUsersAsync()
        {
            step:
            try
            {
                string url = "http://139.28.223.134:5000/users/get_all_profiles";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                if (jsonResponse?.ok == true && jsonResponse.response.HasValue)
                {
                    return JsonSerializer.Deserialize<List<UserProfile>>(jsonResponse.response.Value.ToString());
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all users: {ex.Message}");
                goto step;
                return null;
            }
        }

        public static async Task<List<User>> GetSwipedUsersAsync(string username)
        {
        step:
            try
            {
                string url = $"http://139.28.223.134:5000/users/get_swiped_users?username={Uri.EscapeDataString(username)}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<User>();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);

                if (jsonResponse?.ok == true && jsonResponse.response.HasValue)
                {
                    var usersData = JsonSerializer.Deserialize<List<UserData>>(jsonResponse.response.Value.GetRawText());
                    return usersData.Select(u => new User(
                        username: u.username,
                        name: u.name,
                        surname: u.surname, // Фамилия теперь передаётся
                        tags: new List<string>(),  // Если теги не нужны, оставляем по умолчанию
                        vocation: u.vocation,
                        info: u.info,
                        skills: ""  // При необходимости можно передать навыки
                    )
                    {
                        IsMutual = u.is_mutual,
                        IsAcceptedMe = u.is_accepted_me,
                        IsILikedHim = u.is_i_liked_him
                    }).ToList();
                }

                return new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting swiped users: {ex.Message}");
                goto step;
                return new List<User>();
            }
        }


        public class UserData
        {
            public string username { get; set; }
            public string name { get; set; }
            public string surname { get; set; }  // Добавлено поле фамилии
            public string vocation { get; set; }
            public string info { get; set; }
            public bool is_mutual { get; set; }
            public bool is_accepted_me { get; set; }
            public bool is_i_liked_him { get; set; }
        }

    }
}