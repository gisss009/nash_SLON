using SLON.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

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
            var username = AuthService.GetUsernameAsync();

            // Свайпнутые пользователи
            var swipedUsers = await AuthService.GetSwipedUsersAsync(username);
            favorites = swipedUsers ?? new List<User>();

            // Свайпнутые события
            var swipedEvents = await AuthService.GetSwipedEventsAsync(username);
            favoriteEvents.Clear();
            foreach (var ed in swipedEvents)
            {
                // Преобразуем EventData в Event-модель
                var start = DateTime.ParseExact(ed.date_from, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var end = DateTime.ParseExact(ed.date_to, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var ev = new Event(ed.hash, ed.name, ed.categories, ed.description, ed.location,
                                   ed.@public == 1, ed.online == 1)
                {
                    StartDate = start,
                    EndDate = end
                };
                favoriteEvents.Add(ev);
            }
        }

    }
}
