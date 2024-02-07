using System.Diagnostics;
using System.IO.Compression;
using YuzuModDownloader.Classes.Entities;
using YuzuModDownloader.Classes.Extensions;

namespace YuzuModDownloader.Classes.Downloaders
{
    public class ModDownloader
    {
        private const string Quote = "\"";
        private readonly IHttpClientFactory _clientFactory;
        private readonly bool _isModDataLocationToBeDeleted;
        private readonly bool _isDownloadedModArchivesToBeDeleted;
        private string _sevenZipExePath;

        protected ModDownloader(IHttpClientFactory clientFactory, bool isModDataLocationToBeDeleted, bool isDownloadedModArchivesToBeDeleted)
        {
            UserDirPath = GetUserDirectoryPath();
            ModDirectoryPath = GetModPath(UserDirPath);
            _sevenZipExePath = "";
            _clientFactory = clientFactory;
            _isModDataLocationToBeDeleted = isModDataLocationToBeDeleted;
            _isDownloadedModArchivesToBeDeleted = isDownloadedModArchivesToBeDeleted;
        }

        protected string UserDirPath { get; } = "";

        protected string ModDirectoryPath { get; } = "";

        protected internal Action<int, string>? UpdateProgress;

        protected void RaiseUpdateProgress(int progressPercentage, string progressText) => UpdateProgress?.Invoke(progressPercentage, progressText);

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
        protected async Task DownloadGameDatabaseAsync(string url)
        {
            // download game database & place in same directory as executable
            string fileName = url[(url.LastIndexOf('/') + 1)..];
            const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            await using var file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 8192, fileOptions);
            var progressReporter = new Progress<float>(progress =>
            {
                var progressPercentage = (int)(progress * 100);
                RaiseUpdateProgress(progressPercentage, $"Downloading {fileName} ...");
            });
            var client = _clientFactory.CreateClient("GitHub-YuzuModDownloader");
            await client.DownloadAsync(url, file, progressReporter);
            RaiseUpdateProgress(100, $"Downloading {fileName} ...");
        }

        /// <summary>
        /// Reads the Games list, downloads Mods per game and extracts them into the Mod Data Location.
        /// </summary>
        /// <param name="games">List of Games.</param>
        /// <returns></returns>
        protected async Task DownloadModsAsync(List<Game> games)
        {
            var client = _clientFactory.CreateClient();
            const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            foreach (var game in games)
            {
                // clear mod data location if user has checked the option 
                if (_isModDataLocationToBeDeleted)
                {
                    DeleteModDataLocationPath(game.ModDataLocation);
                }

                foreach (var url in game.ModDownloadUrls)
                {
                    // download each mod url 
                    string fileName = url.AbsoluteUri[(url.AbsoluteUri.LastIndexOf('/') + 1)..].Trim();
                    await using (var file = new FileStream(Path.Combine(game.ModDataLocation, fileName), FileMode.Create, FileAccess.Write, FileShare.None, 8192, fileOptions))
                    {
                        var progressReporter = new Progress<float>(progress =>
                        {
                            var progressPercentage = (int)(progress * 100);
                            RaiseUpdateProgress(progressPercentage, $"Downloading {fileName} ...");
                        });
                        await client.DownloadAsync(url.AbsoluteUri, file, progressReporter);
                    }

                    // file has been downloaded, unpack it 
                    RaiseUpdateProgress(0, $"Unpacking {fileName} ...");

                    // unzip downloaded mod 
                    var psi = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = _sevenZipExePath,
                        Arguments = $"x {Quote}{Path.Combine(game.ModDataLocation, fileName)}{Quote} -o{Quote}{game.ModDataLocation}{Quote} -aoa"
                    };

                    using (var p = Process.Start(psi)!)
                    {
                        await p.WaitForExitAsync();
                        RaiseUpdateProgress(100, $"Unpacking {fileName} ...");
                    }
                }
            }

            if (_isDownloadedModArchivesToBeDeleted)
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
            string[] possibleLocations = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe")
            };

            foreach (string location in possibleLocations)
            {
                if (File.Exists(location))
                {
                    _sevenZipExePath = location;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Downloads 7-Zip and extracts it inside the Yuzu User Directory path.
        /// </summary>
        /// <returns></returns>
        private async Task DownloadSevenZip()
        {
            string prerequisitesLocation = Path.Combine(UserDirPath, "7z");
            string sevenZLocation = Path.Combine(prerequisitesLocation, "7z.zip");
            _sevenZipExePath = Path.Combine(prerequisitesLocation, "7z.exe");

            // set linux paths 
            if (OperatingSystem.IsLinux())
                _sevenZipExePath = Path.Combine(prerequisitesLocation, "7zz");
            
            // if 7z was already downloaded before, use that 
            if (File.Exists(_sevenZipExePath))
                return;

            // otherwise, download 7zip
            Directory.CreateDirectory(prerequisitesLocation);
            string downloadUrl = OperatingSystem.IsLinux()
                ? "assets/7z/23.01/7z-linux.zip"
                : "assets/7z/23.01/7z.zip";
            var client = _clientFactory.CreateClient("GitHub-YuzuModDownloader");
            const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            await using (var file = new FileStream(sevenZLocation, FileMode.Create, FileAccess.Write, FileShare.None, 8192, fileOptions))
            {
                var progressReporter = new Progress<float>(progress =>
                {
                    var progressPercentage = (int)(progress * 100);
                    RaiseUpdateProgress(progressPercentage, "Downloading 7-Zip ...");
                });
                await client.DownloadAsync(downloadUrl, file, progressReporter);
            }

            // unpack 7zip
            RaiseUpdateProgress(0, "Unpacking 7-Zip ...");
            using (var archive = ZipFile.OpenRead(sevenZLocation))
            {
                int totalFiles = archive.Entries.Count;
                int copiedFiles = 0;
                foreach (var entry in archive.Entries)
                {
                    entry.ExtractToFile(Path.Combine(prerequisitesLocation, entry.FullName), true);
                    copiedFiles++;
                    int progressPercentage = (int)((double)copiedFiles / totalFiles * 100);
                    RaiseUpdateProgress(progressPercentage, "Unpacking 7-Zip ...");
                }
            }

            // add execute permissions on 7zz, if on linux 
            if (OperatingSystem.IsLinux())
            {
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c {Quote}chmod +x {_sevenZipExePath}{Quote}"
                };

                using (var p = Process.Start(psi)!)
                {
                    await p.WaitForExitAsync();
                }
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
        private static void DeleteModDataLocationPath(string path)
        {
            if (!Directory.Exists(path))
                return;

            // clear all subfolders within /load/XXXXXXXXXXXXXXXX/
            var di = new DirectoryInfo(path);
            foreach (var subDirectory in di.GetDirectories())
            {
                subDirectory.Delete(true);
            }

            // clear all remnant files directly inside /load/XXXXXXXXXXXXXXXX/
            foreach (var files in di.GetFiles())
            {
                files.Delete();
            }
        }

        private static string GetUserDirectoryPath()
        {
            // if linux 
            if (OperatingSystem.IsLinux())
            {
                // Flatpak version of Yuzu : Path to $HOME/.var/app/org.yuzu_emu.yuzu/config/yuzu
                string flatpakConfigDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".var", "app", "org.yuzu_emu.yuzu", "config", "yuzu");
                if (Directory.Exists(flatpakConfigDirectoryPath))
                    return flatpakConfigDirectoryPath;

                // Standard Yuzu Installation : Path to $XDG_CONFIG_HOME/yuzu
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu");
            }

            // otherwise, assume windows 
            return Directory.Exists("user") ?
                Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!, "user") :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu");
        }

        /// <summary>
        /// Extracts the Mod Path from the Yuzu 'User' folder.
        /// </summary>
        /// <param name="baseDirPath">Absolute directory path to the Yuzu 'User' folder.</param>
        /// <returns></returns>
        private static string GetModPath(string baseDirPath)
        {
            // read in qt-config.ini 
            // extract value from load_directory key 
            // format and return path 
            string configPath = Path.Combine(baseDirPath, "config", "qt-config.ini");   // windows qt-config.ini path 

            // if linux, modify path 
            if (OperatingSystem.IsLinux())
                configPath = Path.Combine(baseDirPath, "qt-config.ini");

            using var reader = new StreamReader(configPath);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (!line.StartsWith("load_directory=", StringComparison.OrdinalIgnoreCase))
                    continue;

                // current line starts with "load_directory=", remove the key out of string 
                // this way handles any "=" inside folder names
                string loadDirectoryPath = line.Replace("load_directory=", "");

                // cleanup dirty characters and use "/" as the base separator 
                loadDirectoryPath = loadDirectoryPath.Replace($"{Quote}", "");
                loadDirectoryPath = loadDirectoryPath.Replace(@"\\", "/");
                loadDirectoryPath = loadDirectoryPath.Replace(@"\", "/");

                // split the path then return path using OS delimiters
                string formattedLoadDirectoryPath = Path.Combine(loadDirectoryPath.Split("/"));

                // prepend "/" if Linux 
                if (OperatingSystem.IsLinux())
                    formattedLoadDirectoryPath = $"/{formattedLoadDirectoryPath}";

                return formattedLoadDirectoryPath;
            }

            // fallback to %appdata%\yuzu\load
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu", "load");
        }

        private static void DeleteFiles(string path, params string[] extensions)
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
