using System;

using Prism.Windows.Mvvm;

namespace RoomInfo.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        string _occupancy = default(string);
        public string Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        public InfoViewModel()
        {
        }
    }
}
