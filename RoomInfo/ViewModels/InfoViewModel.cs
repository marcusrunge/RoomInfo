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
using RoomInfo.Services;
using ServiceLibrary;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace RoomInfo.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        IDatabaseService _databaseService;
        IApplicationDataService _applicationDataService;
        ILiveTileUpdateService _liveTileUpdateService;
        AgendaItem _activeAgendaItem;

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

        public InfoViewModel(IUnityContainer unityContainer)
        {
            _databaseService = unityContainer.Resolve<IDatabaseService>();
            _applicationDataService = unityContainer.Resolve<IApplicationDataService>();
            _liveTileUpdateService = unityContainer.Resolve<ILiveTileUpdateService>();
        }

        public async override void OnNavigatedTo(NavigatedToEventArgs navigatedToEventArgs, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigatedToEventArgs, viewModelState);
            var resourceLoader = ResourceLoader.GetForCurrentView();
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            string logoFileName = _applicationDataService.GetSetting<string>("LogoFileName");
            CompanyLogo = new Uri(assets.Path + "/" + logoFileName);
            CompanyName = _applicationDataService.GetSetting<string>("CompanyName");
            RoomName = _applicationDataService.GetSetting<string>("RoomName");
            RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber");
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Clock = DateTime.Now.ToString("t", cultureInfo) + " " + resourceLoader.GetString("InfoViewModel_Clock");
            Date = DateTime.Now.ToString("D", cultureInfo);
            DispatcherTimer dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            dispatcherTimer.Tick += (s, e) =>
            {
                Clock = DateTime.Now.ToString("t", cultureInfo) + " " + resourceLoader.GetString("InfoViewModel_Clock");
                Date = DateTime.Now.ToString("D", cultureInfo);
            };
            dispatcherTimer.Start();
            SelectedComboBoxIndex = _applicationDataService.GetSetting<bool>("OccupancyOverridden") ? _applicationDataService.GetSetting<int>("OverriddenOccupancy") : _applicationDataService.GetSetting<int>("StandardOccupancy");
            ResetButtonVisibility = _applicationDataService.GetSetting<bool>("OccupancyOverridden") ? Visibility.Visible : Visibility.Collapsed;
            Occupancy = OccupancyVisualState.UndefinedVisualState;
            Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
            await UpdateDayAgenda();
        }

        private async Task UpdateDayAgenda()
        {
            if (AgendaItems == null) AgendaItems = new ObservableCollection<AgendaItem>();
            else AgendaItems.Clear();
            DateTime dateTimeNow = DateTime.Now;
            var agendaItems = await _databaseService.GetAgendaItemsAsync(dateTimeNow);
            for (int i = 0; i < agendaItems.Count; i++)
            {
                AgendaItems.Add(agendaItems[i]);
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
                        Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                        AgendaItems.RemoveAt(0);
                        _activeAgendaItem = null;
                        await UpdateDayAgenda();
                        _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
                    });

                }, endTimeSpan);
            }
        }

        private ICommand _overrideOccupancyCommand;
        public ICommand OverrideOccupancyCommand => _overrideOccupancyCommand ?? (_overrideOccupancyCommand = new DelegateCommand<object>(async (param) =>
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
        }));

        private ICommand _updateDataTemplateWidthCommand;
        public ICommand UpdateDataTemplateWidthCommand => _updateDataTemplateWidthCommand ?? (_updateDataTemplateWidthCommand = new DelegateCommand<object>((param) =>
        {
            if (param == null) return;
            for (int i = 0; i < AgendaItems.Count; i++)
            {
                AgendaItems[i].Width = (double)param;
            }
        }));
    }
}
