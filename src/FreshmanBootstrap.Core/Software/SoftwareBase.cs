using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

internal sealed class CandidateCollector
{
    private readonly HashSet<string> _paths = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Paths => _paths;

    public void Add(string executablePath) => _paths.Add(executablePath);
}

internal static class DetectionHelpers
{
    public static SoftwareInspection BuildInspection(IReadOnlyList<InstalledVersion> versions)
    {
        var ordered = versions
            .OrderByDescending(version => version.Version)
            .ThenBy(version => version.ExecutablePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new SoftwareInspection(ordered, ordered.FirstOrDefault());
    }
}

internal static class OptionValues
{
    public static string? GetPath(IReadOnlyList<IInstallOption> options, string id)
    {
        var value = options
            .OfType<PathInstallOption>()
            .FirstOrDefault(option => option.Id == id)
            ?.PathValue.Trim();

        return string.IsNullOrEmpty(value) ? null : value;
    }

    public static bool GetBool(IReadOnlyList<IInstallOption> options, string id, bool fallback)
    {
        return options
            .OfType<BoolInstallOption>()
            .FirstOrDefault(option => option.Id == id)
            ?.BoolValue ?? fallback;
    }
}

public abstract class SoftwareBase : ISoftware
{
    protected SoftwareBase(CoreServices services)
    {
        Services = services;
    }

    protected CoreServices Services { get; }

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public abstract string Description { get; }

    public string? PackagedVersion => Services.Payloads.GetVersion(Id);

    public abstract Task<SoftwareInspection> InspectAsync(CancellationToken cancellationToken = default);

    public abstract IReadOnlyList<IInstallOption> CreateInstallOptions();

    public abstract IReadOnlyList<IPostProcessOperation> CreatePostProcessOperations(SoftwareInspection inspection);

    public abstract Task<InstallResult> InstallAsync(
        IReadOnlyList<IInstallOption> options,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken = default);

    protected virtual bool AcceptsVersion(VersionNumber version) => true;

    protected async Task<InstallResult> RunInstallerWithDirectoryAsync(
        IReadOnlyList<IInstallOption> options,
        Func<string, string, InstallCommand> commandFactory,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken)
    {
        var installDirectory = OptionValues.GetPath(options, SoftwareOptionIds.InstallDirectory);
        if (installDirectory is null)
        {
            return InstallResult.Fail(Strings.InstallOptions.PathRequired);
        }

        return await RunInstallerAsync(
            installerPath => commandFactory(installerPath, installDirectory),
            progress,
            cancellationToken);
    }

    private protected async Task<IReadOnlyList<InstalledVersion>> ProbeCandidatesAsync(
        CandidateCollector collector,
        CancellationToken cancellationToken)
    {
        var installed = new List<InstalledVersion>();
        foreach (var executablePath in collector.Paths)
        {
            var version = await ProbeVersionAsync(executablePath, cancellationToken);
            if (version is not { IsValid: true } || !AcceptsVersion(version.Value))
            {
                continue;
            }

            installed.Add(new InstalledVersion(version.Value, executablePath));
        }

        return installed;
    }

    private protected void AddPathCandidates(CandidateCollector collector, params string[] executableNames)
    {
        foreach (var entry in Services.PathEnvironment.MachineEntries.Concat(Services.PathEnvironment.UserEntries))
        {
            var directory = Services.PathEnvironment.Expand(entry);
            if (PathListLogic.DirectoryEquals(directory, Services.WindowsAppsDirectory))
            {
                continue;
            }

            foreach (var executableName in executableNames)
            {
                var executablePath = Path.Combine(directory, executableName);
                if (Services.Files.FileExists(executablePath))
                {
                    collector.Add(executablePath);
                }
            }
        }
    }

    private protected void AddIfExists(CandidateCollector collector, string executablePath)
    {
        if (Services.Files.FileExists(executablePath))
        {
            collector.Add(executablePath);
        }
    }

    private async Task<VersionNumber?> ProbeVersionAsync(string executablePath, CancellationToken cancellationToken)
    {
        try
        {
            var result = await Services.ProcessRunner.RunAsync(
                executablePath,
                "--version",
                TimeSpan.FromSeconds(15),
                cancellationToken);

            return VersionNumber.TryParse(result.FirstOutputLine, out var version) ? version : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Services.Logger.Debug(exception, "Version probe failed for {Executable}", executablePath);
            return null;
        }
    }

    private async Task<InstallResult> RunInstallerAsync(
        Func<string, InstallCommand> commandFactory,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken)
    {
        var installerPath = Services.Payloads.GetInstallerPath(Id);
        if (installerPath is null)
        {
            return InstallResult.Fail(string.Format(Strings.EnvironmentPage.PayloadMissing, DisplayName));
        }

        if (!Services.Files.FileExists(installerPath))
        {
            return InstallResult.Fail(string.Format(Strings.EnvironmentPage.PackageFileMissing, installerPath));
        }

        var command = commandFactory(installerPath);
        progress?.Report(new InstallProgress(string.Format(Strings.EnvironmentPage.StageInstalling, DisplayName)));

        ProcessResult result;
        try
        {
            result = await Services.ProcessRunner.RunAsync(
                command.ExecutablePath,
                command.Arguments,
                TimeSpan.FromMinutes(30),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Services.Logger.Error(exception, "Installer execution failed for {Software}", Id);
            return InstallResult.Fail(exception.Message);
        }

        if (result.TimedOut)
        {
            return InstallResult.Fail(string.Format(
                Strings.EnvironmentPage.InstallFailed,
                DisplayName,
                Strings.EnvironmentPage.ProcessTimedOut));
        }

        if (!InstallerExitCodes.IsSuccess(result.ExitCode))
        {
            return InstallResult.Fail(string.Format(
                Strings.EnvironmentPage.InstallFailed,
                DisplayName,
                string.Format(Strings.EnvironmentPage.ProcessFailed, result.ExitCode, LastErrorLine(result))));
        }

        return InstallResult.Ok(result.ExitCode);
    }

    private static string LastErrorLine(ProcessResult result)
    {
        var text = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
        var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return lines.Length > 0 ? lines[^1] : string.Empty;
    }
}
