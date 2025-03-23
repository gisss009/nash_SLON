namespace SLON.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Categories { get; set; }
        public string Info { get; set; }
        public string Place { get; set; }
        public bool IsPublic { get; set; }
        public bool IsOnline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<User> AddedParticipants { get; set; } = new();

        // Вычисляемое свойство для отображения диапазона дат
        public string DateRange => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";

        public Event(int id, string name, List<string> categories, string info, string place, bool is_public, bool is_online)
        {
            Id = id;
            Name = name;
            Categories = categories;
            Info = info;
            Place = place;
            IsPublic = is_public;
            IsOnline = is_online;
        }
    }
}
