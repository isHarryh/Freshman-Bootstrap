using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Tests;

public class CommandLineTests
{
    [Theory]
    [InlineData("simple", "simple")]
    [InlineData(@"C:\Program Files\Git", @"""C:\Program Files\Git""")]
    [InlineData(@"C:\Program Files\dir\", @"""C:\Program Files\dir\\""")]
    [InlineData("a\"b", @"""a\""b""")]
    public void Quote_ProducesWindowsCompatibleArguments(string input, string expected)
    {
        Assert.Equal(expected, CommandLine.Quote(input));
    }

    [Fact]
    public void Quote_HandlesEmptyString()
    {
        Assert.Equal("\"\"", CommandLine.Quote(string.Empty));
    }
}
