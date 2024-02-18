using YuzuModDownloader.Classes.Downloaders;
using YuzuModDownloader.Classes.Downloaders.Interfaces;

namespace YuzuModDownloader.Classes.Utilities;

public static class ModDownloaderFactory
{
    public static IModDownloader Create(int index, IHttpClientFactory clientFactory, bool clearModDataLocation, bool deleteDownloadedModArchives) => index switch
    {
        0 => new OfficialYuzuModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),
        1 => new TheBoy181ModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),
        2 => new HolographicWingsTotkModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),
        _ => new OfficialYuzuModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),   // fallback 
    };
}
