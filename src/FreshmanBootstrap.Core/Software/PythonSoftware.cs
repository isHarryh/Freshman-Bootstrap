using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

public sealed class PythonSoftware : SoftwareBase
{
    private const string PythonCoreKey = @"SOFTWARE\Python\PythonCore";

    public PythonSoftware(CoreServices services)
        : base(services)
    {
    }

    public override string Id => SoftwareIds.Python;

    public override string DisplayName => Strings.Software.PythonName;

    public override string Description => Strings.Software.PythonDescription;

    public override async Task<SoftwareInspection> InspectAsync(CancellationToken cancellationToken = default)
    {
        var collector = new CandidateCollector();
        AddPathCandidates(collector, "python.exe");

        foreach (var root in new[] { RegistryRoot.LocalMachine, RegistryRoot.CurrentUser })
        {
            foreach (var bitness in new[] { RegistryBitness.X64, RegistryBitness.X86 })
            {
                foreach (var subKey in Services.Registry.GetSubKeyNames(root, bitness, PythonCoreKey))
                {
                    if (!subKey.StartsWith("3.", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var installPath = Services.Registry.GetValue(root, bitness, $@"{PythonCoreKey}\{subKey}\InstallPath", null);
                    if (!string.IsNullOrWhiteSpace(installPath))
                    {
                        AddIfExists(collector, Path.Combine(installPath, "python.exe"));
                    }
                }
            }
        }

        foreach (var directory in Services.Files.EnumerateDirectories(Services.ProgramFilesDirectory, "Python3*"))
        {
            AddIfExists(collector, Path.Combine(directory, "python.exe"));
        }

        var userPrograms = Path.Combine(Services.LocalAppDataDirectory, "Programs");
        foreach (var directory in Services.Files.EnumerateDirectories(userPrograms, "Python*"))
        {
            AddIfExists(collector, Path.Combine(directory, "python.exe"));
            foreach (var nested in Services.Files.EnumerateDirectories(directory, "Python3*"))
            {
                AddIfExists(collector, Path.Combine(nested, "python.exe"));
            }
        }

        var versions = await ProbeCandidatesAsync(collector, cancellationToken);
        return DetectionHelpers.BuildInspection(versions);
    }

    protected override bool AcceptsVersion(VersionNumber version)
    {
        return version.Parts.Count > 0 && version.Parts[0] == 3;
    }

    public override IReadOnlyList<IInstallOption> CreateInstallOptions()
    {
        return new IInstallOption[]
        {
            new PathInstallOption(
                SoftwareOptionIds.InstallDirectory,
                Strings.InstallOptions.InstallDirectory,
                GetRecommendedDirectory()),
            new BoolInstallOption(
                SoftwareOptionIds.PythonLauncher,
                Strings.InstallOptions.PythonLauncher,
                true),
        };
    }

    public override IReadOnlyList<IPostProcessOperation> CreatePostProcessOperations(SoftwareInspection inspection)
    {
        if (inspection.Best is not { } best)
        {
            return Array.Empty<IPostProcessOperation>();
        }

        var directories = new List<string> { best.Directory };
        var scriptsDirectory = Path.Combine(best.Directory, "Scripts");
        if (Services.Files.DirectoryExists(scriptsDirectory))
        {
            directories.Add(scriptsDirectory);
        }

        return new IPostProcessOperation[]
        {
            new EnsurePathEntriesOperation(
                Services,
                "python.path.add",
                Strings.PostProcess.PythonAddPath,
                directories),
            new WindowsAppsPriorityOperation(
                Services,
                "python.path.windowsapps",
                Strings.PostProcess.PythonWindowsApps,
                directories),
            new EnsureExecutableFirstOperation(
                Services,
                "python.path.first",
                Strings.PostProcess.PythonFirst,
                best.Directory,
                new[] { "python.exe" }),
        };
    }

    public override Task<InstallResult> InstallAsync(
        IReadOnlyList<IInstallOption> options,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        var launcher = OptionValues.GetBool(options, SoftwareOptionIds.PythonLauncher, true);

        return RunInstallerWithDirectoryAsync(
            options,
            (installerPath, installDirectory) => InstallCommands.Python(installerPath, installDirectory, launcher),
            progress,
            cancellationToken);
    }

    private string GetRecommendedDirectory()
    {
        var packagedVersion = PackagedVersion;
        if (VersionNumber.TryParse(packagedVersion, out var version) && version.Parts.Count >= 2)
        {
            return Path.Combine(Services.ProgramFilesDirectory, $"Python{version.Parts[0]}{version.Parts[1]}");
        }

        return Path.Combine(Services.ProgramFilesDirectory, "Python3");
    }
}
