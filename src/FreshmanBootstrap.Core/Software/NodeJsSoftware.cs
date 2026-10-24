using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

public sealed class NodeJsSoftware : SoftwareBase
{
    public NodeJsSoftware(CoreServices services)
        : base(services)
    {
    }

    public override string Id => SoftwareIds.NodeJs;

    public override string DisplayName => Strings.Software.NodeJsName;

    public override string Description => Strings.Software.NodeJsDescription;

    public override async Task<SoftwareInspection> InspectAsync(CancellationToken cancellationToken = default)
    {
        var collector = new CandidateCollector();
        AddPathCandidates(collector, "node.exe");

        AddIfExists(
            collector,
            Path.Combine(Services.ProgramFilesDirectory, "nodejs", "node.exe"));

        foreach (var entry in Services.Registry.FindUninstallEntries("Node.js"))
        {
            if (!string.IsNullOrWhiteSpace(entry.InstallLocation))
            {
                AddIfExists(collector, Path.Combine(entry.InstallLocation, "node.exe"));
            }
        }

        var versions = await ProbeCandidatesAsync(collector, cancellationToken);
        return DetectionHelpers.BuildInspection(versions);
    }

    protected override bool AcceptsVersion(VersionNumber version)
    {
        return version.Parts.Count > 0 && version.Parts[0] >= 22;
    }

    public override IReadOnlyList<IInstallOption> CreateInstallOptions()
    {
        return new IInstallOption[]
        {
            new PathInstallOption(
                SoftwareOptionIds.InstallDirectory,
                Strings.InstallOptions.InstallDirectory,
                Path.Combine(Services.ProgramFilesDirectory, "nodejs")),
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
                "nodejs.path.first",
                Strings.PostProcess.NodeJsFirst,
                best.Directory,
                new[] { "node.exe" }),
        };
    }

    public override Task<InstallResult> InstallAsync(
        IReadOnlyList<IInstallOption> options,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        return RunInstallerWithDirectoryAsync(options, InstallCommands.NodeJs, progress, cancellationToken);
    }
}
