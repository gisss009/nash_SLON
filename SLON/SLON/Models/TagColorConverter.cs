using System.Globalization;
using Microsoft.Maui.Controls;

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
                    "IT" => Color.FromArgb("#3541DC"),
                    "Creation" => Color.FromArgb("#0A6779"),
                    "Sport" => Color.FromArgb("#A92123"),
                    "Science" => Color.FromArgb("#038756"),
                    "Business" => Color.FromArgb("#640693"),
                    "Education" => Color.FromArgb("#B55E24"),
                    "Social" => Color.FromArgb("#FF6F61"),
                    "Health" => Color.FromArgb("#6B5B95"),
                    // Если подкатегории совпадают с основными – используем тот же цвет:
                    "Art" or "Design" or "Innovation" or "Creativity" or "Prototyping" => Color.FromArgb("#0A6779"),
                    "Learning" or "Teaching" or "Courses" or "Workshops" or "Tutoring" => Color.FromArgb("#B55E24"),
                    "Programming" or "Cybersecurity" or "AI" or "Cloud Computing" => Color.FromArgb("#3541DC"),
                    "Community" or "Networking" or "Events" or "Charity" or "Social Media" => Color.FromArgb("#FF6F61"),
                    "Entrepreneurship" or "Management" or "Marketing" or "Finance" or "Startups" => Color.FromArgb("#640693"),
                    "Research" or "Physics" or "Biology" or "Chemistry" or "Astronomy" => Color.FromArgb("#038756"),
                    "Football" or "Basketball" or "Tennis" or "Running" or "Cycling" => Color.FromArgb("#A92123"),
                    "Fitness" or "Nutrition" or "Wellness" or "Yoga" or "Medicine" => Color.FromArgb("#6B5B95"),
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
