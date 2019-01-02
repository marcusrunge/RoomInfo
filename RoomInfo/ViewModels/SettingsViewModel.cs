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
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.System.Profile;
using ModelLibrary;
using Windows.Globalization;
using Windows.ApplicationModel.Core;

namespace RoomInfo.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase
    {
        IApplicationDataService _applicationDataService;
        IIotService _iotService;
        INavigationService _navigationService;

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
                
        string _tcpPort = default(string);
        public string TcpPort { get => _tcpPort; set { SetProperty(ref _tcpPort, value); if (!string.IsNullOrEmpty(UdpPort)) _applicationDataService.SaveSetting("TcpPort", TcpPort); } }

        string _udpPort = default(string);
        public string UdpPort { get => _udpPort; set { SetProperty(ref _udpPort, value); if (!string.IsNullOrEmpty(UdpPort)) _applicationDataService.SaveSetting("UdpPort", UdpPort); } }

        Visibility _iotPanelVisibility = default(Visibility);
        public Visibility IotPanelVisibility { get => _iotPanelVisibility; set { SetProperty(ref _iotPanelVisibility, value); } }

        string _reservedProperty = default(string);
        public string ReservedProperty { get => _reservedProperty; set { SetProperty(ref _reservedProperty, value); } }

        ModelLibrary.Language _language = default(ModelLibrary.Language);
        public ModelLibrary.Language Language { get => _language; set { SetProperty(ref _language, value); } }

        public SettingsViewModel(IApplicationDataService applicationDataService, IIotService iotService, INavigationService navigationService)
        {
            _applicationDataService = applicationDataService;
            _iotService = iotService;
            _navigationService = navigationService;
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
                    if (Windows.Globalization.Language.CurrentInputMethodLanguageTag.Equals("de-DE") ) return ModelLibrary.Language.de_DE;
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
                StorageFolder assets = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                await file.CopyAsync(assets, file.Name, NameCollisionOption.ReplaceExisting);
                _applicationDataService.SaveSetting("LogoFileName", file.Name);
                await LoadCompanyLogo();
            }
        }));

        private ICommand _deleteLogoCommand;
        public ICommand DeleteLogoCommand => _deleteLogoCommand ?? (_deleteLogoCommand = new DelegateCommand<object>(async (param) =>
        {
            StorageFolder assets = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            string logoFileName = _applicationDataService.GetSetting<string>("LogoFileName");
            StorageFile storageFile = await assets.GetFileAsync(logoFileName);
            await storageFile.DeleteAsync();
            _applicationDataService.RemoveSetting("LogoFileName");
            await LoadCompanyLogo();
        }));

        private ICommand _configWiFiCommand;
        public ICommand ConfigWiFiCommand => _configWiFiCommand ?? (_configWiFiCommand = new DelegateCommand<object>((param) =>
        {
            
        }));

        private ICommand _restartCommand;
        public ICommand RestartCommand => _restartCommand ?? (_restartCommand = new DelegateCommand<object>((param) =>
        {

        }));

        private ICommand _shutdownCommand;
        public ICommand ShutdownCommand => _shutdownCommand ?? (_shutdownCommand = new DelegateCommand<object>((param) =>
        {

        }));

        private async Task LoadCompanyLogo()
        {
            StorageFolder assets = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            string logoFileName = _applicationDataService.GetSetting<string>("LogoFileName");
            CompanyLogo = new Uri(assets.Path + "/" + logoFileName);
        }

        public void KeyDown(object sender, KeyRoutedEventArgs e)
        {
            int virtualKey = (int)e.Key;
            if ((virtualKey > 47 && virtualKey < 58) || (virtualKey > 95 && virtualKey < 106)) e.Handled = false;
            else e.Handled = true;
        }
    }
}
