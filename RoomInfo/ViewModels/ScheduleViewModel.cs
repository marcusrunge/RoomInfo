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
                var calendarWeek = new CalendarWeek
                {
                    WeekDayOneDate = "01.01",
                    WeekDayTwoDate = "02.01",
                    WeekDayThreeDate = "03.01",
                    WeekDayFourDate = "04.01",
                    WeekDayFiveDate = "05.01",
                    WeekDaySixDate = "06.01",
                    WeekDaySevenDate = "07.01",
                    WeekDayOne = new ObservableCollection<AgendaItem>(),
                    WeekDayTwo = new ObservableCollection<AgendaItem>(),
                    WeekDayThree = new ObservableCollection<AgendaItem>(),
                    WeekDayFour = new ObservableCollection<AgendaItem>(),
                    WeekDayFive = new ObservableCollection<AgendaItem>(),
                    WeekDaySix = new ObservableCollection<AgendaItem>(),
                    WeekDaySeven = new ObservableCollection<AgendaItem>()
                };
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
