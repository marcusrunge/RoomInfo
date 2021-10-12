using ModelLibrary;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace RoomInfo.Helpers
{
    public class PropertyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            if (value.GetType() == typeof(int))
            {
                switch ((OccupancyVisualState)((int)value))
                {
                    case OccupancyVisualState.FreeVisualState: return resourceLoader.GetString("Info_OccupancyFree/Content");
                    case OccupancyVisualState.PresentVisualState: return resourceLoader.GetString("Info_OccupancyPresent/Content");
                    case OccupancyVisualState.AbsentVisualState: return resourceLoader.GetString("Info_OccupancyAbsent/Content");
                    case OccupancyVisualState.BusyVisualState: return resourceLoader.GetString("Info_OccupancyBusy/Content");
                    case OccupancyVisualState.OccupiedVisualState: return resourceLoader.GetString("Info_OccupancyOccupied/Content");
                    case OccupancyVisualState.LockedVisualState: return resourceLoader.GetString("Info_OccupancyLocked/Content");
                    case OccupancyVisualState.HomeVisualState: return resourceLoader.GetString("Info_OccupancyHome/Content");
                    case OccupancyVisualState.UndefinedVisualState: return null;
                    default: return null;
                }
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}