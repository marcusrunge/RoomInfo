using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace RoomInfo.Helpers
{
    public class CalendarViewDayItemChangingEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = (CalendarViewDayItemChangingEventArgs)value;
            var element = (FrameworkElement)parameter;
            return args;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
