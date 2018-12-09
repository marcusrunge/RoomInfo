using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using RoomInfo.Models;
using RoomInfo.Services;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace RoomInfo.ViewModels
{
    public enum OccupancyVisualState { FreeVisualState, BusyVisualState, OccupiedVisualState, AbsentVisualState, UndefinedVisualState }
    public class InfoViewModel : ViewModelBase
    {
        IDatabaseService _databaseService;
        IApplicationDataService _applicationDataService;

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

        public InfoViewModel(IUnityContainer unityContainer)
        {
            _databaseService = unityContainer.Resolve<IDatabaseService>();
            _applicationDataService = unityContainer.Resolve<IApplicationDataService>();
        }

        public async override void OnNavigatedTo(NavigatedToEventArgs navigatedToEventArgs, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigatedToEventArgs, viewModelState);
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            string logoFileName = _applicationDataService.GetSetting<string>("LogoFileName");
            CompanyLogo = new Uri(assets.Path + "/" + logoFileName);
            CompanyName = _applicationDataService.GetSetting<string>("CompanyName");
            RoomName = _applicationDataService.GetSetting<string>("RoomName");
            RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber");
            CultureInfo cultureInfo = new CultureInfo("de-DE");
            Clock = DateTime.Now.ToString("t", cultureInfo) + " Uhr";
            Date = DateTime.Now.ToString("D", cultureInfo);
            DispatcherTimer dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            dispatcherTimer.Tick += (s, e) =>
            {
                Clock = DateTime.Now.ToString("t", cultureInfo) + " Uhr";
                Date = DateTime.Now.ToString("D", cultureInfo);
            };
            dispatcherTimer.Start();            
            SelectedComboBoxIndex = _applicationDataService.GetSetting<int>("StandardOccupancy");
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
            UpdateTimerTask();
        }

        private void UpdateTimerTask()
        {            
            CoreDispatcher coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            if (AgendaItems.Count > 0)
            {
                if (AgendaItems[0].Start < DateTime.Now && AgendaItems[0].End > DateTime.Now)
                {
                    Occupancy = (OccupancyVisualState)AgendaItems[0].Occupancy;
                    SelectedComboBoxIndex = AgendaItems[0].Occupancy;
                }
                else
                {
                    TimeSpan startTimeSpan = AgendaItems[0].Start - DateTime.Now;
                    ThreadPoolTimer startThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
                    {
                        await coreDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            Occupancy = (OccupancyVisualState)AgendaItems[0].Occupancy;
                            SelectedComboBoxIndex = AgendaItems[0].Occupancy;
                        });
                    }, startTimeSpan);
                }

                TimeSpan endTimeSpan = AgendaItems[0].End - DateTime.Now;
                ThreadPoolTimer endThreadPoolTimer = ThreadPoolTimer.CreateTimer(async (source) =>
                {
                    await coreDispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        SelectedComboBoxIndex = _applicationDataService.GetSetting<int>("StandardOccupancy");
                        Occupancy = (OccupancyVisualState)SelectedComboBoxIndex;
                        AgendaItems.RemoveAt(0);
                        await UpdateDayAgenda();
                    });

                }, endTimeSpan);
            }
        }
    }
}
