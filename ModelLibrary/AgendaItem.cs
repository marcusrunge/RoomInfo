using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ModelLibrary
{
    public class AgendaItemContext : DbContext
    {
        public DbSet<AgendaItem> AgendaItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=AgendaItems.db");
        }
    }

    public class AgendaItem : INotifyPropertyChanged
    {                
        IEventAggregator _eventAggregator = default(IEventAggregator);
        [NotMapped]
        public IEventAggregator EventAggregator
        {
            get => _eventAggregator;
            set
            {
                SetProperty(ref _eventAggregator, value);
                EventAggregator.GetEvent<UpdateWidthEvent>().Subscribe(w => Width = w);
            }
        }

        int _id = default(int);
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        string _title = default(string);
        public string Title { get => _title; set { SetProperty(ref _title, value); } }

        DateTimeOffset _start = default(DateTimeOffset);
        public DateTimeOffset Start { get => _start; set { SetProperty(ref _start, value); } }

        DateTimeOffset _end = default(DateTimeOffset);
        public DateTimeOffset End { get => _end; set { SetProperty(ref _end, value); } }

        bool _isAllDayEvent = default(bool);
        public bool IsAllDayEvent { get => _isAllDayEvent; set { SetProperty(ref _isAllDayEvent, value); } }

        bool _isOverridden = default(bool);
        public bool IsOverridden { get => _isOverridden; set { SetProperty(ref _isOverridden, value); } }

        string _description = default(string);
        public string Description { get => _description; set { SetProperty(ref _description, value); } }

        int _occupancy = default(int);
        public int Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        public long TimeStamp { get; set; }

        public bool IsDeleted { get; set; }
                
        double _width = default(double);
        [JsonIgnore]
        [NotMapped]
        public double Width { get => _width; set { SetProperty(ref _width, value); } }
                
        double _mediumFontSize = default(double);
        [JsonIgnore]
        [NotMapped]
        public double MediumFontSize { get => _mediumFontSize; set { SetProperty(ref _mediumFontSize, value); } }

        double _largeFontSize = default(double);
        [JsonIgnore]
        [NotMapped]
        public double LargeFontSize { get => _largeFontSize; set { SetProperty(ref _largeFontSize, value); } }

        private ICommand _updateReservationCommand;
        [JsonIgnore]
        [NotMapped]
        public ICommand UpdateReservationCommand => _updateReservationCommand ?? (_updateReservationCommand = new DelegateCommand<object>((param) =>
        {
            EventAggregator.GetEvent<UpdateReservationEvent>().Publish(this);
        }));

        private ICommand _deleteReservationCommand;
        [JsonIgnore]
        [NotMapped]
        public ICommand DeleteReservationCommand => _deleteReservationCommand ?? (_deleteReservationCommand = new DelegateCommand<object>((param) =>
        {
            EventAggregator.GetEvent<DeleteReservationEvent>().Publish(param);
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);
            return true;
        }
        void RaisePropertyChanged([CallerMemberName]string propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
    }
}
