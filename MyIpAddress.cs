using System.Net;

namespace Appboxstudios.ClipboardBroadcaster
{
    public class MyIpAddress : IPAddress
    {
        public MyIpAddress(IPAddress ip)
            : base(ip.GetAddressBytes())
        {
            IsSending = false;
        }

        public bool IsSending { get; set; }
    }
}