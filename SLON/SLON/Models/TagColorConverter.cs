using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace SLON.Models
{
    public class TagColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category)
            {
                return category switch
                {
                    "Creation" => Color.FromArgb("#FFA36A"),
                    "Education" => Color.FromArgb("#53DAAF"),
                    "IT" => Color.FromArgb("#6A8DFF"),
                    "Social" => Color.FromArgb("#45D62B"),
                    "Business" => Color.FromArgb("#D6BC2B"),
                    "Science" => Color.FromArgb("#FF6A6A"),


                    //Creation
                    "Art" => Color.FromArgb("#FFA36A"),
                    "Design" => Color.FromArgb("#FFA36A"),
                    "Innovation" => Color.FromArgb("#FFA36A"),
                    "Creativity" => Color.FromArgb("#FFA36A"),
                    "Prototyping" => Color.FromArgb("#FFA36A"),

                    //"Education"
                    "Learning" => Color.FromArgb("#53DAAF"),
                    "Teaching" => Color.FromArgb("#53DAAF"),
                    "Courses" => Color.FromArgb("#53DAAF"),
                    "Workshops"=> Color.FromArgb("#53DAAF"),
                    "Tutoring" => Color.FromArgb("#53DAAF"),

                    // IT 1
                    "Programming" => Color.FromArgb("#6A8DFF"),
                    "Cybersecurity" => Color.FromArgb("#6A8DFF"),
                    "AI" => Color.FromArgb("#6A8DFF"),
                    "Cloud Computing" => Color.FromArgb("#6A8DFF"),

                    //Social
                    "Community"=> Color.FromArgb("#45D62B"),
                    "Networking"=> Color.FromArgb("#45D62B"),
                    "Events" => Color.FromArgb("#45D62B"),
                    "Charity" => Color.FromArgb("#45D62B"),
                    "Social Media" => Color.FromArgb("#45D62B"),

                    //Business

                    "Entrepreneurship" => Color.FromArgb("#D6BC2B"),
                    "Management"  => Color.FromArgb("#D6BC2B"),
                    "Marketing"  => Color.FromArgb("#D6BC2B"),
                    "Finance"  => Color.FromArgb("#D6BC2B"),
                    "Startups" => Color.FromArgb("#D6BC2B"),

                    //Science
                    "Research"  => Color.FromArgb("#FF6A6A"),
                    "Physics"  => Color.FromArgb("#FF6A6A"),
                    "Biology" => Color.FromArgb("#FF6A6A"),
                    "Chemistry" => Color.FromArgb("#FF6A6A"),
                    "Astronomy" => Color.FromArgb("#FF6A6A"),

                    _ => Colors.Gray
                };
            }
            return Color.FromArgb("#444");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
