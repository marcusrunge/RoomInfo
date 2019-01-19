using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;
using Prism.Unity.Windows;
using Prism.Windows.AppModel;

using RoomInfo.Services;
using ApplicationServiceLibrary;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using NetworkServiceLibrary;
using Windows.Globalization;
using Microsoft.HockeyApp;

namespace RoomInfo
{
    [Windows.UI.Xaml.Data.Bindable]
    public sealed partial class App : PrismUnityApplication
    {
        IApplicationDataService _applicationDataService;
        IUserDatagramService _userDatagramService;
        ITransmissionControlService _transmissionControlService;
        public App()
        {
            InitializeComponent();
            HockeyClient.Current.Configure("5bccdd1ce267413199ecaaeebbf88295");
        }

        protected override void ConfigureContainer()
        {
            // register a singleton using Container.RegisterType<IInterface, Type>(new ContainerControlledLifetimeManager());
            base.ConfigureContainer();
            Container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));
            Container.RegisterType<ISettingsService, SettingsService>();
            Container.RegisterType<IDatabaseService, DatabaseService>();
            Container.RegisterType<IApplicationDataService, ApplicationDataService>();
            Container.RegisterType<ILiveTileUpdateService, LiveTileUpdateService>();
            Container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IBackgroundTaskService, BackgroundTaskService>();
            Container.RegisterType<ITransmissionControlService, TransmissionControlService>();
            Container.RegisterType<IUserDatagramService, UserDatagramService>();
            Container.RegisterType<IDateTimeValidationService, DateTimeValidationService>();
            Container.RegisterType<IIotService, IotService>();
            Container.RegisterType<IBackgroundTaskRegistrationProvider, BackgroundTaskRegistrationProvider>();
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            return LaunchApplicationAsync(PageTokens.PivotPage, null);
        }

        private Task LaunchApplicationAsync(string page, object launchParam)
        {
            _applicationDataService = Container.Resolve<IApplicationDataService>();
            _userDatagramService = Container.Resolve<IUserDatagramService>();
            _transmissionControlService = Container.Resolve<ITransmissionControlService>();
            if (string.IsNullOrEmpty(_applicationDataService.GetSetting<string>("Guid"))) _applicationDataService.SaveSetting("Guid", Guid.NewGuid().ToString());
            ThemeSelectorService.SetRequestedTheme();
            SetSelectedLanguage();
            NavigationService.Navigate(page, launchParam);
            Window.Current.Activate();
            return Task.CompletedTask;
        }

        private void SetSelectedLanguage()
        {
            var language = _applicationDataService.GetSetting<string>("Language");
            if (string.IsNullOrEmpty(language)) return;
            else ApplicationLanguages.PrimaryLanguageOverride = language;            
        }

        protected override Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            CreateAndConfigureContainer();
        }

        protected override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            await ThemeSelectorService.InitializeAsync().ConfigureAwait(false);

            // We are remapping the default ViewNamePage and ViewNamePageViewModel naming to ViewNamePage and ViewNameViewModel to
            // gain better code reuse with other frameworks and pages within Windows Template Studio
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewModelTypeName = string.Format(CultureInfo.InvariantCulture, "RoomInfo.ViewModels.{0}ViewModel, RoomInfo", viewType.Name.Substring(0, viewType.Name.Length - 4));
                return Type.GetType(viewModelTypeName);
            });
            await base.OnInitializeAsync(args);
        }

        protected override void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();
            UnityServiceLocator unityServiceLocator = new UnityServiceLocator(Container);
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator);
        }

        protected override async Task OnSuspendingApplicationAsync()
        {
            await _userDatagramService.TransferOwnership();
            await _transmissionControlService.TransferOwnership();
            await base.OnSuspendingApplicationAsync();
        }
    }
}
