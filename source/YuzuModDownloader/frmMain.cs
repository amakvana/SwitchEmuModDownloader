using System;
using System.Diagnostics;
using System.IO;
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
            if (!AppUpdater.IsLatestVersion())
            {
                MessageBox.Show("New version of Yuzu Mod Downloader available, please download from https://github.com/amakvana/YuzuModDownloader", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start("https://github.com/amakvana/YuzuModDownloader");
                Application.Exit();
            }

            // set UI defaults 
            lblProgress.Text = "";
            toolTip1.SetToolTip(chkClearModDataLocation, "Check this to delete all existing mods before downloading the latest switch mods");
            toolTip1.SetToolTip(btnDownload, "Download Yuzu Game Mods for current switch games dumped");
            directoryTextBox.Text = Directory.Exists("user") ?
                Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "user") :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu");
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if(!ValidateDirectory(directoryTextBox.Text)) return;

            // disable form controls
            ToggleControls(false);

            // get prerequisites
            string gameTitleIDsXml = "GameTitleIDs.xml";
            var modDownloader = new ModDownloader(directoryTextBox.Text);
            modDownloader.UpdateProgress += ModDownloader_UpdateProgress;
            await modDownloader.DownloadPrerequisitesAsync();
            await modDownloader.DownloadGameTitleIdDatabaseAsync(gameTitleIDsXml);
            var gameTitleNameIDs = modDownloader.ReadGameTitleIdDatabase(gameTitleIDsXml);

            // download mods for each game 
            int totalGames = 0;
            foreach (var g in gameTitleNameIDs)
            {
                string titleName = g.Key;
                string titleId = g.Value;
                await modDownloader.DownloadTitleModsAsync(titleName, titleId, "https://github.com/yuzu-emu/yuzu/wiki/Switch-Mods", chkClearModDataLocation.Checked);
                totalGames++;
            }

            // enable form controls
            MessageBox.Show($"Done! Mods downloaded for {totalGames} games.{Environment.NewLine}{Environment.NewLine}" +
                            $"To toggle specific game mods On/Off:{Environment.NewLine}" +
                            $"Run yuzu > Right-Click on a game > Properties > Add-Ons", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void ToggleControls(bool value)
        {
            btnDownload.Enabled = value;
            chkClearModDataLocation.Enabled = value;
            directoryTextBox.Enabled = value;
            directoryBrowseButton.Enabled = value;
        }

        private bool ValidateDirectory(string dir)
        {
            try
            {
                Path.GetFullPath(dir);
                return true;
            } catch (ArgumentException _)
            {
                MessageBox.Show($"The directory {dir} is invalid", "Invalid directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void directoryBrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = directoryTextBox.Text;
            folderBrowserDialog1.ShowDialog();
            directoryTextBox.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}