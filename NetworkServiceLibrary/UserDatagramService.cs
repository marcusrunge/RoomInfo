using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using System;
using ModelLibrary;
using Newtonsoft.Json;
using ApplicationServiceLibrary;
using Prism.Events;
using Windows.Storage.Streams;
using Windows.Networking;
using System.IO;

namespace NetworkServiceLibrary
{
    public interface IUserDatagramService
    {
        Task StartListenerAsync();
        void StopListener();
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
        IBackgroundTaskRegistrationProvider _backgroundTaskRegistrationProvider;
        IIotService _iotService;
        DatagramSocket _datagramSocket;
        private int _transferOwnershipCount;

        public UserDatagramService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, ITransmissionControlService transmissionControlService, IEventAggregator eventAggregator, IBackgroundTaskRegistrationProvider backgroundTaskRegistrationProvider, IIotService iotService)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _transmissionControlService = transmissionControlService;
            _eventAggregator = eventAggregator;
            _backgroundTaskRegistrationProvider = backgroundTaskRegistrationProvider;
            _iotService = iotService;
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
                if(_backgroundTaskRegistrationProvider.BackgroundTaskRegistration != null) _datagramSocket.EnableTransferOwnership(_backgroundTaskRegistrationProvider.BackgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);                
                _datagramSocket.MessageReceived += async (s, e) =>
                {
                    uint stringLength = e.GetDataReader().UnconsumedBufferLength;
                    var incomingMessage = e.GetDataReader().ReadString(stringLength);
                    var package = JsonConvert.DeserializeObject<Package>(incomingMessage);
                    switch ((PayloadType)package.PayloadType)
                    {
                        case PayloadType.Occupancy:
                            break;
                        case PayloadType.Room:
                            break;
                        case PayloadType.Schedule:
                            break;
                        case PayloadType.StandardWeek:
                            break;
                        case PayloadType.RequestOccupancy:
                            break;
                        case PayloadType.RequestSchedule:
                            break;
                        case PayloadType.RequestStandardWeek:
                            break;
                        case PayloadType.IotDim:
                            break;
                        case PayloadType.AgendaItem:
                            break;
                        case PayloadType.AgendaItemId:
                            break;
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
                };
                await _datagramSocket.BindServiceNameAsync(_applicationDataService.GetSetting<string>("UdpPort"));
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }

            _eventAggregator.GetEvent<PortChangedEvent>().Subscribe(async () =>
            {
                StopListener();
                await StartListenerAsync();
            });
        }

        public void StopListener()
        {
            if (_datagramSocket != null)
            {
                _datagramSocket.Dispose();
                _datagramSocket = null;
            }
        }

        public async Task TransferOwnership()
        {
            if (_datagramSocket != null)
            {
                await _datagramSocket.CancelIOAsync();
                var dataWriter = new DataWriter();
                ++_transferOwnershipCount;
                dataWriter.WriteInt32(_transferOwnershipCount);
                var context = new SocketActivityContext(dataWriter.DetachBuffer());
                _datagramSocket.TransferOwnership("UserDatagramSocket", context);
                StopListener();
            }
        }
    }
}
