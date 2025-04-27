using Microsoft.Maui.Storage;
using SLON.Models;
using System.Net;
using System.Net.Http.Headers;
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
            const string endpoint = "http://139.28.223.134:5000/users/get_profile";
            UserProfile result = null;

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    // 1) Собираем URL
                    string url = $"{endpoint}?username={Uri.EscapeDataString(username)}";

                    // 2) Создаём HttpWebRequest с буферизацией и распаковкой
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.AllowReadStreamBuffering = true;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    // 3) Получаем ответ
                    using var response = (HttpWebResponse)await request.GetResponseAsync();
                    using var stream = response.GetResponseStream();
                    using var reader = new StreamReader(stream);

                    // 4) Читаем весь JSON одним куском
                    string json = await reader.ReadToEndAsync();

                    // 5) Десериализуем обёртку
                    var wrapper = JsonSerializer.Deserialize<JsonResponse>(json);
                    if (wrapper?.ok == true && wrapper.response.HasValue)
                    {
                        // 6) Десериализуем UserProfile из внутреннего JSON
                        result = JsonSerializer.Deserialize<UserProfile>(
                            wrapper.response.Value.GetRawText()
                        );
                        return result;
                    }
                    else
                    {
                        Console.WriteLine("GetUserProfileAsync: неверный формат ответа, повторяем.");
                    }
                }
                catch (WebException wex) when (attempt < 3)
                {
                    Console.WriteLine(
                        $"GetUserProfileAsync попытка {attempt} не удалась: {wex.Message}. Повтор через 500 мс");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetUserProfileAsync окончательно упала: {ex.Message}");
                    break;
                }
            }

            return result;
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
            const string endpoint = "http://139.28.223.134:5000/users/get_all_user_events";
            var result = new List<EventData>();

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    // 1) Формируем URL
                    string url = $"{endpoint}?username={Uri.EscapeDataString(username)}";

                    // 2) Создаём HttpWebRequest, буферизуем и включаем автоматическое распаковывание
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.AllowReadStreamBuffering = true;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    // 3) Получаем ответ
                    using var response = (HttpWebResponse)await request.GetResponseAsync();
                    using var stream = response.GetResponseStream();
                    using var reader = new StreamReader(stream);

                    // 4) Читаем весь JSON одним куском
                    string json = await reader.ReadToEndAsync();

                    // 5) Десериализуем обёртку
                    var wrapper = JsonSerializer.Deserialize<JsonResponse>(json);
                    if (wrapper?.ok == true && wrapper.response.HasValue)
                    {
                        // 6) Преобразуем внутренний массив в List<EventData>
                        var events = JsonSerializer.Deserialize<List<EventData>>(
                            wrapper.response.Value.GetRawText()
                        );
                        return events ?? new List<EventData>();
                    }
                    else
                    {
                        Console.WriteLine("GetAllUserEventsAsync: неожиданный формат ответа, повторяем.");
                    }
                }
                catch (WebException wex) when (attempt < 3)
                {
                    Console.WriteLine(
                        $"GetAllUserEventsAsync попытка {attempt} не удалась: {wex.Message}. Повтор через 500 мс");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetAllUserEventsAsync окончательно упала: {ex.Message}");
                    break;
                }
            }

            return result;
        }


        public static async Task<bool> AddEventAsync(EventData eventData)
        {
            Step:
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
                Console.WriteLine($"Error adding event: {ex.Message}");
                goto Step;
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
                        surname: default,
                        tags: default(List<string>), // tak kak eto zdez ne vazhno
                        vocation: u.vocation,
                        info: u.info,
                        skills: default(string) // tak kak eto zdez ne vazhno
                    )).ToList();
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

        #region favorites

        // ========================
        // Уведомления (лайки) 
        // ========================
        public static async Task<bool> AcceptUserAsync(string usernameTwo)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    string usernameOne = GetUsernameAsync();

                    var url = $"http://139.28.223.134:5000/likes/accept_user" +
                              $"?username_one={Uri.EscapeDataString(usernameOne)}" +
                              $"&username_two={Uri.EscapeDataString(usernameTwo)}";

                    var response = await _httpClient.PostAsync(url, null); 
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);
                    return jsonResponse?.ok ?? false;
                }
                catch (Exception ex) when (attempt < 3)
                {
                    Console.WriteLine($"AcceptUser attempt {attempt} failed: {ex.Message}. Retrying...");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AcceptUser error: {ex.Message}");
                }
            }

            return false;
        }

        public static async Task<bool> DeclineUserAsync(string usernameTwo)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    string usernameOne = GetUsernameAsync();

                    var url = $"http://139.28.223.134:5000/likes/decline_user" +
                              $"?username_one={Uri.EscapeDataString(usernameOne)}" +
                              $"&username_two={Uri.EscapeDataString(usernameTwo)}";

                    var response = await _httpClient.PostAsync(url, null);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var jsonResponse = JsonSerializer.Deserialize<JsonResponse>(responseBody);
                    return jsonResponse?.ok ?? false;
                }
                catch (Exception ex) when (attempt < 3)
                {
                    Console.WriteLine($"DeclineUser attempt {attempt} failed: {ex.Message}. Retrying...");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DeclineUser error: {ex.Message}");
                }
            }

            return false;
        }
        public static async Task<List<User>> GetRequestsUsersAsync()
        {
            const string endpoint = "http://139.28.223.134:5000/likes/get_requests";
            var list = new List<User>();

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    // 1) Формируем URL
                    string currentUser = await SecureStorage.GetAsync(UsernameKey) ?? string.Empty;
                    string url = $"{endpoint}?username={Uri.EscapeDataString(currentUser)}";

                    // 2) Создаём HttpWebRequest с буферизацией и распаковкой
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.AllowReadStreamBuffering = true;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    // 3) Получаем ответ
                    using var response = (HttpWebResponse)await request.GetResponseAsync();
                    using var stream = response.GetResponseStream();
                    using var reader = new StreamReader(stream);

                    // 4) Читаем весь JSON одним куском
                    string json = await reader.ReadToEndAsync();

                    // 5) Десериализуем обёртку
                    var wrapper = JsonSerializer.Deserialize<JsonResponse>(json);
                    if (wrapper?.ok == true && wrapper.response.HasValue)
                    {
                        // 6) Собираем List<User>
                        foreach (var item in wrapper.response.Value.EnumerateArray())
                        {
                            var uname = item.GetProperty("username").GetString() ?? "";
                            var name = item.GetProperty("name").GetString() ?? "";
                            var surname = item.GetProperty("surname").GetString() ?? "";
                            var voc = item.GetProperty("vocation").GetString() ?? "";
                            var info = item.GetProperty("description").GetString() ?? "";

                            var user = new User(
                                username: uname,
                                name: name,
                                surname: surname,
                                tags: new List<string>(),      // можно расширить при необходимости
                                vocation: voc,
                                info: info,
                                skills: ""                     // нет данных — пустая строка
                            );
                            Console.WriteLine($"{name} {surname}");
                            list.Add(user);
                        }

                        return list;
                    }
                }
                catch (WebException wex) when (attempt < 3)
                {
                    Console.WriteLine($"GetRequestsUsersAsync попытка {attempt} не удалась: {wex.Message}. Повтор через 500 мс");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetRequestsUsersAsync окончательно упала: {ex.Message}");
                }
            }

            return list;
        }



        public static async Task<List<UserData>> GetAcceptedUsersDataAsync()
        {
            const string endpoint = "http://139.28.223.134:5000/likes/get_accepted"; // likes endpoint :contentReference[oaicite:0]{index=0}&#8203;:contentReference[oaicite:1]{index=1}
            var list = new List<UserData>();

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    // 1) Формируем URL
                    string currentUser = await SecureStorage.GetAsync(UsernameKey) ?? string.Empty;
                    string url = $"{endpoint}?username={Uri.EscapeDataString(currentUser)}";

                    // 2) Создаём HttpWebRequest с буферизацией
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.AllowReadStreamBuffering = true;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    // 3) Получаем ответ
                    using var response = (HttpWebResponse)await request.GetResponseAsync();
                    using var stream = response.GetResponseStream();
                    using var reader = new StreamReader(stream);

                    // 4) Читаем весь ответ разом
                    string json = await reader.ReadToEndAsync();

                    // 5) Десериализуем и собираем UserData
                    var wrapper = JsonSerializer.Deserialize<JsonResponse>(json);
                    if (wrapper?.ok == true && wrapper.response.HasValue)
                    {
                        foreach (var item in wrapper.response.Value.EnumerateArray())
                        {
                            list.Add(new UserData
                            {
                                username = item.GetProperty("username").GetString() ?? "",
                                name = item.GetProperty("name").GetString() ?? "",
                                surname = item.GetProperty("surname").GetString() ?? "",
                                vocation = item.GetProperty("vocation").GetString() ?? "",
                                info = item.GetProperty("description").GetString() ?? ""
                            });
                        }
                        return list;
                    }
                }
                catch (WebException wex) when (attempt < 3)
                {
                    Console.WriteLine($"GetAcceptedUsersDataAsync попытка {attempt} не удалась: {wex.Message}. Повтор через 500 мс");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetAcceptedUsersDataAsync окончательно упала: {ex.Message}");
                }
            }

            return list;
        }
        public static async Task<bool> DeleteProfileSwipedUserAsync(string usernameTo)
        {
            string username = GetUsernameAsync();
            
            string url = $"http://139.28.223.134:5000/users/delete_profile_swiped_user?username={Uri.EscapeDataString(username)}&username_to={Uri.EscapeDataString(usernameTo)}";

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                        
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST"; // всё ещё POST
                    request.AllowReadStreamBuffering = true;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    // ничего не отправляем в теле
                    request.ContentLength = 0;

                    using var response = (HttpWebResponse)await request.GetResponseAsync();
                    using var stream = response.GetResponseStream();
                    using var reader = new StreamReader(stream);
                    string json = await reader.ReadToEndAsync();

                    var wrapper = JsonSerializer.Deserialize<JsonResponse>(json);
                    if (wrapper?.ok == true)
                        return true;

                    Console.WriteLine($"DeleteProfileSwipedUserAsync: неверный ответ: {json}");
                }
                catch (WebException wex) when (attempt < 3)
                {
                    Console.WriteLine($"DeleteProfileSwipedUserAsync попытка {attempt} не удалась: {wex.Message}. Повтор через 500 мс");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DeleteProfileSwipedUserAsync окончательно упала: {ex.Message}");
                    break;
                }
            }

            return false;
        }
        #endregion

        #region Avatar

        // Помощник для правильного Content-Type по расширению
        private static string GetContentType(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        // В секции Avatar
        public static async Task<bool> UploadUserAvatarAsync(byte[] fileData, string fileName)
        {
            const string endpoint = "http://139.28.223.134:5000/photos/upload";
            var username = GetUsernameAsync() ?? string.Empty;

            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            using var content = new MultipartFormDataContent();

            // Теперь ByteArrayContent точно не упадёт при копировании
            var byteContent = new ByteArrayContent(fileData);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));
            content.Add(byteContent, "file", fileName);

            content.Add(new StringContent(username), "username");

            var response = await _httpClient.PostAsync(endpoint, content);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonResponse>(json)?.ok == true;
        }

        public static async Task<ImageSource?> GetUserAvatarAsync(string username)
        {
            var url = $"http://139.28.223.134:5000/photos/image/{Uri.EscapeDataString(username)}";

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        Console.WriteLine($"Размер загруженной аватарки: {bytes.Length} байт");
                        return ImageSource.FromStream(() => new MemoryStream(bytes));
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        // Если код 400 - аватарки точно нет, прерываем цикл без дальнейших попыток
                        Console.WriteLine("GetUserAvatarAsync: Аватарка не найдена (400 Bad Request)");
                        return null;
                    }

                    Console.WriteLine($"GetUserAvatarAsync: статус {response.StatusCode}");
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is IOException)
                {
                    if (attempt < 3)
                    {
                        Console.WriteLine($"Попытка {attempt} не удалась: {ex.Message}. Повтор через 500 мс");
                        await Task.Delay(500);
                    }
                    else
                    {
                        Console.WriteLine($"GetUserAvatarAsync окончательно упала: {ex.Message}");
                    }
                }
            }

            return null;
        }


        #endregion

        public class UserData
        {
            public string username { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
            public string vocation { get; set; }
            public string info { get; set; }
        }
    }
}