using Microsoft.Practices.Unity;
using ApplicationServiceLibrary;
using Windows.ApplicationModel.Background;

namespace BackgroundComponent
{
    public sealed class LiveTileUpdateBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        ILiveTileUpdateService _liveTileUpdateService;
        IUnityContainer _unityContainer;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IDatabaseService, DatabaseService>();
            _unityContainer.RegisterType<ILiveTileUpdateService, LiveTileUpdateService>();
            _liveTileUpdateService = _unityContainer.Resolve<ILiveTileUpdateService>();
            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
            _deferral.Complete();
        }
    }
}
