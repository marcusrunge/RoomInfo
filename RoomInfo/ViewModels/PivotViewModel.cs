
using ApplicationServiceLibrary;
using BackgroundComponent;
using Microsoft.Practices.Unity;
using NetworkServiceLibrary;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Background;

namespace RoomInfo.ViewModels
{
    public class PivotViewModel : ViewModelBase
    {
        IApplicationDataService _applicationDataService;
        IBackgroundTaskService _backgroundTaskService;
        ILiveTileUpdateService _liveTileUpdateService;
        IUserDatagramService _userDatagramService;
        ITransmissionControlService _transmissionControlService;

        public PivotViewModel(IUnityContainer unityContainer)
        {
            _applicationDataService = unityContainer.Resolve<IApplicationDataService>();
            _backgroundTaskService = unityContainer.Resolve<IBackgroundTaskService>();
            _liveTileUpdateService = unityContainer.Resolve<ILiveTileUpdateService>();
            _userDatagramService = unityContainer.Resolve<IUserDatagramService>();
            _transmissionControlService = unityContainer.Resolve<ITransmissionControlService>();
        }

        public async override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            if (string.IsNullOrEmpty(_applicationDataService.GetSetting<string>("TcpPort"))) _applicationDataService.SaveSetting("TcpPort", "8273");
            if (string.IsNullOrEmpty(_applicationDataService.GetSetting<string>("UdpPort"))) _applicationDataService.SaveSetting("UdpPort", "8274");
            _liveTileUpdateService.UpdateTile(_liveTileUpdateService.CreateTile(await _liveTileUpdateService.GetActiveAgendaItem()));
            await _backgroundTaskService.Register<LiveTileUpdateBackgroundTask>(new TimeTrigger(15, false));
            await _userDatagramService.StartListenerAsync();
            await _transmissionControlService.StartListenerAsync();

        }
    }
}
