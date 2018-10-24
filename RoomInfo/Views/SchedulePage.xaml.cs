using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoomInfo.Models;
using RoomInfo.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RoomInfo.Views
{
    public sealed partial class SchedulePage : Page
    {
        private ScheduleViewModel ViewModel => DataContext as ScheduleViewModel;
        private List<AgendaItem> agendaItems;
        public SchedulePage()
        {
            InitializeComponent();
            agendaItems = new List<AgendaItem>();
            for (int i = 0; i < 10; i++)
            {
                agendaItems.Add(new AgendaItem() { Title = "i = " + i });
            }
        }

        private void calendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {            
            if (args.Phase == 0)
            {                
                args.Item.DataContext = agendaItems;
            }
        }
    }
}
