using Avalonia.Controls;
using Avalonia.Interactivity;
using SwitchEmuModDownloader.Classes.Entities;

namespace SwitchEmuModDownloader;

public partial class DownloadConfirmationWindow : Window
{
    public DownloadConfirmationWindow() : this([]) { }

    public DownloadConfirmationWindow(List<Game> games)
    {
        InitializeComponent();

        // set UI defaults 
        BtnToggleDetails.Content = "Show Details";
        LblModsDetected.IsVisible = false;
        TxtModsDetected.IsVisible = false;

        // if no games, disable details button
        var gamesWithMods = games.Where(g => g.ModDownloadUrls.Count != 0).ToList();
        if (gamesWithMods.Count == 0)
        {
            BtnToggleDetails.IsEnabled = false;
            return;
        }

        // otherwise, populate the form with the details 
        LblDoneTotal.Content = LblDoneTotal.Content!.ToString()!.Replace("0", gamesWithMods.Count.ToString());
        TxtModsDetected.Text = string.Join(Environment.NewLine, gamesWithMods.Select(g => g.TitleName));
    }

    public void BtnOK_Click(object sender, RoutedEventArgs e) => Close();

    public void BtnToggleDetails_Click(object sender, RoutedEventArgs e)
    {
        // change size of the form depending what button text displays 
        LblModsDetected.IsVisible = !LblModsDetected.IsVisible;
        TxtModsDetected.IsVisible = !TxtModsDetected.IsVisible;
        BtnToggleDetails.Content = BtnToggleDetails.Content switch
        {
            "Show Details" => "Hide Details",
            _ => "Show Details"
        };
    }
}
