using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using RoomInfo.Models;
using Windows.UI.Xaml;

namespace RoomInfo.ViewModels
{
    public enum OccupancyVisualState { FreeVisualState, BusyVisualState, OccupiedVisualState }
    public class InfoViewModel : ViewModelBase
    {
        OccupancyVisualState _occupancy = default(OccupancyVisualState);
        public OccupancyVisualState Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        string _clock = default(string);
        public string Clock { get => _clock; set { SetProperty(ref _clock, value); } }

        string _date = default(string);
        public string Date { get => _date; set { SetProperty(ref _date, value); } }

        string _room = default(string);
        public string Room { get => _room; set { SetProperty(ref _room, value); } }

        int _selectedComboBoxIndex = default(int);
        public int SelectedComboBoxIndex { get => _selectedComboBoxIndex; set { SetProperty(ref _selectedComboBoxIndex, value); } }

        ObservableCollection<AgendaItem> _agendaItems = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> AgendaItems { get => _agendaItems; set { SetProperty(ref _agendaItems, value); } }

        public InfoViewModel()
        {
            //Test
            AgendaItems = new ObservableCollection<AgendaItem>
            {
                new AgendaItem(),
                new AgendaItem()
            };
        }

        public override void OnNavigatedTo(NavigatedToEventArgs navigatedToEventArgs, Dictionary<string, object> viewModelState)
        {            
            CultureInfo cultureInfo = new CultureInfo("de-DE");
            Clock = DateTime.Now.ToString("t", cultureInfo) + " Uhr";
            Date = DateTime.Now.ToString("D", cultureInfo);
            DispatcherTimer dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            dispatcherTimer.Tick += (s, e) =>
            {
                Clock = DateTime.Now.ToString("t", cultureInfo) + " Uhr";
                Date = DateTime.Now.ToString("D", cultureInfo);
            };
            Occupancy = OccupancyVisualState.FreeVisualState;
            SelectedComboBoxIndex = 0;
            base.OnNavigatedTo(navigatedToEventArgs, viewModelState);
        }
    }
}
