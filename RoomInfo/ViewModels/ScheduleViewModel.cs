﻿using System;
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

        public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            //await _databaseService.AddAgendaItemAsync(new AgendaItem() { Title = "Test1", DateTime = "15:00" });
            //await _databaseService.AddAgendaItemAsync(new AgendaItem() { Title = "Test2", DateTime = "16:00" });
            //var agendaItems = await _databaseService.GetAgendaItemsAsync();
        }

        private ICommand _showReservationFlyoutCommand;
        public ICommand ShowReservationFlyoutCommand => _showReservationFlyoutCommand ?? (_showReservationFlyoutCommand = new DelegateCommand<object>(async (param) =>
            {
                _flyout = (param as Flyout);
            }));

        private ICommand _hideReservationFlyoutCommand;
        public ICommand HideReservationFlyoutCommand => _hideReservationFlyoutCommand ?? (_hideReservationFlyoutCommand = new DelegateCommand<object>((param) =>
        {
            _flyout.Hide();
        }));

        private ICommand _addReservationCommand;
        public ICommand AddReservationCommand => _addReservationCommand ?? (_addReservationCommand = new DelegateCommand<object>((param) =>
        {
        }));

        private ICommand _deleteReservationCommand;
        public ICommand DeleteReservationCommand => _deleteReservationCommand ?? (_deleteReservationCommand = new DelegateCommand<object>((param) =>
        {
        }));
    }
}
