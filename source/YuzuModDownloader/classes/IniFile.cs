using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace YuzuModDownloader
{
    // IniFile - Revision 11
    // Author: Danny Beckett 
    // URL: https://stackoverflow.com/a/14906422

    class IniFile 
    {
        private readonly string Path;
        private readonly string Exe = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string iniPath = null)
        {
            Path = new FileInfo(iniPath ?? Exe + ".ini").FullName;
        }

        public string Read(string key, string section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(section ?? Exe, key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string key, string value, string section = null)
        {
            WritePrivateProfileString(section ?? Exe, key, value, Path);
        }

        public void DeleteKey(string key, string section = null)
        {
            Write(key, null, section ?? Exe);
        }

        public void DeleteSection(string section = null)
        {
            Write(null, null, section ?? Exe);
        }

        public bool KeyExists(string key, string section = null)
        {
            return Read(key, section).Length > 0;
        }
    }
}
