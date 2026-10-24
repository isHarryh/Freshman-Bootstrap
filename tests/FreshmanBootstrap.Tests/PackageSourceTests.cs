using FreshmanBootstrap.Core.Services.PackageSources;

namespace FreshmanBootstrap.Tests;

public class PackageSourceTests
{
    private const string GitReleaseJson = """
        {
          "draft": false,
          "prerelease": false,
          "tag_name": "v2.55.0.windows.5",
          "assets": [
            {"name": "Git-2.55.0.5-arm64.exe", "browser_download_url": "https://example.com/arm64.exe"},
            {"name": "MinGit-2.55.0.5-64-bit.zip", "browser_download_url": "https://example.com/mingit.zip"},
            {"name": "Git-2.55.0.5-64-bit.exe", "browser_download_url": "https://example.com/git.exe", "digest": "sha256:d065a4e2"}
          ]
        }
        """;

    private const string PowerShellReleaseJson = """
        {
          "draft": false,
          "prerelease": false,
          "tag_name": "v7.6.6",
          "assets": [
            {"name": "PowerShell-7.6.6-win-arm64.msi", "browser_download_url": "https://example.com/arm64.msi"},
            {"name": "PowerShell-7.6.6-win-x64.msi", "browser_download_url": "https://example.com/x64.msi", "digest": "sha256:beef"}
          ]
        }
        """;

    private const string VSCodeUpdateJson = """
        {
          "url": "https://vscode.download.prss.microsoft.com/dbazure/download/stable/2242ebbb/VSCodeSetup-x64-1.139.0.exe",
          "productVersion": "1.139.0",
          "sha256hash": "f885c2e6"
        }
        """;

    private const string PythonReleasesJson = """
        [
          {"cycle": "3.14", "support": "2027-10-01", "latest": "3.14.7"},
          {"cycle": "3.13", "support": "2026-10-01", "latest": "3.13.15"},
          {"cycle": "3.12", "support": "2025-04-02", "latest": "3.12.14"},
          {"cycle": "3.11", "support": false, "latest": "3.11.16"},
          {"cycle": "2.7", "support": false, "latest": "2.7.18"}
        ]
        """;

    private const string NodeIndexJson = """
        [
          {"version": "v25.1.0", "lts": false},
          {"version": "v24.21.0", "lts": "Krypton"},
          {"version": "v22.20.0", "lts": "Jod"}
        ]
        """;

    [Fact]
    public void GitRelease_Picks64BitInstaller()
    {
        var package = GitPackageSource.ParseLatestRelease(GitReleaseJson);

        Assert.NotNull(package);
        Assert.Equal("2.55.0.5", package!.Version);
        Assert.Equal("Git-2.55.0.5-64-bit.exe", package.FileName);
        Assert.Equal("https://example.com/git.exe", package.Url);
        Assert.Equal("d065a4e2", package.Sha256);
    }

    [Fact]
    public void PowerShellRelease_PicksWinX64Msi()
    {
        var package = PowerShellPackageSource.ParseLatestRelease(PowerShellReleaseJson);

        Assert.NotNull(package);
        Assert.Equal("7.6.6", package!.Version);
        Assert.Equal("PowerShell-7.6.6-win-x64.msi", package.FileName);
        Assert.Equal("beef", package.Sha256);
    }

    [Fact]
    public void VSCodeUpdate_UsesProductVersion()
    {
        var package = VSCodePackageSource.ParseUpdateInfo(VSCodeUpdateJson);

        Assert.NotNull(package);
        Assert.Equal("1.139.0", package!.Version);
        Assert.Equal("VSCodeSetup-x64-1.139.0.exe", package.FileName);
        Assert.Equal("f885c2e6", package.Sha256);
    }

    [Fact]
    public void PythonReleases_PicksOldestActiveBugfixBranch()
    {
        var package = PythonPackageSource.ParseOldestBugfixBranch(PythonReleasesJson, new DateOnly(2026, 9, 24));

        Assert.NotNull(package);
        Assert.Equal("3.13.15", package!.Version);
        Assert.Equal("python-3.13.15-amd64.exe", package.FileName);
        Assert.Equal("https://www.python.org/ftp/python/3.13.15/python-3.13.15-amd64.exe", package.Url);
    }

    [Fact]
    public void PythonReleases_SwitchesBranch_WhenSupportEnds()
    {
        var package = PythonPackageSource.ParseOldestBugfixBranch(PythonReleasesJson, new DateOnly(2026, 10, 2));

        Assert.NotNull(package);
        Assert.Equal("3.14.7", package!.Version);
    }

    [Fact]
    public void NodeIndex_PicksLatestLts()
    {
        var package = NodeJsPackageSource.ParseLatestLts(NodeIndexJson);

        Assert.NotNull(package);
        Assert.Equal("24.21.0", package!.Version);
        Assert.Equal("node-v24.21.0-x64.msi", package.FileName);
        Assert.Equal("https://nodejs.org/dist/v24.21.0/node-v24.21.0-x64.msi", package.Url);
    }

    [Fact]
    public void NodeShasums_ParsesMatchingFile()
    {
        const string shasums = """
            1111111111111111111111111111111111111111111111111111111111111111  node-v24.21.0-x64.zip
            2222222222222222222222222222222222222222222222222222222222222222  node-v24.21.0-x64.msi
            """;

        var hash = NodeJsPackageSource.ParseShasums(shasums, "node-v24.21.0-x64.msi");

        Assert.Equal("2222222222222222222222222222222222222222222222222222222222222222", hash);
    }

    [Fact]
    public void NodeShasums_ReturnsNull_WhenFileMissing()
    {
        var hash = NodeJsPackageSource.ParseShasums("aaa  other-file.zip", "node-v24.21.0-x64.msi");

        Assert.Null(hash);
    }

    [Fact]
    public void GitRelease_ReturnsNull_WhenNoMatchingAsset()
    {
        var package = GitPackageSource.ParseLatestRelease("""{"assets": []}""");

        Assert.Null(package);
    }
}
