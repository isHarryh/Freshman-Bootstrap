using System.Globalization;
using System.Text.Json;
using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Services.PackageSources;

public sealed class PythonPackageSource : IPackageSource
{
    private const string ReleasesUrl = "https://endoflife.date/api/python.json";

    public string SoftwareId => Software.SoftwareIds.Python;

    public string DisplayName => Strings.Software.PythonName;

    public async Task<ResolvedPackage?> ResolveAsync(IDownloadClient client, CancellationToken cancellationToken = default)
    {
        var json = await client.GetStringAsync(ReleasesUrl, cancellationToken);
        return ParseOldestBugfixBranch(json, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public static ResolvedPackage? ParseOldestBugfixBranch(string json, DateOnly today)
    {
        using var document = JsonDocument.Parse(json);

        VersionNumber? bestCycle = null;
        string? bestLatest = null;

        foreach (var entry in document.RootElement.EnumerateArray())
        {
            if (!entry.TryGetProperty("cycle", out var cycleElement))
            {
                continue;
            }

            var cycleText = cycleElement.GetString();
            if (cycleText is null || !cycleText.StartsWith("3.", StringComparison.Ordinal))
            {
                continue;
            }

            if (!entry.TryGetProperty("support", out var supportElement))
            {
                continue;
            }

            if (!IsBugfixActive(supportElement, today))
            {
                continue;
            }

            if (!VSN(cycleText, out var cycle))
            {
                continue;
            }

            if (bestCycle is not null && cycle >= bestCycle.Value)
            {
                continue;
            }

            if (!entry.TryGetProperty("latest", out var latestElement))
            {
                continue;
            }

            var latest = latestElement.GetString();
            if (string.IsNullOrWhiteSpace(latest) || !VSN(latest, out _))
            {
                continue;
            }

            bestCycle = cycle;
            bestLatest = latest;
        }

        if (bestCycle is null || bestLatest is null)
        {
            return null;
        }

        var fileName = $"python-{bestLatest}-amd64.exe";
        var url = $"https://www.python.org/ftp/python/{bestLatest}/{fileName}";
        return new ResolvedPackage(Software.SoftwareIds.Python, bestLatest, fileName, url, null);
    }

    private static bool IsBugfixActive(JsonElement supportElement, DateOnly today)
    {
        return supportElement.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => ParseSupportDate(supportElement.GetString(), today),
            _ => false,
        };
    }

    private static bool ParseSupportDate(string? text, DateOnly today)
    {
        return DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var supportEnd)
            && supportEnd >= today;
    }

    private static bool VSN(string text, out VersionNumber version)
    {
        return VersionNumber.TryParse(text, out version) && version.Parts.Count >= 2;
    }
}
