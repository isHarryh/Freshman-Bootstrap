using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshmanBootstrap.Core;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Software;

namespace FreshmanBootstrap.App.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(
        SoftwareCatalog catalog,
        Func<IReadOnlyList<PathRepairPlan>, bool> confirmPathRepair,
        Func<UpgradeConfirmation, Task<bool>> confirmUpgrade,
        AboutViewModel about)
    {
        About = about;
        Items = new ObservableCollection<SoftwareItemViewModel>(
            catalog.All.Select(software => new SoftwareItemViewModel(software, confirmPathRepair, confirmUpgrade)));
    }

    public ObservableCollection<SoftwareItemViewModel> Items { get; }

    public AboutViewModel About { get; }

    public string WindowTitle => Strings.Common.AppTitle;

    public string RefreshText => Strings.EnvironmentPage.Refresh;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool CanRefresh => !IsBusy;

    [RelayCommand]
    private async Task RefreshAllAsync()
    {
        IsBusy = true;
        StatusMessage = Strings.Common.Loading;

        try
        {
            foreach (var item in Items)
            {
                await item.RefreshAsync();
            }

            StatusMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanRefresh));
    }
}
