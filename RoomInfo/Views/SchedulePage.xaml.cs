using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoomInfo.Models;
using RoomInfo.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace RoomInfo.Views
{
    public sealed partial class SchedulePage : Page
    {
        private ScheduleViewModel ViewModel => DataContext as ScheduleViewModel;

        public SchedulePage()
        {
            InitializeComponent();            
        }       

        private void flyoutGrid_Loading(FrameworkElement sender, object args)
        {
            sender.DataContext = DataContext;
        }
    }
}
