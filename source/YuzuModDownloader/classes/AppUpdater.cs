using System.IO;
using System.Net;
using System.Windows.Forms;

namespace YuzuModDownloader
{
    public static class AppUpdater
    {
        public enum CurrentVersion
        {
            LatestVersion,
            UpdateAvailable,
            NotSupported
        }

        public static CurrentVersion CheckVersion()
        {
            // latest version is always on top line 
            // so we check and see how many times the loop has iterated and compare it against 1 
            const int LatestVersionLineLocation = 1;
            string onlineVersion = "";
            string currentAppVersion = Application.ProductVersion.Trim();

            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://raw.githubusercontent.com/amakvana/YuzuModDownloader/main/version"))
                using (var reader = new StreamReader(stream))
                {
                    int i = 1;
                    while ((onlineVersion = reader.ReadLine()) != null)
                    {
                        if (currentAppVersion == onlineVersion.Trim() && LatestVersionLineLocation == i)
                        {
                            return CurrentVersion.LatestVersion;
                        }
                        else if (currentAppVersion == onlineVersion.Trim() && LatestVersionLineLocation != i)
                        {
                            return CurrentVersion.UpdateAvailable;
                        }
                        i++;
                    }
                }
                return CurrentVersion.NotSupported;
            }
            catch
            {
                return CurrentVersion.NotSupported;
            }
        }
    }
}
