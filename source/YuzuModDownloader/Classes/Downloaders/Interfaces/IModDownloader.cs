using YuzuModDownloader.Classes.Entities;

namespace YuzuModDownloader.Classes.Downloaders.Interfaces;

public interface IModDownloader
{
    event Action<int, string> UpdateProgress;
    Task DownloadPrerequisitesAsync();
    Task DownloadGameDatabaseAsync(string url);
    Task DownloadModsAsync(List<Game> games);
    Task<List<Game>> ReadGameTitlesDatabaseAsync();
}
