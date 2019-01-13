using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using System;
using ModelLibrary;
using Newtonsoft.Json;
using BackgroundComponent;
using ApplicationServiceLibrary;
using Prism.Events;

namespace NetworkServiceLibrary
{
    public interface IUserDatagramService
    {
        Task StartListenerAsync();
        Task StopListenerAsync();
    }
    public class UserDatagramService : IUserDatagramService
    {
        IApplicationDataService _applicationDataService;
        IBackgroundTaskService _backgroundTaskService;
        ITransmissionControlService _transmissionControlService;
        IEventAggregator _eventAggregator;
        DatagramSocket _datagramSocket;

        public UserDatagramService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, ITransmissionControlService transmissionControlService, IEventAggregator eventAggregator)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _transmissionControlService = transmissionControlService;
            _eventAggregator = eventAggregator;
            _datagramSocket = new DatagramSocket();
        }
        public async Task StartListenerAsync()
        {
            try
            {
                var window = CoreWindow.GetForCurrentThread();
                var dispatcher = window.Dispatcher;
                var backgroundTaskRegistration = await _backgroundTaskService.Register<UserDatagramBackgroundTask>(new SocketActivityTrigger());
                _datagramSocket.EnableTransferOwnership(backgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                _datagramSocket.MessageReceived += async (s, e) =>
                {
                    var roomPackage = new Room() { RoomGuid = _applicationDataService.GetSetting<string>("Guid"), RoomName = _applicationDataService.GetSetting<string>("RoomName"), RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber") };
                    var json = JsonConvert.SerializeObject(roomPackage);
                    await _transmissionControlService.SendStringData(e.RemoteAddress, _applicationDataService.GetSetting<string>("TcpPort"), json);
                    await s.CancelIOAsync();
                    s.Dispose();
                };
                await _datagramSocket.BindServiceNameAsync(_applicationDataService.GetSetting<string>("UdpPort"));
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }

            _eventAggregator.GetEvent<PortChangedEvent>().Subscribe(async ()=>
            {
                await StopListenerAsync();
                await StartListenerAsync();
            });
        }

        public async Task StopListenerAsync()
        {
            if (_datagramSocket != null)
            {
                await _datagramSocket.CancelIOAsync();
                _datagramSocket.Dispose();
                _datagramSocket = null;
            }
        }
    }
}
