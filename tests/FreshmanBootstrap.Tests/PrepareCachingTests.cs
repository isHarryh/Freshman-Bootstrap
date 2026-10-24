using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Tests;

public class PrepareCachingTests
{
    private static ResolvedPackage Package(string? sha256)
    {
        return new ResolvedPackage("git", "2.55.0.5", "Git-2.55.0.5-64-bit.exe", "https://example.com/git.exe", sha256);
    }

    private static PayloadEntry Entry(string sha256)
    {
        return new PayloadEntry
        {
            SoftwareId = "git",
            Version = "2.55.0.5",
            FileName = "Git-2.55.0.5-64-bit.exe",
            Sha256 = sha256,
            SourceUrl = "https://example.com/git.exe",
        };
    }

    [Fact]
    public void UsesCache_WhenOfficialHashMatchesLocalFile()
    {
        var result = PrepareCommand.ShouldUseCache(Package("abc123"), Entry("old-value"), "abc123");

        Assert.True(result);
    }

    [Fact]
    public void Redownloads_WhenOfficialHashDiffersFromLocalFile()
    {
        var result = PrepareCommand.ShouldUseCache(Package("abc123"), Entry("abc123"), "corrupted");

        Assert.False(result);
    }

    [Fact]
    public void UsesCache_WhenOnlyRecordedHashIsAvailableAndMatches()
    {
        var result = PrepareCommand.ShouldUseCache(Package(null), Entry("abc123"), "abc123");

        Assert.True(result);
    }

    [Fact]
    public void Redownloads_WhenRecordedHashDiffersFromLocalFile()
    {
        var result = PrepareCommand.ShouldUseCache(Package(null), Entry("abc123"), "corrupted");

        Assert.False(result);
    }

    [Fact]
    public void Redownloads_WhenNoHashIsKnown()
    {
        var result = PrepareCommand.ShouldUseCache(Package(null), Entry(string.Empty), "anything");

        Assert.False(result);
    }

    [Fact]
    public void Redownloads_WhenManifestEntryMissing()
    {
        var result = PrepareCommand.ShouldUseCache(Package("abc123"), null, "abc123");

        Assert.False(result);
    }

    [Fact]
    public void Redownloads_WhenLocalHashUnavailable()
    {
        var result = PrepareCommand.ShouldUseCache(Package("abc123"), Entry("abc123"), null);

        Assert.False(result);
    }

    [Fact]
    public void HashComparison_IsCaseInsensitive()
    {
        var result = PrepareCommand.ShouldUseCache(Package("ABC123"), Entry("abc123"), "abc123");

        Assert.True(result);
    }
}
