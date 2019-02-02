using Microsoft.Practices.Unity;
using ModelLibrary;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using RoomInfo.Helpers;
using ApplicationServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace RoomInfo.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        IDatabaseService _databaseService;
        List<AgendaItem> _agendaItems;
        CalendarPanel calendarPanel;
        IEventAggregator _eventAggregator;
        readonly IUnityContainer _unityContainer;
        AgendaItem _agendaItem;
        double _agendaItemWidth;
        IDateTimeValidationService _dateTimeValidationService;

        string _topDate = default(string);
        public string TopDate { get => _topDate; set { SetProperty(ref _topDate, value); } }

        int _id = default(int);
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        DateTimeOffset _startDate = default(DateTimeOffset);
        public DateTimeOffset StartDate { get => _startDate; set { SetProperty(ref _startDate, value); EndDate = StartDate; } }

        DateTimeOffset _endDate = default(DateTimeOffset);
        public DateTimeOffset EndDate { get => _endDate; set { SetProperty(ref _endDate, value); } }

        TimeSpan _startTime = default(TimeSpan);
        public TimeSpan StartTime
        {
            get => _startTime; set
            {
                SetProperty(ref _startTime, value);
                EndTime = StartTime;
                IsReservationButtonEnabled = _dateTimeValidationService.Validate(new AgendaItem() { Start = StartDate.Add(StartTime), End = EndDate.Add(EndTime) }, _agendaItems);
            }
        }

        TimeSpan _endTime = default(TimeSpan);
        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                SetProperty(ref _endTime, value);
                IsReservationButtonEnabled = _dateTimeValidationService.Validate(new AgendaItem() { Start = StartDate.Add(StartTime), End = EndDate.Add(EndTime) }, _agendaItems);
            }
        }

        bool _isReservationButtonEnabled = default(bool);
        public bool IsReservationButtonEnabled { get => _isReservationButtonEnabled; set { SetProperty(ref _isReservationButtonEnabled, value); } }

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

        public ScheduleViewModel(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
            _databaseService = unityContainer.Resolve<IDatabaseService>();
            _dateTimeValidationService = unityContainer.Resolve<IDateTimeValidationService>();
            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
        }

        public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            await UpdateCalendarViewDayItems();
            _eventAggregator.GetEvent<DeleteReservationEvent>().Subscribe(async (o) =>
            {
                try
                {
                    await _databaseService.RemoveAgendaItemAsync(o as AgendaItem);
                    await UpdateCalendarViewDayItems((o as AgendaItem).Start.Date);
                }
                catch { }
            });
            _eventAggregator.GetEvent<UpdateReservationEvent>().Subscribe((x) =>
            {
                _agendaItem = x;
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
                IsReservationButtonEnabled = true;
            });
            _eventAggregator.GetEvent<RemoteAgendaItemsUpdatedEvent>().Subscribe(async () => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await UpdateCalendarViewDayItems()));
        }

        private ICommand _showReservationFlyoutCommand;
        public ICommand ShowReservationFlyoutCommand => _showReservationFlyoutCommand ?? (_showReservationFlyoutCommand = new DelegateCommand<object>((param) =>
            {
                var now = DateTime.Now;
                StartDate = now.Date;
                StartTime = TimeSpan.FromSeconds(now.TimeOfDay.TotalSeconds);
                EndDate = now.Date;
                EndTime = TimeSpan.FromSeconds(now.TimeOfDay.TotalSeconds);
                Title = "";
                Description = "";
                IsAllDayEvent = false;
                SelectedComboBoxIndex = 2;
                IsReservationButtonEnabled = false;
            }));

        private ICommand _hideReservationCommand;
        public ICommand HideReservationCommand => _hideReservationCommand ?? (_hideReservationCommand = new DelegateCommand<object>((param) =>
        {
            (((param as Grid).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
            IsFlyoutOpen = false;
            Id = 0;
        }));

        private ICommand _addOrUpdateReservationCommand;
        public ICommand AddOrUpdateReservationCommand => _addOrUpdateReservationCommand ?? (_addOrUpdateReservationCommand = new DelegateCommand<object>(async (param) =>
        {
            try
            {
                bool hasDateChanged = false;
                DateTime previousDate = DateTime.MinValue;
                if (_agendaItem != null)
                {
                    hasDateChanged = _agendaItem.Start.Date != StartDate.Date ? true : false;
                    previousDate = _agendaItem.Start.Date;
                }
                else _agendaItem = new AgendaItem();

                StartDate = StartDate.Add(StartDate.TimeOfDay + StartTime);
                EndDate = EndDate.Date;
                EndDate = EndDate.Add(EndDate.TimeOfDay + EndTime);
                if (!_dateTimeValidationService.Validate(new AgendaItem() { Start = StartDate, End = EndDate, Id = _agendaItem.Id }, await _databaseService.GetAgendaItemsAsync())) return;
                if (Id == 0) await _databaseService.AddAgendaItemAsync(new AgendaItem() { EventAggregator = _eventAggregator, Title = Title, Start = StartDate, End = EndDate, Description = Description, IsAllDayEvent = IsAllDayEvent, Occupancy = SelectedComboBoxIndex, TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() });
                else
                {
                    _agendaItem.Title = Title;
                    _agendaItem.Start = StartDate;
                    _agendaItem.End = EndDate;
                    _agendaItem.Description = Description;
                    _agendaItem.IsAllDayEvent = IsAllDayEvent;
                    _agendaItem.Occupancy = SelectedComboBoxIndex;
                    _agendaItem.TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    await _databaseService.UpdateAgendaItemAsync(_agendaItem);
                }
                Id = 0;
                (((param as Grid).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
                IsFlyoutOpen = false;
                await UpdateCalendarViewDayItems(StartDate.Date);
                if (hasDateChanged) await UpdateCalendarViewDayItems(previousDate);
            }
            catch { }

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

        private ICommand _updateWidthCommand;
        public ICommand UpdateWidthCommand => _updateWidthCommand ?? (_updateWidthCommand = new DelegateCommand<object>((param) =>
        {
            if (param == null) return;
            else _eventAggregator.GetEvent<UpdateWidthEvent>().Publish((double)param);
        }));

        private async Task UpdateCalendarViewDayItems()
        {
            _agendaItems = await _databaseService.GetAgendaItemsAsync();
            _agendaItems.Sort();
            var calendarViewDayItems = calendarPanel.Children().OfType<CalendarViewDayItem>();
            foreach (var calendarViewDayItem in calendarViewDayItems)
            {
                List<AgendaItem> dayAgendaItems = _agendaItems.Where((x) => x.Start.Date == calendarViewDayItem.Date.Date).Select((x) =>
                {
                    x.EventAggregator = _eventAggregator;
                    return x;
                }).ToList();
                calendarViewDayItem.DataContext = dayAgendaItems;
                calendarViewDayItem.GotFocus += (s, e) =>
                {
                    FlyoutParent = s as FrameworkElement;
                };
                _agendaItemWidth = calendarViewDayItem.ActualWidth;
            }
            _eventAggregator.GetEvent<UpdateWidthEvent>().Publish(_agendaItemWidth);
        }

        private async Task UpdateCalendarViewDayItems(DateTime dateTime)
        {
            _agendaItems = await _databaseService.GetAgendaItemsAsync();
            _agendaItems.Sort();
            var calendarViewDayItems = calendarPanel.Children().OfType<CalendarViewDayItem>();
            foreach (var calendarViewDayItem in calendarViewDayItems)
            {
                if (calendarViewDayItem.Date.DateTime.Date == dateTime.Date)
                {
                    List<AgendaItem> dayAgendaItems = _agendaItems.Where((x) => x.Start.Date == calendarViewDayItem.Date.Date).Select((x) =>
                    {
                        x.EventAggregator = _eventAggregator;
                        return x;
                    }).ToList();
                    calendarViewDayItem.DataContext = dayAgendaItems;
                    calendarViewDayItem.GotFocus += (s, e) =>
                    {
                        FlyoutParent = s as FrameworkElement;
                    };
                    _agendaItemWidth = calendarViewDayItem.ActualWidth;
                }
                _eventAggregator.GetEvent<UpdateWidthEvent>().Publish(_agendaItemWidth);
            }
        }
    }
}
