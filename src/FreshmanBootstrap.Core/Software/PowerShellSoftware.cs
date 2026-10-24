using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

public sealed class PowerShellSoftware : SoftwareBase
{
    private const string InstalledVersionsKey = @"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions";

    public PowerShellSoftware(CoreServices services)
        : base(services)
    {
    }

    public override string Id => SoftwareIds.PowerShell;

    public override string DisplayName => Strings.Software.PowerShellName;

    public override string Description => Strings.Software.PowerShellDescription;

    public override async Task<SoftwareInspection> InspectAsync(CancellationToken cancellationToken = default)
    {
        var collector = new CandidateCollector();
        AddPathCandidates(collector, "pwsh.exe");

        AddIfExists(
            collector,
            Path.Combine(Services.ProgramFilesDirectory, "PowerShell", "7", "pwsh.exe"));
        AddIfExists(
            collector,
            Path.Combine(Services.ProgramFilesDirectory, "PowerShell", "7-preview", "pwsh.exe"));

        foreach (var root in new[] { RegistryRoot.LocalMachine, RegistryRoot.CurrentUser })
        {
            foreach (var bitness in new[] { RegistryBitness.X64, RegistryBitness.X86 })
            {
                foreach (var subKey in Services.Registry.GetSubKeyNames(root, bitness, InstalledVersionsKey))
                {
                    var installLocation = Services.Registry.GetValue(
                        root,
                        bitness,
                        $@"{InstalledVersionsKey}\{subKey}",
                        "InstallLocation");

                    if (!string.IsNullOrWhiteSpace(installLocation))
                    {
                        AddIfExists(collector, Path.Combine(installLocation, "pwsh.exe"));
                    }
                }
            }
        }

        var versions = await ProbeCandidatesAsync(collector, cancellationToken);
        return DetectionHelpers.BuildInspection(versions);
    }

    protected override bool AcceptsVersion(VersionNumber version)
    {
        return version.Parts.Count > 0 && version.Parts[0] >= 7;
    }

    public override IReadOnlyList<IInstallOption> CreateInstallOptions()
    {
        return new IInstallOption[]
        {
            new PathInstallOption(
                SoftwareOptionIds.InstallDirectory,
                Strings.InstallOptions.InstallDirectory,
                Path.Combine(Services.ProgramFilesDirectory, "PowerShell", "7")),
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
                "powershell.path.first",
                Strings.PostProcess.PowerShellFirst,
                best.Directory,
                new[] { "pwsh.exe" }),
        };
    }

    public override Task<InstallResult> InstallAsync(
        IReadOnlyList<IInstallOption> options,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        return RunInstallerWithDirectoryAsync(options, InstallCommands.PowerShell, progress, cancellationToken);
    }
}
