using System.IO;
using System.Windows;
using FreshmanBootstrap.App.Presentation;
using FreshmanBootstrap.App.Presentation.ViewModels;
using FreshmanBootstrap.App.Presentation.Views;
using FreshmanBootstrap.App.Services;
using FreshmanBootstrap.Core;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace FreshmanBootstrap.App;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow(AppHost host)
    {
        InitializeComponent();

        SystemThemeWatcher.Watch(this);

        var dialogService = new DialogService();
        var uninstallerPath = UninstallLauncher.FindUninstaller(Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory);

        var about = new AboutViewModel(
            confirmUninstall: dialogService.ConfirmUninstallAsync,
            uninstall: () => RunUninstallerAsync(uninstallerPath!, dialogService),
            canUninstall: uninstallerPath is not null);

        _viewModel = new MainViewModel(host.Software, dialogService.ConfirmPathRepair, dialogService.ConfirmUpgradeAsync, about);
        DataContext = _viewModel;

        EnvironmentNavigationItem.Content = Strings.MainWindow.EnvironmentPageName;
        AboutNavigationItem.Content = Strings.MainWindow.AboutPageName;

        RootNavigation.Navigated += (_, args) =>
        {
            if (args.Page is FrameworkElement page)
            {
                page.DataContext = _viewModel;
            }
        };

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        RootNavigation.Navigate(typeof(EnvironmentView));

        await _viewModel.RefreshAllCommand.ExecuteAsync(null);
    }

    private static async Task RunUninstallerAsync(string uninstallerPath, DialogService dialogService)
    {
        if (!UninstallLauncher.TryLaunch(uninstallerPath, out var error))
        {
            await dialogService.ShowErrorAsync(string.Format(Strings.AboutPage.UninstallFailed, error ?? string.Empty));
            return;
        }

        Application.Current.Shutdown();
    }
}
