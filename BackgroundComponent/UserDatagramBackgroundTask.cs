using ApplicationServiceLibrary;
using Microsoft.Practices.Unity;
using ModelLibrary;
using Newtonsoft.Json;
using System;
using System.IO;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;

namespace BackgroundComponent
{
    public sealed class UserDatagramBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        IUnityContainer _unityContainer;
        IApplicationDataService _applicationDataService;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IApplicationDataService, ApplicationDataService>();
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
                        var datagramSocket = socketInformation.DatagramSocket;
                        datagramSocket.MessageReceived += async (s, e) =>
                        {
                            var roomPackage = new Room() { RoomName = _applicationDataService.GetSetting<string>("RoomName"), RoomNumber = _applicationDataService.GetSetting<string>("RoomNumber") };
                            var json = JsonConvert.SerializeObject(roomPackage);
                            try
                            {
                                using (var streamSocket = new StreamSocket())
                                {
                                    await streamSocket.ConnectAsync(s.Information.RemoteAddress, _applicationDataService.GetSetting<string>("TcpPort"));
                                    using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                                    {
                                        using (var streamWriter = new StreamWriter(outputStream))
                                        {
                                            await streamWriter.WriteLineAsync(json);
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
                            await datagramSocket.CancelIOAsync();
                            datagramSocket.TransferOwnership("UserDatagramSocket");
                        };
                        break;
                    case SocketActivityTriggerReason.ConnectionAccepted:
                        break;
                    case SocketActivityTriggerReason.KeepAliveTimerExpired:
                        socketInformation.DatagramSocket.TransferOwnership("UserDatagramSocket");
                        break;
                    case SocketActivityTriggerReason.SocketClosed:
                        datagramSocket = new DatagramSocket();
                        datagramSocket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                        await datagramSocket.CancelIOAsync();
                        datagramSocket.TransferOwnership("UserDatagramSocket");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                try
                {
                    var datagramSocket = new DatagramSocket();
                    datagramSocket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.DoNotWake);
                    await datagramSocket.CancelIOAsync();
                    datagramSocket.TransferOwnership("UserDatagramSocket");
                }
                catch { }
            }
            _deferral.Complete();
        }
    }
}
