using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Appboxstudios.ClipboardBroadcaster
{
    public class ClipBoardHelper
    {
        private static DateTime LastClipBoardSent;
        private static DateTime LastClipBoardReceived;
        private static int port = 20712;
        private static Action<string> log = msg => Console.WriteLine(msg);
        private static void SendMessage(string text, byte messageType = 0, byte[] data = null, string footer = "")
        {
            LastClipBoardSent = DateTime.Now;
            //foreach (MyIpAddress address in remoteAddresses)
            //{
            //    SendMessage(address, text, messageType, data, footer);
            //}
            LastClipBoardSent = DateTime.Now;
        }

        private static void SendMessage(MyIpAddress add, string text, byte messageType = 0, byte[] data = null, string footer = "")
        {
            while (add.IsSending) { Thread.Sleep(5); }
            add.IsSending = true;
            try
            {
                TcpClient tc = new TcpClient();
                tc.Connect(add.ToString(), port);
                var stream = tc.GetStream();
                string txt = text.Length + "";
                stream.WriteByte(messageType);
                if (messageType == 2)
                {
                    txt = data.Length + "";
                    var bt = (byte)txt.Length;
                    stream.WriteByte(bt);
                    stream.Flush();
                    Debug.WriteLine(bt);
                    var bites = Encoding.UTF8.GetBytes(data.Length + "");
                    stream.Write(bites, 0, bites.Length);
                    stream.Flush();
                    var bytes = data;// Encoding.UTF8.GetBytes(text); //.Length.ToString().PadLeft(10, '0'));
                    stream.Write(bytes, 0, bytes.Length);
                    //Thread.Sleep(1000);
                    stream.Flush();
                }
                else
                {
                    var bt = (byte)txt.Length;
                    stream.WriteByte(bt);
                    stream.Flush();
                    var bites = Encoding.UTF8.GetBytes(text.Length + "");
                    stream.Write(bites, 0, bites.Length);
                    stream.Flush();
                    var bytes = Encoding.UTF8.GetBytes(text);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    if (!string.IsNullOrEmpty(footer))
                    {
                        bt = (byte)(footer.Length + "").Length;
                        stream.WriteByte(bt);
                        stream.Flush();
                        bites = Encoding.UTF8.GetBytes(footer.Length + "");
                        stream.Write(bites, 0, bites.Length);
                        stream.Flush();
                        bytes = Encoding.UTF8.GetBytes(footer);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                        var byteGetFiles = stream.ReadByte();
                        string res = Encoding.UTF8.GetString(new[] { (byte)byteGetFiles });
                        if (res == "1")
                        {
                            SendFiles();
                            stream.WriteByte(1);
                        }
                    }
                }
                tc.Close();
            }
            catch (Exception e)
            {
                log(e + "");
            }
            add.IsSending = false;
        }

        private static void SendFiles()
        {
            if (Directory.Exists("d:\\Share\\copyPath\\"))
            {
                Directory.Delete("d:\\Share\\copyPath\\", true);
            }
            Directory.CreateDirectory("d:\\Share\\copyPath\\");
            var filesToTransfer = Clipboard.GetFileDropList();
            foreach (string s in filesToTransfer)
            {
                FileInfo f = new FileInfo(s);
                File.Copy(s, "d:\\Share\\copyPath\\" + f.Name);
            }
        }

        public static void StartListeningForClipBoard()
        {
            TcpListener tcpListener = new TcpListener(port);
            tcpListener.Start();
            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                log("Connected:" + client.Client.RemoteEndPoint);
                var stream = client.GetStream();
                Thread.Sleep(1000);
                bool ok = client.Available > 0;
                if (ok) ok = DateTime.Now.Subtract(LastClipBoardSent).TotalSeconds > 2;
                if (ok)
                    try
                    {
                        byte[] arr = new byte[client.Available];
                        stream.Read(arr, 0, arr.Length);
                        string data = Encoding.UTF8.GetString(arr);
                        if (data != Clipboard.GetText())
                        {
                            prevText = data;
                            Clipboard.SetText(data);
                        }
                        log(data);
                    }
                    catch (Exception e)
                    {
                        log(e + "");
                    }
                else
                {
                    client.Close();
                }
            }
        }

        private static string prevText = "";
        private static StringCollection lastFilesToTransfer;
        private static byte[] prevImage;
        static List<MyIpAddress> remoteAddresses = new List<MyIpAddress>();


        public async static void StartThreads()
        {
            LastClipBoardSent = DateTime.Today;
            RefreshRemoteIps();
            Thread t1 = new Thread(ListenForClipboardChanges);
            t1.Start();
            Thread t2 = new Thread(StartListeningForClipBoard);
            t2.Start();
            await StartListeningForDevices(addr => { });
        }

        static void RefreshRemoteIps()
        {
            var addresses = FindAddressesFromNetworkInterfaces();
            var baseAddresses = addresses.GroupBy(el => el.ToString().Substring(0, el.ToString().LastIndexOf(".") + 1)).Select(el => el.Key).ToList();
            addresses.Clear();
            int tot = baseAddresses.Count * 255;
            int cntr = 0;
            foreach (string baseAddress in baseAddresses)
            {
                string baseAddr = baseAddress;
                int last = baseAddr.LastIndexOf(".") + 1;
                string subnet = baseAddr.Substring(0, last);
                for (int x = 1; x <= 255; x++)
                {
                    cntr++;
                    string ipAddressToTest = subnet + x;
                    log(ipAddressToTest + ":" + cntr + "/" + tot);
                    if (remoteAddresses.Any(address => address.ToString() == ipAddressToTest)) continue;

                    try
                    {
                        try
                        {
                            var now = DateTime.Now;
                            TcpClientWithTimeout tcp = new TcpClientWithTimeout(ipAddressToTest, 20712, 20);
                            TcpClient cl = new TcpClient();
                            tcp.Connect(out cl);
                            if (cl != null)
                                if (cl.Connected)
                                {
                                    var ip = IPAddress.Parse(ipAddressToTest);
                                    var addressItem = new MyIpAddress(ip);
                                    remoteAddresses.Add(addressItem);
                                    if (AddressFoundCallback != null)
                                        AddressFoundCallback(addressItem);
                                    log(ip + "\t" + DateTime.Now.Subtract(now).TotalMilliseconds);
                                }
                        }
                        catch (Exception e)
                        {
                            log(e + "");
                        }
                    }
                    catch (Exception ex) { }
                }
            }
        }

        private static List<IPAddress> FindAddressesFromNetworkInterfaces()
        {
            List<IPAddress> addresses = new List<IPAddress>();
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
            networks =
                networks.Where(
                    conn =>
                        conn.OperationalStatus == OperationalStatus.Up &&
                        conn.NetworkInterfaceType != NetworkInterfaceType.Loopback).ToArray();
            foreach (var addressesToAdd in networks
                .Select(network =>
                    network.GetIPProperties()
                    .UnicastAddresses
                      .Where(entry => entry.Address.AddressFamily == AddressFamily.InterNetwork)
                      .Select(entry => entry.Address)))
            {
                addresses.AddRange(addressesToAdd);
            }
            return addresses;
        }

        private static Action<MyIpAddress> AddressFoundCallback;
        public async static Task StartListeningForDevices(Action<MyIpAddress> foundCallback)
        {
            AddressFoundCallback = foundCallback;
            while (true)
            {
                RefreshRemoteIps();
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
        private static void ListenForClipboardChanges()
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            while (true)
            {
                try
                {
                    Thread.Sleep(400);
                    if (Clipboard.ContainsText())
                    {
                        HandleClipboardText();
                    }
                    else if (Clipboard.ContainsFileDropList())
                    {
                        HandleClipboardFiles();
                    }
                    else if (Clipboard.ContainsImage())
                    {
                        ClipboardSafeThreadHandlingHelper.queue.Add(new ClipboardTask(ClipboardTaskTypeEnum.Image, ClipboardOperationTypeEnum.Copy, HandleClipboardImage));
                        //HandleClipboardImage();
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
                }
            }
        }

        private static void HandleClipboardImage()
        {
            var filesToTransfer = Clipboard.GetImage();
            if (filesToTransfer != null)
            {
                byte[] image = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    filesToTransfer.Save(ms, ImageFormat.Jpeg);
                    image = ms.ToArray();
                }
                filesToTransfer.Save("d:\\Share\\shareableImage.jpg", ImageFormat.Jpeg);
                if (image != prevImage)
                {
                    prevImage = image;
                    Clipboard.Clear();
                    SendMessage("", 2, image);
                }
            }
        }

        private static void HandleClipboardFiles()
        {
            var filesToTransfer = Clipboard.GetFileDropList();
            if (filesToTransfer != lastFilesToTransfer)
            {
                lastFilesToTransfer = filesToTransfer;
                long total = 0;
                foreach (string file in filesToTransfer)
                {
                    FileInfo f = new FileInfo(file);
                    total += f.Length;
                }
                string bytes = "bytes";
                if (total / 1024 > 1024)
                {
                    bytes = "KB";
                    if (total / 1024 / 1024 > 1024)
                    {
                        bytes = "MB";
                        if (total / 1024 / 1024 / 1024 > 1024)
                        {
                            bytes = "GB";
                        }
                    }
                }
                string dataToSend = string.Join("\n", filesToTransfer.Cast<string>().ToList());
                SendMessage(dataToSend, 1, null, total + bytes);
            }
        }

        private static void HandleClipboardText()
        {
            string text = Clipboard.GetText();
            if (text != prevText)
            {
                prevText = text;
                SendMessage(text);
            }
        }

        public static void SetOutput(Action<string> action)
        {
            log = action;
        }

    }
}
