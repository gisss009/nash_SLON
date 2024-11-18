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
        Id = id;
        Name = name;
        Tags = tags;
        Vocation = vocation;
        Info = info;
        Skills = skills;
    }
}

