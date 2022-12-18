using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YuzuModDownloader
{
    public class ModDownloader
    {
        private const string Quote = "\"";
        private string _sevenZipExePath;

        protected ModDownloader()
        {
            UserDirPath = Directory.Exists("user") ?
                Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "user") :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu");
            ModDirectoryPath = GetModPath(UserDirPath);
            _sevenZipExePath = "";
        }

        protected string UserDirPath { get; private set; } = "";

        protected string ModDirectoryPath { get; private set; } = "";

        protected internal bool IsModDataLocationToBeDeleted { get; set; } = false;

        protected internal bool IsDownloadedModArchivesToBeDeleted { get; set; } = true;

        protected internal delegate void UpdateProgressDelegate(int progressPercentage, string progressText);

        protected internal event UpdateProgressDelegate UpdateProgress;

        protected void RaiseUpdateProgressDelegate(int progressPercentage, string progressText)
        {
            UpdateProgress?.Invoke(progressPercentage, progressText);
        }

        /// <summary>
        /// Downloads all prequisites for YuzuModDownloader to function.
        /// </summary>
        /// <returns></returns>
        protected async Task DownloadPrerequisitesAsync()
        {
            if (!IsSevenZipInstalled()) 
                await DownloadSevenZip();
        }

        /// <summary>
        /// Downloads the specified URL and saves it as the file name passed.
        /// </summary>
        /// <param name="url">URL of the file to download</param>
        /// <param name="fileName">Name of the destination local file</param>
        /// <returns></returns>
        protected async Task DownloadGameDatabaseAsync(string url, string fileName)
        {
            // download game database & place in same directory as executable
            using (var client = new WebClient())
            {
                client.DownloadFileCompleted += (s, e) => RaiseUpdateProgressDelegate(100, "Done");
                client.DownloadProgressChanged += (s, e) => RaiseUpdateProgressDelegate(e.ProgressPercentage, $@"Downloading {fileName} ...");
                await client.DownloadFileTaskAsync(url, fileName);
            }
        }

        /// <summary>
        /// Reads the Games list, downloads Mods per game and extracts them into the Mod Data Location.
        /// </summary>
        /// <param name="games">List of Games.</param>
        /// <returns></returns>
        protected async Task DownloadModsAsync(List<Game> games)
        {
            using (var client = new WebClient())
            {
                foreach (var game in games)
                {
                    // clear mod data location if user has checked the option 
                    if (IsModDataLocationToBeDeleted)
                    {
                        DeleteModDataLocationPath(game.ModDataLocation);
                    }

                    foreach (var url in game.ModDownloadUrls)
                    {
                        string fileName = url.AbsoluteUri.Substring(url.AbsoluteUri.LastIndexOf('/') + 1).Trim();

                        client.DownloadFileCompleted += (s, e) =>
                        {
                            RaiseUpdateProgressDelegate(0, $"Unpacking {fileName} ...");

                            // unzip downloaded mod 
                            var psi = new ProcessStartInfo
                            {
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                FileName = _sevenZipExePath,
                                Arguments = $@"x {Quote}{game.ModDataLocation}\{fileName}{Quote} -o{Quote}{game.ModDataLocation}{Quote} -aoa"
                            };
                            using (var p = Process.Start(psi))
                            {
                                p.WaitForExit();
                                RaiseUpdateProgressDelegate(100, "Done");
                            }
                        };
                        client.DownloadProgressChanged += (s, e) => RaiseUpdateProgressDelegate(e.ProgressPercentage, $"Downloading {fileName} ...");
                        await client.DownloadFileTaskAsync(url.AbsoluteUri, $@"{game.ModDataLocation}/{fileName}");
                    }
                }
            }

            if (IsDownloadedModArchivesToBeDeleted)
            {
                DeleteDownloadedModArchiveFiles();
            }
        }

        /// <summary>
        /// Checks if 7-Zip is currently installed on this machine.
        /// </summary>
        /// <returns>
        /// <c>true</c> if installed; otherwise <c>false</c>.
        /// </returns>
        private bool IsSevenZipInstalled()
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            string programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");

            if (programFiles != null && File.Exists($@"{programFiles}\7-Zip\7z.exe"))
            {
                _sevenZipExePath = $@"{programFiles}\7-Zip\7z.exe";
                return true;
            }
            if (programFilesX86 != null && File.Exists($@"{programFilesX86}\7-Zip\7z.exe"))
            {
                _sevenZipExePath = $@"{programFilesX86}\7-Zip\7z.exe";
                return true;
            }
            return false;
        }

        /// <summary>
        /// Downloads 7-Zip and extracts it inside the Yuzu User Directory path.
        /// </summary>
        /// <returns></returns>
        private async Task DownloadSevenZip()
        {
            string prerequisitesLocation = $@"{UserDirPath}\7z";
            string sevenZLocation = $@"{prerequisitesLocation}\7z.zip";

            if (Directory.Exists(prerequisitesLocation))
            {
                DeleteDirectory(prerequisitesLocation, true);
            }
            Directory.CreateDirectory(prerequisitesLocation);

            using (var client = new WebClient())
            {
                client.DownloadFileCompleted += (s, e) =>
                {
                    UpdateProgress(0, "Unpacking prerequisites ...");
                    using (var archive = ZipFile.OpenRead(sevenZLocation))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            entry.ExtractToFile(Path.Combine($"{prerequisitesLocation}", entry.FullName), true);
                        }
                    }
                    _sevenZipExePath = $@"{prerequisitesLocation}\7z.exe";
                    UpdateProgress(100, "Done");
                };
                client.DownloadProgressChanged += (s, e) => RaiseUpdateProgressDelegate(e.ProgressPercentage, "Downloading Prerequisites ...");
                await client.DownloadFileTaskAsync("https://github.com/amakvana/YuzuModDownloader/raw/main/assets/7z/22.01/7z.zip", sevenZLocation);
            }
        }

        /// <summary>
        /// Delete the Downloaded Mod Archive Files
        /// </summary>
        private void DeleteDownloadedModArchiveFiles()
        {
            if (string.IsNullOrWhiteSpace(ModDirectoryPath))
                return;

            // loop through each title_id
            // delete archives from within title_id folder
            var directories = Directory.GetDirectories(ModDirectoryPath);
            foreach (var subDirectory in directories)
            {
                DeleteFiles(subDirectory, ".zip", ".rar", ".7z");
            }
        }

        /// <summary>
        /// Deletes all subdirectories and files inside the Mod Data Location path
        /// </summary>
        /// <param name="path">The absolute path of Mod Data Location</param>
        private void DeleteModDataLocationPath(string path)
        {
            DeleteDirectory(path, true);
        }

        /// <summary>
        /// Extracts the Mod Path from the Yuzu 'User' folder.
        /// </summary>
        /// <param name="baseDirPath">Absolute directory path to the Yuzu 'User' folder.</param>
        /// <returns></returns>
        private string GetModPath(string baseDirPath)
        {
            // read in qt-config.ini 
            // extract value from load_directory key 
            // format and return path 
            string configPath = $@"{baseDirPath}\config\qt-config.ini";
            var ini = new IniFile(configPath);
            string modPath = ini.Read("load_directory", "Data%20Storage").Replace(@"\\", @"\");
            return modPath;
        }

        private void DeleteDirectory(string path, bool recursive = false)
        {
            // https://stackoverflow.com/a/1288747
            // Modified to optionally allow recusive deletion

            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }

            if (recursive)
            {
                var directories = dir.GetDirectories();
                foreach (DirectoryInfo di in directories)
                {
                    DeleteDirectory(di.FullName, true);
                    di.Delete();
                }
            }
        }

        private void DeleteFiles(string path, params string[] extensions)
        {
            // https://stackoverflow.com/a/13301088
            var ext = extensions.ToList();
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                                 .Where(s => ext.Contains(Path.GetExtension(s).TrimStart().ToLowerInvariant()));

            // https://stackoverflow.com/a/8132800
            foreach (var file in files)
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch { }
            }
        }
    }
}
