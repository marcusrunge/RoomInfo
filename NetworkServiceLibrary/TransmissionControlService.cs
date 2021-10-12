using ApplicationServiceLibrary;
using BackgroundComponent;
using ModelLibrary;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace NetworkServiceLibrary
{
    public interface ITransmissionControlService
    {
        Task StartListenerAsync();

        Task TransferOwnership();

        Task SendStringData(HostName hostName, string port, string data);

        Task SendStringData(StreamSocket streamSocket, string data);
    }

    public class TransmissionControlService : ITransmissionControlService
    {
        private IEventAggregator _eventAggregator;
        private IApplicationDataService _applicationDataService;
        private IBackgroundTaskService _backgroundTaskService;
        private IDatabaseService _databaseService;
        private IIotService _iotService;
        private StreamSocketListener _streamSocketListener;
        private BackgroundTaskRegistration _backgroundTaskRegistration;

        public TransmissionControlService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, IEventAggregator eventAggregator, IDatabaseService databaseService, IIotService iotService)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _eventAggregator = eventAggregator;
            _databaseService = databaseService;
            _iotService = iotService;
            _backgroundTaskRegistration = null;
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
                _backgroundTaskRegistration = (BackgroundTaskRegistration)_backgroundTaskService.FindRegistration<SocketActivityTriggerBackgroundTask>();
                if (_backgroundTaskRegistration == null) _backgroundTaskRegistration = await _backgroundTaskService.Register<SocketActivityTriggerBackgroundTask>(new SocketActivityTrigger());
                if (_backgroundTaskRegistration != null) _streamSocketListener.EnableTransferOwnership(_backgroundTaskRegistration.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                await _streamSocketListener.BindServiceNameAsync(_applicationDataService.GetSetting<string>("TcpPort"));
                _streamSocketListener.ConnectionReceived += async (s, e) =>
                {
                    using (StreamReader streamReader = new StreamReader(e.Socket.InputStream.AsStreamForRead()))
                    {
                        await ProcessInputStream(e.Socket, await streamReader.ReadLineAsync());
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
            if (_streamSocketListener != null)
            {
                await _streamSocketListener.CancelIOAsync();
                _streamSocketListener.TransferOwnership("StreamSocket");
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
                                _eventAggregator.GetEvent<RemoteTimespanItemDeletedEvent>().Publish(timeSpanItem);
                                streamSocket.Dispose();
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