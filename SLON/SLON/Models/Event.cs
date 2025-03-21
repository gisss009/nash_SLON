using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Tags { get; set; }
    public string Info { get; set; }
    public string Place { get; set; }

    public Event(int id, string name, string tags, string info, string place)
    {
<<<<<<< HEAD
        Id = id;
        Name = name;
        Tags = tags;
        Info = info;
        Place = place;
=======
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Categories { get; set; }
        public string Info { get; set; }
        public string Place { get; set; }
        public bool IsPublic { get; set; }
        public bool IsOnline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Список для хранения добавленных участников
        public List<User> AddedParticipants { get; set; } = new();

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
>>>>>>> eca5211 (Update Main_Page)
    }
}
