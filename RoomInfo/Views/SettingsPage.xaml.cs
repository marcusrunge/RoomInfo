﻿using RoomInfo.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoomInfo.Views
{
    // TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page
    {
        private SettingsViewModel ViewModel => DataContext as SettingsViewModel;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void flyoutGrid_Loading(FrameworkElement sender, object args)
        {
            sender.DataContext = DataContext;
        }
    }
}