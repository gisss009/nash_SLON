using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models;

public class Settings
{
    public static HashSet<string> selectedUserCategories { get; set; } = new();
    public static HashSet<string> selectedEventCategories { get; set; } = new();
}
