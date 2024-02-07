using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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

        public MainWindow(IServiceProvider serviceProvider) : this() => _clientFactory = serviceProvider.GetService<IHttpClientFactory>();

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
            var confirmationWindow = new DownloadConfirmationWindow(games);
            await confirmationWindow.ShowDialog(this);

            // reset ui
            ToggleUiControls(true);
        }

        public void ExitToolStripMenuItem_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);

        public void YuzuWebsiteToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LaunchUrl("https://yuzu-emu.org/");
        }

        public async void AboutToolStripMenuItem_ClickAsync(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            await aboutWindow.ShowDialog(this);
        }

        private void ModDownloader_UpdateProgress(int progressPercentage, string progressText)
        {
            PbarProgress.Value = progressPercentage;
            PbarProgress.ProgressTextFormat = $"{progressText} ({progressPercentage}%)";
            PbarProgress.InvalidateVisual();
            PbarProgress.UpdateLayout();
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
                    await ShowMessageBox("New version of Yuzu Mod Downloader is available.", MsBox.Avalonia.Enums.Icon.Info);
                    LaunchUrl("https://github.com/amakvana/YuzuModDownloader");
                    break;
                case AppUpdater.CurrentVersion.NotSupported:
                    await ShowMessageBox("This version of Yuzu Mod Downloader is no longer supported.", MsBox.Avalonia.Enums.Icon.Error);
                    LaunchUrl("https://github.com/amakvana/YuzuModDownloader");
                    Environment.Exit(0);
                    break;
                case AppUpdater.CurrentVersion.Undetectable:
                    await ShowMessageBox("Network Connection Error! Please check your internet connection and try again.", MsBox.Avalonia.Enums.Icon.Error);
                    Environment.Exit(1);
                    break;
            }
        }

        private async Task ShowMessageBox(string message, Icon icon) => await MessageBoxManager.GetMessageBoxStandard(new()
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentTitle = this.Title,
            ContentMessage = $"{message}{Environment.NewLine}{Environment.NewLine}" +
                                 "Please download the latest version from https://github.com/amakvana/YuzuModDownloader",
            Icon = icon,
            WindowIcon = this.Icon,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        }).ShowWindowDialogAsync(this);

        private static void LaunchUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "Open"
            })?.Dispose();
        }
    } 
}