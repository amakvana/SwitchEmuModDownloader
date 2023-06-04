using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using YuzuModDownloader.Classes.Downloaders;
using YuzuModDownloader.Classes.Entities;
using YuzuModDownloader.Classes.Updaters;

namespace YuzuModDownloader
{
    public partial class MainWindow : Window
    {
        private readonly IHttpClientFactory? _clientFactory;

        public MainWindow()
        {
            InitializeComponent();

            // lock gui controls while checking if app is latest 
            ToggleUiControls(false);
        }

        public MainWindow(IServiceProvider serviceProvider) : this()
        {
            _clientFactory = serviceProvider.GetService<IHttpClientFactory>();
        }

        public async void Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            // check app is latest 
            await CheckAppVersionAsync();

            // if app is latest, unlock controls 
            ToggleUiControls(true);
        }

        public async void BtnDownload_ClickAsync(object sender, RoutedEventArgs e)
        {
            // disable form controls
            ToggleUiControls(false);

            int totalGames = 0;
            var games = new List<Game>();
            switch (CboModRepos.SelectedIndex)
            {
                case 0:     // official switch-mods repo
                    var oyModDownloader = new OfficialYuzuModDownloader(_clientFactory!, ClearModDataLocationToolStripMenuItem.IsChecked.GetValueOrDefault(), DeleteDownloadedModArchivesToolStripMenuItem.IsChecked.GetValueOrDefault());
                    oyModDownloader.UpdateProgress += ModDownloader_UpdateProgress;
                    await oyModDownloader.DownloadPrerequisitesAsync();
                    games = await oyModDownloader.ReadGameTitlesDatabaseAsync();
                    await oyModDownloader.DownloadModsAsync(games);
                    break;

                case 1:     // theboy181 repo
                    var tb181ModDownloader = new TheBoy181ModDownloader(_clientFactory!, ClearModDataLocationToolStripMenuItem.IsChecked.GetValueOrDefault(), DeleteDownloadedModArchivesToolStripMenuItem.IsChecked.GetValueOrDefault());
                    tb181ModDownloader.UpdateProgress += ModDownloader_UpdateProgress;
                    await tb181ModDownloader.DownloadPrerequisitesAsync();
                    games = await tb181ModDownloader.ReadGameTitlesDatabaseAsync();
                    await tb181ModDownloader.DownloadModsAsync(games);
                    break;

                case 2:     // holographicwings totk repo
                    var holographicWingsModDownloader = new HolographicWingsTotkModDownloader(_clientFactory!, ClearModDataLocationToolStripMenuItem.IsChecked.GetValueOrDefault(), DeleteDownloadedModArchivesToolStripMenuItem.IsChecked.GetValueOrDefault());
                    holographicWingsModDownloader.UpdateProgress += ModDownloader_UpdateProgress;
                    await holographicWingsModDownloader.DownloadPrerequisitesAsync();
                    games = await holographicWingsModDownloader.ReadGameTitlesDatabaseAsync();
                    await holographicWingsModDownloader.DownloadModsAsync(games);
                    break;

                default:
                    break;  // do nothing 
            }

            // tell user mods have been downloaded 
            PbarProgress.ProgressTextFormat = "Done!";
            totalGames = (games.Count > 0) ? games.Where(g => g.ModDownloadUrls.Count > 0).Count() : 0;
            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                ContentTitle = this.Title,
                ContentMessage = $"Done! Mods downloaded for {totalGames} games.{Environment.NewLine}{Environment.NewLine}" +
                                    $"To toggle specific game mods On/Off:{Environment.NewLine}" +
                                    $"Run yuzu > Right-Click on a game > Properties > Add-Ons",
                Icon = MessageBox.Avalonia.Enums.Icon.Success,
                WindowIcon = this.Icon,
                WindowStartupLocation= WindowStartupLocation.CenterOwner
            }).ShowDialog(this);

            // reset ui
            ToggleUiControls(true);
        }

        public void ExitToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void YuzuWebsiteToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://yuzu-emu.org/")
            {
                UseShellExecute = true,
                Verb = "Open"
            })?.Dispose();
        }

        public void AboutToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog(this);
        }

        private void ModDownloader_UpdateProgress(int progressPercentage, string progressText)
        {
            PbarProgress.Value = progressPercentage;
            PbarProgress.ProgressTextFormat = $"{progressText} ({progressPercentage}%)";
        }

        private void ToggleUiControls(bool value)
        {
            CboModRepos.IsEnabled = value;
            BtnDownload.IsEnabled = value;
            OptionsToolStripMenuItem.IsEnabled = value;
            PbarProgress.Value = 0;
        }

        private async Task CheckAppVersionAsync()
        {
            var updater = new AppUpdater(_clientFactory!);

            var currentAppVersion = await updater.CheckVersionAsync();
            switch (currentAppVersion)
            {
                case AppUpdater.CurrentVersion.UpdateAvailable:
                    await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
                    {
                        ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        ContentTitle = this.Title,
                        ContentMessage = $"New version of Yuzu Mod Downloader is available.{Environment.NewLine}{Environment.NewLine}" +
                                            "Please download the latest version from https://github.com/amakvana/YuzuModDownloader",
                        Icon = MessageBox.Avalonia.Enums.Icon.Info,
                        WindowIcon = this.Icon,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }).ShowDialog(this);
                    Process.Start(new ProcessStartInfo("https://github.com/amakvana/YuzuModDownloader")
                    {
                        UseShellExecute = true,
                        Verb = "Open"
                    })?.Dispose();
                    break;
                case AppUpdater.CurrentVersion.NotSupported:
                    await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
                    {
                        ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        ContentTitle = this.Title,
                        ContentMessage = $"This version of Yuzu Mod Downloader is no longer supported.{Environment.NewLine}{Environment.NewLine}" +
                                            "Please download the latest version from https://github.com/amakvana/YuzuModDownloader",
                        Icon = MessageBox.Avalonia.Enums.Icon.Error,
                        WindowIcon = this.Icon,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }).ShowDialog(this);
                    Process.Start(new ProcessStartInfo("https://github.com/amakvana/YuzuModDownloader")
                    {
                        UseShellExecute = true,
                        Verb = "Open"
                    });
                    Environment.Exit(0);
                    break;
                case AppUpdater.CurrentVersion.Undetectable:
                    await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
                    {
                        ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        ContentTitle = this.Title,
                        ContentMessage = $"Network Connection Error! Please check your internet connection and try again.",
                        Icon = MessageBox.Avalonia.Enums.Icon.Error,
                        WindowIcon = this.Icon,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }).ShowDialog(this);
                    Environment.Exit(0);
                    break;
            }
        }
    }
}