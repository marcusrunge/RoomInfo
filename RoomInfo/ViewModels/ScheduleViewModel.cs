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
        IDatabaseService _databaseService;
        List<AgendaItem> _agendaItems;
        CalendarPanel calendarPanel;
        IEventAggregator _eventAggregator;

        string _topDate = default(string);
        public string TopDate { get => _topDate; set { SetProperty(ref _topDate, value); } }

        int _id = default(int);
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        DateTimeOffset _startDate = default(DateTimeOffset);
        public DateTimeOffset StartDate { get => _startDate; set { SetProperty(ref _startDate, value); EndDate = StartDate; } }

        DateTimeOffset _endDate = default(DateTimeOffset);
        public DateTimeOffset EndDate { get => _endDate; set { SetProperty(ref _endDate, value); } }

        TimeSpan _startTime = default(TimeSpan);
        public TimeSpan StartTime { get => _startTime; set { SetProperty(ref _startTime, value); EndTime = StartTime; } }

        TimeSpan _endTime = default(TimeSpan);
        public TimeSpan EndTime { get => _endTime; set { SetProperty(ref _endTime, value); } }

        bool _isAllDayEvent = default(bool);
        public bool IsAllDayEvent
        {
            get => _isAllDayEvent; set
            {
                SetProperty(ref _isAllDayEvent, value);
                if (_isAllDayEvent)
                {
                    StartTime = TimeSpan.FromHours(0);
                    EndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
                }
            }
        }

        string _title = default(string);
        public string Title { get => _title; set { SetProperty(ref _title, value); } }

        string _description = default(string);
        public string Description { get => _description; set { SetProperty(ref _description, value); } }

        bool _isFlyoutOpen = default(bool);
        public bool IsFlyoutOpen { get => _isFlyoutOpen; set { SetProperty(ref _isFlyoutOpen, value); } }

        FrameworkElement _flyoutParent = default(FrameworkElement);
        public FrameworkElement FlyoutParent { get => _flyoutParent; set { SetProperty(ref _flyoutParent, value); } }

        int _selectedComboBoxIndex = default(int);
        public int SelectedComboBoxIndex { get => _selectedComboBoxIndex; set { SetProperty(ref _selectedComboBoxIndex, value); } }

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
                await UpdateCalendarViewDayItems((o as AgendaItem).Start.Date);
            });
            _eventAggregator.GetEvent<UpdateReservationEvent>().Subscribe((x) =>
            {
                Id = (x).Id;
                StartDate = (x).Start.Date;
                StartTime = (x).Start.TimeOfDay;
                EndDate = (x).End.Date;
                EndTime = (x).End.TimeOfDay;
                Title = (x).Title;
                Description = (x).Description;
                IsAllDayEvent = (x).IsAllDayEvent;
                SelectedComboBoxIndex = (x).Occupancy;
                IsFlyoutOpen = true;
            });
        }

        private ICommand _showReservationFlyoutCommand;
        public ICommand ShowReservationFlyoutCommand => _showReservationFlyoutCommand ?? (_showReservationFlyoutCommand = new DelegateCommand<object>(async (param) =>
            {
                var now = DateTime.Now;
                StartDate = now.Date;
                StartTime = TimeSpan.FromTicks(now.TimeOfDay.Ticks);
                EndDate = now.Date;
                EndTime = TimeSpan.FromTicks(now.TimeOfDay.Ticks);
                Title = "";
                Description = "";
                IsAllDayEvent = false;
                SelectedComboBoxIndex = 2;
            }));

        private ICommand _hideReservationCommand;
        public ICommand HideReservationCommand => _hideReservationCommand ?? (_hideReservationCommand = new DelegateCommand<object>((param) =>
        {
            (((param as Grid).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
            IsFlyoutOpen = false;
        }));

        private ICommand _addOrUpdateReservationCommand;
        public ICommand AddOrUpdateReservationCommand => _addOrUpdateReservationCommand ?? (_addOrUpdateReservationCommand = new DelegateCommand<object>(async (param) =>
        {
            StartDate = StartDate.Add(StartDate.TimeOfDay + StartTime);
            EndDate = EndDate.Add(EndDate.TimeOfDay + EndTime);
            if (Id == 0) await _databaseService.AddAgendaItemAsync(new AgendaItem(_eventAggregator) { Title = Title, Start = StartDate, End = EndDate, Description = Description, IsAllDayEvent = IsAllDayEvent, Occupancy = SelectedComboBoxIndex });
            else await _databaseService.UpdateAgendaItemAsync(new AgendaItem(_eventAggregator) { Id = Id, Title = Title, Start = StartDate, End = EndDate, Description = Description, IsAllDayEvent = IsAllDayEvent, Occupancy = SelectedComboBoxIndex });
            Id = 0;
            (((param as Grid).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
            IsFlyoutOpen = false;
            await UpdateCalendarViewDayItems(StartDate.Date);
        }));

        private ICommand _handleCalendarViewDayItemChangingCommand;
        public ICommand HandleCalendarViewDayItemChangingCommand => _handleCalendarViewDayItemChangingCommand ?? (_handleCalendarViewDayItemChangingCommand = new DelegateCommand<object>((param) =>
        {
            var frameworkElementCalendarViewDayItemChangingEventArgs = param as CalendarViewDayItemChangingEventArgs;
            if (calendarPanel == null) calendarPanel = frameworkElementCalendarViewDayItemChangingEventArgs.Item.Parent as CalendarPanel;
        }));

        private ICommand _saveFlyoutCommand;
        public ICommand SaveFlyoutCommand => _saveFlyoutCommand ?? (_saveFlyoutCommand = new DelegateCommand<object>((param) =>
        {
        }));

        private async Task UpdateCalendarViewDayItems()
        {
            _agendaItems = await _databaseService.GetAgendaItemsAsync();
            var calendarViewDayItems = calendarPanel.Children().OfType<CalendarViewDayItem>();
            foreach (var calendarViewDayItem in calendarViewDayItems)
            {
                List<AgendaItem> dayAgendaItems = _agendaItems.Where((x) => x.Start.Date == calendarViewDayItem.Date.Date).Select((x) => x).ToList();
                calendarViewDayItem.DataContext = dayAgendaItems;
                calendarViewDayItem.GotFocus += (s, e) =>
                {
                    FlyoutParent = s as FrameworkElement;
                };
            }
        }

        private async Task UpdateCalendarViewDayItems(DateTime dateTime)
        {
            _agendaItems = await _databaseService.GetAgendaItemsAsync();
            var calendarViewDayItems = calendarPanel.Children().OfType<CalendarViewDayItem>();
            foreach (var calendarViewDayItem in calendarViewDayItems)
            {
                if (calendarViewDayItem.Date.DateTime.Date == dateTime.Date)
                {
                    List<AgendaItem> dayAgendaItems = _agendaItems.Where((x) => x.Start.Date == calendarViewDayItem.Date.Date).Select((x) => x).ToList();
                    calendarViewDayItem.DataContext = dayAgendaItems;
                    calendarViewDayItem.GotFocus += (s, e) =>
                    {
                        FlyoutParent = s as FrameworkElement;
                    };
                }
            }
        }
    }
}
