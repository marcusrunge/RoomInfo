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
        private ListView rightTappedListView;

        public SchedulePage()
        {
            InitializeComponent();            
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            var attachedFlyout = Flyout.GetAttachedFlyout(frameworkElement);
            attachedFlyout.ShowAt(frameworkElement);
        }
        
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = sender as MenuFlyoutItem;
            if (menuFlyoutItem != null && rightTappedListView != null)
            {
                var contextFlyout = menuFlyoutItem.ContextFlyout;
                contextFlyout.ShowAt(rightTappedListView);
            }
        }

        private void listView_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            rightTappedListView = sender as ListView;
        }

        private void flyoutGrid_Loading(FrameworkElement sender, object args)
        {
            sender.DataContext = DataContext;
        }

        private void hideReservationButton_Click(object sender, RoutedEventArgs e)
        {
            reservationFlyout.Hide();
        }

        private void saveReservationButton_Click(object sender, RoutedEventArgs e)
        {
            reservationFlyout.Hide();
        }
    }
}
