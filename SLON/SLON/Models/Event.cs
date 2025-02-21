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
        Id = id;
        Name = name;
        Tags = tags;
        Info = info;
        Place = place;
    }
}
