using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshmanBootstrap.Core;
using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.App.Presentation.ViewModels;

public partial class SoftwareItemViewModel : ObservableObject
{
    private readonly ISoftware _software;
    private readonly Func<IReadOnlyList<PathRepairPlan>, bool> _confirmPathRepair;
    private SoftwareInspection _inspection = SoftwareInspection.Empty;
    private IReadOnlyList<IInstallOption> _installOptions = Array.Empty<IInstallOption>();

    public SoftwareItemViewModel(ISoftware software, Func<IReadOnlyList<PathRepairPlan>, bool> confirmPathRepair)
    {
        _software = software;
        _confirmPathRepair = confirmPathRepair;
    }

    public string Name => _software.DisplayName;

    public string Description => _software.Description;

    public string PackagedVersionText => _software.PackagedVersion ?? Strings.EnvironmentPage.VersionPlaceholder;

    public string InstalledVersionsHeader => Strings.EnvironmentPage.InstalledVersionsHeader;

    public string PostProcessHeader => Strings.EnvironmentPage.PostProcessHeader;

    public ObservableCollection<PostProcessItemViewModel> PostProcessItems { get; } = new();

    public ObservableCollection<string> InstalledVersionLines { get; } = new();

    [ObservableProperty]
    private string _installedVersionText = Strings.EnvironmentPage.VersionPlaceholder;

    [ObservableProperty]
    private Visibility _installedVersionsVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _postProcessVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private string _statusText = Strings.EnvironmentPage.Unknown;

    [ObservableProperty]
    private string _actionText = Strings.EnvironmentPage.Install;

    [ObservableProperty]
    private SoftwareStatus _status = SoftwareStatus.Unknown;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private Visibility _errorVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _busyVisibility = Visibility.Collapsed;

    public bool CanAct =>
        !IsBusy && Status is SoftwareStatus.NotInstalled or SoftwareStatus.NeedsRepair or SoftwareStatus.Failed;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        SetError(null);
        IsBusy = true;
        Status = SoftwareStatus.Working;
        StatusText = Strings.EnvironmentPage.Working;

        try
        {
            _inspection = await _software.InspectAsync(cancellationToken);
            _installOptions = _software.CreateInstallOptions();
            UpdateInstalledVersions();
            await UpdatePostProcessAsync(cancellationToken);
            UpdateStatus();
        }
        catch (Exception exception)
        {
            SetError(string.Format(Strings.EnvironmentPage.DetectionFailed, Name, exception.Message));
            Status = SoftwareStatus.Failed;
            StatusText = Strings.EnvironmentPage.Failed;
            ActionText = Strings.EnvironmentPage.Retry;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task InstallAsync()
    {
        SetError(null);
        IsBusy = true;
        Status = SoftwareStatus.Working;
        StatusText = Strings.EnvironmentPage.Working;

        try
        {
            var progress = new Progress<InstallProgress>(report => StatusText = report.Stage);
            var result = await _software.InstallAsync(_installOptions, progress);
            if (!result.Success)
            {
                SetFailure(result.ErrorMessage);
                return;
            }
        }
        catch (Exception exception)
        {
            SetFailure(exception.Message);
            return;
        }

        IsBusy = false;
        await RefreshAsync();
    }

    public async Task RepairAsync()
    {
        SetError(null);
        IsBusy = true;
        StatusText = Strings.EnvironmentPage.Working;

        try
        {
            var operations = _software.CreatePostProcessOperations(_inspection);
            var unsatisfied = new List<IPathRepairOperation>();
            var previewPlans = new List<PathRepairPlan>();

            foreach (var operation in operations)
            {
                if (await operation.IsSatisfiedAsync())
                {
                    continue;
                }

                if (operation is not IPathRepairOperation pathOperation)
                {
                    continue;
                }

                var plan = await pathOperation.PlanAsync();
                if (plan is null)
                {
                    continue;
                }

                unsatisfied.Add(pathOperation);
                previewPlans.Add(plan);
            }

            if (previewPlans.Count > 0 && !_confirmPathRepair(previewPlans))
            {
                IsBusy = false;
                await RefreshAsync();
                return;
            }

            foreach (var operation in unsatisfied)
            {
                var plan = await operation.PlanAsync();
                if (plan is not null)
                {
                    await operation.ExecuteAsync(plan);
                }
            }
        }
        catch (Exception exception)
        {
            SetError(string.Format(Strings.EnvironmentPage.RepairFailed, Name, exception.Message));
            Status = SoftwareStatus.Failed;
            StatusText = Strings.EnvironmentPage.Failed;
            ActionText = Strings.EnvironmentPage.Retry;
            IsBusy = false;
            return;
        }

        IsBusy = false;
        await RefreshAsync();
    }

    [RelayCommand]
    private Task PrimaryActionAsync()
    {
        return Status switch
        {
            SoftwareStatus.NotInstalled => InstallAsync(),
            SoftwareStatus.NeedsRepair => RepairAsync(),
            SoftwareStatus.Failed => RefreshAsync(),
            _ => Task.CompletedTask,
        };
    }

    partial void OnIsBusyChanged(bool value)
    {
        BusyVisibility = value ? Visibility.Visible : Visibility.Collapsed;
        OnPropertyChanged(nameof(CanAct));
    }

    partial void OnStatusChanged(SoftwareStatus value)
    {
        OnPropertyChanged(nameof(CanAct));
    }

    private void UpdateInstalledVersions()
    {
        InstalledVersionLines.Clear();
        foreach (var version in _inspection.InstalledVersions)
        {
            InstalledVersionLines.Add(string.Format(
                Strings.EnvironmentPage.InstalledVersionLineFormat,
                version.Version,
                version.ExecutablePath));
        }

        InstalledVersionsVisibility = InstalledVersionLines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        InstalledVersionText = _inspection.Best?.Version.ToString() ?? Strings.EnvironmentPage.VersionPlaceholder;
    }

    private async Task UpdatePostProcessAsync(CancellationToken cancellationToken)
    {
        PostProcessItems.Clear();
        var operations = _software.CreatePostProcessOperations(_inspection);
        foreach (var operation in operations)
        {
            var satisfied = await operation.IsSatisfiedAsync(cancellationToken);
            PostProcessItems.Add(new PostProcessItemViewModel(operation.Description, satisfied));
        }

        PostProcessVisibility = PostProcessItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateStatus()
    {
        if (!_inspection.IsInstalled)
        {
            Status = SoftwareStatus.NotInstalled;
            StatusText = Strings.EnvironmentPage.NotInstalled;
            ActionText = Strings.EnvironmentPage.Install;
            return;
        }

        if (PostProcessItems.All(item => item.IsChecked))
        {
            Status = SoftwareStatus.Installed;
            StatusText = Strings.EnvironmentPage.Installed;
            ActionText = Strings.EnvironmentPage.NoAction;
            return;
        }

        Status = SoftwareStatus.NeedsRepair;
        StatusText = Strings.EnvironmentPage.NeedsRepair;
        ActionText = Strings.EnvironmentPage.Repair;
    }

    private void SetFailure(string? message)
    {
        SetError(message);
        Status = SoftwareStatus.Failed;
        StatusText = Strings.EnvironmentPage.Failed;
        ActionText = Strings.EnvironmentPage.Retry;
        IsBusy = false;
    }

    private void SetError(string? message)
    {
        ErrorMessage = message ?? string.Empty;
        ErrorVisibility = string.IsNullOrWhiteSpace(message) ? Visibility.Collapsed : Visibility.Visible;
    }
}
