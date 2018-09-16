using Prism.Windows.Mvvm;

namespace RoomInfo.Models
{
    public class AgendaItem : ViewModelBase
    {
        string _title = default(string);
        public string Title { get => _title; set { SetProperty(ref _title, value); } }

        string _dateTime = default(string);
        public string DateTime { get => _dateTime; set { SetProperty(ref _dateTime, value); } }
    }
}
