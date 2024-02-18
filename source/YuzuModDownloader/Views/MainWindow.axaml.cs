using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Diagnostics;
using YuzuModDownloader.Classes.Downloaders.Interfaces;
using YuzuModDownloader.Classes.Entities;
using YuzuModDownloader.Classes.Updaters;
using YuzuModDownloader.Classes.Utilities;

namespace YuzuModDownloader;

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

        // download the mods 
        IModDownloader modDownloader = ModDownloaderFactory.Create(CboModRepos.SelectedIndex, _clientFactory!, ClearModDataLocationToolStripMenuItem.IsChecked.GetValueOrDefault(), DeleteDownloadedModArchivesToolStripMenuItem.IsChecked.GetValueOrDefault());
        modDownloader.UpdateProgress += ModDownloader_UpdateProgress;
        await modDownloader.DownloadPrerequisitesAsync();
        List<Game> games = await modDownloader.ReadGameTitlesDatabaseAsync();
        await modDownloader.DownloadModsAsync(games);

        // tell user mods have been downloaded 
        PbarProgress.ProgressTextFormat = "Done!";
        await new DownloadConfirmationWindow(games).ShowDialog(this);

        // reset ui
        ToggleUiControls(true);
    }

    //// ====================================================
    ////  MENUSTRIP CONTROLS 
    //// ====================================================
    
    public void ExitToolStripMenuItem_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);

    public void YuzuWebsiteToolStripMenuItem_Click(object sender, RoutedEventArgs e) => LaunchUrl("https://yuzu-emu.org/");

    public async void AboutToolStripMenuItem_ClickAsync(object sender, RoutedEventArgs e) => await new AboutWindow().ShowDialog(this);

    //// ====================================================
    ////  METHODS 
    //// ====================================================

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
                await ShowMessageBoxAsync("New version of Yuzu Mod Downloader is available.", MsBox.Avalonia.Enums.Icon.Info);
                LaunchUrl("https://github.com/amakvana/YuzuModDownloader");
                break;
            case AppUpdater.CurrentVersion.NotSupported:
                await ShowMessageBoxAsync("This version of Yuzu Mod Downloader is no longer supported.", MsBox.Avalonia.Enums.Icon.Error);
                LaunchUrl("https://github.com/amakvana/YuzuModDownloader");
                Environment.Exit(0);
                break;
            case AppUpdater.CurrentVersion.Undetectable:
                await ShowMessageBoxAsync("Network Connection Error! Please check your internet connection and try again.", MsBox.Avalonia.Enums.Icon.Error);
                Environment.Exit(1);
                break;
        }
    }

    private async Task ShowMessageBoxAsync(string message, Icon icon) => await MessageBoxManager.GetMessageBoxStandard(new()
    {
        ButtonDefinitions = ButtonEnum.Ok,
        ContentTitle = this.Title,
        ContentMessage = $"{message}{Environment.NewLine}{Environment.NewLine}" +
                             "Please download the latest version from https://github.com/amakvana/YuzuModDownloader",
        Icon = icon,
        WindowIcon = this.Icon,
        WindowStartupLocation = WindowStartupLocation.CenterOwner
    }).ShowWindowDialogAsync(this);

    private static void LaunchUrl(string url) => Process.Start(new ProcessStartInfo(url)
    {
        UseShellExecute = true,
        Verb = "Open"
    })?.Dispose();
} 