using SwitchEmuModDownloader.Classes.Entities;

namespace SwitchEmuModDownloader.Classes.Downloaders.Interfaces;

public interface IModDownloader
{
    event Action<int, string> ProgressChanged;
    Task DownloadPrerequisitesAsync();
    Task DownloadGameDatabaseAsync(string url);
    Task DownloadModsAsync(List<Game> games);
    Task<List<Game>> ReadGameTitlesDatabaseAsync();
}
