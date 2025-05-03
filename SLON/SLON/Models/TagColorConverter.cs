using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace SLON.Models
{
    public class TagColorConverter : IValueConverter
    {
        // Заполняется снаружи (в LoadProfileAsync), только пользовательские теги → категории
        public static Dictionary<string, string> TagToCategory { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string tag) || string.IsNullOrWhiteSpace(tag))
                return Colors.Gray;

            // 1) Сначала см. динамическую карту тег→основная категория
            if (TagToCategory.TryGetValue(tag.Trim(), out var mainCategory))
            {
                return ColorForCategory(mainCategory);
            }

            // 2) Если не нашли в динамике, возможно это сама категория ивента
            //    — покрасим по ней же
            return ColorForCategory(tag.Trim());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private Color ColorForCategory(string category) => category switch
        {
            "IT" => Color.FromArgb("#3541DC"),
            "Creation" => Color.FromArgb("#0A6779"),
            "Sport" => Color.FromArgb("#A92123"),
            "Science" => Color.FromArgb("#038756"),
            "Business" => Color.FromArgb("#640693"),
            "Education" => Color.FromArgb("#B55E24"),
            "Social" => Color.FromArgb("#FF6F61"),
            "Health" => Color.FromArgb("#6B5B95"),
            // здесь можно продолжить любые подкатегории, если нужно
            _ => Colors.Gray,
        };
    }
}
