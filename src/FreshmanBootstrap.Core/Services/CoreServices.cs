using FreshmanBootstrap.Core.Abstractions;
using Serilog;

namespace FreshmanBootstrap.Core.Services;

public sealed class CoreServices
{
    public required IProcessRunner ProcessRunner { get; init; }

    public required IFileProbe Files { get; init; }

    public required IRegistryProbe Registry { get; init; }

    public required PathEnvironmentService PathEnvironment { get; init; }

    public required PayloadProvider Payloads { get; init; }

    public required ILogger Logger { get; init; }

    public string ProgramFilesDirectory { get; init; } =
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public string ProgramFilesX86Directory { get; init; } =
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

    public string LocalAppDataDirectory { get; init; } =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public string WindowsAppsDirectory { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Microsoft",
        "WindowsApps");
}
