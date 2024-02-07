using System.Reflection;

namespace YuzuModDownloader.Classes.Updaters
{
    public sealed class AppUpdater(IHttpClientFactory clientFactory)
    {
        private const int LatestVersionLineLocation = 1;
        private readonly string _currentAppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString().Trim()!;

        public enum CurrentVersion
        {
            LatestVersion,
            UpdateAvailable,
            NotSupported,
            Undetectable
        }

        public async Task<CurrentVersion> CheckVersionAsync()
        {
            // latest version is always on top line 
            // so we check and see how many times the loop has iterated and compare it against 1 
            if (_currentAppVersion is null)
                return CurrentVersion.NotSupported;

            try
            {
                var client = clientFactory.CreateClient("GitHub-YuzuModDownloader");
                using var response = await client.GetAsync("version", HttpCompletionOption.ResponseHeadersRead);

                // if response isn't okay, return undetectable
                if (!response.IsSuccessStatusCode)
                    return CurrentVersion.Undetectable;

                // otherwise get the version and parse it 
                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                int i = 1;
                string? onlineVersion;
                while ((onlineVersion = await reader.ReadLineAsync()) is not null)
                {
                    if (_currentAppVersion == onlineVersion.Trim() && LatestVersionLineLocation == i)
                    {
                        return CurrentVersion.LatestVersion;
                    }
                    else if (_currentAppVersion == onlineVersion.Trim() && LatestVersionLineLocation != i)
                    {
                        return CurrentVersion.UpdateAvailable;
                    }
                    i++;
                }
                return CurrentVersion.NotSupported;
            }
            catch
            {
                // connection issues
                return CurrentVersion.Undetectable;
            }
        }
    }
}
