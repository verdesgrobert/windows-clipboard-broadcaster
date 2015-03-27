using System;
using System.ComponentModel;
using System.Net;
using System.Windows;

namespace Appboxstudios.ClipboardBroadcaster
{

    public class MyIpAddress : INotifyPropertyChanged
    {
        private string _endpointName;
        private bool _isSending;
        private Visibility _connecting;
        public IPAddress Address { get; set; }
        public MyIpAddress(IPAddress ip)
        {
            Connecting = ip == null ? Visibility.Visible : Visibility.Collapsed;
            Address = ip;
            IsSending = false;

            EndpointName = ip == null ? "Searching" : GetMachineNameFromIPAddress(ip.ToString());
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

        public string EndpointName
        {
            get { return _endpointName; }
            set
            {
                _endpointName = value;
                SetPropertyChanged("EndpointName");
            }
        }

        private void SetPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsSending
        {
            get { return _isSending; }
            set
            {
                _isSending = value;
                SetPropertyChanged("EndpointName");
            }
        }

        public Visibility Connecting
        {
            get { return _connecting; }
            set
            {
                _connecting = value;
                SetPropertyChanged("EndpointName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}