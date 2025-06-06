﻿using SLON.Services;
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

        private async static void InitializeTestData()
        {
            mutual.Add(new User("test2", "Test", "Test", new List<string>(), "sdf", "sdf", "asfd", new List<string>()));
            favorites = await AuthService.GetSwipedUsersAsync(AuthService.GetUsernameAsync());

            if (_initialized) return;
            _initialized = true;
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
            RejectedUsers.Clear();
            RejectedEvents.Clear();
            SecureStorage.Remove("RejectedUsers");
            SecureStorage.Remove("RejectedEvents");
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

            var mutualUsers = await AuthService.GetMutualUsersAsync(username);
            mutual = new ObservableCollection<User>(mutualUsers);

        }
        public static ObservableCollection<string> RejectedUsers { get; } = new();
        public static ObservableCollection<string> RejectedEvents { get; } = new();

        public static async Task LoadRejectedFromStorage()
        {
            var rejectedUsers = await SecureStorage.GetAsync("RejectedUsers");
            if (!string.IsNullOrEmpty(rejectedUsers))
            {
                foreach (var user in rejectedUsers.Split(','))
                {
                    if (!string.IsNullOrEmpty(user))
                        RejectedUsers.Add(user);
                }
            }

            var rejectedEvents = await SecureStorage.GetAsync("RejectedEvents");
            if (!string.IsNullOrEmpty(rejectedEvents))
            {
                foreach (var ev in rejectedEvents.Split(','))
                {
                    if (!string.IsNullOrEmpty(ev))
                        RejectedEvents.Add(ev);
                }
            }
        }

        public static async Task SaveRejectedToStorage()
        {
            await SecureStorage.SetAsync("RejectedUsers", string.Join(",", RejectedUsers));
            await SecureStorage.SetAsync("RejectedEvents", string.Join(",", RejectedEvents));
        }

    }
}
