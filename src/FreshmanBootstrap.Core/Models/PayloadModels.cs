namespace FreshmanBootstrap.Core.Models;

public sealed class PayloadManifest
{
    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset GeneratedAt { get; set; }

    public List<PayloadEntry> Packages { get; set; } = new();
}

public sealed class PayloadEntry
{
    public string SoftwareId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Sha256 { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;
}

public sealed record ResolvedPackage(
    string SoftwareId,
    string Version,
    string FileName,
    string Url,
    string? Sha256);
