using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

using RoomInfo.Helpers;
using RoomInfo.Services;
using ApplicationServiceLibrary;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using ModelLibrary;
using Windows.Globalization;
using Windows.ApplicationModel.Core;
using System.Collections.ObjectModel;
using Prism.Events;
using System.Linq;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using RoomInfo.Views;
using Windows.UI.Core;
using Windows.Storage.Pickers;
using System.Text;
using Windows.ApplicationModel.Email;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.Resources;

namespace RoomInfo.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase
    {
        IApplicationDataService _applicationDataService;
        IEventAggregator _eventAggregator;
        IIotService _iotService;
        INavigationService _navigationService;
        IDatabaseService _databaseService;

        int _selectedComboBoxIndex = default(int);
        public int SelectedComboBoxIndex { get => _selectedComboBoxIndex; set { SetProperty(ref _selectedComboBoxIndex, value); } }

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        public ElementTheme ElementTheme { get { return _elementTheme; } set => SetProperty(ref _elementTheme, value); }

        private string _versionDescription;
        public string VersionDescription { get => _versionDescription; set { SetProperty(ref _versionDescription, value); } }

        string _roomName = default(string);
        public string RoomName { get => _roomName; set { SetProperty(ref _roomName, value); _applicationDataService.SaveSetting("RoomName", _roomName); } }

        string _roomNumber = default(string);
        public string RoomNumber { get => _roomNumber; set { SetProperty(ref _roomNumber, value); _applicationDataService.SaveSetting("RoomNumber", _roomNumber); } }

        string _companyName = default(string);
        public string CompanyName { get => _companyName; set { SetProperty(ref _companyName, value); _applicationDataService.SaveSetting("CompanyName", _companyName); } }

        Uri _companyLogo = default(Uri);
        public Uri CompanyLogo { get => _companyLogo; set { SetProperty(ref _companyLogo, value); } }

        Visibility _selectLogoButtonStdVisibility = default(Visibility);
        public Visibility SelectLogoButtonStdVisibility { get => _selectLogoButtonStdVisibility; set { SetProperty(ref _selectLogoButtonStdVisibility, value); } }

        Visibility _selectLogoButtonIoTVisibility = default(Visibility);
        public Visibility SelectLogoButtonIoTVisibility { get => _selectLogoButtonIoTVisibility; set { SetProperty(ref _selectLogoButtonIoTVisibility, value); } }

        ObservableCollection<ExceptionLogItem> _exceptionLogItems = default(ObservableCollection<ExceptionLogItem>);
        public ObservableCollection<ExceptionLogItem> ExceptionLogItems { get => _exceptionLogItems; set { SetProperty(ref _exceptionLogItems, value); } }

        ObservableCollection<TimespanItem> _monday = default(ObservableCollection<TimespanItem>);
        public ObservableCollection<TimespanItem> Monday { get => _monday; set { SetProperty(ref _monday, value); } }

        ObservableCollection<TimespanItem> _tuesday = default(ObservableCollection<TimespanItem>);
        public ObservableCollection<TimespanItem> Tuesday { get => _tuesday; set { SetProperty(ref _tuesday, value); } }

        ObservableCollection<TimespanItem> _wednesday = default(ObservableCollection<TimespanItem>);
        public ObservableCollection<TimespanItem> Wednesday { get => _wednesday; set { SetProperty(ref _wednesday, value); } }

        ObservableCollection<TimespanItem> _thursday = default(ObservableCollection<TimespanItem>);
        public ObservableCollection<TimespanItem> Thursday { get => _thursday; set { SetProperty(ref _thursday, value); } }

        ObservableCollection<TimespanItem> _friday = default(ObservableCollection<TimespanItem>);
        public ObservableCollection<TimespanItem> Friday { get => _friday; set { SetProperty(ref _friday, value); } }

        ObservableCollection<TimespanItem> _saturday = default(ObservableCollection<TimespanItem>);
        public ObservableCollection<TimespanItem> Saturday { get => _saturday; set { SetProperty(ref _saturday, value); } }

        ObservableCollection<TimespanItem> _sunday = default(ObservableCollection<TimespanItem>);
        public ObservableCollection<TimespanItem> Sunday { get => _sunday; set { SetProperty(ref _sunday, value); } }

        bool _isFlyoutOpen = default(bool);
        public bool IsFlyoutOpen { get => _isFlyoutOpen; set { SetProperty(ref _isFlyoutOpen, value); } }

        string _dayOfWeek = default(string);
        public string DayOfWeek { get => _dayOfWeek; set { SetProperty(ref _dayOfWeek, value); } }

        string _tcpPort = default(string);
        public string TcpPort
        {
            get => _tcpPort;
            set
            {
                SetProperty(ref _tcpPort, value);
                string previousPort = _applicationDataService.GetSetting<string>("TcpPort");
                if (!string.IsNullOrEmpty(TcpPort)) _applicationDataService.SaveSetting("TcpPort", TcpPort);
                if (!previousPort.Equals(TcpPort)) _eventAggregator.GetEvent<PortChangedEvent>().Publish();
            }
        }

        string _udpPort = default(string);
        public string UdpPort
        {
            get => _udpPort;
            set
            {
                SetProperty(ref _udpPort, value);
                string previousPort = _applicationDataService.GetSetting<string>("UdpPort");
                if (!string.IsNullOrEmpty(UdpPort)) _applicationDataService.SaveSetting("UdpPort", UdpPort);
                if (!previousPort.Equals(UdpPort)) _eventAggregator.GetEvent<PortChangedEvent>().Publish();
            }
        }

        Visibility _iotPanelVisibility = default(Visibility);
        public Visibility IotPanelVisibility { get => _iotPanelVisibility; set { SetProperty(ref _iotPanelVisibility, value); } }

        ObservableCollection<FileItem> _fileItems = default(ObservableCollection<FileItem>);
        public ObservableCollection<FileItem> FileItems { get => _fileItems; set { SetProperty(ref _fileItems, value); } }

        TimespanItem _timespanItem = default(TimespanItem);
        public TimespanItem TimespanItem { get => _timespanItem; set { SetProperty(ref _timespanItem, value); } }

        ModelLibrary.Language _language = default(ModelLibrary.Language);
        public ModelLibrary.Language Language { get => _language; set { SetProperty(ref _language, value); } }

        public SettingsViewModel(IApplicationDataService applicationDataService, IIotService iotService, INavigationService navigationService, IEventAggregator eventAggregator, IDatabaseService databaseService)
        {
            _applicationDataService = applicationDataService;
            _iotService = iotService;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;
            _databaseService = databaseService;
        }

        private ICommand _switchThemeCommand;
        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new DelegateCommand<object>(
                        async (param) =>
                        {
                            ElementTheme = (ElementTheme)param;
                            await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
                        });
                }

                return _switchThemeCommand;
            }
        }

        private ICommand _switchLanguageCommand;
        public ICommand SwitchLanguageCommand => _switchLanguageCommand ?? (_switchLanguageCommand = new DelegateCommand<object>(async (param) =>
        {
            _applicationDataService.SaveSetting("Language", (string)param);
            ApplicationLanguages.PrimaryLanguageOverride = (string)param;
            await CoreApplication.RequestRestartAsync("Language");
        }));

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            VersionDescription = GetVersionDescription();
            SelectedComboBoxIndex = _applicationDataService.GetSetting<int>("StandardOccupancy");
            RoomName = _applicationDataService.GetSetting<string>("RoomName");
            RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber");
            CompanyName = _applicationDataService.GetSetting<string>("CompanyName");
            TcpPort = _applicationDataService.GetSetting<string>("TcpPort");
            UdpPort = _applicationDataService.GetSetting<string>("UdpPort");
            if (string.IsNullOrEmpty(TcpPort)) TcpPort = "8273";
            if (string.IsNullOrEmpty(UdpPort)) UdpPort = "8274";
            await LoadCompanyLogo();
            IotPanelVisibility = _iotService.IsIotDevice() ? Visibility.Visible : Visibility.Collapsed;
            Language = LoadLanguage();
            _eventAggregator.GetEvent<FileItemSelectionChangedUpdatedEvent>().Subscribe(async i =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var fileUri = FileItems.Where(x => x.Id == i).Select(x => x.ImageUri).FirstOrDefault();
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
                        StorageFile storageFile = await StorageFile.GetFileFromPathAsync(fileUri.LocalPath);
                        await storageFile.CopyAsync(assets, storageFile.Name, NameCollisionOption.ReplaceExisting);
                        _applicationDataService.SaveSetting("LogoFileName", storageFile.Name);
                        await LoadCompanyLogo();
                    }
                    catch { }
                });
                //InjectedInputKeyboardInfo injectedInputKeyboardInfo = new InjectedInputKeyboardInfo
                //{
                //    VirtualKey = (ushort)VirtualKey.Escape
                //};
                //InputInjector.TryCreate().InjectKeyboardInput(new List<InjectedInputKeyboardInfo> { injectedInputKeyboardInfo });
            });
            if (_iotService.IsIotDevice())
            {
                SelectLogoButtonIoTVisibility = Visibility.Visible;
                SelectLogoButtonStdVisibility = Visibility.Collapsed;
            }
            else
            {
                SelectLogoButtonIoTVisibility = Visibility.Collapsed;
                SelectLogoButtonStdVisibility = Visibility.Visible;
            }
            if (ExceptionLogItems == null) ExceptionLogItems = new ObservableCollection<ExceptionLogItem>();
            else ExceptionLogItems.Clear();
            (await _databaseService.GetExceptionLogItemsAsync()).ForEach(x => ExceptionLogItems.Add(x));

            Monday = new ObservableCollection<TimespanItem>();
            Tuesday = new ObservableCollection<TimespanItem>();
            Wednesday = new ObservableCollection<TimespanItem>();
            Thursday = new ObservableCollection<TimespanItem>();
            Friday = new ObservableCollection<TimespanItem>();
            Saturday = new ObservableCollection<TimespanItem>();
            Sunday = new ObservableCollection<TimespanItem>();
        }

        private ModelLibrary.Language LoadLanguage()
        {
            switch (_applicationDataService.GetSetting<string>("Language"))
            {
                case "de-DE":
                    return ModelLibrary.Language.de_DE;
                case "en-US":
                    return ModelLibrary.Language.en_US;
                default:
                    if (Windows.Globalization.Language.CurrentInputMethodLanguageTag.Equals("de-DE")) return ModelLibrary.Language.de_DE;
                    else return ModelLibrary.Language.en_US;
            }
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Windows.ApplicationModel.Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private ICommand _setStandardOccupancyCommand;
        public ICommand SetStandardOccupancyCommand => _setStandardOccupancyCommand ?? (_setStandardOccupancyCommand = new DelegateCommand<object>((param) =>
        {
            _applicationDataService.SaveSetting("StandardOccupancy", SelectedComboBoxIndex);
        }));

        private ICommand _selectLogoCommand;
        public ICommand SelectLogoCommand => _selectLogoCommand ?? (_selectLogoCommand = new DelegateCommand<object>(async (param) =>
        {
            if (_iotService.IsIotDevice())
            {
                FileItems = new ObservableCollection<FileItem>();
                QueryOptions queryOption = new QueryOptions(CommonFileQuery.OrderByTitle, new string[] { ".jpg", ".jpeg", ".png" })
                {
                    FolderDepth = FolderDepth.Shallow
                };
                var files = await KnownFolders.PicturesLibrary.CreateFileQueryWithOptions(queryOption).GetFilesAsync();
                int id = 0;
                foreach (var file in files)
                {
                    id++;
                    BitmapImage bitmapImage = new BitmapImage();
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        bitmapImage.DecodePixelWidth = 56;
                        await bitmapImage.SetSourceAsync(fileStream);
                    }
                    FileItems.Add(new FileItem()
                    {
                        FileName = file.DisplayName,
                        ImageUri = new Uri(file.Path),
                        ImageSource = bitmapImage,
                        Id = id
                    });
                }
            }
            else
            {
                FileOpenPicker openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
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
                    await file.CopyAsync(assets, file.Name, NameCollisionOption.ReplaceExisting);
                    _applicationDataService.SaveSetting("LogoFileName", file.Name);
                    await LoadCompanyLogo();
                }
            }
        }));

        private ICommand _deleteLogoCommand;
        public ICommand DeleteLogoCommand => _deleteLogoCommand ?? (_deleteLogoCommand = new DelegateCommand<object>(async (param) =>
        {
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
            if (!string.IsNullOrEmpty(logoFileName))
            {
                StorageFile storageFile = await assets.GetFileAsync(logoFileName);
                await storageFile.DeleteAsync();
                _applicationDataService.RemoveSetting("LogoFileName");
                await LoadCompanyLogo();
            }
        }));

        private ICommand _configWiFiCommand;
        public ICommand ConfigWiFiCommand => _configWiFiCommand ?? (_configWiFiCommand = new DelegateCommand<object>((param) =>
        {
            var currentWindow = Window.Current;
            currentWindow.Content = new WiFiUserControl();
            currentWindow.Activate();
        }));

        private ICommand _restartCommand;
        public ICommand RestartCommand => _restartCommand ?? (_restartCommand = new DelegateCommand<object>((param) =>
        {
            _iotService.Restart();
        }));

        private ICommand _shutdownCommand;
        public ICommand ShutdownCommand => _shutdownCommand ?? (_shutdownCommand = new DelegateCommand<object>((param) =>
        {
            _iotService.Shutdown();
        }));

        //private ICommand _reservedCommand;
        //public ICommand ReservedCommand => _reservedCommand ?? (_reservedCommand = new DelegateCommand<object>((param) =>
        //{

        //}));

        private async Task LoadCompanyLogo()
        {
            try
            {
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
            }
            catch { }
        }

        public void KeyDown(object sender, KeyRoutedEventArgs e)
        {
            int virtualKey = (int)e.Key;
            if ((virtualKey > 47 && virtualKey < 58) || (virtualKey > 95 && virtualKey < 106)) e.Handled = false;
            else e.Handled = true;
        }

        public void Flyout_Closing(Windows.UI.Xaml.Controls.Primitives.FlyoutBase sender, Windows.UI.Xaml.Controls.Primitives.FlyoutBaseClosingEventArgs args)
        {
            _eventAggregator.GetEvent<CollapseLowerGridEvent>().Publish();
        }

        private ICommand _deleteExeptionLogCommand;
        public ICommand DeleteExeptionLogCommand => _deleteExeptionLogCommand ?? (_deleteExeptionLogCommand = new DelegateCommand<object>(async (param) =>
        {
            ExceptionLogItems.Clear();
            await _databaseService.RemoveExceptionLogItemsAsync();
        }));

        private ICommand _sendExceptionLogCommand;
        public ICommand SendExceptionLogCommand => _sendExceptionLogCommand ?? (_sendExceptionLogCommand = new DelegateCommand<object>(async (param) =>
        {
            if (_iotService.IsIotDevice()) return;
            var stringBuilder = new StringBuilder();
            foreach (var exceptionLogItem in ExceptionLogItems)
            {
                stringBuilder.AppendLine(
                    exceptionLogItem.TimeStamp + ": " +
                    exceptionLogItem.Message + ", " +
                    exceptionLogItem.Source + ", " +
                    exceptionLogItem.StackTrace);
            }
            var emailMessage = new EmailMessage
            {
                Subject = "RoomInfo ExceptionLog",
                Body = stringBuilder.ToString()
            };
            emailMessage.To.Add(new EmailRecipient("code_m@outlook.de"));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }));

        private ICommand _addTimespanItemCommand;
        public ICommand AddTimespanItemCommand => _addTimespanItemCommand ?? (_addTimespanItemCommand = new DelegateCommand<object>((param) =>
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();            
            TimespanItem = new TimespanItem() { TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() };
            switch ((string)param)
            {
                case "Monday":
                    DayOfWeek = resourceLoader.GetString("Settings_StandardWeek_Monday/Text");
                    TimespanItem.DayOfWeek = (int)System.DayOfWeek.Monday;
                    break;
                case "Tuesday":
                    DayOfWeek = resourceLoader.GetString("Settings_StandardWeek_Tuesday/Text");
                    TimespanItem.DayOfWeek = (int)System.DayOfWeek.Tuesday;
                    break;
                case "Wednesday":
                    DayOfWeek = resourceLoader.GetString("Settings_StandardWeek_Wednesday/Text");
                    TimespanItem.DayOfWeek = (int)System.DayOfWeek.Wednesday;
                    break;
                case "Thursday":
                    DayOfWeek = resourceLoader.GetString("Settings_StandardWeek_Thursday/Text");
                    TimespanItem.DayOfWeek = (int)System.DayOfWeek.Thursday;
                    break;
                case "Friday":
                    DayOfWeek = resourceLoader.GetString("Settings_StandardWeek_Friday/Text");
                    TimespanItem.DayOfWeek = (int)System.DayOfWeek.Friday;
                    break;
                case "Saturday":
                    DayOfWeek = resourceLoader.GetString("Settings_StandardWeek_Saturday/Text");
                    TimespanItem.DayOfWeek = (int)System.DayOfWeek.Saturday;
                    break;
                case "Sunday":
                    DayOfWeek = resourceLoader.GetString("Settings_StandardWeek_Sunday/Text");
                    TimespanItem.DayOfWeek = (int)System.DayOfWeek.Sunday;
                    break;
                default:
                    break;
            }
        }));

        private ICommand _editTimespanItemCommand;
        public ICommand EditTimespanItemCommand => _editTimespanItemCommand ?? (_editTimespanItemCommand = new DelegateCommand<object>((param) =>
        {

        }));

        private ICommand _deleteTimespanItemCommand;
        public ICommand DeleteTimespanItemCommand => _deleteTimespanItemCommand ?? (_deleteTimespanItemCommand = new DelegateCommand<object>((param) =>
        {

        }));

        private ICommand _hideTimespanItemCommand;
        public ICommand HideTimespanItemCommand => _hideTimespanItemCommand ?? (_hideTimespanItemCommand = new DelegateCommand<object>((param) =>
        {
            (((param as Grid).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
            IsFlyoutOpen = false;
        }));

        private ICommand _addOrUpdateTimespanItemCommand;
        public ICommand AddOrUpdateTimespanItemCommand => _addOrUpdateTimespanItemCommand ?? (_addOrUpdateTimespanItemCommand = new DelegateCommand<object>(async (param) =>
        {

        }));
    }
}
