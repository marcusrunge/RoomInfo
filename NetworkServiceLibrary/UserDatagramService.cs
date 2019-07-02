using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using System;
using ModelLibrary;
using Newtonsoft.Json;
using ApplicationServiceLibrary;
using Prism.Events;
using Windows.Networking;
using System.IO;
using Windows.ApplicationModel.Background;
using BackgroundComponent;

namespace NetworkServiceLibrary
{
    public interface IUserDatagramService
    {
        Task StartListenerAsync();        
        Task TransferOwnership();
        Task SendStringData(HostName hostName, string port, string data);
        Task SendStringData(DatagramSocket datagramSocket, string data);
    }
    public class UserDatagramService : IUserDatagramService
    {
        IApplicationDataService _applicationDataService;
        IBackgroundTaskService _backgroundTaskService;
        ITransmissionControlService _transmissionControlService;
        IEventAggregator _eventAggregator;
        IIotService _iotService;
        DatagramSocket _datagramSocket;
        BackgroundTaskRegistration _backgroundTaskRegistration;

        public UserDatagramService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, ITransmissionControlService transmissionControlService, IEventAggregator eventAggregator, IIotService iotService)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _transmissionControlService = transmissionControlService;
            _eventAggregator = eventAggregator;
            _iotService = iotService;
            _backgroundTaskRegistration = null;
        }

        public async Task SendStringData(HostName hostName, string port, string data)
        {
            try
            {
                using (var datagramSocket = new DatagramSocket())
                {
                    await datagramSocket.ConnectAsync(hostName, port);
                    using (Stream outputStream = datagramSocket.OutputStream.AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(data);
                            await streamWriter.FlushAsync();
                        }
                    }
                    datagramSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }
        }

        public async Task SendStringData(DatagramSocket datagramSocket, string data)
        {
            try
            {
                using (datagramSocket)
                {
                    using (Stream outputStream = datagramSocket.OutputStream.AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(data);
                            await streamWriter.FlushAsync();
                        }
                    }
                    datagramSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }
        }

        public async Task StartListenerAsync()
        {
            try
            {            
                _datagramSocket = new DatagramSocket();
                var window = CoreWindow.GetForCurrentThread();
                var dispatcher = window.Dispatcher;
                _backgroundTaskRegistration = (BackgroundTaskRegistration)_backgroundTaskService.FindRegistration<SocketActivityTriggerBackgroundTask>();
                if (_backgroundTaskRegistration == null) _backgroundTaskRegistration = await _backgroundTaskService.Register<SocketActivityTriggerBackgroundTask>(new SocketActivityTrigger());
                if (_backgroundTaskRegistration != null) _datagramSocket.EnableTransferOwnership(_backgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                await _datagramSocket.BindServiceNameAsync(_applicationDataService.GetSetting<string>("UdpPort"));
                _datagramSocket.MessageReceived += async (s, e) =>
                {
                    uint stringLength = e.GetDataReader().UnconsumedBufferLength;
                    var incomingMessage = e.GetDataReader().ReadString(stringLength);
                    var package = JsonConvert.DeserializeObject<Package>(incomingMessage);
                    if (package != null)
                    {
                        switch ((PayloadType)package.PayloadType)
                        {                            
                            case PayloadType.Discovery:
                                package = new Package()
                                {
                                    PayloadType = (int)PayloadType.Room,
                                    Payload = new Room()
                                    {
                                        RoomGuid = _applicationDataService.GetSetting<string>("Guid"),
                                        RoomName = _applicationDataService.GetSetting<string>("RoomName"),
                                        RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber"),
                                        Occupancy = _applicationDataService.GetSetting<int>("ActualOccupancy"),
                                        IsIoT = _iotService.IsIotDevice()
                                    }
                                };
                                var json = JsonConvert.SerializeObject(package);
                                await _transmissionControlService.SendStringData(e.RemoteAddress, _applicationDataService.GetSetting<string>("TcpPort"), json);
                                break;
                            case PayloadType.PropertyChanged:
                                break;
                            default:
                                break;
                        }
                    }
                };                
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }

            _eventAggregator.GetEvent<PortChangedEvent>().Subscribe(async () =>
            {                
                await StartListenerAsync();
            });
        }        

        public async Task TransferOwnership()
        {
            if (_datagramSocket != null)
            {
                await _datagramSocket.CancelIOAsync();
                _datagramSocket.TransferOwnership("UserDatagramSocket");
            }
        }
    }
}
