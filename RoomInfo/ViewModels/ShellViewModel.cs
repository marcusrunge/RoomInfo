﻿using ApplicationServiceLibrary;
using BackgroundComponent;
using Microsoft.Practices.Unity;
using NetworkServiceLibrary;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using RoomInfo.Helpers;
using RoomInfo.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace RoomInfo.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private static INavigationService _navigationService;
        private Frame _frame;
        private WinUI.NavigationView _navigationView;
        private bool _isBackEnabled;
        private WinUI.NavigationViewItem _selected;
        private IApplicationDataService _applicationDataService;
        private IBackgroundTaskService _backgroundTaskService;
        private ILiveTileUpdateService _liveTileUpdateService;
        private IUserDatagramService _userDatagramService;
        private ITransmissionControlService _transmissionControlService;

        public ICommand ItemInvokedCommand { get; }

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { SetProperty(ref _isBackEnabled, value); }
        }

        public WinUI.NavigationViewItem Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        public ShellViewModel(INavigationService navigationServiceInstance, IUnityContainer unityContainer)
        {
            _navigationService = navigationServiceInstance;
            _applicationDataService = unityContainer.Resolve<IApplicationDataService>();
            _backgroundTaskService = unityContainer.Resolve<IBackgroundTaskService>();
            _liveTileUpdateService = unityContainer.Resolve<ILiveTileUpdateService>();
            _userDatagramService = unityContainer.Resolve<IUserDatagramService>();
            _transmissionControlService = unityContainer.Resolve<ITransmissionControlService>();
            ItemInvokedCommand = new DelegateCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked);
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
        }

        public async void Initialize(Frame frame, WinUI.NavigationView navigationView)
        {
            _frame = frame;
            _navigationView = navigationView;
            _frame.NavigationFailed += (sender, e) =>
            {
                throw e.Exception;
            };
            _frame.Navigated += Frame_Navigated;
            _navigationView.BackRequested += OnBackRequested;
            if (string.IsNullOrEmpty(_applicationDataService.GetSetting<string>("TcpPort"))) _applicationDataService.SaveSetting("TcpPort", "8273");
            if (string.IsNullOrEmpty(_applicationDataService.GetSetting<string>("UdpPort"))) _applicationDataService.SaveSetting("UdpPort", "8274");
            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
            try
            {
                if (_backgroundTaskService.FindRegistration<LiveTileUpdateBackgroundTask>() == null) await _backgroundTaskService.Register<LiveTileUpdateBackgroundTask>(new TimeTrigger(15, false));
            }
            catch { }
            await _userDatagramService.StartListenerAsync();
            await _transmissionControlService.StartListenerAsync();
        }

        private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                _navigationService.Navigate("Settings", null);
                return;
            }

            var item = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            var pageKey = item.GetValue(NavHelper.NavigateToProperty) as string;
            _navigationService.Navigate(pageKey, null);
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = _navigationService.CanGoBack();
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            Selected = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            _navigationService.GoBack();
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var sourcePageKey = sourcePageType.Name;
            sourcePageKey = sourcePageKey.Substring(0, sourcePageKey.Length - 4);
            var pageKey = menuItem.GetValue(NavHelper.NavigateToProperty) as string;
            return pageKey == sourcePageKey;
        }
    }
}