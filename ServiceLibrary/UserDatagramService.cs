using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using System;

namespace ServiceLibrary
{
    public interface IUserDatagramService
    {
        Task StartListenerAsync();
    }
    public class UserDatagramService : IUserDatagramService
    {
        IApplicationDataService _applicationDataService;
        IBackgroundTaskService _backgroundTaskService;

        public UserDatagramService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
        }
        public async Task StartListenerAsync()
        {
            var window = CoreWindow.GetForCurrentThread();
            var dispatcher = window.Dispatcher;
            var datagramSocket = new DatagramSocket();
            var backgroundTaskRegistration = await _backgroundTaskService.Register<UserDatagramService>(new SocketActivityTrigger());
            datagramSocket.EnableTransferOwnership(backgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
            await datagramSocket.BindServiceNameAsync(_applicationDataService.GetSetting<string>("UdpPort"));
            datagramSocket.MessageReceived += async (s, e) =>
              {

              };
        }
    }
}
