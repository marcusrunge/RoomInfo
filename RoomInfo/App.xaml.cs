﻿using ApplicationServiceLibrary;
using Microsoft.HockeyApp;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using ModelLibrary;
using NetworkServiceLibrary;
using Prism.Events;
using Prism.Mvvm;
using Prism.Unity.Windows;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using RoomInfo.Services;
using RoomInfo.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoomInfo
{
    [Windows.UI.Xaml.Data.Bindable]
    public sealed partial class App : PrismUnityApplication
    {
        private IApplicationDataService _applicationDataService;
        private IUserDatagramService _userDatagramService;
        private ITransmissionControlService _transmissionControlService;
        private IDatabaseService _databaseService;
        private List<ExceptionLogItem> _exceptionLogItems;

        public App()
        {
            _exceptionLogItems = new List<ExceptionLogItem>();
            try
            {
                InitializeComponent();
                HockeyClient.Current.Configure("5bccdd1ce267413199ecaaeebbf88295");
            }
            catch (Exception e)
            {
                if (_exceptionLogItems != null) _exceptionLogItems.Add(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        protected override void ConfigureContainer()
        {
            // register a singleton using Container.RegisterType<IInterface, Type>(new ContainerControlledLifetimeManager());
            base.ConfigureContainer();
            try
            {
                Container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));
                Container.RegisterType<ISettingsService, SettingsService>();
                Container.RegisterType<IDatabaseService, DatabaseService>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IApplicationDataService, ApplicationDataService>();
                Container.RegisterType<ILiveTileUpdateService, LiveTileUpdateService>();
                Container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IBackgroundTaskService, BackgroundTaskService>();
                Container.RegisterType<ITransmissionControlService, TransmissionControlService>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IUserDatagramService, UserDatagramService>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IDateTimeValidationService, DateTimeValidationService>();
                Container.RegisterType<IIotService, IotService>();
            }
            catch (Exception e)
            {
                if (_exceptionLogItems != null) _exceptionLogItems.Add(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            return LaunchApplicationAsync(PageTokens.InfoPage, null);
        }

        private async Task LaunchApplicationAsync(string page, object launchParam)
        {
            try
            {
                _applicationDataService = Container.Resolve<IApplicationDataService>();
                _userDatagramService = Container.Resolve<IUserDatagramService>();
                _transmissionControlService = Container.Resolve<ITransmissionControlService>();
                _databaseService = Container.Resolve<IDatabaseService>();
                if (string.IsNullOrEmpty(_applicationDataService.GetSetting<string>("Guid"))) _applicationDataService.SaveSetting("Guid", Guid.NewGuid().ToString());
                ThemeSelectorService.SetRequestedTheme();
                await SetSelectedLanguage();
                NavigationService.Navigate(page, launchParam);
                Window.Current.Activate();
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            if (_databaseService != null && _exceptionLogItems != null && _exceptionLogItems.Count > 0)
            {
                _exceptionLogItems.ForEach(x => _databaseService.AddExceptionLogItem(x));
            }

            //return Task.CompletedTask;
        }

        private async Task SetSelectedLanguage()
        {
            try
            {
                var language = _applicationDataService.GetSetting<string>("Language");
                if (string.IsNullOrEmpty(language)) return;
                else ApplicationLanguages.PrimaryLanguageOverride = language;
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        protected override Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            try
            {
                CreateAndConfigureContainer();
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        protected override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            try
            {
                await ThemeSelectorService.InitializeAsync().ConfigureAwait(false);

                // We are remapping the default ViewNamePage and ViewNamePageViewModel naming to ViewNamePage and ViewNameViewModel to
                // gain better code reuse with other frameworks and pages within Windows Template Studio
                ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
                {
                    var viewModelTypeName = string.Format(CultureInfo.InvariantCulture, "RoomInfo.ViewModels.{0}ViewModel, RoomInfo", viewType.Name.Substring(0, viewType.Name.Length - 4));
                    return Type.GetType(viewModelTypeName);
                });
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            await base.OnInitializeAsync(args);
        }

        protected override async void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();
            try
            {
                UnityServiceLocator unityServiceLocator = new UnityServiceLocator(Container);
                ServiceLocator.SetLocatorProvider(() => unityServiceLocator);
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        protected override IDeviceGestureService OnCreateDeviceGestureService()
        {
            var service = base.OnCreateDeviceGestureService();
            service.UseTitleBarBackButton = false;
            return service;
        }

        public void SetNavigationFrame(Frame frame)
        {
            var sessionStateService = Container.Resolve<ISessionStateService>();
            CreateNavigationService(new FrameFacadeAdapter(frame), sessionStateService);
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = Container.Resolve<ShellPage>();
            shell.SetRootFrame(rootFrame);
            return shell;
        }

        protected override async Task OnSuspendingApplicationAsync()
        {
            try
            {
                await _userDatagramService.TransferOwnership();
                await _transmissionControlService.TransferOwnership();
                await base.OnSuspendingApplicationAsync();
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }
    }
}