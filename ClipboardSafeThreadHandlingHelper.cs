using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Appboxstudios.ClipboardBroadcaster
{
    public class ClipboardSafeThreadHandlingHelper
    {
        public static BlockingCollection<ClipboardTask> queue = new BlockingCollection<ClipboardTask>();
        [STAThread]
        public static void HandleClipboardQueueTask()
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            foreach (ClipboardTask task in queue)
            {
                task.Handle();
            }
        }
    }
}
