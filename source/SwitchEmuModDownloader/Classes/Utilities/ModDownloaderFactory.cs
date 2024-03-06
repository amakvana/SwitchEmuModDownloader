using SwitchEmuModDownloader.Classes.Downloaders;
using SwitchEmuModDownloader.Classes.Downloaders.Interfaces;

namespace SwitchEmuModDownloader.Classes.Utilities;

public static class ModDownloaderFactory
{
    public static IModDownloader Create(int index, IHttpClientFactory clientFactory, bool clearModDataLocation, bool deleteDownloadedModArchives) => index switch
    {
        0 => new OfficialSwitchModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),
        1 => new TheBoy181ModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),
        2 => new HolographicWingsTotkModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),
        _ => new OfficialSwitchModDownloader(clientFactory, clearModDataLocation, deleteDownloadedModArchives),   // fallback 
    };
}
