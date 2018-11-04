using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using RoomInfo.Events;
using RoomInfo.Helpers;
using RoomInfo.Models;
using RoomInfo.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace RoomInfo.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        Flyout _flyout;
        IDatabaseService _databaseService;
        List<AgendaItem> _agendaItems;
        CalendarPanel calendarPanel;
        IEventAggregator _eventAggregator;

        string _topDate = default(string);
        public string TopDate { get => _topDate; set { SetProperty(ref _topDate, value); } }

        //ObservableCollection<CalendarWeek> _calendarWeeks = default(ObservableCollection<CalendarWeek>);
        //public ObservableCollection<CalendarWeek> CalendarWeeks { get => _calendarWeeks; set { SetProperty(ref _calendarWeeks, value); } }

        DateTimeOffset _startDate = default(DateTimeOffset);
        public DateTimeOffset StartDate { get => _startDate; set { SetProperty(ref _startDate, value); EndDate = StartDate; } }

        DateTimeOffset _endDate = default(DateTimeOffset);
        public DateTimeOffset EndDate { get => _endDate; set { SetProperty(ref _endDate, value); } }

        TimeSpan _startTime = default(TimeSpan);
        public TimeSpan StartTime { get => _startTime; set { SetProperty(ref _startTime, value); EndTime = StartTime; } }

        TimeSpan _endTime = default(TimeSpan);
        public TimeSpan EndTime { get => _endTime; set { SetProperty(ref _endTime, value); } }

        bool _isAllDayEvent = default(bool);
        public bool IsAllDayEvent { get => _isAllDayEvent; set
            {
                SetProperty(ref _isAllDayEvent, value);
                if (_isAllDayEvent)
                {
                    StartTime = TimeSpan.FromHours(0);
                    EndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
                }
            }}

        string _title = default(string);
        public string Title { get => _title; set { SetProperty(ref _title, value); } }

        string _description = default(string);
        public string Description { get => _description; set { SetProperty(ref _description, value); } }

        //ObservableCollection<AgendaItem> _agendaItems = default(ObservableCollection<AgendaItem>);
        //public ObservableCollection<AgendaItem> AgendaItems { get => _agendaItems; set { SetProperty(ref _agendaItems, value); } }

        public ScheduleViewModel(IDatabaseService databaseService, IEventAggregator eventAggregator)
        {
            _databaseService = databaseService;
            _eventAggregator = eventAggregator;
        }

        public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);            
            await UpdateCalendarViewDayItems();
            _eventAggregator.GetEvent<DeleteReservationEvent>().Subscribe(async (o) =>
            {
                await _databaseService.RemoveAgendaItemAsync(o as AgendaItem);
                await UpdateCalendarViewDayItems();
            });
            _eventAggregator.GetEvent<UpdateReservationEvent>().Subscribe((o) =>
            {

            });
        }        

        private ICommand _showReservationFlyoutCommand;
        public ICommand ShowReservationFlyoutCommand => _showReservationFlyoutCommand ?? (_showReservationFlyoutCommand = new DelegateCommand<object>(async (param) =>
            {
                _flyout = (param as Flyout);
                var now = DateTime.Now;
                StartDate = now.Date;
                StartTime = TimeSpan.FromTicks(now.TimeOfDay.Ticks);
                EndDate = now.Date;
                EndTime = TimeSpan.FromTicks(now.TimeOfDay.Ticks);
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
            StartDate = StartDate.Add(StartDate.TimeOfDay + StartTime);
            EndDate = EndDate.Add(EndDate.TimeOfDay + EndTime);
            await _databaseService.AddAgendaItemAsync(new AgendaItem(_eventAggregator) { Title = Title, Start = StartDate, End = EndDate, Description = Description, IsAllDayEvent = IsAllDayEvent });
            _flyout.Hide();
            _flyout = null;            
            await UpdateCalendarViewDayItems();
        }));        

        private ICommand _handleCalendarViewDayItemChangingCommand;
        public ICommand HandleCalendarViewDayItemChangingCommand => _handleCalendarViewDayItemChangingCommand ?? (_handleCalendarViewDayItemChangingCommand = new DelegateCommand<object>((param) =>
        {

            var frameworkElementCalendarViewDayItemChangingEventArgs = param as CalendarViewDayItemChangingEventArgs;
            if(calendarPanel == null) calendarPanel = frameworkElementCalendarViewDayItemChangingEventArgs.Item.Parent as CalendarPanel;
        }));

        private async Task UpdateCalendarViewDayItems()
        {
            _agendaItems = await _databaseService.GetAgendaItemsAsync();
            var calendarViewDayItems = calendarPanel.Children().OfType<CalendarViewDayItem>();
            foreach (var calendarViewDayItem in calendarViewDayItems)
            {
                List<AgendaItem> dayAgendaItems = _agendaItems.Where((x) => x.Start.Date == calendarViewDayItem.Date.Date).Select((x) => x).ToList();
                calendarViewDayItem.DataContext = dayAgendaItems;                
            }
        }
    }
}
