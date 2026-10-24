namespace FreshmanBootstrap.Core.Models;

public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError, bool TimedOut)
{
    public bool Succeeded => !TimedOut && ExitCode == 0;

    public string FirstOutputLine
    {
        get
        {
            foreach (var line in StandardOutput.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0)
                {
                    return trimmed;
                }
            }

            return string.Empty;
        }
    }
}

public sealed record InstallResult(bool Success, string? ErrorMessage, int ExitCode)
{
    public static InstallResult Ok(int exitCode = 0) => new(true, null, exitCode);

    public static InstallResult Fail(string message, int exitCode = -1) => new(false, message, exitCode);
}

public sealed record InstallProgress(string Stage, int? Percent = null);

public sealed record InstallCommand(string ExecutablePath, string Arguments);

public static class InstallerExitCodes
{
    public static bool IsSuccess(int exitCode) => exitCode is 0 or 1641 or 3010;
}
