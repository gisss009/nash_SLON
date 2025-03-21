using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Tags { get; set; }
    public string Vocation { get; set; }
    public string Info { get; set; }
    public string Skills { get; set; }

    public User(int id, string name, string tags, string vocation, string info, string skills)
    {
<<<<<<< HEAD
        Id = id;
        Name = name;
        Tags = tags;
        Vocation = vocation;
        Info = info;
        Skills = skills;
=======
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public Dictionary<string, List<string>> Tags_dic { get; set; }
        public string Vocation { get; set; }   
        public string Info { get; set; }     
        public string Skills { get; set; }     

        public bool IsMutual { get; set; }     // Если true - во "взаимных"
        public bool IsAcceptedMe { get; set; } // Принял ли он меня
        public bool IsILikedHim { get; set; }  // Лайкнул ли я его

        public User(int id, string name, List<string> tags, string vocation, string info, string skills)
        {
            Id = id;
            Name = name;
            Tags = tags;
            Vocation = vocation;
            Info = info;
            Skills = skills;
            IsMutual = false;
            IsAcceptedMe = false;
            IsILikedHim = false;
        }
>>>>>>> eca5211 (Update Main_Page)
    }
}

