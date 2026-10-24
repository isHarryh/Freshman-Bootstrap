using System.Windows;
using FreshmanBootstrap.App.Presentation.Views;
using FreshmanBootstrap.Core;
using FreshmanBootstrap.Core.Models;
using Wpf.Ui.Controls;
using UiMessageBox = Wpf.Ui.Controls.MessageBox;
using UiMessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace FreshmanBootstrap.App.Presentation;

public sealed class DialogService
{
    public bool ConfirmPathRepair(IReadOnlyList<PathRepairPlan> plans)
    {
        var dialog = new PathRepairDialog(plans)
        {
            Owner = Application.Current.MainWindow,
        };

        return dialog.ShowDialog() == true;
    }

    public async Task<bool> ConfirmUpgradeAsync(UpgradeConfirmation confirmation)
    {
        var dialog = new UiMessageBox
        {
            Title = Strings.UpgradeDialog.Title,
            Content = string.Format(
                Strings.UpgradeDialog.MessageFormat,
                confirmation.SoftwareName,
                confirmation.CurrentVersion,
                confirmation.TargetVersion),
            PrimaryButtonText = Strings.UpgradeDialog.Confirm,
            CloseButtonText = Strings.Common.Cancel,
            Topmost = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
        };

        return await dialog.ShowDialogAsync() == UiMessageBoxResult.Primary;
    }

    public async Task<bool> ConfirmUninstallAsync()
    {
        var dialog = new UiMessageBox
        {
            Title = Strings.AboutPage.UninstallConfirmTitle,
            Content = Strings.AboutPage.UninstallConfirmMessage,
            PrimaryButtonText = Strings.AboutPage.Uninstall,
            CloseButtonText = Strings.Common.Cancel,
            PrimaryButtonAppearance = ControlAppearance.Danger,
            Topmost = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
        };

        return await dialog.ShowDialogAsync() == UiMessageBoxResult.Primary;
    }

    public async Task ShowErrorAsync(string message)
    {
        var dialog = new UiMessageBox
        {
            Title = AppInfo.DisplayName,
            Content = message,
            CloseButtonText = Strings.Common.Ok,
            Topmost = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
        };

        await dialog.ShowDialogAsync();
    }
}
