using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Software;

namespace FreshmanBootstrap.Tests;

public class SoftwareDetectionTests
{
    [Fact]
    public async Task Git_DetectsPathAndRegistryCandidates()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\Git\cmd")
            .WithProgram(@"X:\Git\cmd\git.exe", "git version 2.55.0.windows.5");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.ProcessRunner.SetOutput(@"X:\OldGit\cmd\git.exe", "git version 2.50.0.windows.1");
        environment.Registry.UninstallEntries.Add(new UninstallEntry(
            "Git For Windows",
            "2.50.0",
            @"X:\OldGit",
            RegistryRoot.LocalMachine,
            RegistryBitness.X64));

        var software = new GitSoftware(environment.Services);
        var inspection = await software.InspectAsync();

        Assert.Equal(2, inspection.InstalledVersions.Count);
        Assert.Equal("2.55.0", inspection.Best!.Version.ToString());
        Assert.Equal(@"X:\Git\cmd", inspection.Best.Directory);
    }

    [Fact]
    public async Task Python_IgnoresPython2_AndBuildsPostProcesses()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\Python27", @"X:\Tools")
            .WithProgram(@"X:\Python27\python.exe", "Python 2.7.18");
        environment.Files.AddDirectory(@"X:\Program Files\Python313");
        environment.Files.AddDirectory(@"X:\Program Files\Python313\Scripts");
        environment.WithProgram(@"X:\Program Files\Python313\python.exe", "Python 3.13.15");
        environment.WithProgram(@"X:\Python312\python.exe", "Python 3.12.14");
        environment.Registry.AddSubKey(
            RegistryRoot.LocalMachine,
            RegistryBitness.X64,
            @"SOFTWARE\Python\PythonCore",
            "3.12");
        environment.Registry.SetValue(
            RegistryRoot.LocalMachine,
            RegistryBitness.X64,
            @"SOFTWARE\Python\PythonCore\3.12\InstallPath",
            null,
            @"X:\Python312");

        var software = new PythonSoftware(environment.Services);
        var inspection = await software.InspectAsync();

        Assert.Equal(2, inspection.InstalledVersions.Count);
        Assert.Equal("3.13.15", inspection.Best!.Version.ToString());
        Assert.Equal(@"X:\Program Files\Python313", inspection.Best.Directory);

        var operations = software.CreatePostProcessOperations(inspection);
        Assert.Equal(3, operations.Count);
        Assert.IsType<EnsurePathEntriesOperation>(operations[0]);
        Assert.IsType<WindowsAppsPriorityOperation>(operations[1]);
        Assert.IsType<EnsureExecutableFirstOperation>(operations[2]);
    }

    [Fact]
    public async Task PowerShell_IgnoresWindowsPowerShell5()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\PS5")
            .WithProgram(@"X:\PS5\pwsh.exe", "PowerShell 5.1.0");
        environment.Registry.AddSubKey(
            RegistryRoot.LocalMachine,
            RegistryBitness.X64,
            @"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions",
            "{7ba1f6b3-0000-0000-0000-000000000000}");
        environment.Registry.SetValue(
            RegistryRoot.LocalMachine,
            RegistryBitness.X64,
            @"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{7ba1f6b3-0000-0000-0000-000000000000}",
            "InstallLocation",
            @"X:\Program Files\PowerShell\7");
        environment.WithProgram(@"X:\Program Files\PowerShell\7\pwsh.exe", "PowerShell 7.6.6");

        var software = new PowerShellSoftware(environment.Services);
        var inspection = await software.InspectAsync();

        var detected = Assert.Single(inspection.InstalledVersions);
        Assert.Equal("7.6.6", detected.Version.ToString());
    }

    [Fact]
    public async Task VSCode_ReadsVersionFromCommandShim_AndIgnoresInsiders()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\VSCode\bin");
        environment.Files.AddFile(@"X:\VSCode\bin\code.cmd");
        environment.ProcessRunner.SetOutput(@"X:\VSCode\bin\code.cmd", "1.139.0\n2242ebbb54efeeb0129e08e919e7e8d43033cd83\nx64\n");
        environment.WithProgram(@"X:\VSCodeInsiders\bin\code.cmd", "1.140.0\n2242ebbb54efeeb0129e08e919e7e8d43033cd83\nx64\n");
        environment.Registry.UninstallEntries.Add(new UninstallEntry(
            "Microsoft Visual Studio Code",
            "1.139.0",
            @"X:\VSCode",
            RegistryRoot.LocalMachine,
            RegistryBitness.X64));
        environment.Registry.UninstallEntries.Add(new UninstallEntry(
            "Microsoft Visual Studio Code Insiders",
            "1.140.0",
            @"X:\VSCodeInsiders",
            RegistryRoot.CurrentUser,
            RegistryBitness.X64));

        var software = new VSCodeSoftware(environment.Services);
        var inspection = await software.InspectAsync();

        var detected = Assert.Single(inspection.InstalledVersions);
        Assert.Equal("1.139.0", detected.Version.ToString());
    }

    [Fact]
    public async Task NodeJs_IgnoresVersionsBelow22()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\node18")
            .WithProgram(@"X:\node18\node.exe", "v18.20.0");
        environment.WithProgram(@"X:\Program Files\nodejs\node.exe", "v24.21.0");

        var software = new NodeJsSoftware(environment.Services);
        var inspection = await software.InspectAsync();

        var detected = Assert.Single(inspection.InstalledVersions);
        Assert.Equal("24.21.0", detected.Version.ToString());
    }

    [Fact]
    public async Task Inspection_ReportsBestVersionAmongCandidates()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\Git\cmd");
        environment.WithProgram(@"X:\Git\cmd\git.exe", "git version 2.54.0.windows.1");
        environment.WithProgram(@"X:\Git2\cmd\git.exe", "git version 2.55.0.windows.5");
        environment.Registry.UninstallEntries.Add(new UninstallEntry(
            "Git For Windows",
            null,
            @"X:\Git2",
            RegistryRoot.LocalMachine,
            RegistryBitness.X64));

        var software = new GitSoftware(environment.Services);
        var inspection = await software.InspectAsync();

        Assert.Equal(2, inspection.InstalledVersions.Count);
        Assert.Equal("2.55.0", inspection.InstalledVersions[0].Version.ToString());
        Assert.Equal(@"X:\Git2\cmd\git.exe", inspection.InstalledVersions[0].ExecutablePath);
        Assert.Same(inspection.InstalledVersions[0], inspection.Best);
    }
}
