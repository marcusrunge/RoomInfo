using System;
using System.Collections.ObjectModel;
using Prism.Windows.Mvvm;
using RoomInfo.Models;
using RoomInfo.Services;

namespace RoomInfo.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        IDatabaseService _databaseService;

        ObservableCollection<CalendarWeek> _calendarWeeks = default(ObservableCollection<CalendarWeek>);
        public ObservableCollection<CalendarWeek> CalendarWeeks { get => _calendarWeeks; set { SetProperty(ref _calendarWeeks, value); } }

        public ScheduleViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            CalendarWeeks = new ObservableCollection<CalendarWeek>();
            for (int i = 0; i < 10; i++)
            {
                var calendarWeek = new CalendarWeek();
                calendarWeek.WeekDayOne = new ObservableCollection<AgendaItem>();
                calendarWeek.WeekDayTwo = new ObservableCollection<AgendaItem>();
                calendarWeek.WeekDayThree = new ObservableCollection<AgendaItem>();
                calendarWeek.WeekDayFour = new ObservableCollection<AgendaItem>();
                calendarWeek.WeekDayFive = new ObservableCollection<AgendaItem>();
                calendarWeek.WeekDaySix = new ObservableCollection<AgendaItem>();
                calendarWeek.WeekDaySeven = new ObservableCollection<AgendaItem>();
                for (int j = 0; j < 5; j++)
                {
                    calendarWeek.WeekDayOne.Add(new AgendaItem());
                    calendarWeek.WeekDayTwo.Add(new AgendaItem());
                    calendarWeek.WeekDayThree.Add(new AgendaItem());
                    calendarWeek.WeekDayFour.Add(new AgendaItem());
                    calendarWeek.WeekDayFive.Add(new AgendaItem());
                    calendarWeek.WeekDaySix.Add(new AgendaItem());
                    calendarWeek.WeekDaySeven.Add(new AgendaItem());
                }
                CalendarWeeks.Add(calendarWeek);
            }
        }
    }
}
