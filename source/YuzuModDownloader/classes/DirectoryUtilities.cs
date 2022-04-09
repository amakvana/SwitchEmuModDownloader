using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YuzuModDownloader
{
    public static class DirectoryUtilities
    {
        // https://stackoverflow.com/a/1288747
        // Modified to optionally allow recusive deletion
        public static void DeleteDirectory(string path, bool recursive = false)
        {
            var dir = new DirectoryInfo(path);
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            if (recursive)
            {
                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    DeleteDirectory(di.FullName, true);
                    di.Delete();
                }
            }
        }

        // https://stackoverflow.com/a/8132800
        public static void DeleteFiles(string path, params string[] extensions)
        {
            List<FileInfo> files = GetFiles(path, extensions);
            foreach (FileInfo file in files)
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }
            }
        }

        // https://stackoverflow.com/a/8132800
        // Modified to speed up enumeration process 
        private static List<FileInfo> GetFiles(string path, params string[] extensions)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (string ext in extensions)
            {
                list.AddRange(new DirectoryInfo(path).EnumerateFiles("*" + ext)
                    .Where(p => p.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase)));
            }
            return list;
        }
    }
}
