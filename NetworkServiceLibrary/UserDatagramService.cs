using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using System;
using ModelLibrary;
using Newtonsoft.Json;
using BackgroundComponent;
using ApplicationServiceLibrary;

namespace NetworkServiceLibrary
{
    public interface IUserDatagramService
    {
        Task StartListenerAsync();
    }
    public class UserDatagramService : IUserDatagramService
    {
        IApplicationDataService _applicationDataService;
        IBackgroundTaskService _backgroundTaskService;
        ITransmissionControlService _transmissionControlService;

        public UserDatagramService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, ITransmissionControlService transmissionControlService)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _transmissionControlService = transmissionControlService;
        }
        public async Task StartListenerAsync()
        {
            var window = CoreWindow.GetForCurrentThread();
            var dispatcher = window.Dispatcher;
            var datagramSocket = new DatagramSocket();
            var backgroundTaskRegistration = await _backgroundTaskService.Register<UserDatagramBackgroundTask>(new SocketActivityTrigger());
            datagramSocket.EnableTransferOwnership(backgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
            datagramSocket.MessageReceived += async (s, e) =>
              {
                  var roomPackage = new Room() { RoomGuid = _applicationDataService.GetSetting<string>("Guid"), RoomName = _applicationDataService.GetSetting<string>("RoomName"), RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber") };                  
                  var json = JsonConvert.SerializeObject(roomPackage);
                  await _transmissionControlService.SendStringData(e.RemoteAddress, _applicationDataService.GetSetting<string>("TcpPort"), json);
              };
            await datagramSocket.BindServiceNameAsync(_applicationDataService.GetSetting<string>("UdpPort"));
        }
    }
}
