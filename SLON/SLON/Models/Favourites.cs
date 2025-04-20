using SLON.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SLON.Models
{
    public static class Favourites
    {
        // Все пользователи, которых я лайкнул (All)
        public static List<User> favorites { get; set; } = new();

        public static ObservableCollection<Event> favoriteEvents { get; set; } = new();

        // Список взаимных лайков (Mutual)
        public static ObservableCollection<User> mutual { get; set; } = new();

        // Список тех, кто меня запросил (Requests)
        public static ObservableCollection<User> requests { get; set; } = new();

        // Список тех, кто меня уже принял (Accepted)
        public static ObservableCollection<User> accepted { get; set; } = new();

        // Флаг начальной инициализации, если требуется
        private static bool _initialized = false;

        static Favourites()
        {
            // Инициализация может выполняться здесь, но актуальное обновление должно происходить отдельно.
            // InitializeTestData(); // Если тестовые данные нужны в режиме отладки
        }

        /// <summary>
        /// Очищает все локальные списки избранного.
        /// Вызывается при выходе из аккаунта.
        /// </summary>
        public static void ResetFavorites()
        {
            favorites.Clear();
            favoriteEvents.Clear();
            mutual.Clear();
            requests.Clear();
            accepted.Clear();
        }

        /// <summary>
        /// Обновляет (перезагружает) избранное для текущего пользователя.
        /// Вызывается после успешного входа/регистрации.
        /// </summary>
        public static async Task RefreshFavoritesAsync()
        {
            // Получаем актуальные свайпнутые пользователи для текущего аккаунта
            var username = AuthService.GetUsernameAsync();
            Debug.WriteLine($"[RefreshFavoritesAsync] Получен username: {username}");
            var swiped = await AuthService.GetSwipedUsersAsync(username);
            Debug.WriteLine($"[RefreshFavoritesAsync] Получено пользователей: {(swiped != null ? swiped.Count.ToString() : "null")}");

            favorites = swiped ?? new List<User>();

            // Выведите содержимое favorites, чтобы убедиться, что список не пустой, если сервер вернул данные.
            foreach (var user in favorites)
            {
                Debug.WriteLine($"[RefreshFavoritesAsync] Пользователь: {user.Username} - {user.Name}");
            }
        }

    }
}
