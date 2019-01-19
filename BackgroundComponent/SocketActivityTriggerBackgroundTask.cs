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
    public sealed class SocketActivityTriggerBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        IUnityContainer _unityContainer;
        IApplicationDataService _applicationDataService;
        IDatabaseService _databaseService;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IApplicationDataService, ApplicationDataService>();
            _unityContainer.RegisterType<IDatabaseService, DatabaseService>();
            _applicationDataService = _unityContainer.Resolve<IApplicationDataService>();
            _databaseService = _unityContainer.Resolve<IDatabaseService>();
            try
            {
                var socketActivityTriggerDetails = taskInstance.TriggerDetails as SocketActivityTriggerDetails;
                var socketInformation = socketActivityTriggerDetails.SocketInformation;
                switch (socketActivityTriggerDetails.Reason)
                {
                    case SocketActivityTriggerReason.None:
                        break;
                    case SocketActivityTriggerReason.SocketActivity:
                        switch (socketInformation.SocketKind)
                        {
                            case SocketActivityKind.None:
                                break;
                            case SocketActivityKind.StreamSocketListener:
                                break;
                            case SocketActivityKind.DatagramSocket:
                                var datagramSocket = socketInformation.DatagramSocket;
                                datagramSocket.MessageReceived += async (s, e) =>
                                {
                                    var roomPackage = new Package()
                                    {
                                        PayloadType = (int)PayloadType.Room,
                                        Payload = new Room()
                                        {
                                            RoomGuid = _applicationDataService.GetSetting<string>("Guid"),
                                            RoomName = _applicationDataService.GetSetting<string>("RoomName"),
                                            RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber"),
                                            Occupancy = _applicationDataService.GetSetting<int>("ActualOccupancy")
                                        }
                                    };
                                    var json = JsonConvert.SerializeObject(roomPackage);
                                    await SendStringData(new StreamSocket(), s.Information.RemoteAddress, _applicationDataService.GetSetting<string>("TcpPort"), json);
                                    await datagramSocket.CancelIOAsync();
                                    datagramSocket.TransferOwnership(socketInformation.Id);
                                };
                                break;
                            case SocketActivityKind.StreamSocket:
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
                                                streamSocket.TransferOwnership(socketInformation.Id);
                                                break;
                                            case PayloadType.Schedule:
                                                agendaItems = (List<AgendaItem>)package.Payload;
                                                await _databaseService.UpdateAgendaItemsAsync(agendaItems);
                                                streamSocket.TransferOwnership(socketInformation.Id);
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
                                            case PayloadType.StandardWeek:
                                                break;
                                            case PayloadType.RequestStandardWeek:
                                                break;
                                            default:                                                
                                                break;
                                        }
                                        streamSocket.TransferOwnership(socketInformation.Id);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case SocketActivityTriggerReason.ConnectionAccepted:
                        break;
                    case SocketActivityTriggerReason.KeepAliveTimerExpired:
                        switch (socketInformation.SocketKind)
                        {
                            case SocketActivityKind.None:
                                break;
                            case SocketActivityKind.StreamSocketListener:
                                break;
                            case SocketActivityKind.DatagramSocket:
                                var datagramSocket = socketInformation.DatagramSocket;
                                datagramSocket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                                await datagramSocket.CancelIOAsync();
                                datagramSocket.TransferOwnership(socketInformation.Id);
                                break;
                            case SocketActivityKind.StreamSocket:
                                var streamSocket = socketInformation.StreamSocket;
                                streamSocket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                                await streamSocket.CancelIOAsync();
                                streamSocket.TransferOwnership(socketInformation.Id);
                                break;
                            default:
                                break;
                        }
                        break;
                    case SocketActivityTriggerReason.SocketClosed:
                        switch (socketInformation.SocketKind)
                        {
                            case SocketActivityKind.None:
                                break;
                            case SocketActivityKind.StreamSocketListener:
                                break;
                            case SocketActivityKind.DatagramSocket:
                                var datagramSocket = socketInformation.DatagramSocket;
                                datagramSocket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                                await datagramSocket.CancelIOAsync();
                                datagramSocket.TransferOwnership(socketInformation.Id);
                                break;
                            case SocketActivityKind.StreamSocket:
                                var streamSocket = socketInformation.StreamSocket;
                                streamSocket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                                await streamSocket.CancelIOAsync();
                                streamSocket.TransferOwnership(socketInformation.Id);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch { }
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
        }
    }
}
