using System;
using System.Net.Sockets;
using System.Threading;

namespace Appboxstudios.ClipboardBroadcaster
{
    public class TcpClientWithTimeout
    {
        protected string _hostname;
        protected int _port;
        protected int _timeout_milliseconds;
        protected TcpClient connection;
        protected bool connected;
        protected Exception exception;

        public TcpClientWithTimeout(string hostname, int port, int timeout_milliseconds)
        {
            _hostname = hostname;
            _port = port;
            _timeout_milliseconds = timeout_milliseconds;
        }
        public bool Connect(out TcpClient client)
        {
            // kick off the thread that tries to connect
            connected = false;
            exception = null;
            Thread thread = new Thread(new ThreadStart(BeginConnect));
            thread.IsBackground = true; // So that a failed connection attempt 
            // wont prevent the process from terminating while it does the long timeout
            thread.Start();

            // wait for either the timeout or the thread to finish
            thread.Join(_timeout_milliseconds);
            client = connection;
            if (connected)
            {
                thread.Abort();
                return connected;
            }
            return false;
        }
        protected void BeginConnect()
        {
            try
            {
                connection = new TcpClient();
                connection.Connect(_hostname, _port);
                // record that it succeeded, for the main thread to return to the caller
                connected = true;
            }
            catch (Exception ex)
            {
                // record the exception for the main thread to re-throw back to the calling code
                exception = ex;
            }
        }
    }
}