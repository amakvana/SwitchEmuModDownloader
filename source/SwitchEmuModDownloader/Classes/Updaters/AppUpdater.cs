using System.Reflection;

namespace SwitchEmuModDownloader.Classes.Updaters;

public sealed class AppUpdater(IHttpClientFactory clientFactory)
{
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
        if (_currentAppVersion is null)
            return CurrentVersion.NotSupported;

        try
        {
            var client = clientFactory.CreateClient("GitHub-SwitchEmuModDownloader");
            using var response = await client.GetAsync("version", HttpCompletionOption.ResponseHeadersRead);

            // if response isn't okay, return undetectable
            if (!response.IsSuccessStatusCode)
                return CurrentVersion.Undetectable;

            // otherwise get the version and parse it 
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            List<string> onlineVersions = (await reader.ReadToEndAsync()).Split('\n').Select(s => s.Trim()).ToList();

            if (onlineVersions.First() == _currentAppVersion)
                return CurrentVersion.LatestVersion;

            if (!onlineVersions.Contains(_currentAppVersion))
                return CurrentVersion.NotSupported;

            return CurrentVersion.UpdateAvailable;
        }
        catch
        {
            return CurrentVersion.Undetectable;
        }
    }
}
