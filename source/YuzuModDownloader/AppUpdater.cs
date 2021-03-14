using System.IO;
using System.Net;
using System.Windows.Forms;

namespace YuzuModDownloader
{
    public static class AppUpdater
    {
        public static bool IsLatestVersion()
        {
            try
            {
                string onlineVersion = "";
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://raw.githubusercontent.com/amakvana/YuzuModDownloader/main/version"))
                using (var reader = new StreamReader(stream))
                {
                    onlineVersion = reader.ReadToEnd();
                }
                return Application.ProductVersion.Trim() == onlineVersion.Trim();
            }
            catch
            {
                return false;
            }
        }
    }
}
