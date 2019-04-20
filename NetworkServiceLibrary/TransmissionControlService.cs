using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System;
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
        void StopListener();
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
        IBackgroundTaskRegistrationProvider _backgroundTaskRegistrationProvider;
        StreamSocketListener _streamSocketListener;
        private int _transferOwnershipCount;

        public TransmissionControlService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, IEventAggregator eventAggregator, IDatabaseService databaseService, IIotService iotService, IBackgroundTaskRegistrationProvider backgroundTaskRegistrationProvider)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _eventAggregator = eventAggregator;
            _databaseService = databaseService;
            _iotService = iotService;
            _backgroundTaskRegistrationProvider = backgroundTaskRegistrationProvider;
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
                if (_backgroundTaskRegistrationProvider.BackgroundTaskRegistration != null) _streamSocketListener.EnableTransferOwnership(_backgroundTaskRegistrationProvider.BackgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
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
                StopListener();
                await StartListenerAsync();
            });
        }

        public void StopListener()
        {
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
                StopListener();
            }
        }

        private async Task ProcessInputStream(StreamSocket streamSocket, string inputStream)
        {
            try
            {
                Package package = JsonConvert.DeserializeObject<Package>(inputStream);
                if (package != null)
                {
                    string json;
                    List<AgendaItem> agendaItems;
                    List<TimeSpanItem> timeSpanItems;
                    switch ((PayloadType)package.PayloadType)
                    {
                        case PayloadType.Occupancy:
                            _eventAggregator.GetEvent<RemoteOccupancyOverrideEvent>().Publish((int)Convert.ChangeType(package.Payload, typeof(int)));
                            streamSocket.Dispose();
                            break;
                        case PayloadType.Schedule:
                            agendaItems = JsonConvert.DeserializeObject<List<AgendaItem>>(package.Payload.ToString());
                            await _databaseService.UpdateAgendaItemsAsync(agendaItems, true);
                            _eventAggregator.GetEvent<RemoteAgendaItemsUpdatedEvent>().Publish();
                            streamSocket.Dispose();
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
                            streamSocket.Dispose();
                            break;
                        case PayloadType.StandardWeek:
                            timeSpanItems = JsonConvert.DeserializeObject<List<TimeSpanItem>>(package.Payload.ToString());
                            await _databaseService.UpdateTimeSpanItemsAsync(timeSpanItems, true);
                            _eventAggregator.GetEvent<StandardWeekUpdatedEvent>().Publish((int)DateTime.Now.DayOfWeek);
                            streamSocket.Dispose();
                            break;
                        case PayloadType.RequestStandardWeek:
                            timeSpanItems = await _databaseService.GetTimeSpanItemsAsync();
                            package.PayloadType = (int)PayloadType.StandardWeek;
                            package.Payload = timeSpanItems;
                            json = JsonConvert.SerializeObject(package);
                            await SendStringData(streamSocket, json);
                            break;
                        case PayloadType.AgendaItem:
                            var agendaItem = JsonConvert.DeserializeObject<AgendaItem>(package.Payload.ToString());
                            if (!agendaItem.IsDeleted && agendaItem.Id < 1)
                            {
                                int id = await _databaseService.AddAgendaItemAsync(agendaItem);
                                _eventAggregator.GetEvent<RemoteAgendaItemsUpdatedEvent>().Publish();
                                package.PayloadType = (int)PayloadType.AgendaItemId;
                                package.Payload = id;
                                json = JsonConvert.SerializeObject(package);
                                await SendStringData(streamSocket, json);
                            }
                            else if (agendaItem.IsDeleted && agendaItem.Id > 0)
                            {
                                await _databaseService.RemoveAgendaItemAsync(agendaItem.Id);
                                streamSocket.Dispose();
                                _eventAggregator.GetEvent<RemoteAgendaItemDeletedEvent>().Publish(agendaItem.Id);
                            }
                            else if (agendaItem.Id > 0)
                            {
                                await _databaseService.UpdateAgendaItemAsync(agendaItem, true);
                                streamSocket.Dispose();
                                _eventAggregator.GetEvent<RemoteAgendaItemsUpdatedEvent>().Publish();
                            }
                            else streamSocket.Dispose();
                            break;
                        case PayloadType.TimeSpanItem:
                            var timeSpanItem = JsonConvert.DeserializeObject<TimeSpanItem>(package.Payload.ToString());
                            if (!timeSpanItem.IsDeleted && timeSpanItem.Id < 1)
                            {
                                int id = await _databaseService.AddTimeSpanItemAsync(timeSpanItem);
                                _eventAggregator.GetEvent<StandardWeekUpdatedEvent>().Publish(timeSpanItem.DayOfWeek);
                                package.PayloadType = (int)PayloadType.TimeSpanItemId;
                                package.Payload = id;
                                json = JsonConvert.SerializeObject(package);
                                await SendStringData(streamSocket, json);
                            }
                            else if (timeSpanItem.IsDeleted && timeSpanItem.Id > 0)
                            {
                                await _databaseService.RemoveTimeSpanItemAsync(timeSpanItem.Id);
                                streamSocket.Dispose();
                                _eventAggregator.GetEvent<RemoteTimeSpanItemDeletedEvent>().Publish(timeSpanItem.Id);
                            }
                            else if (timeSpanItem.Id > 0)
                            {
                                await _databaseService.UpdateTimeSpanItemAsync(timeSpanItem, true);
                                streamSocket.Dispose();
                                _eventAggregator.GetEvent<StandardWeekUpdatedEvent>().Publish(timeSpanItem.DayOfWeek);
                            }
                            else streamSocket.Dispose();
                            break;
                        case PayloadType.TimeSpanItemId:
                            break;
                        default:
                            streamSocket.Dispose();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                streamSocket.Dispose();
            }
        }
    }
}
