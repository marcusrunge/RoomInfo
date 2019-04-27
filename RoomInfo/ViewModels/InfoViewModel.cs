using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using ModelLibrary;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using ApplicationServiceLibrary;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Prism.Events;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Newtonsoft.Json;
using NetworkServiceLibrary;
using Windows.Networking;
using System.Linq;

namespace RoomInfo.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        IDatabaseService _databaseService;
        IApplicationDataService _applicationDataService;
        ILiveTileUpdateService _liveTileUpdateService;
        IEventAggregator _eventAggregator;
        IIotService _iotService;
        IUserDatagramService _userDatagramService;
        AgendaItem _activeAgendaItem;
        double _agendaItemWidth;
        ResourceLoader _resourceLoader;
        Package _propertyChangedPackage;
        ThreadPoolTimer _startThreadPoolTimer, _endThreadPoolTimer, _startTimeSpanThreadPoolTimer, _stopTimeSpanThreadPoolTimer;
        DayOfWeek _dayOfWeek;
        CoreDispatcher _coreDispatcher;

        class WeekDayChangedEventArgs
        {
            public WeekDayChangedEventArgs(DayOfWeek dayOfWeek) { DayOfWeek = dayOfWeek; }
            public DayOfWeek DayOfWeek { get; }
        }
        delegate void WeekDayChangedEventHandler(object s, WeekDayChangedEventArgs e);
        event WeekDayChangedEventHandler _weekDayChangedEvent;
        void OnWeekDayChangedEvent(object s, WeekDayChangedEventArgs e) => _weekDayChangedEvent?.Invoke(s, e);

        OccupancyVisualState _occupancy = default;
        public OccupancyVisualState Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        string _clock = default;
        public string Clock { get => _clock; set { SetProperty(ref _clock, value); } }

        string _date = default;
        public string Date { get => _date; set { SetProperty(ref _date, value); } }

        string _room = default;
        public string Room { get => _room; set { SetProperty(ref _room, value); } }

        int _selectedComboBoxIndex = default;
        public int SelectedComboBoxIndex { get => _selectedComboBoxIndex; set { SetProperty(ref _selectedComboBoxIndex, value); } }

        ObservableCollection<AgendaItem> _agendaItems = default;
        public ObservableCollection<AgendaItem> AgendaItems { get => _agendaItems; set { SetProperty(ref _agendaItems, value); } }

        Uri _companyLogo = default;
        public Uri CompanyLogo { get => _companyLogo; set { SetProperty(ref _companyLogo, value); } }

        string _companyName = default;
        public string CompanyName { get => _companyName; set { SetProperty(ref _companyName, value); } }

        string _roomName = default;
        public string RoomName { get => _roomName; set { SetProperty(ref _roomName, value); } }

        string _roomNumber = default;
        public string RoomNumber { get => _roomNumber; set { SetProperty(ref _roomNumber, value); } }

        Visibility _resetButtonVisibility = default;
        public Visibility ResetButtonVisibility { get => _resetButtonVisibility; set { SetProperty(ref _resetButtonVisibility, value); } }

        double _mediumFontSize = default;
        public double MediumFontSize { get => _mediumFontSize; set { SetProperty(ref _mediumFontSize, value); } }

        double _mediumToLargeFontSize = default;
        public double MediumToLargeFontSize { get => _mediumToLargeFontSize; set { SetProperty(ref _mediumToLargeFontSize, value); } }

        double _largeFontSize = default;
        public double LargeFontSize { get => _largeFontSize; set { SetProperty(ref _largeFontSize, value); } }

        double _extraLargeFontSize = default;
        public double ExtraLargeFontSize { get => _extraLargeFontSize; set { SetProperty(ref _extraLargeFontSize, value); } }

        double _superLargeFontSize = default;
        public double SuperLargeFontSize { get => _superLargeFontSize; set { SetProperty(ref _superLargeFontSize, value); } }

        Visibility _brightnessAdjustmentVisibility = default;
        public Visibility BrightnessAdjustmentVisibility { get => _brightnessAdjustmentVisibility; set { SetProperty(ref _brightnessAdjustmentVisibility, value); } }

        public InfoViewModel(IUnityContainer unityContainer)
        {
            _databaseService = unityContainer.Resolve<IDatabaseService>();
            _applicationDataService = unityContainer.Resolve<IApplicationDataService>();
            _liveTileUpdateService = unityContainer.Resolve<ILiveTileUpdateService>();
            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
            _iotService = unityContainer.Resolve<IIotService>();
            _userDatagramService = unityContainer.Resolve<IUserDatagramService>();
            _propertyChangedPackage = new Package() { PayloadType = (int)PayloadType.PropertyChanged };
        }

        public async override void OnNavigatedTo(NavigatedToEventArgs navigatedToEventArgs, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigatedToEventArgs, viewModelState);
            _resourceLoader = ResourceLoader.GetForCurrentView();
            _coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            StorageFolder assets = null;
            IReadOnlyList<StorageFolder> storageFolders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
            foreach (var storageFolder in storageFolders)
            {
                if (storageFolder.Name.Equals("Logo"))
                {
                    assets = await ApplicationData.Current.LocalFolder.GetFolderAsync("Logo");
                    break;
                }
            }
            if (assets == null)
            {
                await ApplicationData.Current.LocalFolder.CreateFolderAsync("Logo");
                assets = await ApplicationData.Current.LocalFolder.GetFolderAsync("Logo");
            }
            string logoFileName = _applicationDataService.GetSetting<string>("LogoFileName");
            CompanyLogo = new Uri(assets.Path + "/" + logoFileName);
            CompanyName = _applicationDataService.GetSetting<string>("CompanyName");
            RoomName = _applicationDataService.GetSetting<string>("RoomName");
            RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber");
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Clock = DateTime.Now.ToString("t", cultureInfo) + " " + _resourceLoader.GetString("InfoViewModel_Clock");
            Date = DateTime.Now.ToString("D", cultureInfo);
            DispatcherTimer dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            dispatcherTimer.Tick += (s, e) =>
            {
                Clock = DateTime.Now.ToString("t", cultureInfo) + " " + _resourceLoader.GetString("InfoViewModel_Clock");
                Date = DateTime.Now.ToString("D", cultureInfo);
                if (_dayOfWeek != DateTime.Now.DayOfWeek)
                {
                    OnWeekDayChangedEvent(s, new WeekDayChangedEventArgs(DateTime.Now.DayOfWeek));
                    _dayOfWeek = DateTime.Now.DayOfWeek;
                }
            };
            dispatcherTimer.Start();
            SelectedComboBoxIndex = _applicationDataService.GetSetting<bool>("OccupancyOverridden") ? _applicationDataService.GetSetting<int>("OverriddenOccupancy") : _applicationDataService.GetSetting<int>("StandardOccupancy");
            ResetButtonVisibility = _applicationDataService.GetSetting<bool>("OccupancyOverridden") ? Visibility.Visible : Visibility.Collapsed;
            Occupancy = OccupancyVisualState.UndefinedVisualState;
            Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
            _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
            await UpdateStandardWeek(DateTime.Now.DayOfWeek);
            await UpdateDayAgenda();
            _eventAggregator.GetEvent<RemoteOccupancyOverrideEvent>().Subscribe(async i =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    SelectedComboBoxIndex = i;
                    Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                    await OverrideOccupancy();
                });
            });
            _eventAggregator.GetEvent<RemoteAgendaItemsUpdatedEvent>().Subscribe(async () => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await UpdateDayAgenda();
            }));
            BrightnessAdjustmentVisibility = _iotService.IsIotDevice() ? Visibility.Visible : Visibility.Collapsed;
            _eventAggregator.GetEvent<RemoteAgendaItemDeletedEvent>().Subscribe(async i =>
            {
                var agendaItem = AgendaItems.Where(x => x.Id == i).Select(x => x).FirstOrDefault();
                if (agendaItem != null)
                {
                    var now = DateTimeOffset.Now;
                    if (now >= agendaItem.Start && now <= agendaItem.End)
                    {
                        await _coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            SelectedComboBoxIndex = _applicationDataService.GetSetting<int>("StandardOccupancy");
                            _applicationDataService.SaveSetting("OverriddenOccupancy", (int)Occupancy);
                            _applicationDataService.SaveSetting("OccupancyOverridden", false);
                            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
                            ResetButtonVisibility = Visibility.Collapsed;
                            Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                            await UpdateDayAgenda();
                            await UpdateStandardWeek(DateTime.Now.DayOfWeek);
                        });
                    }
                }
            });
            _weekDayChangedEvent += async (s, e) => { await UpdateStandardWeek(e.DayOfWeek); };
            _eventAggregator.GetEvent<StandardWeekUpdatedEvent>().Subscribe(async i =>
            {
                await _coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if ((int)DateTime.Now.DayOfWeek == i)
                    {
                        ResetOccupancyCommand.Execute(null);
                        await UpdateStandardWeek(DateTime.Now.DayOfWeek);
                    }
                });
            });
        }

        async Task<bool> UpdateStandardWeek(DayOfWeek dayOfWeek)
        {
            var currentTimeSpanItem = (await _databaseService.GetTimeSpanItemsAsync()).Where(x => x.DayOfWeek == (int)dayOfWeek).Where(x => x.Start < DateTime.Now.TimeOfDay).Where(x => x.End > DateTime.Now.TimeOfDay).Select(x => x).FirstOrDefault();
            var nextTimeSpanItem = (await _databaseService.GetTimeSpanItemsAsync()).Where(x => x.DayOfWeek == (int)dayOfWeek).Where(x => x.Start >= DateTime.Now.TimeOfDay).Select(x => x).FirstOrDefault();

            if (currentTimeSpanItem != null)
            {
                await _coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ResetButtonVisibility = Visibility.Collapsed;
                    SelectedComboBoxIndex = currentTimeSpanItem.Occupancy;
                    Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                    _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
                    _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                    SetTimeSpanStopTimer(currentTimeSpanItem.End);
                });
                return true;
            }
            else if (nextTimeSpanItem != null)
            {
                SetTimeSpanStartTimer(nextTimeSpanItem);
            }
            return false;
        }

        void SetTimeSpanStartTimer(TimeSpanItem nextTimeSpanItem)
        {
            if (_startTimeSpanThreadPoolTimer != null) _startTimeSpanThreadPoolTimer.Cancel();
            TimeSpan startTimeSpan = nextTimeSpanItem.Start - DateTime.Now.TimeOfDay;
            _startTimeSpanThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
            {
                await _coreDispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                {
                    SelectedComboBoxIndex = nextTimeSpanItem.Occupancy;
                    Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                    _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
                    _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                });
            }, startTimeSpan);
            SetTimeSpanStopTimer(nextTimeSpanItem.End);
        }

        void SetTimeSpanStopTimer(TimeSpan end)
        {
            if (_stopTimeSpanThreadPoolTimer != null) _stopTimeSpanThreadPoolTimer.Cancel();
            TimeSpan stopTimeSpan = end - DateTime.Now.TimeOfDay;
            _stopTimeSpanThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
            {
                await _coreDispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                {
                    ResetOccupancyCommand.Execute(null);
                    await UpdateStandardWeek(DateTime.Now.DayOfWeek);
                });
            }, stopTimeSpan);
        }

        private async Task OverrideOccupancy()
        {
            _applicationDataService.SaveSetting("OverriddenOccupancy", SelectedComboBoxIndex);
            _applicationDataService.SaveSetting("OccupancyOverridden", true);
            if (_activeAgendaItem != null)
            {
                _activeAgendaItem.IsOverridden = true;
                await _databaseService.UpdateAgendaItemAsync(_activeAgendaItem);
            }
            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
            ResetButtonVisibility = _applicationDataService.GetSetting<int>("StandardOccupancy") == SelectedComboBoxIndex
                ? Visibility.Collapsed
                : Visibility.Visible;
            _applicationDataService.SaveSetting("ActualOccupancy", SelectedComboBoxIndex);
            await _userDatagramService.SendStringData(new HostName("255.255.255.255"), _applicationDataService.GetSetting<string>("UdpPort"), JsonConvert.SerializeObject(_propertyChangedPackage));
        }

        private async Task UpdateDayAgenda()
        {
            if (AgendaItems == null) AgendaItems = new ObservableCollection<AgendaItem>();
            else AgendaItems.Clear();
            DateTime dateTimeNow = DateTime.Now;
            var agendaItems = await _databaseService.GetAgendaItemsAsync(dateTimeNow);
            //agendaItems.Sort();
            for (int i = 0; i < agendaItems.Count; i++)
            {
                AgendaItems.Add(agendaItems[i]);
                try
                {
                    AgendaItems[i].SetDueTime();
                }
                catch (Exception e)
                {
                    if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                }
            }
            await UpdateTimerTask();
        }

        private async Task UpdateTimerTask()
        {
            try
            {
                if (_startThreadPoolTimer != null) _startThreadPoolTimer.Cancel();
                if (_endThreadPoolTimer != null) _endThreadPoolTimer.Cancel();
                if (AgendaItems.Count > 0)
                {
                    if (AgendaItems[0].Start < DateTime.Now && AgendaItems[0].End > DateTime.Now)
                    {
                        //_activeAgendaItem = AgendaItems[0];
                        if (!AgendaItems[0].IsOverridden)
                        {
                            _activeAgendaItem = AgendaItems[0];
                            if (_startTimeSpanThreadPoolTimer != null) _startTimeSpanThreadPoolTimer.Cancel();
                            if (_stopTimeSpanThreadPoolTimer != null) _stopTimeSpanThreadPoolTimer.Cancel();
                            Occupancy = (OccupancyVisualState)AgendaItems[0].Occupancy;
                            _applicationDataService.SaveSetting("OccupancyOverridden", false);
                            ResetButtonVisibility = Visibility.Collapsed;
                            SelectedComboBoxIndex = AgendaItems[0].Occupancy;
                            _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                            await _userDatagramService.SendStringData(new HostName("255.255.255.255"), _applicationDataService.GetSetting<string>("UdpPort"), JsonConvert.SerializeObject(_propertyChangedPackage));
                        }
                    }
                    else if (!AgendaItems[0].IsOverridden)
                    {
                        TimeSpan startTimeSpan = AgendaItems[0].Start - DateTime.Now;
                        _startThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
                        {
                            if (_startTimeSpanThreadPoolTimer != null) _startTimeSpanThreadPoolTimer.Cancel();
                            if (_stopTimeSpanThreadPoolTimer != null) _stopTimeSpanThreadPoolTimer.Cancel();
                            await _coreDispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                            {
                                Occupancy = (OccupancyVisualState)AgendaItems[0].Occupancy;
                                _applicationDataService.SaveSetting("OccupancyOverridden", false);
                                ResetButtonVisibility = Visibility.Collapsed;
                                SelectedComboBoxIndex = AgendaItems[0].Occupancy;
                                _activeAgendaItem = AgendaItems[0];
                                _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                                await _userDatagramService.SendStringData(new HostName("255.255.255.255"), _applicationDataService.GetSetting<string>("UdpPort"), JsonConvert.SerializeObject(_propertyChangedPackage));
                            });
                        }, startTimeSpan);
                    }

                    _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));

                    TimeSpan endTimeSpan = AgendaItems[0].End - DateTime.Now;
                    _endThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
                    {
                        await _coreDispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                        {
                            if (!await UpdateStandardWeek(DateTime.Now.DayOfWeek))
                            {
                                SelectedComboBoxIndex = _applicationDataService.GetSetting<int>("StandardOccupancy");
                                ResetButtonVisibility = Visibility.Collapsed;
                                Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                                if (AgendaItems.Count > 0) AgendaItems.RemoveAt(0);
                                _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
                                _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                            }
                            _activeAgendaItem = null;
                            await UpdateDayAgenda();
                            await _userDatagramService.SendStringData(new HostName("255.255.255.255"), _applicationDataService.GetSetting<string>("UdpPort"), JsonConvert.SerializeObject(_propertyChangedPackage));
                        });

                    }, endTimeSpan);
                }
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        private async Task<bool> GetIsStandardWeekActive(DayOfWeek dayOfWeek) => (await _databaseService.GetTimeSpanItemsAsync()).Where(x => x.DayOfWeek == (int)dayOfWeek).Where(x => x.Start < DateTime.Now.TimeOfDay).Where(x => x.End > DateTime.Now.TimeOfDay).Select(x => x).FirstOrDefault() == null;

        private ICommand _overrideOccupancyCommand;
        public ICommand OverrideOccupancyCommand => _overrideOccupancyCommand ?? (_overrideOccupancyCommand = new DelegateCommand<object>(async (param) =>
        {
            await OverrideOccupancy();
        }));

        private ICommand _resetOccupancyCommand;
        public ICommand ResetOccupancyCommand => _resetOccupancyCommand ?? (_resetOccupancyCommand = new DelegateCommand<object>(async (param) =>
        {
            _applicationDataService.SaveSetting("OccupancyOverridden", false);
            if (_activeAgendaItem != null)
            {
                _applicationDataService.SaveSetting("OverriddenOccupancy", (int)Occupancy);
                _activeAgendaItem.IsOverridden = false;
                SelectedComboBoxIndex = _activeAgendaItem.Occupancy;
                await _databaseService.UpdateAgendaItemAsync(_activeAgendaItem);
            }
            else
            {
                SelectedComboBoxIndex = _applicationDataService.GetSetting<int>("StandardOccupancy");
                _applicationDataService.SaveSetting("OverriddenOccupancy", (int)Occupancy);
            }
            await UpdateStandardWeek(DateTime.Now.DayOfWeek);
            Occupancy = OccupancyVisualState.UndefinedVisualState;
            Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
            ResetButtonVisibility = Visibility.Collapsed;
            _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
            await _userDatagramService.SendStringData(new HostName("255.255.255.255"), _applicationDataService.GetSetting<string>("UdpPort"), JsonConvert.SerializeObject(_propertyChangedPackage));
        }));

        private ICommand _updateDataTemplateWidthCommand;
        public ICommand UpdateDataTemplateWidthCommand => _updateDataTemplateWidthCommand ?? (_updateDataTemplateWidthCommand = new DelegateCommand<object>((param) =>
        {
            if (param == null) return;
            ListView listView = (ListView)param;
            _agendaItemWidth = listView.ActualWidth - 24;

        }));

        private ICommand _updateFontSizeCommand;
        public ICommand UpdateFontSizeCommand => _updateFontSizeCommand ?? (_updateFontSizeCommand = new DelegateCommand<object>((param) =>
        {
            if (param == null) return;
            else
            {
                Grid grid = param as Grid;
                MediumFontSize = grid.ActualHeight / 26.66;
                MediumToLargeFontSize = grid.ActualHeight / 20;
                LargeFontSize = grid.ActualHeight / 17.77;
                ExtraLargeFontSize = grid.ActualHeight / 13.33;
                SuperLargeFontSize = grid.ActualHeight / 4.44;
            }
        }));

        public void Agenda_LayoutUpdated(object sender, object e)
        {
            if (AgendaItems == null) return;
            for (int i = 0; i < AgendaItems.Count; i++)
            {
                AgendaItems[i].Width = _agendaItemWidth;
                AgendaItems[i].MediumFontSize = MediumFontSize;
                AgendaItems[i].LargeFontSize = MediumToLargeFontSize;
            }
        }

        private ICommand _dimCommand;
        public ICommand DimCommand => _dimCommand ?? (_dimCommand = new DelegateCommand<object>(async (param) =>
        {
            await _iotService.Dim(true);
        }));

        private ICommand _brightCommand;
        public ICommand BrightCommand => _brightCommand ?? (_brightCommand = new DelegateCommand<object>(async (param) =>
        {
            await _iotService.Dim(false);
        }));
    }
}
