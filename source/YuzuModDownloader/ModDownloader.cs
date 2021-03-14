using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace YuzuModDownloader
{
    public class ModDownloader
    {
        private readonly string UserDirPath;

        public ModDownloader()
        {
            UserDirPath = Directory.Exists("user") ?
                Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "user") :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu");
        }

        public delegate void UpdateProgressDelegate(int progressPercentage, string progressText);

        public event UpdateProgressDelegate UpdateProgress;

        public async Task DownloadGameTitleIdDatabaseAsync(string fileName)
        {
            // download latest GameTitleID's & place in same directory as executable
            using (var wc = new WebClient())
            {
                wc.DownloadFileCompleted += (s, e) => UpdateProgress(100, "Done");
                wc.DownloadProgressChanged += (s, e) => UpdateProgress(e.ProgressPercentage, "Downloading Latest Game TitleID's ...");
                await wc.DownloadFileTaskAsync("https://raw.githubusercontent.com/amakvana/YuzuModDownloader/main/assets/GameTitleIDs.xml", fileName);
            }
        }

        public async Task DownloadPrerequisitesAsync()
        {
            // download 7zip so we can use this to extract mods
            // faster and more cleaner
            string prerequisitesLocation = $"{UserDirPath}/7z";
            string sevenZLocation = $"{prerequisitesLocation}/7z.zip";

            if (!Directory.Exists(prerequisitesLocation)) Directory.CreateDirectory(prerequisitesLocation);
            using (var wc = new WebClient())
            {
                wc.DownloadFileCompleted += (s, e) =>
                {
                    UpdateProgress(0, "Unpacking prerequisites ...");
                    using (var archive = ZipFile.OpenRead(sevenZLocation))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            entry.ExtractToFile(Path.Combine($"{prerequisitesLocation}", entry.FullName), true);
                        }
                    }
                    UpdateProgress(100, "Done");
                };
                wc.DownloadProgressChanged += (s, e) => UpdateProgress(e.ProgressPercentage, "Downloading Prerequisites ...");
                await wc.DownloadFileTaskAsync("https://my.cloudme.com/v1/ws2/:amakvana/:7z/7z.zip", sevenZLocation);
            }
        }

        public Dictionary<string, string> ReadGameTitleIdDatabase(string gameTitleIDsXml)
        {
            // detect yuzu user directory 
            // loop through /load/ folder & get title names from title Id's
            // return list 

            var d = new Dictionary<string, string>();
            var doc = new XmlDocument();

            doc.Load(gameTitleIDsXml);
            var nodes = doc.DocumentElement.SelectNodes("//*[local-name()='games']/*[local-name()='game']");
            foreach (XmlNode node in nodes)
            {
                string titleName = node["title_name"].InnerText.Trim();
                string titleId = node["title_id"].InnerText.Trim();
                
                if (!string.IsNullOrWhiteSpace(titleId) && Directory.Exists($"{UserDirPath}/load/{titleId}"))
                {
                    d.Add(titleName, titleId);
                }
            }
            return d;
        }

        public async Task DownloadTitleModsAsync(string titleName, string titleId, string modWebsiteUrl, bool deleteExistingMods)
        {
            // fetch all download links for current game
            // extract them into appropriate directory

            var web = new HtmlWeb();
            var htmlDoc = web.Load(modWebsiteUrl);
            var nodes = htmlDoc.DocumentNode.SelectNodes(@"//h3[contains(., """ + titleName + "\")]/following::table[1]//td//a");

            // if true, delete existing mods 
            if (deleteExistingMods)
            {
                DeleteModData($@"{UserDirPath}\load\{titleId}");
            }

            // download all mods for game 
            if (nodes != null)
            {
                using (var wc = new WebClient())
                {
                    foreach (HtmlNode node in nodes)
                    {
                        string modName = node.InnerText;
                        string modDownloadUrl = node.Attributes["href"].Value.Trim();
                        string fileName = modDownloadUrl.Split('/').Last().Trim();

                        // if url is gamebanana, scrape page and get actual download
                        //if (modDownloadUrl.Contains("gamebanana.com"))
                        //{
                        //    // modUrl = GetGameBananaDownloadUrl(modUrl, out fileName);

                        //    // NEED to fix when files are extracted from GameBanana packages
                        //}
                        //else if (modDownloadUrl.Contains("bit.ly"))
                        //{
                        //    // coming soon ...
                        //}
                        if (modDownloadUrl.EndsWith(".zip") || modDownloadUrl.EndsWith(".rar") || modDownloadUrl.EndsWith(".7z"))
                        {
                            wc.DownloadFileCompleted += (s, e) =>
                            {
                                UpdateProgress(0, $"Unpacking {fileName} ...");

                                // unzip downloaded mod 
                                var psi = new ProcessStartInfo
                                {
                                    WindowStyle = ProcessWindowStyle.Hidden,
                                    FileName = $@"{UserDirPath}\7z\7z.exe",
                                    Arguments = "x \"" + $@"{UserDirPath}\load\{titleId}\{fileName}" + "\" -o" + $@"{UserDirPath}\load\{titleId}" + " -aoa"
                                };
                                using (var p = Process.Start(psi))
                                {
                                    UpdateProgress(100, "Done");
                                }
                            };
                            wc.DownloadProgressChanged += (s, e) => UpdateProgress(e.ProgressPercentage, $"Downloading {fileName} ...");
                            await wc.DownloadFileTaskAsync(modDownloadUrl, $"{UserDirPath}/load/{titleId}/{fileName}");
                        }
                    }
                }
            }
        }

        private void DeleteModData(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                DeleteModData(di.FullName);
                di.Delete();
            }
        }


        //private string GetGameBananaDownloadUrl(string url, out string fileName)
        //{
        //    // grabs the download url from the gamebanana mod webpage
        //    var web = new HtmlWeb();
        //    var htmlDoc = web.Load(url);
        //    var node = htmlDoc.DocumentNode.SelectSingleNode("//module[@id=\"FilesModule\"]/div//a");
        //    string downloadUrl = node.Attributes["href"].Value;
        //    fileName = htmlDoc.DocumentNode.SelectSingleNode("//module[@id=\"FilesModule\"]/div//code").InnerText.Trim();
        //    return downloadUrl;
        //}

    }
}
