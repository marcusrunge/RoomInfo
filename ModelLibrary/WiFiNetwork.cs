using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ModelLibrary
{
    public class WiFiNetwork : BindableBase
    {
        int _id = default(int);        
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        string _networkName = default(string);
        public string NetworkName { get => _networkName; set { SetProperty(ref _networkName, value); } }

        SecureString _preSharedKey = default(SecureString);
        public SecureString PreSharedKey { get => _preSharedKey; set { SetProperty(ref _preSharedKey, value); } }
    }
}
