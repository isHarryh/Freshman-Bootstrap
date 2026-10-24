namespace FreshmanBootstrap.Core.Models;

public enum SoftwareStatus
{
    Unknown,
    NotInstalled,
    Installed,
    NeedsRepair,
    Working,
    Failed,
}

public sealed record InstalledVersion(
    VersionNumber Version,
    string ExecutablePath)
{
    public string Directory => Path.GetDirectoryName(ExecutablePath) ?? string.Empty;
}

public sealed record SoftwareInspection(
    IReadOnlyList<InstalledVersion> InstalledVersions,
    InstalledVersion? Best,
    string? ErrorMessage = null)
{
    public static SoftwareInspection Empty { get; } = new(Array.Empty<InstalledVersion>(), null);

    public bool IsInstalled => Best is not null;
}
