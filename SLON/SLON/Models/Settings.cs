namespace SLON.Models
{
    public static class Settings
    {
        public static HashSet<string> selectedUserCategories { get; set; } = new();
        public static HashSet<string> selectedEventCategories { get; set; } = new();

        // Новые свойства для фильтрации событий
        public static bool? SelectedEventIsPublic { get; set; }
        public static bool? SelectedEventIsOnline { get; set; }
        public static DateTime? SelectedEventStartDate { get; set; }
        public static DateTime? SelectedEventEndDate { get; set; }
    }
}
