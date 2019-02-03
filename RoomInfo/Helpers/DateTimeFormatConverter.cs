using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace RoomInfo.Helpers
{
    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((DateTimeOffset)value).ToString(@"dd.MM.yy ") + ((DateTimeOffset)value).TimeOfDay.ToString(@"hh\:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
