using System.Text.Encodings.Web;
using System.Text.Json;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Services;

public static class JsonConfig
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}

internal static class PayloadManifestFile
{
    public static PayloadManifest? TryRead(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<PayloadManifest>(stream, JsonConfig.Options);
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public sealed class PayloadProvider
{
    private readonly string? _directory;
    private readonly Lazy<PayloadManifest?> _manifest;

    public PayloadProvider(string? directory)
    {
        _directory = directory;
        _manifest = new Lazy<PayloadManifest?>(LoadManifest);
    }

    public PayloadManifest? Manifest => _manifest.Value;

    public PayloadEntry? GetEntry(string softwareId)
    {
        return Manifest?.Packages.FirstOrDefault(entry =>
            string.Equals(entry.SoftwareId, softwareId, StringComparison.OrdinalIgnoreCase));
    }

    public string? GetVersion(string softwareId) => GetEntry(softwareId)?.Version;

    public string? GetInstallerPath(string softwareId)
    {
        var entry = GetEntry(softwareId);
        if (entry is null || _directory is null)
        {
            return null;
        }

        return Path.Combine(_directory, entry.FileName);
    }

    private PayloadManifest? LoadManifest()
    {
        return _directory is null
            ? null
            : PayloadManifestFile.TryRead(Path.Combine(_directory, PayloadLocator.ManifestFileName));
    }
}
