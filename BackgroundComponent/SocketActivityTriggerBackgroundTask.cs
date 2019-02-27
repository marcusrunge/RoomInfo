﻿using ApplicationServiceLibrary;
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
        IIotService _iotService;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IApplicationDataService, ApplicationDataService>();
            _unityContainer.RegisterType<IDatabaseService, DatabaseService>();
            _unityContainer.RegisterType<IIotService, IotService>();
            try
            {
                _databaseService = _unityContainer.Resolve<IDatabaseService>();
            }
            catch (Exception)
            {
                _deferral.Complete();
                return;
            }

            try
            {
                _applicationDataService = _unityContainer.Resolve<IApplicationDataService>();
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                _deferral.Complete();
                return;
            }

            try
            {
                _iotService = _unityContainer.Resolve<IIotService>();
            }
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                _deferral.Complete();
                return;
            }

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
                                            await SendStringData(new StreamSocket(), socketInformation.Id, s.Information.RemoteAddress, _applicationDataService.GetSetting<string>("TcpPort"), json);
                                            break;
                                        case PayloadType.PropertyChanged:
                                            break;
                                        default:
                                            break;
                                    }
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
                                        try
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
                                                    await streamSocket.CancelIOAsync();
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
                                                    await SendStringData(streamSocket, socketInformation.Id, streamSocket.Information.RemoteHostName, streamSocket.Information.RemotePort, json);
                                                    break;
                                                case PayloadType.RequestSchedule:
                                                    agendaItems = await _databaseService.GetAgendaItemsAsync();
                                                    package.PayloadType = (int)PayloadType.Schedule;
                                                    package.Payload = agendaItems;
                                                    json = JsonConvert.SerializeObject(package);
                                                    await SendStringData(streamSocket, socketInformation.Id, streamSocket.Information.RemoteHostName, streamSocket.Information.RemotePort, json);
                                                    break;
                                                case PayloadType.StandardWeek:
                                                    break;
                                                case PayloadType.RequestStandardWeek:
                                                    break;
                                                case PayloadType.AgendaItem:
                                                    var agendaItemToAdd = (AgendaItem)package.Payload;
                                                    if (agendaItemToAdd.Id < 1)
                                                    {
                                                        int id = await _databaseService.AddAgendaItemAsync(agendaItemToAdd);
                                                        package.PayloadType = (int)PayloadType.AgendaItemId;
                                                        package.Payload = id;
                                                        json = JsonConvert.SerializeObject(package);
                                                        await SendStringData(streamSocket, socketInformation.Id, json);
                                                    }
                                                    else
                                                    {
                                                        await _databaseService.UpdateAgendaItemAsync(agendaItemToAdd);
                                                        await streamSocket.CancelIOAsync();
                                                        streamSocket.TransferOwnership(socketInformation.Id);
                                                    }
                                                    break;
                                                default:
                                                    await streamSocket.CancelIOAsync();
                                                    streamSocket.TransferOwnership(socketInformation.Id);
                                                    break;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            streamReader.Close();
                                            inputStream.Close();
                                        }
                                        await streamSocket.CancelIOAsync();
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
            catch (Exception e)
            {
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        private async Task SendStringData(StreamSocket streamSocket, string streamSocketId, HostName hostName, string port, string data)
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
            catch (Exception e)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            await streamSocket.CancelIOAsync();
            streamSocket.TransferOwnership(streamSocketId);
        }

        private async Task SendStringData(StreamSocket streamSocket, string streamSocketId, string data)
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
                await streamSocket.CancelIOAsync();
                streamSocket.TransferOwnership(streamSocketId);
            }
            catch (Exception e)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
                if (_databaseService != null) await _databaseService.AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }
    }
}
