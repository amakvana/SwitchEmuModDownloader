using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace YuzuModDownloader
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // check if software is latest version 
            CheckAppVersion();

            // set UI defaults 
            cboModRepos.SelectedIndex = 0;
            lblProgress.Text = "";
            clearModDataLocationToolStripMenuItem.ToolTipText = "Deletes all existing mods before downloading the latest Yuzu Game Mods";
            deleteDownloadedModArchivesToolStripMenuItem.ToolTipText = "Deletes all downloaded mod archive files once unpacked";
            toolTip1.SetToolTip(btnDownload, "Download Yuzu Game Mods for current switch games dumped");
            toolTip1.SetToolTip(cboModRepos, "Available repositories to download Yuzu Mods");
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            // disable form controls
            ToggleControls(false);

            int totalGames = 0;
            var games = new List<Game>();
            switch (cboModRepos.SelectedIndex)
            {
                case 0:     // official switch-mods repo
                    var oymDownloader = new OfficialYuzuModDownloader
                    {
                        IsModDataLocationToBeDeleted = clearModDataLocationToolStripMenuItem.Checked,
                        IsDownloadedModArchivesToBeDeleted = deleteDownloadedModArchivesToolStripMenuItem.Checked
                    };
                    oymDownloader.UpdateProgress += ModDownloader_UpdateProgress;
                    await oymDownloader.DownloadPrerequisitesAsync();
                    games = await oymDownloader.ReadGameTitlesDatabaseAsync();
                    await oymDownloader.DownloadModsAsync(games);
                    totalGames = games.Where(g => g.ModDownloadUrls.Count > 0).Count();
                    break;

                case 1:     // theboy181 repo
                    var tb181mDownloader = new TheBoy181ModDownloader
                    {
                        IsModDataLocationToBeDeleted = clearModDataLocationToolStripMenuItem.Checked,
                        IsDownloadedModArchivesToBeDeleted = deleteDownloadedModArchivesToolStripMenuItem.Checked
                    };
                    tb181mDownloader.UpdateProgress += ModDownloader_UpdateProgress;
                    await tb181mDownloader.DownloadPrerequisitesAsync();
                    games = await tb181mDownloader.ReadGameTitlesDatabaseAsync();
                    await tb181mDownloader.DownloadModsAsync(games);
                    totalGames = games.Where(g => g.ModDownloadUrls.Count > 0).Count();
                    break;

                default: break;  // do nothing 
            }

            // tell user mods have been downloaded 
            MessageBox.Show($"Done! Mods downloaded for {totalGames} games.{Environment.NewLine}{Environment.NewLine}" +
                            $"To toggle specific game mods On/Off:{Environment.NewLine}" +
                            $"Run yuzu > Right-Click on a game > Properties > Add-Ons", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            // reset ui
            ToggleControls(true);
        }

        private void ModDownloader_UpdateProgress(int progressPercentage, string progressText)
        {
            pbarProgress.Value = progressPercentage;
            pbarProgress.Refresh();
            lblProgress.Text = progressText;
            lblProgress.Refresh();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void YuzuWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://yuzu-emu.org/");
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new frmAbout())
            {
                f.ShowDialog();
            }
        }

        private void CheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckAppVersion(true);
        }

        private void ToggleControls(bool value)
        {
            cboModRepos.Enabled = value;
            btnDownload.Enabled = value;
            optionsToolStripMenuItem.Enabled = value;
            lblProgress.Text = "";
            pbarProgress.Value = 0;
        }

        private void CheckAppVersion(bool manualCheck = false)
        {
            var currentAppVersion = AppUpdater.CheckVersion();
            switch (currentAppVersion)
            {
                case AppUpdater.CurrentVersion.LatestVersion when manualCheck:
                    MessageBox.Show("You currently have the latest version of Yuzu Mod Downloader", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case AppUpdater.CurrentVersion.UpdateAvailable:
                    MessageBox.Show("New version of Yuzu Mod Downloader available, please download from https://github.com/amakvana/YuzuModDownloader", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Process.Start("https://github.com/amakvana/YuzuModDownloader");
                    break;
                case AppUpdater.CurrentVersion.NotSupported:
                    MessageBox.Show("This version of Yuzu Mod Downloader is no longer supported, please download the latest version from https://github.com/amakvana/YuzuModDownloader", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Process.Start("https://github.com/amakvana/YuzuModDownloader");
                    Application.Exit();
                    break;
            }
        }
    }
}