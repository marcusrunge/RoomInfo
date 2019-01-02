using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.System.Profile;

namespace ApplicationServiceLibrary
{
    public interface IIotService
    {
        bool IsIotDevice();
        void Shutdown();
        void Restart();
        Task ConfigWifi();
    }
    public class IotService : IIotService
    {
        public async Task ConfigWifi() => await Launcher.LaunchUriAsync(new Uri("ms-settings:network-wifi"));

        public bool IsIotDevice() => AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.IoT");

        public void Restart() => ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0));

        public void Shutdown() => ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
    }
}
