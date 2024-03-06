using PuppeteerSharp;
using System.Xml;
using SwitchEmuModDownloader.Classes.Managers;
using SwitchEmuModDownloader.Classes.Entities;

namespace SwitchEmuModDownloader.Classes.Downloaders;

public sealed class TheBoy181ModDownloader(IHttpClientFactory clientFactory, bool isModDataLocationToBeDeleted, bool isDownloadedModArchivesToBeDeleted) : ModDownloader(clientFactory, isModDataLocationToBeDeleted, isDownloadedModArchivesToBeDeleted)
{
    private const string TheBoy181Xml = "theboy181.xml";

    public override async Task DownloadPrerequisitesAsync()
    {
        await base.DownloadPrerequisitesAsync();
        await DownloadGameDatabaseAsync($"assets/{TheBoy181Xml}");
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
        RaiseUpdateProgress(0, "Scanning Games Library ...");

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
        using var reader = XmlReader.Create(TheBoy181Xml, new()
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
                    string modUrlPath = await reader.ReadElementContentAsStringAsync();

                    if (string.IsNullOrWhiteSpace(titleId) || !Directory.Exists(Path.Combine(ModDirectoryPath, titleId)))
                        break;

                    // game found 
                    string titleVersion = await GetTitleVersionAsync(titleId);
                    games.Add(new()
                    {
                        TitleID = titleId,
                        TitleName = GetTitleFromModUrlPath(modUrlPath),
                        ModDataLocation = Path.Combine(ModDirectoryPath, titleId),
                        TitleVersion = titleVersion,
                        ModDownloadUrls = await GetModDownloadUrlsAsync(browser, modUrlPath, titleVersion)   // detect urls for each game and populate the downloads 
                    });
                    break;

                default: break;     // do nothing 
            }
        }

        RaiseUpdateProgress(100, "Scanning Games Library ...");
        return games;
    }

    private static string GetTitleFromModUrlPath(string modUrlPath) => modUrlPath.Split(@"/")[0];

    /// <summary>
    /// Gets the Title Version information from /cache/game_list/
    /// </summary>
    /// <param name="titleId">The TitleID of the current game</param>
    /// <returns>Title Version if exists, otherise returns 1.0.0</returns>
    private async Task<string> GetTitleVersionAsync(string titleId)
    {
        string pv = Path.Combine(UserDirPath, "cache", "game_list", $"{titleId}.pv.txt");
        const string defaultVersion = "1.0.0";

        // if no pv file, return 1.0.0
        if (!File.Exists(pv))
            return defaultVersion;

        // otherwise, read in pv.txt
        using var reader = new StreamReader(pv);
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (!line.StartsWith("Update (", StringComparison.OrdinalIgnoreCase))
                continue;

            // extract version from line containing Update (X.X.X)
            int from = line.IndexOf('(') + 1;
            int to = line.LastIndexOf(')');
            return line[from..to];     // extract and return X.X.X 
        }

        return defaultVersion;     // fallback
    }

    /// <summary>
    /// Retrieves all of the download URLs for a specific title.
    /// </summary>
    /// <param name="titleName">Title of the game.</param>
    /// <returns>List of Uri's containing the Urls to Mods.</returns>
    private static async Task<List<Uri>> GetModDownloadUrlsAsync(IBrowser browser, string modUrlPath, string titleVersion)
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
        await page.GoToAsync($@"https://github.com/theboy181/switch-ptchtxt-mods/tree/main/{modUrlPath}/{titleVersion}").ConfigureAwait(false);
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
        if (File.Exists(TheBoy181Xml))
            File.Delete(TheBoy181Xml);
    }
}
