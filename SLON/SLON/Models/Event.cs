﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models;

public class Event
{
    public int Id;
    public string Name;
    public string Tags;
    public string Info;
    public string Place;

    public Event(int id, string name, string info, string place)
    {
        Id = id;
        Name = name;
        Info = info;
        Place = place;
    }
}