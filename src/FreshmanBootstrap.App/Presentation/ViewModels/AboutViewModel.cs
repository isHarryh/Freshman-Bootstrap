using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshmanBootstrap.Core;

namespace FreshmanBootstrap.App.Presentation.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly Func<Task<bool>> _confirmUninstall;
    private readonly Func<Task> _uninstall;

    public AboutViewModel(Func<Task<bool>> confirmUninstall, Func<Task> uninstall, bool canUninstall)
    {
        _confirmUninstall = confirmUninstall;
        _uninstall = uninstall;
        CanUninstall = canUninstall;
    }

    public string Title => AppInfo.EnglishName;

    public string Subtitle => AppInfo.DisplayName;

    public string VersionText => string.Format(Strings.AboutPage.VersionLabel, AppInfo.VersionText);

    public string CopyrightText => AppInfo.CopyrightText;

    public string GitHubUrl => AppInfo.GitHubUrl;

    public string GitHubDisplayText => string.Format(Strings.AboutPage.GitHubLabel, AppInfo.GitHubUrl);

    public string UninstallText => Strings.AboutPage.Uninstall;

    public string UninstallHint => Strings.AboutPage.UninstallHint;

    public bool CanUninstall { get; }

    [RelayCommand(CanExecute = nameof(CanUninstall))]
    private async Task UninstallAsync()
    {
        if (await _confirmUninstall())
        {
            await _uninstall();
        }
    }
}
