using System;

using RoomInfo.ViewModels;

using Windows.UI.Xaml.Controls;

namespace RoomInfo.Views
{
    public sealed partial class InfoPage : Page
    {
        private InfoViewModel ViewModel => DataContext as InfoViewModel;

        public InfoPage()
        {
            InitializeComponent();
        }
    }
}
