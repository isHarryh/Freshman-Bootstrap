using System.Text.Json;
using System.Text.RegularExpressions;
using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Services.PackageSources;

internal static class GitHubReleaseParser
{
    public static ResolvedPackage? FindAsset(string json, string softwareId, string assetPattern)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("assets", out var assets))
        {
            return null;
        }

        var regex = new Regex(assetPattern, RegexOptions.IgnoreCase);
        foreach (var asset in assets.EnumerateArray())
        {
            if (!asset.TryGetProperty("name", out var nameElement))
            {
                continue;
            }

            var name = nameElement.GetString();
            if (name is null || !regex.IsMatch(name))
            {
                continue;
            }

            if (!asset.TryGetProperty("browser_download_url", out var urlElement))
            {
                continue;
            }

            var url = urlElement.GetString();
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var sha256 = asset.TryGetProperty("digest", out var digestElement)
                ? NormalizeDigest(digestElement.GetString())
                : null;

            return new ResolvedPackage(softwareId, ExtractVersionFromName(name, regex), name, url, sha256);
        }

        return null;
    }

    private static string ExtractVersionFromName(string name, Regex pattern)
    {
        var match = pattern.Match(name);
        if (match.Success && match.Groups["version"].Success)
        {
            return match.Groups["version"].Value;
        }

        return VersionNumber.TryParse(name, out var version) ? version.ToString() : name;
    }

    private static string? NormalizeDigest(string? digest)
    {
        if (string.IsNullOrWhiteSpace(digest))
        {
            return null;
        }

        var separator = digest.IndexOf(':');
        return separator >= 0 ? digest[(separator + 1)..] : digest;
    }
}

public sealed class GitPackageSource : IPackageSource
{
    public string SoftwareId => Software.SoftwareIds.Git;

    public string DisplayName => Strings.Software.GitName;

    public async Task<ResolvedPackage?> ResolveAsync(IDownloadClient client, CancellationToken cancellationToken = default)
    {
        var json = await client.GetStringAsync(
            "https://api.github.com/repos/git-for-windows/git/releases/latest",
            cancellationToken);

        return ParseLatestRelease(json);
    }

    public static ResolvedPackage? ParseLatestRelease(string json)
    {
        return GitHubReleaseParser.FindAsset(
            json,
            Software.SoftwareIds.Git,
            @"^Git-(?<version>\d+(?:\.\d+)*)-64-bit\.exe$");
    }
}

public sealed class PowerShellPackageSource : IPackageSource
{
    public string SoftwareId => Software.SoftwareIds.PowerShell;

    public string DisplayName => Strings.Software.PowerShellName;

    public async Task<ResolvedPackage?> ResolveAsync(IDownloadClient client, CancellationToken cancellationToken = default)
    {
        var json = await client.GetStringAsync(
            "https://api.github.com/repos/PowerShell/PowerShell/releases/latest",
            cancellationToken);

        return ParseLatestRelease(json);
    }

    public static ResolvedPackage? ParseLatestRelease(string json)
    {
        return GitHubReleaseParser.FindAsset(
            json,
            Software.SoftwareIds.PowerShell,
            @"^PowerShell-(?<version>\d+(?:\.\d+)*)-win-x64\.msi$");
    }
}
