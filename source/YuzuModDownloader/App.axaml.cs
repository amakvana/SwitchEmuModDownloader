using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace YuzuModDownloader;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // prepare HttpClient for Dependency injection throughout application 
            var services = new ServiceCollection();
            services.AddHttpClient();

            services.AddHttpClient("GitHub-YuzuModDownloader", client =>
            {
                client.BaseAddress = new Uri("https://raw.githubusercontent.com/amakvana/YuzuModDownloader/main/");
                client.DefaultRequestHeaders.Add("accept", "application/vnd.github.raw");
                client.DefaultRequestHeaders.Add("user-agent", "request");
                //client.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient("GitHub-OfficialSwitchMods", client =>
            {
                client.BaseAddress = new Uri("https://github.com/yuzu-emu/yuzu/wiki/Switch-Mods/");
                client.DefaultRequestHeaders.Add("user-agent", "request");
                //client.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient("GitHub-Api", client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("accept", "application/vnd.github+json");
                client.DefaultRequestHeaders.Add("user-agent", "request");
                //client.Timeout = TimeSpan.FromMinutes(5);
            });

            var serviceProvider = services.BuildServiceProvider();
            desktop.MainWindow = new MainWindow(serviceProvider);
        }

        base.OnFrameworkInitializationCompleted();
    }
}