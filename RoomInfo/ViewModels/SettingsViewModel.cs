using ApplicationServiceLibrary;
using ModelLibrary;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using RoomInfo.Helpers;
using RoomInfo.Services;
using RoomInfo.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace RoomInfo.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase
    {
        private IApplicationDataService _applicationDataService;
        private IEventAggregator _eventAggregator;
        private IIotService _iotService;
        private INavigationService _navigationService;
        private IDatabaseService _databaseService;
        private bool _isStandardWeekUpdatedEventAcceptable;

        private int _selectedComboBoxIndex = default;
        public int SelectedComboBoxIndex { get => _selectedComboBoxIndex; set { SetProperty(ref _selectedComboBoxIndex, value); } }

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        public ElementTheme ElementTheme { get { return _elementTheme; } set => SetProperty(ref _elementTheme, value); }

        private string _versionDescription;
        public string VersionDescription { get => _versionDescription; set { SetProperty(ref _versionDescription, value); } }

        private string _roomName = default;
        public string RoomName { get => _roomName; set { SetProperty(ref _roomName, value); _applicationDataService.SaveSetting("RoomName", _roomName); } }

        private string _roomNumber = default;
        public string RoomNumber { get => _roomNumber; set { SetProperty(ref _roomNumber, value); _applicationDataService.SaveSetting("RoomNumber", _roomNumber); } }

        private string _companyName = default;
        public string CompanyName { get => _companyName; set { SetProperty(ref _companyName, value); _applicationDataService.SaveSetting("CompanyName", _companyName); } }

        private Uri _companyLogo = default;
        public Uri CompanyLogo { get => _companyLogo; set { SetProperty(ref _companyLogo, value); } }

        private Visibility _selectLogoButtonStdVisibility = default;
        public Visibility SelectLogoButtonStdVisibility { get => _selectLogoButtonStdVisibility; set { SetProperty(ref _selectLogoButtonStdVisibility, value); } }

        private Visibility _selectLogoButtonIoTVisibility = default;
        public Visibility SelectLogoButtonIoTVisibility { get => _selectLogoButtonIoTVisibility; set { SetProperty(ref _selectLogoButtonIoTVisibility, value); } }

        private ObservableCollection<ExceptionLogItem> _exceptionLogItems = default;
        public ObservableCollection<ExceptionLogItem> ExceptionLogItems { get => _exceptionLogItems; set { SetProperty(ref _exceptionLogItems, value); } }

        private ObservableCollection<TimeSpanItem> _monday = default;
        public ObservableCollection<TimeSpanItem> Monday { get => _monday; set { SetProperty(ref _monday, value); } }

        private ObservableCollection<TimeSpanItem> _tuesday = default;
        public ObservableCollection<TimeSpanItem> Tuesday { get => _tuesday; set { SetProperty(ref _tuesday, value); } }

        private ObservableCollection<TimeSpanItem> _wednesday = default;
        public ObservableCollection<TimeSpanItem> Wednesday { get => _wednesday; set { SetProperty(ref _wednesday, value); } }

        private ObservableCollection<TimeSpanItem> _thursday = default;
        public ObservableCollection<TimeSpanItem> Thursday { get => _thursday; set { SetProperty(ref _thursday, value); } }

        private ObservableCollection<TimeSpanItem> _friday = default;
        public ObservableCollection<TimeSpanItem> Friday { get => _friday; set { SetProperty(ref _friday, value); } }

        private ObservableCollection<TimeSpanItem> _saturday = default;
        public ObservableCollection<TimeSpanItem> Saturday { get => _saturday; set { SetProperty(ref _saturday, value); } }

        private ObservableCollection<TimeSpanItem> _sunday = default;
        public ObservableCollection<TimeSpanItem> Sunday { get => _sunday; set { SetProperty(ref _sunday, value); } }

        private bool _isFlyoutOpen = default;
        public bool IsFlyoutOpen { get => _isFlyoutOpen; set { SetProperty(ref _isFlyoutOpen, value); } }

        private string _dayOfWeek = default;
        public string DayOfWeek { get => _dayOfWeek; set { SetProperty(ref _dayOfWeek, value); } }

        private string _tcpPort = default;

        public string TcpPort
        {
            get => _tcpPort;
            set
            {
                SetProperty(ref _tcpPort, value);
                string previousPort = _applicationDataService.GetSetting<string>("TcpPort");
                if (!string.IsNullOrEmpty(TcpPort)) _applicationDataService.SaveSetting("TcpPort", TcpPort);
                if (!string.IsNullOrEmpty(previousPort) && !previousPort.Equals(TcpPort)) _eventAggregator.GetEvent<PortChangedEvent>().Publish();
            }
        }

        private string _udpPort = default;

        public string UdpPort
        {
            get => _udpPort;
            set
            {
                SetProperty(ref _udpPort, value);
                string previousPort = _applicationDataService.GetSetting<string>("UdpPort");
                if (!string.IsNullOrEmpty(UdpPort)) _applicationDataService.SaveSetting("UdpPort", UdpPort);
                if (!string.IsNullOrEmpty(previousPort) && !previousPort.Equals(UdpPort)) _eventAggregator.GetEvent<PortChangedEvent>().Publish();
            }
        }

        private Visibility _iotPanelVisibility = default;
        public Visibility IotPanelVisibility { get => _iotPanelVisibility; set { SetProperty(ref _iotPanelVisibility, value); } }

        private ObservableCollection<FileItem> _fileItems = default;
        public ObservableCollection<FileItem> FileItems { get => _fileItems; set { SetProperty(ref _fileItems, value); } }

        private TimeSpanItem _timespanItem = default;
        public TimeSpanItem TimespanItem { get => _timespanItem; set { SetProperty(ref _timespanItem, value); } }

        private ModelLibrary.Language _language = default;
        public ModelLibrary.Language Language { get => _language; set { SetProperty(ref _language, value); } }

        private FrameworkElement _flyoutParent = default;
        public FrameworkElement FlyoutParent { get => _flyoutParent; set { SetProperty(ref _flyoutParent, value); } }

        private bool _isSaveButtonEnabled = default;
        public bool IsSaveButtonEnabled { get => _isSaveButtonEnabled; set { SetProperty(ref _isSaveButtonEnabled, value); } }

        public SettingsViewModel(IApplicationDataService applicationDataService, IIotService iotService, INavigationService navigationService, IEventAggregator eventAggregator, IDatabaseService databaseService)
        {
            _applicationDataService = applicationDataService;
            _iotService = iotService;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;
            _databaseService = databaseService;
            _isStandardWeekUpdatedEventAcceptable = true;
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

            var timespanItems = await _databaseService.GetTimeSpanItemsAsync();
            var monday = new List<TimeSpanItem>();
            var tuesday = new List<TimeSpanItem>();
            var wednesday = new List<TimeSpanItem>();
            var thursday = new List<TimeSpanItem>();
            var friday = new List<TimeSpanItem>();
            var saturday = new List<TimeSpanItem>();
            var sunday = new List<TimeSpanItem>();
            foreach (var timespanItem in timespanItems)
            {
                timespanItem.EventAggregator = _eventAggregator;
                switch ((System.DayOfWeek)timespanItem.DayOfWeek)
                {
                    case System.DayOfWeek.Friday:
                        friday.Add(timespanItem);
                        break;

                    case System.DayOfWeek.Monday:
                        monday.Add(timespanItem);
                        break;

                    case System.DayOfWeek.Saturday:
                        saturday.Add(timespanItem);
                        break;

                    case System.DayOfWeek.Sunday:
                        sunday.Add(timespanItem);
                        break;

                    case System.DayOfWeek.Thursday:
                        thursday.Add(timespanItem);
                        break;

                    case System.DayOfWeek.Tuesday:
                        tuesday.Add(timespanItem);
                        break;

                    case System.DayOfWeek.Wednesday:
                        wednesday.Add(timespanItem);
                        break;

                    default:
                        break;
                }
            }
            Monday = new ObservableCollection<TimeSpanItem>(monday.OrderBy(x => x.Start));
            Tuesday = new ObservableCollection<TimeSpanItem>(tuesday.OrderBy(x => x.Start));
            Wednesday = new ObservableCollection<TimeSpanItem>(wednesday.OrderBy(x => x.Start));
            Thursday = new ObservableCollection<TimeSpanItem>(thursday.OrderBy(x => x.Start));
            Friday = new ObservableCollection<TimeSpanItem>(friday.OrderBy(x => x.Start));
            Saturday = new ObservableCollection<TimeSpanItem>(saturday.OrderBy(x => x.Start));
            Sunday = new ObservableCollection<TimeSpanItem>(sunday.OrderBy(x => x.Start));
            _eventAggregator.GetEvent<UpdateTimespanItemEvent>().Subscribe(x =>
            {
                IsSaveButtonEnabled = true;
                TimespanItem = new TimeSpanItem() { DayOfWeek = x.DayOfWeek, End = x.End, Id = x.Id, Occupancy = x.Occupancy, Start = x.Start, TimeStamp = x.TimeStamp, Width = x.Width };
                TimespanItem.EventAggregator = _eventAggregator;
                IsFlyoutOpen = true;
            });
            _eventAggregator.GetEvent<GotFocusEvent>().Subscribe(x =>
            {
                FlyoutParent = x;
            });
            _eventAggregator.GetEvent<DeleteTimespanItemEvent>().Subscribe(x =>
            {
                _databaseService.RemoveTimeSpanItemAsync(x as TimeSpanItem);
                RemoveTimespanItemFromList(x as TimeSpanItem);
                _isStandardWeekUpdatedEventAcceptable = false;
                _eventAggregator.GetEvent<StandardWeekUpdatedEvent>().Publish((x as TimeSpanItem).DayOfWeek);
            });
            IsSaveButtonEnabled = false;
            _eventAggregator.GetEvent<StandardWeekUpdatedEvent>().Subscribe(async (dayOfWeek) =>
            {
                if (_isStandardWeekUpdatedEventAcceptable)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var updatedMonday = new List<TimeSpanItem>();
                        var updatedTuesday = new List<TimeSpanItem>();
                        var updatedWednesday = new List<TimeSpanItem>();
                        var updatedThursday = new List<TimeSpanItem>();
                        var updatedFriday = new List<TimeSpanItem>();
                        var updatedSaturday = new List<TimeSpanItem>();
                        var updatedSunday = new List<TimeSpanItem>();
                        timespanItems = await _databaseService.GetTimeSpanItemsAsync();
                        foreach (var timespanItem in timespanItems)
                        {
                            timespanItem.EventAggregator = _eventAggregator;
                            switch ((System.DayOfWeek)timespanItem.DayOfWeek)
                            {
                                case System.DayOfWeek.Friday:
                                    updatedFriday.Add(timespanItem);
                                    break;

                                case System.DayOfWeek.Monday:
                                    updatedMonday.Add(timespanItem);
                                    break;

                                case System.DayOfWeek.Saturday:
                                    updatedSaturday.Add(timespanItem);
                                    break;

                                case System.DayOfWeek.Sunday:
                                    updatedSunday.Add(timespanItem);
                                    break;

                                case System.DayOfWeek.Thursday:
                                    updatedThursday.Add(timespanItem);
                                    break;

                                case System.DayOfWeek.Tuesday:
                                    updatedTuesday.Add(timespanItem);
                                    break;

                                case System.DayOfWeek.Wednesday:
                                    updatedWednesday.Add(timespanItem);
                                    break;

                                default:
                                    break;
                            }
                        }
                        Monday = new ObservableCollection<TimeSpanItem>(monday.OrderBy(x => x.Start));
                        Tuesday = new ObservableCollection<TimeSpanItem>(tuesday.OrderBy(x => x.Start));
                        Wednesday = new ObservableCollection<TimeSpanItem>(wednesday.OrderBy(x => x.Start));
                        Thursday = new ObservableCollection<TimeSpanItem>(thursday.OrderBy(x => x.Start));
                        Friday = new ObservableCollection<TimeSpanItem>(friday.OrderBy(x => x.Start));
                        Saturday = new ObservableCollection<TimeSpanItem>(saturday.OrderBy(x => x.Start));
                        Sunday = new ObservableCollection<TimeSpanItem>(sunday.OrderBy(x => x.Start));
                    });
                }
                else
                {
                    _isStandardWeekUpdatedEventAcceptable = true;
                    return;
                }
            });
            _eventAggregator.GetEvent<RemoteTimespanItemDeletedEvent>().Subscribe(async x => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => RemoveTimespanItemFromList(x)));
        }

        private void RemoveTimespanItemFromList(TimeSpanItem timeSpanItem)
        {
            switch ((System.DayOfWeek)timeSpanItem.DayOfWeek)
            {
                case System.DayOfWeek.Friday:
                    for (int i = Friday.Count; i > 0; i--)
                    {
                        if (Friday.ElementAt(i - 1).Id == timeSpanItem.Id)
                        {
                            Friday.RemoveAt(i - 1);
                            break;
                        }
                    }
                    break;

                case System.DayOfWeek.Monday:
                    for (int i = Monday.Count; i > 0; i--)
                    {
                        if (Monday.ElementAt(i - 1).Id == timeSpanItem.Id)
                        {
                            Monday.RemoveAt(i - 1);
                            break;
                        }
                    }
                    break;

                case System.DayOfWeek.Saturday:
                    for (int i = Saturday.Count; i > 0; i--)
                    {
                        if (Saturday.ElementAt(i - 1).Id == timeSpanItem.Id)
                        {
                            Saturday.RemoveAt(i - 1);
                            break;
                        }
                    }
                    break;

                case System.DayOfWeek.Sunday:
                    for (int i = Sunday.Count; i > 0; i--)
                    {
                        if (Sunday.ElementAt(i - 1).Id == timeSpanItem.Id)
                        {
                            Sunday.RemoveAt(i - 1);
                            break;
                        }
                    }
                    break;

                case System.DayOfWeek.Thursday:
                    for (int i = Thursday.Count; i > 0; i--)
                    {
                        if (Thursday.ElementAt(i - 1).Id == timeSpanItem.Id)
                        {
                            Thursday.RemoveAt(i - 1);
                            break;
                        }
                    }
                    break;

                case System.DayOfWeek.Tuesday:
                    for (int i = Tuesday.Count; i > 0; i--)
                    {
                        if (Tuesday.ElementAt(i - 1).Id == timeSpanItem.Id)
                        {
                            Tuesday.RemoveAt(i - 1);
                            break;
                        }
                    }
                    break;

                case System.DayOfWeek.Wednesday:
                    for (int i = Wednesday.Count; i > 0; i--)
                    {
                        if (Wednesday.ElementAt(i - 1).Id == timeSpanItem.Id)
                        {
                            Wednesday.RemoveAt(i - 1);
                            break;
                        }
                    }
                    break;

                default:
                    break;
            }
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

        public void Flyout_Closing(FlyoutBase sender, Windows.UI.Xaml.Controls.Primitives.FlyoutBaseClosingEventArgs args)
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
            IsSaveButtonEnabled = false;
            var resourceLoader = ResourceLoader.GetForCurrentView();
            TimespanItem = new TimeSpanItem() { TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(), EventAggregator = _eventAggregator };
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

        private ICommand _hideTimespanItemCommand;

        public ICommand HideTimespanItemCommand => _hideTimespanItemCommand ?? (_hideTimespanItemCommand = new DelegateCommand<object>((param) =>
        {
            (((param as Grid).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
            IsFlyoutOpen = false;
        }));

        private ICommand _addOrUpdateTimespanItemCommand;

        public ICommand AddOrUpdateTimespanItemCommand => _addOrUpdateTimespanItemCommand ?? (_addOrUpdateTimespanItemCommand = new DelegateCommand<object>(async (param) =>
        {
            if (TimespanItem.Id < 1)
            {
                TimespanItem.Id = await _databaseService.AddTimeSpanItemAsync(TimespanItem);
                switch ((System.DayOfWeek)TimespanItem.DayOfWeek)
                {
                    case System.DayOfWeek.Friday:
                        Friday.Add(new TimeSpanItem() { DayOfWeek = TimespanItem.DayOfWeek, End = TimespanItem.End, Id = TimespanItem.Id, Occupancy = TimespanItem.Occupancy, Start = TimespanItem.Start, TimeStamp = TimespanItem.TimeStamp, Width = TimespanItem.Width });
                        Friday = new ObservableCollection<TimeSpanItem>(Friday.OrderBy(x => x.Start));
                        break;

                    case System.DayOfWeek.Monday:
                        Monday.Add(new TimeSpanItem() { DayOfWeek = TimespanItem.DayOfWeek, End = TimespanItem.End, Id = TimespanItem.Id, Occupancy = TimespanItem.Occupancy, Start = TimespanItem.Start, TimeStamp = TimespanItem.TimeStamp, Width = TimespanItem.Width });
                        Monday = new ObservableCollection<TimeSpanItem>(Monday.OrderBy(x => x.Start));
                        break;

                    case System.DayOfWeek.Saturday:
                        Saturday.Add(new TimeSpanItem() { DayOfWeek = TimespanItem.DayOfWeek, End = TimespanItem.End, Id = TimespanItem.Id, Occupancy = TimespanItem.Occupancy, Start = TimespanItem.Start, TimeStamp = TimespanItem.TimeStamp, Width = TimespanItem.Width });
                        Saturday = new ObservableCollection<TimeSpanItem>(Saturday.OrderBy(x => x.Start));
                        break;

                    case System.DayOfWeek.Sunday:
                        Sunday.Add(new TimeSpanItem() { DayOfWeek = TimespanItem.DayOfWeek, End = TimespanItem.End, Id = TimespanItem.Id, Occupancy = TimespanItem.Occupancy, Start = TimespanItem.Start, TimeStamp = TimespanItem.TimeStamp, Width = TimespanItem.Width });
                        Sunday = new ObservableCollection<TimeSpanItem>(Sunday.OrderBy(x => x.Start));
                        break;

                    case System.DayOfWeek.Thursday:
                        Thursday.Add(new TimeSpanItem() { DayOfWeek = TimespanItem.DayOfWeek, End = TimespanItem.End, Id = TimespanItem.Id, Occupancy = TimespanItem.Occupancy, Start = TimespanItem.Start, TimeStamp = TimespanItem.TimeStamp, Width = TimespanItem.Width });
                        Thursday = new ObservableCollection<TimeSpanItem>(Thursday.OrderBy(x => x.Start));
                        break;

                    case System.DayOfWeek.Tuesday:
                        Tuesday.Add(new TimeSpanItem() { DayOfWeek = TimespanItem.DayOfWeek, End = TimespanItem.End, Id = TimespanItem.Id, Occupancy = TimespanItem.Occupancy, Start = TimespanItem.Start, TimeStamp = TimespanItem.TimeStamp, Width = TimespanItem.Width });
                        Tuesday = new ObservableCollection<TimeSpanItem>(Tuesday.OrderBy(x => x.Start));
                        break;

                    case System.DayOfWeek.Wednesday:
                        Wednesday.Add(new TimeSpanItem() { DayOfWeek = TimespanItem.DayOfWeek, End = TimespanItem.End, Id = TimespanItem.Id, Occupancy = TimespanItem.Occupancy, Start = TimespanItem.Start, TimeStamp = TimespanItem.TimeStamp, Width = TimespanItem.Width });
                        Wednesday = new ObservableCollection<TimeSpanItem>(Wednesday.OrderBy(x => x.Start));
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch ((System.DayOfWeek)TimespanItem.DayOfWeek)
                {
                    case System.DayOfWeek.Friday:
                        for (int i = 0; i < Friday.Count; i++)
                        {
                            if (Friday[i].Id == TimespanItem.Id)
                            {
                                Friday[i].End = TimespanItem.End;
                                Friday[i].Occupancy = TimespanItem.Occupancy;
                                Friday[i].Start = TimespanItem.Start;
                                Friday[i].TimeStamp = TimespanItem.TimeStamp;
                            }
                        }
                        break;

                    case System.DayOfWeek.Monday:
                        for (int i = 0; i < Monday.Count; i++)
                        {
                            if (Monday[i].Id == TimespanItem.Id)
                            {
                                Monday[i].End = TimespanItem.End;
                                Monday[i].Occupancy = TimespanItem.Occupancy;
                                Monday[i].Start = TimespanItem.Start;
                                Monday[i].TimeStamp = TimespanItem.TimeStamp;
                            }
                        }
                        break;

                    case System.DayOfWeek.Saturday:
                        for (int i = 0; i < Sunday.Count; i++)
                        {
                            if (Saturday[i].Id == TimespanItem.Id)
                            {
                                Saturday[i].End = TimespanItem.End;
                                Saturday[i].Occupancy = TimespanItem.Occupancy;
                                Saturday[i].Start = TimespanItem.Start;
                                Saturday[i].TimeStamp = TimespanItem.TimeStamp;
                            }
                        }
                        break;

                    case System.DayOfWeek.Sunday:
                        for (int i = 0; i < Sunday.Count; i++)
                        {
                            if (Sunday[i].Id == TimespanItem.Id)
                            {
                                Sunday[i].End = TimespanItem.End;
                                Sunday[i].Occupancy = TimespanItem.Occupancy;
                                Sunday[i].Start = TimespanItem.Start;
                                Sunday[i].TimeStamp = TimespanItem.TimeStamp;
                            }
                        }
                        break;

                    case System.DayOfWeek.Thursday:
                        for (int i = 0; i < Thursday.Count; i++)
                        {
                            if (Thursday[i].Id == TimespanItem.Id)
                            {
                                Thursday[i].End = TimespanItem.End;
                                Thursday[i].Occupancy = TimespanItem.Occupancy;
                                Thursday[i].Start = TimespanItem.Start;
                                Thursday[i].TimeStamp = TimespanItem.TimeStamp;
                            }
                        }
                        break;

                    case System.DayOfWeek.Tuesday:
                        for (int i = 0; i < Tuesday.Count; i++)
                        {
                            if (Tuesday[i].Id == TimespanItem.Id)
                            {
                                Tuesday[i].End = TimespanItem.End;
                                Tuesday[i].Occupancy = TimespanItem.Occupancy;
                                Tuesday[i].Start = TimespanItem.Start;
                                Tuesday[i].TimeStamp = TimespanItem.TimeStamp;
                            }
                        }
                        break;

                    case System.DayOfWeek.Wednesday:
                        for (int i = 0; i < Wednesday.Count; i++)
                        {
                            if (Wednesday[i].Id == TimespanItem.Id)
                            {
                                Wednesday[i].End = TimespanItem.End;
                                Wednesday[i].Occupancy = TimespanItem.Occupancy;
                                Wednesday[i].Start = TimespanItem.Start;
                                Wednesday[i].TimeStamp = TimespanItem.TimeStamp;
                            }
                        }
                        break;

                    default:
                        break;
                }
                await _databaseService.UpdateTimeSpanItemAsync(TimespanItem);
            }
            (((param as Grid).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
            IsFlyoutOpen = false;
            _eventAggregator.GetEvent<StandardWeekUpdatedEvent>().Publish(TimespanItem.DayOfWeek);
            TimespanItem = null;
        }));

        private ICommand _validateTimeCommand;

        public ICommand ValidateTimeCommand => _validateTimeCommand ?? (_validateTimeCommand = new DelegateCommand<object>((param) =>
        {
            if (TimespanItem != null)
            {
                List<TimeSpanItem> timeSpanItems;
                switch ((System.DayOfWeek)TimespanItem.DayOfWeek)
                {
                    case System.DayOfWeek.Friday:
                        timeSpanItems = Friday.ToList();
                        break;

                    case System.DayOfWeek.Monday:
                        timeSpanItems = Monday.ToList();
                        break;

                    case System.DayOfWeek.Saturday:
                        timeSpanItems = Saturday.ToList();
                        break;

                    case System.DayOfWeek.Sunday:
                        timeSpanItems = Sunday.ToList();
                        break;

                    case System.DayOfWeek.Thursday:
                        timeSpanItems = Thursday.ToList();
                        break;

                    case System.DayOfWeek.Tuesday:
                        timeSpanItems = Tuesday.ToList();
                        break;

                    case System.DayOfWeek.Wednesday:
                        timeSpanItems = Wednesday.ToList();
                        break;

                    default:
                        timeSpanItems = new List<TimeSpanItem>();
                        break;
                }
                if ((param as string).Equals("Start"))
                {
                    var timeSpanStart = TimeValidator.ValidateStartTime(TimespanItem, timeSpanItems);
                    if (timeSpanStart != TimeSpan.Zero) TimespanItem.Start = timeSpanStart;
                }
                else if ((param as string).Equals("End"))
                {
                    var timeSpanStart = TimeValidator.ValidateStartTime(TimespanItem, timeSpanItems);
                    if (timeSpanStart != TimeSpan.Zero) TimespanItem.Start = timeSpanStart;
                    var timeSpanEnd = TimeValidator.ValidateEndTime(TimespanItem, timeSpanItems);
                    if (timeSpanEnd != TimeSpan.Zero) TimespanItem.End = timeSpanEnd;
                }
                IsSaveButtonEnabled = TimespanItem.End <= TimespanItem.Start ? false : true;
            }
        }));

        private double width;
        private ICommand _updateDataTemplateWidthCommand;

        public ICommand UpdateDataTemplateWidthCommand => _updateDataTemplateWidthCommand ?? (_updateDataTemplateWidthCommand = new DelegateCommand<object>((param) =>
        {
            if (param == null) return;
            ListView listView = (ListView)param;
            width = listView.ActualWidth;
        }));

        public void monday_LayoutUpdated(object sender, object e)
        {
            if (Monday != null) for (int i = 0; i < Monday.Count; i++) Monday[i].Width = width;
            if (Tuesday != null) for (int i = 0; i < Tuesday.Count; i++) Tuesday[i].Width = width;
            if (Wednesday != null) for (int i = 0; i < Wednesday.Count; i++) Wednesday[i].Width = width;
            if (Thursday != null) for (int i = 0; i < Thursday.Count; i++) Thursday[i].Width = width;
            if (Friday != null) for (int i = 0; i < Friday.Count; i++) Friday[i].Width = width;
            if (Saturday != null) for (int i = 0; i < Saturday.Count; i++) Saturday[i].Width = width;
            if (Sunday != null) for (int i = 0; i < Sunday.Count; i++) Sunday[i].Width = width;
        }
    }
}