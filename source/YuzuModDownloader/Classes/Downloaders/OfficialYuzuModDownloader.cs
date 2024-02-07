using HtmlAgilityPack;
using System.Xml;
using YuzuModDownloader.Classes.Entities;

namespace YuzuModDownloader.Classes.Downloaders
{
    public sealed class OfficialYuzuModDownloader(IHttpClientFactory clientFactory, bool isModDataLocationToBeDeleted, bool isDownloadedModArchivesToBeDeleted) : ModDownloader(clientFactory, isModDataLocationToBeDeleted, isDownloadedModArchivesToBeDeleted)
    {
        private const string Quote = "\"";
        private const string GameTitleIDsXml = "GameTitleIDs.xml";
        private readonly IHttpClientFactory _clientFactory = clientFactory;
        private readonly HtmlDocument _htmlDoc = new()
        {
             DisableServerSideCode = true
        };

        public new async Task DownloadPrerequisitesAsync()
        {
            await base.DownloadPrerequisitesAsync();
            await base.DownloadGameDatabaseAsync($"assets/{GameTitleIDsXml}");
        }

        public new async Task DownloadModsAsync(List<Game> games)
        {
            await base.DownloadModsAsync(games);
            CleanUp();
        }

        public async Task<List<Game>> ReadGameTitlesDatabaseAsync()
        {
            // detect yuzu user directory 
            // loop through {ModDirPath} folder & get title names from title Id's
            var games = new List<Game>();
            base.RaiseUpdateProgress(0, "Scanning Games Library ...");
            using var reader = XmlReader.Create(GameTitleIDsXml, new()
            {
                Async = true,
                IgnoreComments = true
            });

            // Preload HtmlDocument 
            await DownloadHtmlDocument();

            // scan xml and compare against preloaded htmldocument 
            while (await reader.ReadAsync())
            {
                if (!reader.IsStartElement())
                    continue;

                switch (reader.Name)
                {
                    case "title_name":
                        string titleName = await reader.ReadElementContentAsStringAsync();
                        await reader.ReadAsync();
                        string titleId = await reader.ReadElementContentAsStringAsync();

                        if (string.IsNullOrWhiteSpace(titleId) || !Directory.Exists(Path.Combine(base.ModDirectoryPath, titleId)))
                            break;

                        games.Add(new()
                        {
                            TitleID = titleId,
                            TitleName = titleName,
                            ModDataLocation = Path.Combine(base.ModDirectoryPath, titleId),
                            ModDownloadUrls = GetModDownloadUrls(titleName)   // detect urls for each game and populate the downloads 
                        });
                        break;
                    default:
                        continue;   //do nothing      
                }
            }
            
            base.RaiseUpdateProgress(100, "Scanning Games Library ...");
            return games;
        }

        /// <summary>
        /// Downloads the BaseModsRepoUrl and preloads it into memory 
        /// </summary>
        /// <returns></returns>
        private async Task DownloadHtmlDocument()
        {
            // download the basemodsrepo document once 
            using var client = _clientFactory.CreateClient("GitHub-OfficialSwitchMods");
            var html = await client.GetStringAsync("").ConfigureAwait(false);
            _htmlDoc.LoadHtml(html);
        }

        /// <summary>
        /// Retrieves all of the download URLs for a specific title.
        /// </summary>
        /// <param name="titleName">Title of the game.</param>
        /// <returns>List of Uri's containing the Urls to Mods.</returns>
        private List<Uri> GetModDownloadUrls(string titleName)
        {
            // fetch all download links for current game

            // read switch-mods downloaded webpage and get download links for current game
            var nodes = _htmlDoc.DocumentNode.SelectNodes($"//h3[contains(., {Quote}{titleName}{Quote})]/following::table[1]//td//a");

            // if no links found, return empty list 
            if (nodes is null)
                return [];

            // otherwise process links and add them into downloadUrls list 
            var downloadUrls = new List<Uri>();
            foreach (var node in nodes)
            {
                string url = node.Attributes["href"].Value.Trim();
                if (url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".rar", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                {
                    downloadUrls.Add(new(url));
                }
            }
            return downloadUrls;
        }

        private static void CleanUp()
        {
            if (File.Exists(GameTitleIDsXml))
                File.Delete(GameTitleIDsXml);
        }
    }
}
