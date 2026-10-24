using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Software;

public static class InstallCommands
{
    public const string Msiexec = "msiexec.exe";

    public static InstallCommand Git(string installerPath, string installDirectory)
    {
        return new InstallCommand(
            installerPath,
            $"/VERYSILENT /NORESTART /NOCANCEL /SP- /SUPPRESSMSGBOXES /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS /DIR={Services.CommandLine.Quote(installDirectory)}");
    }

    public static InstallCommand PowerShell(string msiPath, string installDirectory)
    {
        return new InstallCommand(
            Msiexec,
            $"/i {Services.CommandLine.Quote(msiPath)} /qn /norestart ADD_PATH=1 REGISTER_MANIFEST=1 ENABLE_PSREMOTING=0 INSTALLFOLDER={Services.CommandLine.Quote(installDirectory)}");
    }

    public static InstallCommand VSCode(string installerPath, string installDirectory, bool desktopIcon)
    {
        var tasks = "!runcode,addcontextmenufiles,addcontextmenufolders,associatewithfiles,addtopath";
        if (desktopIcon)
        {
            tasks += ",desktopicon";
        }

        return new InstallCommand(
            installerPath,
            $"/VERYSILENT /NORESTART /SUPPRESSMSGBOXES /MERGETASKS={tasks} /DIR={Services.CommandLine.Quote(installDirectory)}");
    }

    public static InstallCommand Python(string installerPath, string installDirectory, bool launcher)
    {
        return new InstallCommand(
            installerPath,
            $"/quiet InstallAllUsers=1 PrependPath=1 Include_launcher={(launcher ? 1 : 0)} Include_test=0 Include_doc=0 Include_pip=1 AssociateFiles=1 Shortcuts=1 TargetDir={Services.CommandLine.Quote(installDirectory)}");
    }

    public static InstallCommand NodeJs(string msiPath, string installDirectory)
    {
        return new InstallCommand(
            Msiexec,
            $"/i {Services.CommandLine.Quote(msiPath)} /qn /norestart INSTALLDIR={Services.CommandLine.Quote(installDirectory)}");
    }
}
