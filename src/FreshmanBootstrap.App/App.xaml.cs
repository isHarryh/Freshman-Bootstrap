using System.IO;
using System.Windows;
using FreshmanBootstrap.Core;
using FreshmanBootstrap.Core.Services;
using Serilog;

namespace FreshmanBootstrap.App;

public partial class App : Application
{
    public AppHost? Host { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ConfigureLogging();
        Log.Information(
            "{App} v{Version} starting with args: {Args}",
            AppInfo.EnglishName,
            AppInfo.VersionText,
            string.Join(' ', e.Args));

        try
        {
            Host = AppHost.Create();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Failed to initialize the application host");
            await new Wpf.Ui.Controls.MessageBox
            {
                Title = AppInfo.DisplayName,
                Content = exception.Message,
                CloseButtonText = Strings.Common.Ok,
            }.ShowDialogAsync();
            Shutdown(1);
            return;
        }

        if (e.Args.Any(argument => string.Equals(argument, AppInfo.PrepareArgument, StringComparison.OrdinalIgnoreCase)))
        {
            var exitCode = 1;
            try
            {
                var fresh = e.Args.Any(argument => string.Equals(argument, AppInfo.FreshArgument, StringComparison.OrdinalIgnoreCase));
                exitCode = await Host.RunPrepareAsync(fresh);
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Prepare failed");
            }

            Shutdown(exitCode);
            return;
        }

        var window = new MainWindow(Host);
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Exiting with code {ExitCode}", e.ApplicationExitCode);
        Host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static void ConfigureLogging()
    {
        Directory.CreateDirectory(AppPaths.LogDirectory);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(AppPaths.LogDirectory, "freshman-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)
            .CreateLogger();
    }
}
