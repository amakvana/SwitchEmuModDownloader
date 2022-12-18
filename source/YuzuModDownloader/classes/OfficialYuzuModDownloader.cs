using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace YuzuModDownloader
{
    public class OfficialYuzuModDownloader : ModDownloader
    {
        private const string BaseModsRepoUrl = "https://github.com/yuzu-emu/yuzu/wiki/Switch-Mods";
        private const string Quote = "\"";
        private const string GameTitleIDsXml = "GameTitleIDs.xml";

        public OfficialYuzuModDownloader() : base() { }

        public new async Task DownloadPrerequisitesAsync()
        {
            await base.DownloadPrerequisitesAsync();
            await base.DownloadGameDatabaseAsync("https://raw.githubusercontent.com/amakvana/YuzuModDownloader/main/assets/GameTitleIDs.xml", GameTitleIDsXml);
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
            using (var reader = XmlReader.Create(GameTitleIDsXml))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement()) 
                        continue;

                    switch (reader.Name)
                    {
                        case "title_name":
                            string titleName = reader.ReadElementContentAsString();
                            reader.Read();
                            string titleId = reader.ReadElementContentAsString();

                            if (string.IsNullOrWhiteSpace(titleId) || !Directory.Exists($"{base.ModDirectoryPath}/{titleId}"))
                                break;

                            var game = new Game
                            {
                                TitleID = titleId,
                                TitleName = titleName,
                                ModDataLocation = $"{base.ModDirectoryPath}/{titleId}",
                                ModDownloadUrls = await GetModDownloadUrls(titleName)   // detect urls for each game and populate the downloads 
                            };
                            
                            games.Add(game);
                            break;
                        default: break;     //do nothing 
                    }
                }
            }
            base.RaiseUpdateProgressDelegate(100, "Done");
            return games;
        }

        /// <summary>
        /// Retrieves all of the download URLs for a specific title.
        /// </summary>
        /// <param name="titleName">Title of the game.</param>
        /// <returns>List of Uri's containing the Urls to Mods.</returns>
        private async Task<List<Uri>> GetModDownloadUrls(string titleName)
        {
            // fetch all download links for current game

            // read switch-mods webpage and get download links for current game
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(BaseModsRepoUrl);
            var nodes = htmlDoc.DocumentNode.SelectNodes($@"//h3[contains(., {Quote}{titleName}{Quote})]/following::table[1]//td//a");

            // if no links found, return empty list 
            if (nodes == null) return new List<Uri>();

            // otherwise process links and add them into downloadUrls list 
            var downloadUrls = new List<Uri>();
            foreach (HtmlNode node in nodes)
            {
                string url = node.Attributes["href"].Value.Trim();
                if (url.EndsWith(".zip") || url.EndsWith(".rar") || url.EndsWith(".7z"))
                {
                    downloadUrls.Add(new Uri(url));
                }
            }
            return downloadUrls;
        }

        private void CleanUp()
        {
            if (File.Exists(GameTitleIDsXml))
                File.Delete(GameTitleIDsXml);
        }
    }
}
