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
        Task Dim(bool dim);
        Task<bool> IsDimmed();
    }
    public class IotService : IIotService
    {
        public Task ConfigWifi()
        {
            throw new NotImplementedException();
        }

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
                    i2CDevice.Write(new byte[] { 0x86, brightness });
                    i2CDevice.Dispose();
                }
                catch { }
            }
        }

        public async Task<bool> IsDimmed()
        {
            bool result = false;
            string i2cDeviceSelector = I2cDevice.GetDeviceSelector();
            I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(0x45);
            IReadOnlyList<DeviceInformation> deviceInformationCollection = await DeviceInformation.FindAllAsync(i2cDeviceSelector);
            if (deviceInformationCollection.Count > 0)
            {
                var i2CDevice = await I2cDevice.FromIdAsync(deviceInformationCollection[0].Id, i2CConnectionSettings);
                var buffer = new byte[2];
                buffer[0] = 0x86;
                try
                {
                    i2CDevice.Read(buffer);
                    if (buffer[1] < 0xff) result = true;
                    i2CDevice.Dispose();
                }
                catch { }
            }
            return result;
        }

        public bool IsIotDevice() => AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.IoT");

        public void Restart() => ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0));

        public void Shutdown() => ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
    }
}
