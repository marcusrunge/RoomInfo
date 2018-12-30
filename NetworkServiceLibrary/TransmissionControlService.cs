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
        StreamSocketListener _streamSocketListener;
        public TransmissionControlService(IApplicationDataService applicationDataService, IBackgroundTaskService backgroundTaskService, IEventAggregator eventAggregator)
        {
            _applicationDataService = applicationDataService;
            _backgroundTaskService = backgroundTaskService;
            _eventAggregator = eventAggregator;
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
                    await streamSocket.CancelIOAsync();
                    streamSocket.Dispose();
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
                            ProcessInputStream(e.Socket, await streamReader.ReadLineAsync());
                        }
                        await s.CancelIOAsync();
                        s.Dispose();
                    }
                };
                await _streamSocketListener.BindServiceNameAsync(_applicationDataService.GetSetting<string>("TcpPort"));
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }
        }

        private void ProcessInputStream(StreamSocket streamSocket, string inputStream)
        {
            Package package = JsonConvert.DeserializeObject<Package>(inputStream);
            switch ((PayloadType)package.PayloadType)
            {
                case PayloadType.Occupancy:
                    break;
                case PayloadType.Room:
                    break;
                case PayloadType.Schedule:
                    break;
                default:
                    break;
            }
        }
    }
}
