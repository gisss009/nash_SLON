using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            if (text != null)
            {
                // Минимальная ширина 100, увеличивается на 8 пикселей за символ
                return Math.Max(100, text.Length * 10);
            }
            return 100; // Ширина по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value; // Обратное преобразование не требуется
        }
    }
}

