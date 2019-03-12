using Microsoft.EntityFrameworkCore;
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
        IEventAggregator _eventAggregator = default(IEventAggregator);
        [JsonIgnore]
        [NotMapped]
        public IEventAggregator EventAggregator { get => _eventAggregator; set { SetProperty(ref _eventAggregator, value); } }

        int _id = default(int);
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        int _dayOfWeek = default(int);
        public int DayOfWeek { get => _dayOfWeek; set { SetProperty(ref _dayOfWeek, value); } }

        TimeSpan _start = default(TimeSpan);
        public TimeSpan Start { get => _start; set { SetProperty(ref _start, value); } }

        TimeSpan _end = default(TimeSpan);
        public TimeSpan End { get => _end; set { SetProperty(ref _end, value); } }

        int _occupancy = default(int);
        public int Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        public long TimeStamp { get; set; }

        public bool IsDeleted { get; set; }

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
