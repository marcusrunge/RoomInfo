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

using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace RoomInfo
{
    [Windows.UI.Xaml.Data.Bindable]
    public sealed partial class App : PrismUnityApplication
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void ConfigureContainer()
        {
            // register a singleton using Container.RegisterType<IInterface, Type>(new ContainerControlledLifetimeManager());
            base.ConfigureContainer();
            Container.RegisterType<IBackgroundTaskService, BackgroundTaskService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILiveTileService, LiveTileService>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));
            Container.RegisterType<ISettingsService, SettingsService>();
            Container.RegisterType<IDatabaseService, DatabaseService>();
            Container.RegisterType<IEventAggregator, EventAggregator>();
            UnityServiceLocator unityServiceLocator = new UnityServiceLocator(Container);
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator); Container.RegisterType<IEventAggregator, EventAggregator>();            
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            return LaunchApplicationAsync(PageTokens.PivotPage, null);
        }

        private async Task LaunchApplicationAsync(string page, object launchParam)
        {
            Services.ThemeSelectorService.SetRequestedTheme();
            NavigationService.Navigate(page, launchParam);
            Window.Current.Activate();
            Container.Resolve<ILiveTileService>().SampleUpdate();
            await Task.CompletedTask;
        }

        protected override Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            CreateAndConfigureContainer();
            Container.Resolve<IBackgroundTaskService>().Start(args.TaskInstance);
        }

        protected override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            await Container.Resolve<IBackgroundTaskService>().RegisterBackgroundTasksAsync();
            await Container.Resolve<ILiveTileService>().EnableQueueAsync().ConfigureAwait(false);
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
    }
}
