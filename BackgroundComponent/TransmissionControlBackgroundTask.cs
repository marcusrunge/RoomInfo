using ApplicationServiceLibrary;
using Microsoft.Practices.Unity;
using ModelLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace BackgroundComponent
{
    public sealed class TransmissionControlBackgroundTask : IBackgroundTask
    {
        IUnityContainer _unityContainer;
        IDatabaseService _databaseService;
        IApplicationDataService _applicationDataService;
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IDatabaseService, DatabaseService>();
            _unityContainer.RegisterType<IApplicationDataService, ApplicationDataService>();
            _databaseService = _unityContainer.Resolve<IDatabaseService>();
            _applicationDataService = _unityContainer.Resolve<IApplicationDataService>();
            try
            {
                var socketActivityTriggerDetails = taskInstance.TriggerDetails as SocketActivityTriggerDetails;
                var socketInformation = socketActivityTriggerDetails.SocketInformation;
                switch (socketActivityTriggerDetails.Reason)
                {
                    case SocketActivityTriggerReason.None:
                        break;
                    case SocketActivityTriggerReason.SocketActivity:
                        var streamSocket = socketInformation.StreamSocket;
                        using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                        {
                            using (StreamReader streamReader = new StreamReader(inputStream))
                            {
                                var package = JsonConvert.DeserializeObject<Package>(await streamReader.ReadLineAsync());
                                string json;
                                List<AgendaItem> agendaItems;
                                switch ((PayloadType)package.PayloadType)
                                {
                                    case PayloadType.Occupancy:
                                        var now = DateTime.Now;
                                        _applicationDataService.SaveSetting("OverriddenOccupancy", (int)package.Payload);
                                        _applicationDataService.SaveSetting("OccupancyOverridden", true);
                                        agendaItems = await _databaseService.GetAgendaItemsAsync(now);
                                        var agendaItem = agendaItems.Where(x => now > x.Start && now < x.End).Select(x => x).FirstOrDefault();
                                        if (agendaItem != null)
                                        {
                                            agendaItem.IsOverridden = true;
                                            await _databaseService.UpdateAgendaItemAsync(agendaItem);
                                        }
                                        streamSocket.TransferOwnership("StreamSocket");
                                        break;
                                    case PayloadType.Schedule:
                                        agendaItems = (List<AgendaItem>)package.Payload;
                                        await _databaseService.UpdateAgendaItemsAsync(agendaItems);
                                        streamSocket.TransferOwnership("StreamSocket");
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
                                    default:
                                        streamSocket.TransferOwnership("StreamSocket");
                                        break;
                                }
                            }
                        }
                        break;
                    case SocketActivityTriggerReason.ConnectionAccepted:
                        break;
                    case SocketActivityTriggerReason.KeepAliveTimerExpired:
                        socketInformation.StreamSocket.TransferOwnership("StreamSocket");
                        break;
                    case SocketActivityTriggerReason.SocketClosed:
                        streamSocket = new StreamSocket();
                        streamSocket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                        await streamSocket.CancelIOAsync();
                        streamSocket.TransferOwnership("StreamSocket");
                        break;
                    default:
                        break;
                }
            }
            catch { }
            _deferral.Complete();
        }

        private async Task SendStringData(StreamSocket streamSocket, HostName hostName, string port, string data)
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
            await streamSocket.CancelIOAsync();
            streamSocket.TransferOwnership("StreamSocket");
        }
    }
}
