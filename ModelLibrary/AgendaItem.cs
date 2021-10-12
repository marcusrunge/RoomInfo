using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ModelLibrary
{
    public class AgendaItemContext : DbContext
    {
        public DbSet<AgendaItem> AgendaItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=RoomInfo.db");
        }
    }

    public class AgendaItem : BindableBase, IComparable
    {
        private CoreDispatcher _coreDispatcher;

        private IEventAggregator _eventAggregator = default;

        [JsonIgnore]
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

        private int _id = default;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        private string _title = default;
        public string Title { get => _title; set { SetProperty(ref _title, value); } }

        private DateTimeOffset _start = default;
        public DateTimeOffset Start { get => _start; set { SetProperty(ref _start, value); } }

        private DateTimeOffset _end = default;
        public DateTimeOffset End { get => _end; set { SetProperty(ref _end, value); } }

        private bool _isAllDayEvent = default;
        public bool IsAllDayEvent { get => _isAllDayEvent; set { SetProperty(ref _isAllDayEvent, value); } }

        private bool _isOverridden = default;
        public bool IsOverridden { get => _isOverridden; set { SetProperty(ref _isOverridden, value); } }

        private string _description = default;
        public string Description { get => _description; set { SetProperty(ref _description, value); } }

        private int _occupancy = default;
        public int Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        public long TimeStamp { get; set; }

        public bool IsDeleted { get; set; }

        private double _width = default;

        [JsonIgnore]
        [NotMapped]
        public double Width { get => _width; set { SetProperty(ref _width, value); } }

        private double _mediumFontSize = default;

        [JsonIgnore]
        [NotMapped]
        public double MediumFontSize { get => _mediumFontSize; set { SetProperty(ref _mediumFontSize, value); } }

        private double _largeFontSize = default;

        [JsonIgnore]
        [NotMapped]
        public double LargeFontSize { get => _largeFontSize; set { SetProperty(ref _largeFontSize, value); } }

        private Visibility _dueTimeVisibility = default;

        [JsonIgnore]
        [NotMapped]
        public Visibility DueTimeVisibility { get => _dueTimeVisibility; set { SetProperty(ref _dueTimeVisibility, value); } }

        private string _dueTime = default;

        [JsonIgnore]
        [NotMapped]
        public string DueTime { get => _dueTime; set { SetProperty(ref _dueTime, value); } }

        public AgendaItem()
        {
            DueTimeVisibility = Visibility.Collapsed;
            var coreWindow = CoreWindow.GetForCurrentThread();
            if (coreWindow != null) _coreDispatcher = coreWindow.Dispatcher;
        }

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

        public int CompareTo(object obj)
        {
            return ((IComparable)Start).CompareTo(((AgendaItem)obj).Start);
        }

        public void SetDueTime()
        {
            var now = DateTime.Now;
            if (Start > now)
            {
                TimeSpan startTimeSpan;
                int countdown = 18;
                if (Start - now < TimeSpan.FromMinutes(18))
                {
                    countdown = (Start - now).Minutes;
                    startTimeSpan = TimeSpan.FromMilliseconds(1);
                    var resourceLoader = ResourceLoader.GetForCurrentView();
                    DueTime = resourceLoader.GetString("AgendaItem_Due/Text") + " " + countdown.ToString() + "min";
                    DueTimeVisibility = Visibility.Visible;
                }
                else startTimeSpan = (Start - TimeSpan.FromMinutes(18)) - now;
                ThreadPoolTimer startThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
                {
                    if (_coreDispatcher == null)
                    {
                        try
                        {
                            _coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                    else try
                        {
                            await _coreDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                            {
                                DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
                                dispatcherTimer.Tick += async (s, e) =>
                                {
                                    if (countdown > 1)
                                    {
                                        DueTimeVisibility = Visibility.Visible;
                                        await _coreDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                        {
                                            var resourceLoader = ResourceLoader.GetForCurrentView();
                                            DueTime = resourceLoader.GetString("AgendaItem_Due/Text") + " " + countdown.ToString() + "min";
                                            countdown--;
                                        });
                                    }
                                    else
                                    {
                                        (s as DispatcherTimer).Stop();
                                        DueTimeVisibility = Visibility.Collapsed;
                                    }
                                };
                                dispatcherTimer.Start();
                            });
                        }
                        catch { }
                }, startTimeSpan);
            }
        }
    }
}