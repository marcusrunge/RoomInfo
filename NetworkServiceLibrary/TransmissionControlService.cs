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

namespace NetworkServiceLibrary
{
    public interface ITransmissionControlService
    {
        Task StartListenerAsync();
        Task SendStringData(HostName hostName, string port, string data);
        Task SendStringData(StreamSocket streamSocket, HostName hostName, string port, string data);
    }
    public class TransmissionControlService : ITransmissionControlService
    {
        IEventAggregator _eventAggregator;
        IApplicationDataService _applicationDataService;
        IBackgroundTaskService _backgroundTaskService;
        IDatabaseService _databaseService;
        IIotService _iotService;
        StreamSocketListener _streamSocketListener;
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
                    await streamSocket.CancelIOAsync();
                    streamSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }
        }

        public async Task SendStringData(StreamSocket streamSocket, HostName hostName, string port, string data)
        {
            try
            {
                using (streamSocket)
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
                _streamSocketListener = new StreamSocketListener();
                var backgroundTaskRegistration = await _backgroundTaskService.Register<TransmissionControlBackgroundTask>(new SocketActivityTrigger());
                _streamSocketListener.EnableTransferOwnership(backgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                _streamSocketListener.ConnectionReceived += async (s, e) =>
                {
                    using (Stream inputStream = e.Socket.InputStream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(inputStream))
                        {
                            await ProcessInputStream(e.Socket, await streamReader.ReadLineAsync());
                        }                        
                    }
                };
                await _streamSocketListener.BindServiceNameAsync(_applicationDataService.GetSetting<string>("TcpPort"));
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
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
                    await SendStringData(streamSocket, streamSocket.Information.RemoteHostName, streamSocket.Information.RemotePort, json);
                    break;
                case PayloadType.RequestSchedule:
                    agendaItems = await _databaseService.GetAgendaItemsAsync();
                    package.PayloadType = (int)PayloadType.Schedule;
                    package.Payload = agendaItems;
                    json = JsonConvert.SerializeObject(package);
                    await SendStringData(streamSocket, streamSocket.Information.RemoteHostName, streamSocket.Information.RemotePort, json);
                    break;
                case PayloadType.IotDim:
                    await _iotService.Dim((bool)package.Payload);
                    break;
                case PayloadType.StandardWeek:
                    break;
                case PayloadType.RequestStandardWeek:
                    break;
                default:
                    break;
            }
            await streamSocket.CancelIOAsync();
            streamSocket.Dispose();
        }
    }
}
