using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System;
using Windows.ApplicationModel.Background;
using BackgroundComponent;
using ApplicationServiceLibrary;
using ModelLibrary;
using Newtonsoft.Json;
using Prism.Events;
using System.Collections.Generic;
using Windows.Storage.Streams;

namespace NetworkServiceLibrary
{
    public interface ITransmissionControlService
    {
        Task StartListenerAsync();
        Task StopListener();
        Task TransferOwnership();
        Task SendStringData(HostName hostName, string port, string data);
        Task SendStringData(StreamSocket streamSocket, string data);
    }
    public class TransmissionControlService : ITransmissionControlService
    {
        IEventAggregator _eventAggregator;
        IApplicationDataService _applicationDataService;
        IBackgroundTaskService _backgroundTaskService;
        IDatabaseService _databaseService;
        IIotService _iotService;
        StreamSocketListener _streamSocketListener;
        private int _transferOwnershipCount;

        public TransmissionControlService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, IEventAggregator eventAggregator, IDatabaseService databaseService, IIotService iotService)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _eventAggregator = eventAggregator;
            _databaseService = databaseService;
            _iotService = iotService;
        }

        public async Task SendStringData(HostName hostName, string port, string data)
        {
            try
            {
                using (var streamSocket = new StreamSocket())
                {
                    await streamSocket.ConnectAsync(hostName, port);
                    using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(data);
                            await streamWriter.FlushAsync();
                        }
                    }
                    streamSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }
        }

        public async Task SendStringData(StreamSocket streamSocket, string data)
        {
            try
            {
                using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(data);
                        await streamWriter.FlushAsync();
                    }
                }
                streamSocket.Dispose();
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
                _streamSocketListener = new StreamSocketListener();
                if (_backgroundTaskService.FindRegistration<TransmissionControlBackgroundTask>() == null)
                {
                    try
                    {
                        var backgroundTaskRegistration = await _backgroundTaskService.Register<TransmissionControlBackgroundTask>(new SocketActivityTrigger());
                        _streamSocketListener.EnableTransferOwnership(backgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                    }
                    catch { }
                }

                _streamSocketListener.ConnectionReceived += async (s, e) =>
                {
                    using (StreamReader streamReader = new StreamReader(e.Socket.InputStream.AsStreamForRead()))
                    {
                        await ProcessInputStream(e.Socket, await streamReader.ReadLineAsync());
                    }
                };
                await _streamSocketListener.BindServiceNameAsync(_applicationDataService.GetSetting<string>("TcpPort"));
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }
            _eventAggregator.GetEvent<PortChangedEvent>().Subscribe(async () =>
            {
                await StopListener();
                await StartListenerAsync();
            });
        }

        public async Task StopListener()
        {
            await _backgroundTaskService.Unregister<TransmissionControlBackgroundTask>();
            if (_streamSocketListener != null)
            {
                _streamSocketListener.Dispose();
                _streamSocketListener = null;
            }
        }

        public async Task TransferOwnership()
        {
            if (_streamSocketListener != null)
            {
                await _streamSocketListener.CancelIOAsync();
                var dataWriter = new DataWriter();
                ++_transferOwnershipCount;
                dataWriter.WriteInt32(_transferOwnershipCount);
                var context = new SocketActivityContext(dataWriter.DetachBuffer());
                _streamSocketListener.TransferOwnership("StreamSocket", context);
                await StopListener();
            }
        }

        private async Task ProcessInputStream(StreamSocket streamSocket, string inputStream)
        {
            Package package = JsonConvert.DeserializeObject<Package>(inputStream);
            string json;
            List<AgendaItem> agendaItems;
            switch ((PayloadType)package.PayloadType)
            {
                case PayloadType.Occupancy:
                    _eventAggregator.GetEvent<RemoteOccupancyOverrideEvent>().Publish((int)package.Payload);
                    break;
                case PayloadType.Schedule:
                    agendaItems = (List<AgendaItem>)package.Payload;
                    await _databaseService.UpdateAgendaItemsAsync(agendaItems);
                    _eventAggregator.GetEvent<RemoteAgendaItemsUpdatedEvent>().Publish();
                    break;
                case PayloadType.RequestOccupancy:
                    int actualOccupancy = _applicationDataService.GetSetting<int>("ActualOccupancy");
                    package.PayloadType = (int)PayloadType.Occupancy;
                    package.Payload = actualOccupancy;
                    json = JsonConvert.SerializeObject(package);
                    await SendStringData(streamSocket, json);
                    break;
                case PayloadType.RequestSchedule:
                    agendaItems = await _databaseService.GetAgendaItemsAsync();
                    package.PayloadType = (int)PayloadType.Schedule;
                    package.Payload = agendaItems;
                    json = JsonConvert.SerializeObject(package);
                    await SendStringData(streamSocket, json);
                    break;
                case PayloadType.IotDim:
                    await _iotService.Dim((bool)package.Payload);
                    break;
                case PayloadType.StandardWeek:
                    break;
                case PayloadType.RequestStandardWeek:
                    break;
                default:
                    streamSocket.Dispose();
                    break;
            }
        }
    }
}
