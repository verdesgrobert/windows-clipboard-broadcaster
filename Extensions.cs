using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Appboxstudios.ClipboardBroadcaster
{
    public static class Extensions
    {
        public static List<DirectoryInfo> GetSubFiles(this DirectoryInfo d)
        {
            List<DirectoryInfo> hiddenFiles = new List<DirectoryInfo>();
            try
            {
                if (d.Attributes.HasFlag(FileAttributes.Hidden))
                    hiddenFiles.Add(d);
                var dirs = d.GetDirectories();
                if (dirs.Any())
                    foreach (DirectoryInfo dir in dirs)
                    {
                        hiddenFiles.AddRange(GetSubFiles(dir));
                    }
            }
            catch (Exception e)
            {
            }
            return hiddenFiles;
        }
    }
}
