using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

public sealed class VSCodeSoftware : SoftwareBase
{
    private static readonly string[] CommandFileNames = { "code.cmd", "code.exe" };

    public VSCodeSoftware(CoreServices services)
        : base(services)
    {
    }

    public override string Id => SoftwareIds.VSCode;

    public override string DisplayName => Strings.Software.VSCodeName;

    public override string Description => Strings.Software.VSCodeDescription;

    public override async Task<SoftwareInspection> InspectAsync(CancellationToken cancellationToken = default)
    {
        var collector = new CandidateCollector();
        AddPathCandidates(collector, "code.cmd");

        AddIfExists(
            collector,
            Path.Combine(Services.ProgramFilesDirectory, "Microsoft VS Code", "bin", "code.cmd"));
        AddIfExists(
            collector,
            Path.Combine(Services.LocalAppDataDirectory, "Programs", "Microsoft VS Code", "bin", "code.cmd"));

        foreach (var entry in Services.Registry.FindUninstallEntries("Visual Studio Code"))
        {
            if (entry.DisplayName.Contains("Insiders", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(entry.InstallLocation))
            {
                AddIfExists(collector, Path.Combine(entry.InstallLocation, "bin", "code.cmd"));
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
                Path.Combine(Services.ProgramFilesDirectory, "Microsoft VS Code")),
            new BoolInstallOption(
                SoftwareOptionIds.DesktopIcon,
                Strings.InstallOptions.DesktopIcon,
                true),
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
                "vscode.path.first",
                Strings.PostProcess.VSCodeFirst,
                best.Directory,
                CommandFileNames),
        };
    }

    public override Task<InstallResult> InstallAsync(
        IReadOnlyList<IInstallOption> options,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        var desktopIcon = OptionValues.GetBool(options, SoftwareOptionIds.DesktopIcon, true);

        return RunInstallerWithDirectoryAsync(
            options,
            (installerPath, installDirectory) => InstallCommands.VSCode(installerPath, installDirectory, desktopIcon),
            progress,
            cancellationToken);
    }
}
