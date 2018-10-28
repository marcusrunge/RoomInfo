using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace RoomInfo.Helpers
{
    public class TimeFormatConverter : IValueConverter
    {     
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan)value).ToString(@"hh\:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
