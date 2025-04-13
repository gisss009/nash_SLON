using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SLON.Models
{
    public class ChatSwipeVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return true;

            bool isEvent = (values[0] is bool b) && b;
            bool isChatAvailable = (values[1] is bool chat) && chat;

            return isEvent || isChatAvailable;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
