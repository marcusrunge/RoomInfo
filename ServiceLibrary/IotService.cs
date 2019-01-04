using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
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
        Task Dim(bool dim);
    }
    public class IotService : IIotService
    {
        public async Task ConfigWifi() => await Launcher.LaunchUriAsync(new Uri("ms-settings:network-wifi"));

        public async Task Dim(bool dim)
        {
            string i2cDeviceSelector = I2cDevice.GetDeviceSelector();
            I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(0x45);
            IReadOnlyList<DeviceInformation> deviceInformationCollection = await DeviceInformation.FindAllAsync(i2cDeviceSelector);
            if (deviceInformationCollection.Count > 0)
            {
                var i2CDevice = await I2cDevice.FromIdAsync(deviceInformationCollection[0].Id, i2CConnectionSettings);
                byte brightness = dim ? (byte)7 : (byte)255;
                try
                {
                    i2CDevice?.Write(new byte[] { 0x86, brightness });
                }
                catch { }
            }
        }

        public bool IsIotDevice() => AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.IoT");

        public void Restart() => ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0));

        public void Shutdown() => ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
    }
}
