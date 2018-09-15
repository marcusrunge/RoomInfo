using System;
using System.Collections.Generic;
using System.Globalization;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Windows.UI.Xaml;

namespace RoomInfo.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        string _occupancy = default(string);
        public string Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        string _clock = default(string);
        public string Clock { get => _clock; set { SetProperty(ref _clock, value); } }

        string _date = default(string);
        public string Date { get => _date; set { SetProperty(ref _date, value); } }

        string _room = default(string);
        public string Room { get => _room; set { SetProperty(ref _room, value); } }

        public InfoViewModel()
        {
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
            base.OnNavigatedTo(navigatedToEventArgs, viewModelState);
        }
    }
}
