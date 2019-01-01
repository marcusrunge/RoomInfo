using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.System.Profile;

namespace ApplicationServiceLibrary
{
    public interface IIotService
    {
        bool IsIotDevice();
    }
    public class IotService : IIotService
    {
        public bool IsIotDevice()
        {            
            //var b = ApiInformation.IsTypePresent(typeof(Windows.Devices.Gpio.GpioController).ToString());
            return true;
        }
    }
}
