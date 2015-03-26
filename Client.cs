using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Appboxstudios.ClipboardBroadcaster
{
    public class Client
    {
        private static DateTime LastClipBoardSent;
        private static DateTime LastClipBoardReceived;
        private const int port = 20712;
        private static string prevText = "";
        public static void StartListeningForRemoteClipBoard()
        {
            var tcpListener = new TcpListener(port);
            tcpListener.Start();
            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                Console.WriteLine("Connected:" + client.Client.RemoteEndPoint);
                var stream = client.GetStream();
                Thread.Sleep(1000);
                bool ok = client.Available > 0;
                if (ok) ok = DateTime.Now.Subtract(LastClipBoardSent).TotalSeconds > 2;
                if (ok)
                    try
                    {
                        var arr = new byte[client.Available];
                        stream.Read(arr, 0, arr.Length);
                        string data = Encoding.UTF8.GetString(arr);
                        if (data != Clipboard.GetText())
                        {
                            prevText = data;
                            Clipboard.SetText(data);
                        }
                        Console.WriteLine(data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                else
                {
                    client.Close();
                }
            }
        }
    }
}
