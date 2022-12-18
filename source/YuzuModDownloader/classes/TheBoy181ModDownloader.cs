using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace YuzuModDownloader
{
    public class TheBoy181ModDownloader : ModDownloader
    {
        private const string BaseModsRepoUrl = "https://github.com/theboy181/switch-ptchtxt-mods/tree/main";
        private const string TheBoy181Xml = "theboy181.xml";

        public TheBoy181ModDownloader() : base() { }

        public new async Task DownloadPrerequisitesAsync()
        {
            await base.DownloadPrerequisitesAsync();
            await base.DownloadGameDatabaseAsync("https://raw.githubusercontent.com/amakvana/YuzuModDownloader/main/assets/theboy181.xml", TheBoy181Xml);
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
            base.RaiseUpdateProgressDelegate(0, "Scanning Games Library ...");
            using (var reader = XmlReader.Create(TheBoy181Xml))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                        continue;

                    switch (reader.Name)
                    {
                        case "title_id":
                            string titleId = reader.ReadElementContentAsString();
                            reader.Read();
                            string modUrlPath = reader.ReadElementContentAsString();

                            if (string.IsNullOrWhiteSpace(titleId) || !Directory.Exists($"{base.ModDirectoryPath}/{titleId}"))
                                break;

                            string titleVersion = await GetTitleVersion(titleId);
                            var game = new Game
                            {
                                TitleID = titleId,
                                ModDataLocation = $"{base.ModDirectoryPath}/{titleId}",
                                TitleVersion = titleVersion,
                                ModDownloadUrls = await GetModDownloadUrls(modUrlPath, titleVersion)   // detect urls for each game and populate the downloads 
                            };
                            
                            games.Add(game);
                            break;

                        default: break;     // do nothing 
                    }
                }
            }
            base.RaiseUpdateProgressDelegate(100, "Done");
            return games;
        }

        /// <summary>
        /// Gets the Title Version information from /cache/game_list/
        /// </summary>
        /// <param name="titleId">The TitleID of the current game</param>
        /// <returns>Title Version if exists, otherise returns 1.0.0</returns>
        private async Task<string> GetTitleVersion(string titleId)
        {
            string pv = $@"{base.UserDirPath}/cache/game_list/{titleId}.pv.txt";
            string defaultVersion = "1.0.0";

            if (!File.Exists(pv)) 
                return defaultVersion;

            using (var f = File.OpenRead(pv))
            using (var reader = new StreamReader(f))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!line.StartsWith("Update (", StringComparison.Ordinal))
                        continue;

                    // extract version from line containing Update (X.X.X)
                    int from = line.IndexOf("(") + 1;
                    int to = line.LastIndexOf(")");
                    return line.Substring(from, to - from);     // extract and return X.X.X 
                }                
            }

            return defaultVersion;     // fallback
        }

        /// <summary>
        /// Retrieves all of the download URLs for a specific title.
        /// </summary>
        /// <param name="titleName">Title of the game.</param>
        /// <returns>List of Uri's containing the Urls to Mods.</returns>
        private async Task<List<Uri>> GetModDownloadUrls(string modUrlPath, string titleVersion)
        {
            // fetch all download links for current game

            // read switch-mods webpage and get download links for current game
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($@"{BaseModsRepoUrl}/{modUrlPath}/{titleVersion}");
            var nodes = htmlDoc.DocumentNode.SelectNodes($@"//h2[@id='files']/following::a[1]/following::div[2]//div[@role='rowheader']//a");
            //var nodes = htmlDoc.DocumentNode.SelectNodes($@"//h2[contains(., {Quote}Files{Quote})]/following::a[1]/following::div[2]//div[@role='rowheader']//a");

            // if no links found, return empty list 
            if (nodes == null) return new List<Uri>();

            //otherwise process links and add them into downloadUrls list
            var downloadUrls = new List<Uri>();
            foreach (HtmlNode node in nodes)
            {
                string url = node.Attributes["href"].Value.Trim();
                if (url.EndsWith(".zip") || url.EndsWith(".rar") || url.EndsWith(".7z"))
                {
                    // prepend github.com to url 
                    if (!url.StartsWith("https://github.com", StringComparison.Ordinal))
                        url = $@"https://github.com{url}";

                    // swap /blob/ for /raw/ to access direct download path 
                    url = url.Replace("/blob/", "/raw/");
                    downloadUrls.Add(new Uri(url));
                }
            }
            return downloadUrls;
        }

        private void CleanUp()
        {
            if (File.Exists(TheBoy181Xml))
                File.Delete(TheBoy181Xml);
        }
    }
}
