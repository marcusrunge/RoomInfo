﻿using System;
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

namespace RoomInfo.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        IDatabaseService _databaseService;
        IApplicationDataService _applicationDataService;
        ILiveTileUpdateService _liveTileUpdateService;
        IEventAggregator _eventAggregator;
        IIotService _iotService;
        AgendaItem _activeAgendaItem;
        double _agendaItemWidth;
        ResourceLoader _resourceLoader;

        OccupancyVisualState _occupancy = default(OccupancyVisualState);
        public OccupancyVisualState Occupancy { get => _occupancy; set { SetProperty(ref _occupancy, value); } }

        string _clock = default(string);
        public string Clock { get => _clock; set { SetProperty(ref _clock, value); } }

        string _date = default(string);
        public string Date { get => _date; set { SetProperty(ref _date, value); } }

        string _room = default(string);
        public string Room { get => _room; set { SetProperty(ref _room, value); } }

        int _selectedComboBoxIndex = default(int);
        public int SelectedComboBoxIndex { get => _selectedComboBoxIndex; set { SetProperty(ref _selectedComboBoxIndex, value); } }

        ObservableCollection<AgendaItem> _agendaItems = default(ObservableCollection<AgendaItem>);
        public ObservableCollection<AgendaItem> AgendaItems { get => _agendaItems; set { SetProperty(ref _agendaItems, value); } }

        Uri _companyLogo = default(Uri);
        public Uri CompanyLogo { get => _companyLogo; set { SetProperty(ref _companyLogo, value); } }

        string _companyName = default(string);
        public string CompanyName { get => _companyName; set { SetProperty(ref _companyName, value); } }

        string _roomName = default(string);
        public string RoomName { get => _roomName; set { SetProperty(ref _roomName, value); } }

        string _roomNumber = default(string);
        public string RoomNumber { get => _roomNumber; set { SetProperty(ref _roomNumber, value); } }

        Visibility _resetButtonVisibility = default(Visibility);
        public Visibility ResetButtonVisibility { get => _resetButtonVisibility; set { SetProperty(ref _resetButtonVisibility, value); } }

        double _mediumFontSize = default(double);
        public double MediumFontSize { get => _mediumFontSize; set { SetProperty(ref _mediumFontSize, value); } }

        double _mediumToLargeFontSize = default(double);
        public double MediumToLargeFontSize { get => _mediumToLargeFontSize; set { SetProperty(ref _mediumToLargeFontSize, value); } }

        double _largeFontSize = default(double);
        public double LargeFontSize { get => _largeFontSize; set { SetProperty(ref _largeFontSize, value); } }

        double _extraLargeFontSize = default(double);
        public double ExtraLargeFontSize { get => _extraLargeFontSize; set { SetProperty(ref _extraLargeFontSize, value); } }

        double _superLargeFontSize = default(double);
        public double SuperLargeFontSize { get => _superLargeFontSize; set { SetProperty(ref _superLargeFontSize, value); } }

        Visibility _brightnessAdjustmentVisibility = default(Visibility);
        public Visibility BrightnessAdjustmentVisibility { get => _brightnessAdjustmentVisibility; set { SetProperty(ref _brightnessAdjustmentVisibility, value); } }

        public InfoViewModel(IUnityContainer unityContainer)
        {
            _databaseService = unityContainer.Resolve<IDatabaseService>();
            _applicationDataService = unityContainer.Resolve<IApplicationDataService>();
            _liveTileUpdateService = unityContainer.Resolve<ILiveTileUpdateService>();
            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
            _iotService = unityContainer.Resolve<IIotService>();
        }

        public async override void OnNavigatedTo(NavigatedToEventArgs navigatedToEventArgs, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigatedToEventArgs, viewModelState);
            _resourceLoader = ResourceLoader.GetForCurrentView();
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
            };
            dispatcherTimer.Start();
            SelectedComboBoxIndex = _applicationDataService.GetSetting<bool>("OccupancyOverridden") ? _applicationDataService.GetSetting<int>("OverriddenOccupancy") : _applicationDataService.GetSetting<int>("StandardOccupancy");
            ResetButtonVisibility = _applicationDataService.GetSetting<bool>("OccupancyOverridden") ? Visibility.Visible : Visibility.Collapsed;
            Occupancy = OccupancyVisualState.UndefinedVisualState;
            Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
            _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
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
            _eventAggregator.GetEvent<RemoteAgendaItemsUpdatedEvent>().Subscribe(async () => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await UpdateDayAgenda()));
            BrightnessAdjustmentVisibility = _iotService.IsIotDevice() ? Visibility.Visible : Visibility.Collapsed;
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
            ResetButtonVisibility = Visibility.Visible;
            _applicationDataService.SaveSetting("ActualOccupancy", SelectedComboBoxIndex);
        }

        private async Task UpdateDayAgenda()
        {
            if (AgendaItems == null) AgendaItems = new ObservableCollection<AgendaItem>();
            else AgendaItems.Clear();
            DateTime dateTimeNow = DateTime.Now;
            var agendaItems = await _databaseService.GetAgendaItemsAsync(dateTimeNow);
            agendaItems.Sort();
            for (int i = 0; i < agendaItems.Count; i++)
            {
                AgendaItems.Add(agendaItems[i]);
                AgendaItems[i].SetDueTime();
            }
            await UpdateTimerTask();
        }

        private async Task UpdateTimerTask()
        {
            CoreDispatcher coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            if (AgendaItems.Count > 0)
            {
                if (AgendaItems[0].Start < DateTime.Now && AgendaItems[0].End > DateTime.Now && !AgendaItems[0].IsOverridden)
                {
                    Occupancy = (OccupancyVisualState)AgendaItems[0].Occupancy;
                    _applicationDataService.SaveSetting("OccupancyOverridden", false);
                    ResetButtonVisibility = Visibility.Collapsed;
                    SelectedComboBoxIndex = AgendaItems[0].Occupancy;
                    _activeAgendaItem = AgendaItems[0];
                    _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                }
                else if (!AgendaItems[0].IsOverridden)
                {
                    TimeSpan startTimeSpan = AgendaItems[0].Start - DateTime.Now;
                    ThreadPoolTimer startThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
                    {
                        await coreDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            Occupancy = (OccupancyVisualState)AgendaItems[0].Occupancy;
                            _applicationDataService.SaveSetting("OccupancyOverridden", false);
                            ResetButtonVisibility = Visibility.Collapsed;
                            SelectedComboBoxIndex = AgendaItems[0].Occupancy;
                            _activeAgendaItem = AgendaItems[0];
                            _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                        });
                    }, startTimeSpan);
                }

                _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));

                TimeSpan endTimeSpan = AgendaItems[0].End - DateTime.Now;
                ThreadPoolTimer endThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
                {
                    await coreDispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        SelectedComboBoxIndex = _applicationDataService.GetSetting<int>("StandardOccupancy");
                        ResetButtonVisibility = Visibility.Collapsed;
                        Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                        if (AgendaItems.Count > 0) AgendaItems.RemoveAt(0);
                        _activeAgendaItem = null;
                        await UpdateDayAgenda();
                        _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
                        _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
                    });

                }, endTimeSpan);
            }
        }

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
            Occupancy = OccupancyVisualState.UndefinedVisualState;
            Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
            ResetButtonVisibility = Visibility.Collapsed;
            _applicationDataService.SaveSetting("ActualOccupancy", (int)Occupancy);
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
