using System;
using System.Net;

namespace Appboxstudios.ClipboardBroadcaster
{

    public class MyIpAddress : IPAddress
    {
        public MyIpAddress(IPAddress ip)
            : base(ip.GetAddressBytes())
        {
            IsSending = false;
            EndpointName = GetMachineNameFromIPAddress(ip.AddressFamily.ToString());
        }

        private static string GetMachineNameFromIPAddress(string ipAdress)
        {
            string machineName = string.Empty;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAdress);

                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                // Machine not found...
            }
            return machineName;
        }
        public string EndpointName { get; set; }
        public bool IsSending { get; set; }
    }
}