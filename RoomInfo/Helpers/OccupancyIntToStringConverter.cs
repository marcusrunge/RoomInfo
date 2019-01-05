using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace RoomInfo.Helpers
{
    public class OccupancyIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            switch ((int)value)
            {
                case 0: return resourceLoader.GetString("Info_OccupancyFree/Content");
                case 1: return resourceLoader.GetString("Info_OccupancyPresent/Content");
                case 2: return resourceLoader.GetString("Info_OccupancyAbsent/Content");
                case 3: return resourceLoader.GetString("Info_OccupancyBusy/Content");
                case 4: return resourceLoader.GetString("Info_OccupancyOccupied/Content");
                case 5: return resourceLoader.GetString("Info_OccupancyLocked/Content");
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
