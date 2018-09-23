using Prism.Windows.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomInfo.Models
{
    public class CalendarWeek : ViewModelBase
    {
        ObservableCollection<AgendaItem> _weekDayOne = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> WeekDayOne { get => _weekDayOne; set { SetProperty(ref _weekDayOne, value); } }

        ObservableCollection<AgendaItem> _weekDayTwo = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> WeekDayTwo { get => _weekDayTwo; set { SetProperty(ref _weekDayTwo, value); } }

        ObservableCollection<AgendaItem> _weekDayThree = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> WeekDayThree { get => _weekDayThree; set { SetProperty(ref _weekDayThree, value); } }

        ObservableCollection<AgendaItem> _weekDayFour = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> WeekDayFour { get => _weekDayFour; set { SetProperty(ref _weekDayFour, value); } }

        ObservableCollection<AgendaItem> _weekDayFive = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> WeekDayFive { get => _weekDayFive; set { SetProperty(ref _weekDayFive, value); } }

        ObservableCollection<AgendaItem> _weekDaySix = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> WeekDaySix { get => _weekDaySix; set { SetProperty(ref _weekDaySix, value); } }

        ObservableCollection<AgendaItem> _weekDaySeven = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> WeekDaySeven { get => _weekDaySeven; set { SetProperty(ref _weekDaySeven, value); } }
    }
}
