using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models;

class Favourites
{ 
    static public HashSet<User> favorites { get; set; } = new();
}
