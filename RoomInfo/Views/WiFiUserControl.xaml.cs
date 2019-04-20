using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace RoomInfo.Views
{
    public sealed partial class WiFiUserControl : UserControl, INotifyPropertyChanged
    {
        WiFiAdapter _wiFiAdapter;
        ObservableCollection<WiFiNetwork> _wiFiNetworks = default;
        public ObservableCollection<WiFiNetwork> WiFiNetworks { get => _wiFiNetworks; set { SetProperty(ref _wiFiNetworks, value); } }

        public WiFiUserControl()
        {
            InitializeComponent();
            WiFiNetworks = new ObservableCollection<WiFiNetwork>();
        }

        private void ShowNetworks()
        {
            if (_wiFiAdapter != null)
            {                
                foreach (var wiFiAvailableNetwork in _wiFiAdapter.NetworkReport.AvailableNetworks)
                {
                    var wiFiAvailableNetworkQuery = WiFiNetworks.Where(x => x.NetworkName.Equals(wiFiAvailableNetwork.Ssid)).Select(x => x).FirstOrDefault();
                    if (wiFiAvailableNetworkQuery == null) WiFiNetworks.Add(new WiFiNetwork(_wiFiAdapter, wiFiAvailableNetwork) { HashCode = wiFiAvailableNetwork.GetHashCode(), NetworkName = wiFiAvailableNetwork.Ssid  });
                }
            }
        }

        private async Task InitializeFirstAdapter()
        {
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access == WiFiAccessStatus.Allowed)
            {
                var wifiAdapterResults = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (wifiAdapterResults.Count >= 1) _wiFiAdapter = await WiFiAdapter.FromIdAsync(wifiAdapterResults[0].Id);                
            }
        }

        private async Task ScanForNetworks()
        {
            if (_wiFiAdapter != null)await _wiFiAdapter.ScanAsync();            
        }

        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning disable CS0628 // Neues geschütztes Element deklariert in versiegelter Klasse
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
#pragma warning restore CS0628 // Neues geschütztes Element deklariert in versiegelter Klasse
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
#pragma warning disable CS0628 // Neues geschütztes Element deklariert in versiegelter Klasse
        protected bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
#pragma warning restore CS0628 // Neues geschütztes Element deklariert in versiegelter Klasse
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);
            return true;
        }
        void RaisePropertyChanged([CallerMemberName]string propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

        private async void UserControl_Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            await InitializeFirstAdapter();
            await ScanForNetworks();
            ShowNetworks();
            if (_wiFiAdapter != null) _wiFiAdapter.AvailableNetworksChanged += (s, e) => ShowNetworks();
        }
    }
}
