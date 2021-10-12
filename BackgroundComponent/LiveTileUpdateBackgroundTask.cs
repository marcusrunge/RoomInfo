using ApplicationServiceLibrary;
using Microsoft.Practices.Unity;
using Windows.ApplicationModel.Background;

namespace BackgroundComponent
{
    public sealed class LiveTileUpdateBackgroundTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private ILiveTileUpdateService _liveTileUpdateService;
        private IUnityContainer _unityContainer;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IDatabaseService, DatabaseService>();
            _unityContainer.RegisterType<IApplicationDataService, ApplicationDataService>();
            _unityContainer.RegisterType<ILiveTileUpdateService, LiveTileUpdateService>();
            _liveTileUpdateService = _unityContainer.Resolve<ILiveTileUpdateService>();
            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
            _deferral.Complete();
        }
    }
}