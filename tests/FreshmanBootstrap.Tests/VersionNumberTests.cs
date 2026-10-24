using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Tests;

public class VersionNumberTests
{
    [Theory]
    [InlineData("git version 2.55.0.windows.5", "2.55.0")]
    [InlineData("v24.21.0", "24.21.0")]
    [InlineData("Python 3.13.15", "3.13.15")]
    [InlineData("PowerShell 7.6.6", "7.6.6")]
    [InlineData("1.139.0", "1.139.0")]
    [InlineData("node-v24.21.0-x64", "24.21.0")]
    public void TryParse_ExtractsFirstVersionToken(string input, string expected)
    {
        Assert.True(VersionNumber.TryParse(input, out var version));
        Assert.Equal(expected, version.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no digits here")]
    [InlineData("v")]
    public void TryParse_ReturnsFalse_ForInvalidInput(string? input)
    {
        Assert.False(VersionNumber.TryParse(input, out _));
    }

    [Fact]
    public void Comparison_UsesNumericComponents()
    {
        Assert.True(VersionNumber.Parse("7.10.0") > VersionNumber.Parse("7.9.9"));
        Assert.True(VersionNumber.Parse("2.55.0") < VersionNumber.Parse("2.55.1"));
        Assert.True(VersionNumber.Parse("3.13") == VersionNumber.Parse("3.13.0"));
        Assert.True(VersionNumber.Parse("24.21.0") <= VersionNumber.Parse("24.21.0"));
    }

    [Fact]
    public void Equality_NormalizesTrailingZeros_ForHashCode()
    {
        Assert.Equal(VersionNumber.Parse("3.13").GetHashCode(), VersionNumber.Parse("3.13.0").GetHashCode());
        Assert.NotEqual(VersionNumber.Parse("3.13").GetHashCode(), VersionNumber.Parse("3.14").GetHashCode());
    }

    [Fact]
    public void Equality_DistinguishesInvalidFromZeroVersion()
    {
        Assert.NotEqual(default, new VersionNumber(0));
        Assert.Equal(default, default(VersionNumber));
    }
}
