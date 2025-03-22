namespace SLON.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Categories { get; set; }
        public string Info { get; set; }
        public string Place { get; set; }
        public bool IsPublic { get; set; }
        public bool IsOnline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Список для хранения добавленных участников
        public List<User> AddedParticipants { get; set; } = new();

        public Event(int id, string name, string categories, string info, string place, bool is_public, bool is_online)
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
