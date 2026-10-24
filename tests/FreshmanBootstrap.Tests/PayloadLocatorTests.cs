using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Tests;

public class PayloadLocatorTests
{
    [Fact]
    public void Resolve_FindsPayloadInParentDirectory()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { @"X:\repo\payload\manifest.json" };

        var result = PayloadLocator.Resolve(@"X:\repo\src\App\bin\Debug", null, existing.Contains);

        Assert.Equal(@"X:\repo\payload", result);
    }

    [Fact]
    public void Resolve_PrefersEnvironmentOverride()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            @"X:\repo\payload\manifest.json",
            @"X:\cache\payload\manifest.json",
        };

        var result = PayloadLocator.Resolve(@"X:\repo\src\App\bin\Debug", @"X:\cache\payload", existing.Contains);

        Assert.Equal(@"X:\cache\payload", result);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenManifestMissing()
    {
        var result = PayloadLocator.Resolve(@"X:\repo\src\App\bin\Debug", null, _ => false);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_IgnoresOverrideWithoutManifest()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { @"X:\repo\payload\manifest.json" };

        var result = PayloadLocator.Resolve(@"X:\repo\src\App\bin\Debug", @"X:\missing", existing.Contains);

        Assert.Equal(@"X:\repo\payload", result);
    }
}
