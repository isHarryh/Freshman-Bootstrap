using System.Diagnostics;
using System.IO;

namespace FreshmanBootstrap.App.Services;

public static class UninstallLauncher
{
    public static string? FindUninstaller(string baseDirectory)
    {
        try
        {
            return Directory
                .GetFiles(baseDirectory, "unins*.exe")
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool TryLaunch(string uninstallerPath, out string? error)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uninstallerPath,
                Arguments = "/SILENT /NORESTART",
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(uninstallerPath) ?? string.Empty,
            });

            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }
}
