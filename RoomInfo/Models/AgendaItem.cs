﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using RoomInfo.Events;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoomInfo.Models
{
    public class AgendaItemContext : DbContext
    {
        public DbSet<AgendaItem> AgendaItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=AgendaItems.db");
        }
    }

    public class AgendaItem : DataModelBase
    {
        [NotMapped]
        public IEventAggregator EventAggregator { get; set; }     

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

        string _description = default(string);
        public string Description { get => _description; set { SetProperty(ref _description, value); } }

        int _occupancy = default(int);
        public int Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        private ICommand _updateReservationCommand;
        public ICommand UpdateReservationCommand => _updateReservationCommand ?? (_updateReservationCommand = new DelegateCommand<object>((param) =>
        {
            EventAggregator.GetEvent<UpdateReservationEvent>().Publish(this);
        }));

        private ICommand _deleteReservationCommand;
        public ICommand DeleteReservationCommand => _deleteReservationCommand ?? (_deleteReservationCommand = new DelegateCommand<object>((param) =>
        {
            EventAggregator.GetEvent<DeleteReservationEvent>().Publish(param);
        }));

        private ICommand _showAttachedFlyoutCommand;
        public ICommand ShowAttachedFlyoutCommand => _showAttachedFlyoutCommand ?? (_showAttachedFlyoutCommand = new DelegateCommand<object>((param) =>
        {
            FrameworkElement frameworkElement = param as FrameworkElement;
            var attachedFlyout = Flyout.GetAttachedFlyout(frameworkElement);
            attachedFlyout.ShowAt(frameworkElement);
        }));
    }
}
