using PuppeteerSharp;
using System.Xml;
using SwitchEmuModDownloader.Classes.Managers;
using SwitchEmuModDownloader.Classes.Entities;

namespace SwitchEmuModDownloader.Classes.Downloaders;

public sealed class LexouilleTMModDownloader(IHttpClientFactory clientFactory, bool isModDataLocationToBeDeleted, bool isDownloadedModArchivesToBeDeleted) : ModDownloader(clientFactory, isModDataLocationToBeDeleted, isDownloadedModArchivesToBeDeleted)
{
    private const string LexouilleTmXml = "lexouilletm.xml";

    public override async Task DownloadPrerequisitesAsync()
    {
        await base.DownloadPrerequisitesAsync();
        await DownloadGameDatabaseAsync($"assets/{LexouilleTmXml}");
    }

    public override async Task DownloadModsAsync(List<Game> games)
    {
        await base.DownloadModsAsync(games);
        CleanUp();
    }

    public override async Task<List<Game>> ReadGameTitlesDatabaseAsync()
    {
        // detect yuzu user directory 
        // loop through {ModDirPath} folder & get title names from title Id's
        var games = new List<Game>();
        OnProgressChanged(0, "Scanning Games Library ...");

        // if no browsers installed, download a copy of chrome
        (var browserInstallationState, string browserInstallationPath) = await BrowserManager.DetectBrowsersAsync();
        if (browserInstallationState == BrowserManager.InstallationState.NotInstalled)
        {
            browserInstallationPath = await BrowserManager.DownloadBrowserAsync();
        }

        // launch browser in headless mode
        await using var browser = await Puppeteer.LaunchAsync(new()
        {
            Headless = true,
            ExecutablePath = browserInstallationPath
        });

        // prepare the xml document 
        using var reader = XmlReader.Create(LexouilleTmXml, new()
        {
            Async = true,
            IgnoreComments = true
        });

        while (await reader.ReadAsync())
        {
            if (!reader.IsStartElement())
                continue;

            switch (reader.Name)
            {
                case "title_id":
                    string titleId = await reader.ReadElementContentAsStringAsync();
                    await reader.ReadAsync();
                    string titleName = await reader.ReadElementContentAsStringAsync();

                    if (string.IsNullOrWhiteSpace(titleId) || !Directory.Exists(Path.Combine(ModDirectoryPath, titleId)))
                        break;

                    // game found 
                    games.Add(new()
                    {
                        TitleID = titleId,
                        TitleName = titleName,
                        ModDataLocation = Path.Combine(ModDirectoryPath, titleId),
                        ModDownloadUrls = await GetModDownloadUrlsAsync(browser, titleName)   // detect urls for each game and populate the downloads 
                    });
                    break;

                default: break;     // do nothing 
            }
        }

        OnProgressChanged(100, "Scanning Games Library ...");
        return games;
    }

    /// <summary>
    /// Retrieves all of the download URLs for a specific title.
    /// </summary>
    /// <param name="titleName">Title of the game.</param>
    /// <returns>List of Uri's containing the Urls to Mods.</returns>
    private static async Task<List<Uri>> GetModDownloadUrlsAsync(IBrowser browser, string titleName)
    {
        // fetch all download links for current game

        // create new webpage in headless browser
        await using var page = await browser.NewPageAsync().ConfigureAwait(false);

        // Disable large rendering media from being loaded into the browser
        await page.SetUserAgentAsync(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0").ConfigureAwait(false);
        await page.SetRequestInterceptionAsync(true).ConfigureAwait(false);
        page.Request += async (s, e) =>
        {
            switch (e.Request.ResourceType)
            {
                // allow only HTML & JS for React.JS to work - huge speed benefits
                case ResourceType.Document or ResourceType.Script:
                    await e.Request.ContinueAsync().ConfigureAwait(false);
                    break;

                default:
                    await e.Request.AbortAsync().ConfigureAwait(false);
                    break;
            }
        };

        // go to the mods webpage for the current game 
        // scrape all a.Link--primary tags and remove duplicates
        await page.GoToAsync($@"https://github.com/LexouilleTM/yuzu-mods-archive/tree/main/{titleName}").ConfigureAwait(false);
        var hrefValues = await page.EvaluateExpressionAsync<string[]>(@"[...new Set([...document.querySelectorAll('a.Link--primary')].map(a => a.href).filter(Boolean))]").ConfigureAwait(false);

        // grab urls 
        return hrefValues?
            .Where(url => url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                       || url.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)
                       || url.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
            .Select(url => new Uri(url.Replace("/blob/", "/raw/").Replace("&#39;", "'")))
            .ToList() ?? [];
    }

    private static void CleanUp()
    {
        if (File.Exists(LexouilleTmXml))
            File.Delete(LexouilleTmXml);
    }
}
