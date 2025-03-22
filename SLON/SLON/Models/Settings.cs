using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models;

public class Settings
{
    static public HashSet<string> selectedCategories { get; set; } = new();
}
