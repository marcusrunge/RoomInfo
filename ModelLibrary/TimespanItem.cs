﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ModelLibrary
{
    public class TimeSpanItemContext : DbContext
    {
        public DbSet<TimeSpanItem> TimeSpanItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=RoomInfo.db");
        }
    }

    public class TimeSpanItem : BindableBase, IComparable
    {
        private IEventAggregator _eventAggregator = default;

        [JsonIgnore]
        [NotMapped]
        public IEventAggregator EventAggregator { get => _eventAggregator; set { SetProperty(ref _eventAggregator, value); } }

        private int _id = default;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        private int _dayOfWeek = default;
        public int DayOfWeek { get => _dayOfWeek; set { SetProperty(ref _dayOfWeek, value); } }

        private TimeSpan _start = default;
        public TimeSpan Start { get => _start; set { SetProperty(ref _start, value); } }

        private TimeSpan _end = default;
        public TimeSpan End { get => _end; set { SetProperty(ref _end, value); } }

        private int _occupancy = default;
        public int Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        public long TimeStamp { get; set; }

        public bool IsDeleted { get; set; }

        private double _width = default;

        [JsonIgnore]
        [NotMapped]
        public double Width { get => _width; set { SetProperty(ref _width, value); } }

        private ICommand _updateTimespanItemCommand;

        [JsonIgnore]
        [NotMapped]
        public ICommand UpdateTimespanItemCommand => _updateTimespanItemCommand ?? (_updateTimespanItemCommand = new DelegateCommand<object>((param) =>
        {
            EventAggregator.GetEvent<UpdateTimespanItemEvent>().Publish(this);
        }));

        private ICommand _deleteTimespanItemCommand;

        [JsonIgnore]
        [NotMapped]
        public ICommand DeleteTimespanItemCommand => _deleteTimespanItemCommand ?? (_deleteTimespanItemCommand = new DelegateCommand<object>((param) =>
        {
            EventAggregator.GetEvent<DeleteTimespanItemEvent>().Publish(param);
        }));

        private ICommand _showAttachedFlyoutCommand;

        [JsonIgnore]
        [NotMapped]
        public ICommand ShowAttachedFlyoutCommand => _showAttachedFlyoutCommand ?? (_showAttachedFlyoutCommand = new DelegateCommand<object>((param) =>
        {
            FrameworkElement frameworkElement = param as FrameworkElement;
            var attachedFlyout = Flyout.GetAttachedFlyout(frameworkElement);
            attachedFlyout.ShowAt(frameworkElement);
        }));

        public int CompareTo(object obj)
        {
            return ((IComparable)Start).CompareTo(((TimeSpanItem)obj).Start);
        }

        private ICommand _relayGotFocusCommand;

        [JsonIgnore]
        [NotMapped]
        public ICommand RelayGotFocusCommand => _relayGotFocusCommand ?? (_relayGotFocusCommand = new DelegateCommand<object>((param) =>
        {
            _eventAggregator.GetEvent<GotFocusEvent>().Publish(param as FrameworkElement);
        }));
    }
}