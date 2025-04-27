using System;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace SLON.Models
{
    public static class Settings
    {
        private const string KeyUserCats = "UserCategories";
        private const string KeyEventCats = "EventCategories";
        private const string KeyEventPublic = "EventIsPublic";
        private const string KeyEventOnline = "EventIsOnline";
        private const string KeyEventStart = "EventStartDate";
        private const string KeyEventEnd = "EventEndDate";

        private static string _prefix = "";

        // Фильтры пользователя
        public static HashSet<string> selectedUserCategories { get; set; }
        public static HashSet<string> selectedEventCategories { get; set; }
        public static bool? SelectedEventIsPublic { get; set; }
        public static bool? SelectedEventIsOnline { get; set; }
        public static DateTime? SelectedEventStartDate { get; set; }
        public static DateTime? SelectedEventEndDate { get; set; }

        /// <summary>
        /// Инициализирует настройки конкретного пользователя.
        /// Вызывать сразу после логина / получения username.
        /// </summary>
        public static void Init(string username)
        {
            _prefix = string.IsNullOrWhiteSpace(username) ? "" : $"{username}_";

            // Загружаем категории пользователей
            var uc = Preferences.Get(_prefix + KeyUserCats, "");
            selectedUserCategories = new HashSet<string>(
                uc.Split(',', StringSplitOptions.RemoveEmptyEntries)
            );

            // Загружаем категории ивентов
            var ec = Preferences.Get(_prefix + KeyEventCats, "");
            selectedEventCategories = new HashSet<string>(
                ec.Split(',', StringSplitOptions.RemoveEmptyEntries)
            );

            // Загружаем флаги public/online (дефолт: public=true, online=false)
            SelectedEventIsPublic = Preferences.Get(_prefix + KeyEventPublic, true);
            SelectedEventIsOnline = Preferences.Get(_prefix + KeyEventOnline, false);

            var sd = Preferences.Get(_prefix + KeyEventStart, "");
            var ed = Preferences.Get(_prefix + KeyEventEnd, "");
            SelectedEventStartDate = DateTime.TryParse(sd, out var d1) ? d1 : (DateTime?)null;
            SelectedEventEndDate = DateTime.TryParse(ed, out var d2) ? d2 : (DateTime?)null;
        }

        /// <summary>
        /// Сохраняет текущие настройки в Preferences под ключами с префиксом.
        /// Вызывать после изменения фильтров (например, в OnSaveClicked).
        /// </summary>
        public static void Save()
        {
            Preferences.Set(_prefix + KeyUserCats, string.Join(",", selectedUserCategories));
            Preferences.Set(_prefix + KeyEventCats, string.Join(",", selectedEventCategories));
            Preferences.Set(_prefix + KeyEventPublic, SelectedEventIsPublic ?? true);
            Preferences.Set(_prefix + KeyEventOnline, SelectedEventIsOnline ?? false);
            Preferences.Set(_prefix + KeyEventStart, SelectedEventStartDate?.ToString("o") ?? "");
            Preferences.Set(_prefix + KeyEventEnd, SelectedEventEndDate?.ToString("o") ?? "");
        }
    }
}
