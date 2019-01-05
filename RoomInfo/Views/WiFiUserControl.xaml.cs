using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace RoomInfo.Views
{
    public sealed partial class WiFiUserControl : UserControl, INotifyPropertyChanged
    {
        public WiFiAdapter WiFiAdapter { get; private set; }
        ObservableCollection<WiFiNetwork> _wiFiNetworks = default(ObservableCollection<WiFiNetwork>);
        public ObservableCollection<WiFiNetwork> WiFiNetworks { get => _wiFiNetworks; set { SetProperty(ref _wiFiNetworks, value); } }

        public WiFiUserControl()
        {
            this.InitializeComponent();
            WiFiNetworks = new ObservableCollection<WiFiNetwork>();
        }

        protected async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            await InitializeFirstAdapter();
            await ScanForNetworks();
            ShowNetworks();
            WiFiAdapter.AvailableNetworksChanged += (s, e) => ShowNetworks();
        }

        private void ShowNetworks()
        {
            foreach (var wiFiAvailableNetwork in WiFiAdapter.NetworkReport.AvailableNetworks)
            {
                WiFiNetworks.Add(new WiFiNetwork() { NetworkName = wiFiAvailableNetwork.Ssid });
            }
        }

        private async Task InitializeFirstAdapter()
        {
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                throw new Exception("WiFiAccessStatus not allowed");
            }
            else
            {
                var wifiAdapterResults = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (wifiAdapterResults.Count >= 1)
                {
                    this.WiFiAdapter = await WiFiAdapter.FromIdAsync(wifiAdapterResults[0].Id);
                }
                else
                {
                    throw new Exception("WiFi Adapter not found.");
                }
            }
        }

        private async Task ScanForNetworks()
        {
            if (this.WiFiAdapter != null)
            {
                await this.WiFiAdapter.ScanAsync();
            }
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
    }
}
