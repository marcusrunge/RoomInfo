﻿using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ModelLibrary
{
    public class WiFiNetwork : BindableBase
    {
        IEventAggregator _eventAggregator;
        bool _isConnected;
        int _hashCode = default(int);
        CoreDispatcher _coreDispatcher;
        public int HashCode { get => _hashCode; set { SetProperty(ref _hashCode, value); } }

        string _networkName = default(string);
        public string NetworkName { get => _networkName; set { SetProperty(ref _networkName, value); } }

        PasswordCredential _passwordCredential = default(PasswordCredential);
        public PasswordCredential PasswordCredential { get => _passwordCredential; set { SetProperty(ref _passwordCredential, value); } }

        bool _automaticReconnect = default(bool);
        public bool AutomaticReconnect { get => _automaticReconnect; set { SetProperty(ref _automaticReconnect, value); } }

        Visibility _lowerGridVisibility = default(Visibility);
        public Visibility LowerGridVisibility { get => _lowerGridVisibility; set { SetProperty(ref _lowerGridVisibility, value); } }

        ResourceLoader _resourceLoader;
        string _connectButtonContent = default(string);
        public string ConnectButtonContent { get => _connectButtonContent; set { SetProperty(ref _connectButtonContent, value); } }

        WiFiAvailableNetwork _wiFiAvailableNetwork;
        WiFiAdapter _wiFiAdapter;

        public WiFiNetwork(WiFiAdapter wiFiAdapter, WiFiAvailableNetwork wiFiAvailableNetwork)
        {
            LowerGridVisibility = Visibility.Collapsed;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            _coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            _wiFiAdapter = wiFiAdapter;
            _wiFiAvailableNetwork = wiFiAvailableNetwork;
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _eventAggregator.GetEvent<WiFiNetworkSelectionChangedUpdatedEvent>().Subscribe((i) => LowerGridVisibility = i == HashCode ? Visibility.Visible : Visibility.Collapsed);
            _eventAggregator.GetEvent<WiFiNetworkConnectionChangedUpdatedEvent>().Subscribe((i) =>
            {
                if (i != HashCode)
                {
                    ConnectButtonContent = _resourceLoader.GetString("WiFiUserControl_ConnectButton/Content");
                    _isConnected = false;
                }
            });
            Task.Run(async () =>
            {
                await _coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var connectedProfile = await _wiFiAdapter.NetworkAdapter.GetConnectedProfileAsync();
                    if (connectedProfile != null && connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid() == _wiFiAvailableNetwork.Ssid)
                    {
                        _isConnected = true;
                        ConnectButtonContent = _resourceLoader.GetString("WiFiUserControl_DisconnectButton/Content");
                    }
                    else ConnectButtonContent = _resourceLoader.GetString("WiFiUserControl_ConnectButton/Content");
                });
            }).Wait();
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await _coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var connectedProfile = await _wiFiAdapter.NetworkAdapter.GetConnectedProfileAsync();
                if (connectedProfile != null && connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid() == _wiFiAvailableNetwork.Ssid)
                {
                    _isConnected = true;
                    ConnectButtonContent = _resourceLoader.GetString("WiFiUserControl_DisconnectButton/Content");
                }
                else ConnectButtonContent = _resourceLoader.GetString("WiFiUserControl_ConnectButton/Content");
            });
        }

        private ICommand _cycleVisibilityCommand;
        public ICommand CycleVisibilityCommand => _cycleVisibilityCommand ?? (_cycleVisibilityCommand = new DelegateCommand<object>((param) =>
        {
            LowerGridVisibility = ((string)param).Equals("true") ? Visibility.Visible : Visibility.Collapsed;
            _eventAggregator.GetEvent<WiFiNetworkSelectionChangedUpdatedEvent>().Publish(HashCode);
        }));

        private ICommand _connectCommand;
        public ICommand ConnectCommand => _connectCommand ?? (_connectCommand = new DelegateCommand<object>(async (param) =>
        {
            if (_isConnected)
            {
                _wiFiAdapter.Disconnect();
                _isConnected = false;
                ConnectButtonContent = _resourceLoader.GetString("WiFiUserControl_ConnectButton/Content");
            }
            else
            {
                await _wiFiAdapter.ConnectAsync(_wiFiAvailableNetwork, AutomaticReconnect ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual, PasswordCredential);
                _isConnected = true;
                ConnectButtonContent = _resourceLoader.GetString("WiFiUserControl_DisconnectButton/Content");
            }
        }));
    }
}
