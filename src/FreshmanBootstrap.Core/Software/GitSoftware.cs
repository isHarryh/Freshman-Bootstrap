using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

public sealed class GitSoftware : SoftwareBase
{
    public GitSoftware(CoreServices services)
        : base(services)
    {
    }

    public override string Id => SoftwareIds.Git;

    public override string DisplayName => Strings.Software.GitName;

    public override string Description => Strings.Software.GitDescription;

    public override async Task<SoftwareInspection> InspectAsync(CancellationToken cancellationToken = default)
    {
        var collector = new CandidateCollector();
        AddPathCandidates(collector, "git.exe");

        AddIfExists(collector, Path.Combine(Services.ProgramFilesDirectory, "Git", "cmd", "git.exe"));
        AddIfExists(collector, Path.Combine(Services.ProgramFilesX86Directory, "Git", "cmd", "git.exe"));
        AddIfExists(collector, Path.Combine(Services.LocalAppDataDirectory, "Programs", "Git", "cmd", "git.exe"));

        foreach (var entry in Services.Registry.FindUninstallEntries("Git"))
        {
            if (!string.IsNullOrWhiteSpace(entry.InstallLocation))
            {
                AddIfExists(collector, Path.Combine(entry.InstallLocation, "cmd", "git.exe"));
            }
        }

        var versions = await ProbeCandidatesAsync(collector, cancellationToken);
        return DetectionHelpers.BuildInspection(versions);
    }

    public override IReadOnlyList<IInstallOption> CreateInstallOptions()
    {
        return new IInstallOption[]
        {
            new PathInstallOption(
                SoftwareOptionIds.InstallDirectory,
                Strings.InstallOptions.InstallDirectory,
                Path.Combine(Services.ProgramFilesDirectory, "Git")),
        };
    }

    public override IReadOnlyList<IPostProcessOperation> CreatePostProcessOperations(SoftwareInspection inspection)
    {
        if (inspection.Best is not { } best)
        {
            return Array.Empty<IPostProcessOperation>();
        }

        return new IPostProcessOperation[]
        {
            new EnsureExecutableFirstOperation(
                Services,
                "git.path.first",
                Strings.PostProcess.GitFirst,
                best.Directory,
                new[] { "git.exe" }),
        };
    }

    public override Task<InstallResult> InstallAsync(
        IReadOnlyList<IInstallOption> options,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        return RunInstallerWithDirectoryAsync(options, InstallCommands.Git, progress, cancellationToken);
    }
}
