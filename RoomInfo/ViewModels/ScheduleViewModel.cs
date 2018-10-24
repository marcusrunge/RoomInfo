using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using RoomInfo.Models;
using RoomInfo.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace RoomInfo.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        Flyout _flyout;
        IDatabaseService _databaseService;

        string _topDate = default(string);
        public string TopDate { get => _topDate; set { SetProperty(ref _topDate, value); } }

        ObservableCollection<CalendarWeek> _calendarWeeks = default(ObservableCollection<CalendarWeek>);
        public ObservableCollection<CalendarWeek> CalendarWeeks { get => _calendarWeeks; set { SetProperty(ref _calendarWeeks, value); } }

        DateTimeOffset _startDate = default(DateTimeOffset);
        public DateTimeOffset StartDate { get => _startDate; set { SetProperty(ref _startDate, value); EndDate = StartDate; } }

        DateTimeOffset _endDate = default(DateTimeOffset);
        public DateTimeOffset EndDate { get => _endDate; set { SetProperty(ref _endDate, value); } }

        TimeSpan _startTime = default(TimeSpan);
        public TimeSpan StartTime { get => _startTime; set { SetProperty(ref _startTime, value); EndTime = StartTime; } }

        TimeSpan _endTime = default(TimeSpan);
        public TimeSpan EndTime { get => _endTime; set { SetProperty(ref _endTime, value); } }

        bool _isAllDayEvent = default(bool);
        public bool IsAllDayEvent { get => _isAllDayEvent; set { SetProperty(ref _isAllDayEvent, value); } }

        string _title = default(string);
        public string Title { get => _title; set { SetProperty(ref _title, value); } }

        string _description = default(string);
        public string Description { get => _description; set { SetProperty(ref _description, value); } }

        ObservableCollection<AgendaItem> _agendaItems = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> AgendaItems { get => _agendaItems; set { SetProperty(ref _agendaItems, value); } }

        public ScheduleViewModel(IDatabaseService databaseService)
        {            
            _databaseService = databaseService;
            CalendarWeeks = new ObservableCollection<CalendarWeek>();
            AgendaItems = new ObservableCollection<AgendaItem>();
            for (int i = 0; i < 10; i++)
            {
                AgendaItems.Add(new AgendaItem() { Title = "i = " + i});
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

        public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            //await _databaseService.AddAgendaItemAsync(new AgendaItem() { Title = "Test1", StartDate= DateTimeOffset.MinValue, EndDate= DateTimeOffset.MaxValue, StartTime = TimeSpan.MinValue, EndTime = TimeSpan.MaxValue, Description="Beschreibung", IsAllDayEvent=false });
            //await _databaseService.AddAgendaItemAsync(new AgendaItem() { Title = "Test2", StartDate = DateTimeOffset.MinValue, EndDate = DateTimeOffset.MaxValue, StartTime = TimeSpan.MinValue, EndTime = TimeSpan.MaxValue, Description = "Beschreibung", IsAllDayEvent = false });
            //var agendaItems = await _databaseService.GetAgendaItemsAsync();
            //if (agendaItems.Count == 0)
            //{

            //}
        }

        private ICommand _showReservationFlyoutCommand;
        public ICommand ShowReservationFlyoutCommand => _showReservationFlyoutCommand ?? (_showReservationFlyoutCommand = new DelegateCommand<object>(async (param) =>
            {
                _flyout = (param as Flyout);
                var now = DateTime.Now;
                StartDate = now;
                StartTime = TimeSpan.FromTicks(now.Ticks);
                EndDate = now;
                EndTime = TimeSpan.FromTicks(now.Ticks);
                Title = "";
                Description = "";
                IsAllDayEvent = false;
            }));

        private ICommand _hideReservationFlyoutCommand;
        public ICommand HideReservationFlyoutCommand => _hideReservationFlyoutCommand ?? (_hideReservationFlyoutCommand = new DelegateCommand<object>((param) =>
        {
            _flyout.Hide();
            _flyout = null;
        }));

        private ICommand _addOrUpdateReservationCommand;
        public ICommand AddOrUpdateReservationCommand => _addOrUpdateReservationCommand ?? (_addOrUpdateReservationCommand = new DelegateCommand<object>(async (param) =>
        {
            StartDate = StartDate.Add(StartTime - StartDate.TimeOfDay);
            EndDate = EndDate.Add(EndTime - EndDate.TimeOfDay);
            await _databaseService.AddAgendaItemAsync(new AgendaItem() { Title = Title, StartDate = StartDate, EndDate = EndDate, StartTime = StartTime, EndTime = EndTime, Description = Description, IsAllDayEvent = IsAllDayEvent });
            _flyout.Hide();
            _flyout = null;
        }));               

        private ICommand _deleteReservationCommand;
        public ICommand DeleteReservationCommand => _deleteReservationCommand ?? (_deleteReservationCommand = new DelegateCommand<object>((param) =>
        {
        }));
    }
}
