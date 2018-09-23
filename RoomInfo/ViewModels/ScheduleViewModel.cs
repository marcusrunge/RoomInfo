using System;
using System.Collections.ObjectModel;
using Prism.Windows.Mvvm;
using RoomInfo.Models;

namespace RoomInfo.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        ObservableCollection<CalendarWeek> _calendarWeeks = default(ObservableCollection<CalendarWeek>);
        public ObservableCollection<CalendarWeek> CalendarWeeks { get => _calendarWeeks; set { SetProperty(ref _calendarWeeks, value); } }

        public ScheduleViewModel()
        {
            CalendarWeeks = new ObservableCollection<CalendarWeek>();
            for (int i = 0; i < 10; i++)
            {
                CalendarWeeks.Add(new CalendarWeek());
            }
        }
    }
}
