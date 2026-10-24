using System.IO;
using FreshmanBootstrap.App.Services;
using FreshmanBootstrap.Core;
using FreshmanBootstrap.Core.Services;
using FreshmanBootstrap.Core.Software;
using Serilog;

namespace FreshmanBootstrap.App;

public sealed class AppHost
{
    private readonly HttpDownloadClient _downloadClient;
    private readonly PrepareCommand _prepareCommand;
    private readonly string? _payloadDirectory;

    private AppHost(
        CoreServices services,
        SoftwareCatalog software,
        string? payloadDirectory,
        HttpDownloadClient downloadClient,
        PrepareCommand prepareCommand)
    {
        Services = services;
        Software = software;
        _payloadDirectory = payloadDirectory;
        _downloadClient = downloadClient;
        _prepareCommand = prepareCommand;
    }

    public CoreServices Services { get; }

    public SoftwareCatalog Software { get; }

    public static AppHost Create()
    {
        var fileProbe = new WindowsFileProbe();
        var payloadDirectory = AppPaths.FindPayloadDirectory(AppContext.BaseDirectory);

        var services = new CoreServices
        {
            ProcessRunner = new WindowsProcessRunner(),
            Files = fileProbe,
            Registry = new WindowsRegistryProbe(),
            PathEnvironment = new PathEnvironmentService(new RegistryPathStore(), new NativeEnvironmentBroadcaster()),
            Payloads = new PayloadProvider(payloadDirectory),
            Logger = Log.Logger,
        };

        var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
            ?? Environment.GetEnvironmentVariable("GH_TOKEN");
        var downloadClient = new HttpDownloadClient(githubToken);
        var prepareCommand = new PrepareCommand(downloadClient, fileProbe, Log.Logger);

        return new AppHost(services, new SoftwareCatalog(services), payloadDirectory, downloadClient, prepareCommand);
    }

    public async Task<int> RunPrepareAsync(bool fresh = false, CancellationToken cancellationToken = default)
    {
        var target = _payloadDirectory ?? FindRepositoryPayloadDirectory();

        Log.Information("Preparing payload into {Directory} (fresh: {Fresh})", target, fresh);
        var exitCode = await _prepareCommand.RunAsync(target, fresh, cancellationToken);
        Log.Information("Prepare finished with exit code {ExitCode}", exitCode);
        return exitCode;
    }

    public void Dispose()
    {
        _downloadClient.Dispose();
    }

    private static string FindRepositoryPayloadDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (var depth = 0; depth < 8 && current is not null; depth++)
        {
            if (File.Exists(Path.Combine(current.FullName, "FreshmanBootstrap.slnx"))
                || File.Exists(Path.Combine(current.FullName, "FreshmanBootstrap.sln")))
            {
                return Path.Combine(current.FullName, "payload");
            }

            current = current.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "payload");
    }
}
