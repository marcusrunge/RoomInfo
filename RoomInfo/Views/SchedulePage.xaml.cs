﻿using System;

using RoomInfo.ViewModels;

using Windows.UI.Xaml.Controls;

namespace RoomInfo.Views
{
    public sealed partial class SchedulePage : Page
    {
        private ScheduleViewModel ViewModel => DataContext as ScheduleViewModel;

        public SchedulePage()
        {
            InitializeComponent();
        }
    }
}
