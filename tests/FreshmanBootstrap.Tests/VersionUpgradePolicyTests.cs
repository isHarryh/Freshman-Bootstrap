using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Tests;

public class VersionUpgradePolicyTests
{
    [Theory]
    [InlineData("3.13.5", "3.13.15")]
    [InlineData("2.55.0", "2.55.1")]
    [InlineData("2.51.0", "2.51.0.2")]
    [InlineData("7.6.6", "7.6.7")]
    [InlineData("3.13", "3.13.15")]
    [InlineData("2.55.0.1", "2.55.0.5")]
    [InlineData("2.55.0.1", "2.55.1.0")]
    [InlineData("1.2.3.4.5", "1.2.99.0.0")]
    public void CanUpgrade_AllowsNonMajorNonMinorUpgrades(string installed, string target)
    {
        Assert.True(VersionUpgradePolicy.CanUpgrade(VersionNumber.Parse(installed), VersionNumber.Parse(target)));
    }

    [Theory]
    [InlineData("3.13.5", "3.13.5")]
    [InlineData("3.13.15", "3.13.5")]
    [InlineData("3.12.5", "3.13.0")]
    [InlineData("22.11.0", "24.21.0")]
    [InlineData("1.138.0", "1.139.0")]
    [InlineData("2.55.9.9", "2.56.0.0")]
    [InlineData("3.13.5", "4.13.9")]
    public void CanUpgrade_RejectsEqualDowngradeAndMajorOrMinorUpgrades(string installed, string target)
    {
        Assert.False(VersionUpgradePolicy.CanUpgrade(VersionNumber.Parse(installed), VersionNumber.Parse(target)));
    }

    [Fact]
    public void CanUpgrade_RejectsInvalidVersions()
    {
        Assert.False(VersionUpgradePolicy.CanUpgrade(default, VersionNumber.Parse("3.13.15")));
        Assert.False(VersionUpgradePolicy.CanUpgrade(VersionNumber.Parse("3.13.5"), default));
    }
}
