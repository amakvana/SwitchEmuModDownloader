using PuppeteerSharp;
using System.Diagnostics;

namespace SwitchEmuModDownloader.Classes.Managers;

public sealed class BrowserManager
{
    public enum InstallationState
    {
        Installed,
        NotInstalled
    }

    public static async Task<(InstallationState, string)> DetectBrowsersAsync()
    {
        if (OperatingSystem.IsLinux())
            return await DetectLinuxBrowsersAsync();

        // otherwise, assume Windows 
        return DetectWindowsBrowsers();
    }

    public static async Task<string> DownloadBrowserAsync()
    {
        // create temp directory to download chrome 
        string tempDownloadDirectoryPath = OperatingSystem.IsLinux() switch
        {
            true => $"/{Path.Combine("var", "tmp", "SwitchEmuModDownloader")}",
            false => Path.Combine(Path.GetTempPath(), "SwitchEmuModDownloader")
        };
        Directory.CreateDirectory(tempDownloadDirectoryPath);

        // download chrome into temp path
        await new BrowserFetcher(new BrowserFetcherOptions()
        {
            Path = tempDownloadDirectoryPath
        }).DownloadAsync();

        // find where the exe is located, then return the full path
        foreach (string file in Directory.EnumerateFiles(tempDownloadDirectoryPath,
            OperatingSystem.IsLinux() ? "chrome" : "chrome.exe",
            SearchOption.AllDirectories))
        {
            return file;
        }
        return "";
    }

    /// <summary>
    /// Detect on Windows whether Google Chrome or Microsoft Edge are installed
    /// </summary>
    /// <returns>Whether a browser is installed and its installation path.</returns>
    private static (InstallationState, string) DetectWindowsBrowsers()
    {
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        // firefox seems to hang when ran headless with puppeteersharp 
        string[] browserPaths = [
            Path.Combine(programFilesX86, "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(programFiles, "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe")
        ];

        // check, in order above, if browsers exist and return first browser that does
        foreach (string browserPath in browserPaths)
        {
            if (File.Exists(browserPath))
            {
                return (InstallationState.Installed, browserPath);
            }
        }

        return (InstallationState.NotInstalled, "");
    }

    /// <summary>
    /// Detect on Linux whether Google Chrome, Microsoft Edge, Chromium or FireFox are installed
    /// </summary>
    /// <returns>Whether a browser is installed and its installation path.</returns>
    private static async Task<(InstallationState, string)> DetectLinuxBrowsersAsync()
    {
        // firefox seems to hang with PuppeterSharp on linux 
        string[] browsers = ["google-chrome-stable", "microsoft-edge-stable"];

        foreach (string browser in browsers)
        {
            string path = await RunShellCommandAsync("which", browser);

            if (!string.IsNullOrWhiteSpace(path))
            {
                return (InstallationState.Installed, path.Trim());
            }
        }

        return (InstallationState.NotInstalled, "");
    }

    private static async Task<string> RunShellCommandAsync(string cmd, string arg)
    {
        using Process process = new()
        {
            StartInfo = new()
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{cmd} {arg}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        string result = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return result;
    }
}
