using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Appboxstudios.ClipboardBroadcaster
{
    public enum ClipboardTaskTypeEnum
    {
        Text, Files, Image
    }

    public enum ClipboardOperationTypeEnum
    {
        Copy, Paste
    }

    public class ClipboardTask
    {
        public ClipboardTask(ClipboardTaskTypeEnum type, ClipboardOperationTypeEnum operation, Action action)
        {
            Type = type;
            Operation = operation;
            Action = action;
        }
        public ClipboardTaskTypeEnum Type { get; set; }

        public ClipboardOperationTypeEnum Operation { get; set; }
        public Action Action { get; set; }
        internal void Handle()
        {
            switch (Operation)
            {
                case ClipboardOperationTypeEnum.Copy:
                    Action();
                    break;
                case ClipboardOperationTypeEnum.Paste:
                    Action();
                    break;
            }
        }
    }
}
