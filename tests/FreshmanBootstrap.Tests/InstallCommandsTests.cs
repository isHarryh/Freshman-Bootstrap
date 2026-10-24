using FreshmanBootstrap.Core.Software;

namespace FreshmanBootstrap.Tests;

public class InstallCommandsTests
{
    [Fact]
    public void Git_UsesInnoSilentSwitchesAndInstallDirectory()
    {
        var command = InstallCommands.Git(@"X:\payload\Git-2.55.0.5-64-bit.exe", @"C:\Program Files\Git");

        Assert.Contains("/VERYSILENT", command.Arguments);
        Assert.Contains(@"/DIR=""C:\Program Files\Git""", command.Arguments);
    }

    [Fact]
    public void PowerShell_UsesMsiexecWithPathAndManifestOptions()
    {
        var command = InstallCommands.PowerShell(@"X:\payload\PowerShell-7.6.6-win-x64.msi", @"C:\Program Files\PowerShell\7");

        Assert.Equal(InstallCommands.Msiexec, command.ExecutablePath);
        Assert.Contains("/i ", command.Arguments);
        Assert.Contains("/qn", command.Arguments);
        Assert.Contains("ADD_PATH=1", command.Arguments);
        Assert.Contains("REGISTER_MANIFEST=1", command.Arguments);
        Assert.Contains(@"""C:\Program Files\PowerShell\7""", command.Arguments);
    }

    [Theory]
    [InlineData(true, "desktopicon")]
    [InlineData(false, "!runcode")]
    public void VSCode_TogglesDesktopIconTask(bool desktopIcon, string marker)
    {
        var command = InstallCommands.VSCode(@"X:\payload\VSCodeSetup-x64.exe", @"C:\Program Files\Microsoft VS Code", desktopIcon);

        Assert.Contains("addtopath", command.Arguments);
        if (desktopIcon)
        {
            Assert.Contains(",desktopicon", command.Arguments);
        }
        else
        {
            Assert.DoesNotContain("desktopicon", command.Arguments);
        }

        Assert.Contains(marker, command.Arguments);
    }

    [Fact]
    public void Python_TogglesLauncherAndSetsAllUsers()
    {
        var withLauncher = InstallCommands.Python(@"X:\payload\python-3.13.15-amd64.exe", @"C:\Program Files\Python313", true);
        var withoutLauncher = InstallCommands.Python(@"X:\payload\python-3.13.15-amd64.exe", @"C:\Program Files\Python313", false);

        Assert.Contains("InstallAllUsers=1", withLauncher.Arguments);
        Assert.Contains("PrependPath=1", withLauncher.Arguments);
        Assert.Contains("Include_launcher=1", withLauncher.Arguments);
        Assert.Contains("Include_launcher=0", withoutLauncher.Arguments);
        Assert.Contains(@"""C:\Program Files\Python313""", withLauncher.Arguments);
    }

    [Fact]
    public void NodeJs_UsesMsiexecWithInstallDirectory()
    {
        var command = InstallCommands.NodeJs(@"X:\payload\node-v24.21.0-x64.msi", @"C:\Program Files\nodejs");

        Assert.Equal(InstallCommands.Msiexec, command.ExecutablePath);
        Assert.Contains("/qn", command.Arguments);
        Assert.Contains(@"""C:\Program Files\nodejs""", command.Arguments);
    }
}
