using System.Text.Json;
using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Services.PackageSources;

public sealed class VSCodePackageSource : IPackageSource
{
    public string SoftwareId => Software.SoftwareIds.VSCode;

    public string DisplayName => Strings.Software.VSCodeName;

    public async Task<ResolvedPackage?> ResolveAsync(IDownloadClient client, CancellationToken cancellationToken = default)
    {
        var json = await client.GetStringAsync(
            "https://update.code.visualstudio.com/api/update/win32-x64/stable/latest",
            cancellationToken);

        return ParseUpdateInfo(json);
    }

    public static ResolvedPackage? ParseUpdateInfo(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("productVersion", out var versionElement))
        {
            return null;
        }

        if (!root.TryGetProperty("url", out var urlElement))
        {
            return null;
        }

        var version = versionElement.GetString();
        var url = urlElement.GetString();
        if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var sha256 = root.TryGetProperty("sha256hash", out var shaElement) ? shaElement.GetString() : null;
        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);

        return new ResolvedPackage(Software.SoftwareIds.VSCode, version, fileName, url, sha256);
    }
}

public sealed class NodeJsPackageSource : IPackageSource
{
    public string SoftwareId => Software.SoftwareIds.NodeJs;

    public string DisplayName => Strings.Software.NodeJsName;

    public async Task<ResolvedPackage?> ResolveAsync(IDownloadClient client, CancellationToken cancellationToken = default)
    {
        var json = await client.GetStringAsync("https://nodejs.org/dist/index.json", cancellationToken);
        var package = ParseLatestLts(json);
        if (package is null)
        {
            return null;
        }

        try
        {
            var shasums = await client.GetStringAsync(
                $"https://nodejs.org/dist/v{package.Version}/SHASUMS256.txt",
                cancellationToken);
            var sha256 = ParseShasums(shasums, package.FileName);
            return sha256 is null ? package : package with { Sha256 = sha256 };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return package;
        }
    }

    public static string? ParseShasums(string content, string fileName)
    {
        foreach (var line in content.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[1].Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                return parts[0].ToLowerInvariant();
            }
        }

        return null;
    }

    public static ResolvedPackage? ParseLatestLts(string json)
    {
        using var document = JsonDocument.Parse(json);
        foreach (var entry in document.RootElement.EnumerateArray())
        {
            if (!entry.TryGetProperty("lts", out var ltsElement) || ltsElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            if (!entry.TryGetProperty("version", out var versionElement))
            {
                continue;
            }

            var rawVersion = versionElement.GetString();
            if (string.IsNullOrWhiteSpace(rawVersion))
            {
                continue;
            }

            if (!VersionNumber.TryParse(rawVersion, out var version))
            {
                continue;
            }

            var fileName = $"node-v{version}-x64.msi";
            var url = $"https://nodejs.org/dist/v{version}/{fileName}";
            return new ResolvedPackage(Software.SoftwareIds.NodeJs, version.ToString(), fileName, url, null);
        }

        return null;
    }
}
