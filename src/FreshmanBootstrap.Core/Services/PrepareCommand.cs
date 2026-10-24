using System.Text.Json;
using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services.PackageSources;
using Serilog;

namespace FreshmanBootstrap.Core.Services;

public sealed class PrepareCommand
{
    private readonly IDownloadClient _client;
    private readonly IFileProbe _files;
    private readonly ILogger _logger;
    private readonly IReadOnlyList<IPackageSource> _sources;

    public PrepareCommand(
        IDownloadClient client,
        IFileProbe files,
        ILogger logger,
        IReadOnlyList<IPackageSource>? sources = null)
    {
        _client = client;
        _files = files;
        _logger = logger;
        _sources = sources ?? new IPackageSource[]
        {
            new GitPackageSource(),
            new PowerShellPackageSource(),
            new VSCodePackageSource(),
            new PythonPackageSource(),
            new NodeJsPackageSource(),
        };
    }

    public async Task<int> RunAsync(
        string payloadDirectory,
        bool fresh = false,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(payloadDirectory);

        var manifestPath = Path.Combine(payloadDirectory, PayloadLocator.ManifestFileName);
        var existing = PayloadManifestFile.TryRead(manifestPath);
        var packages = new List<PayloadEntry>();
        var failures = 0;

        foreach (var source in _sources)
        {
            var previous = existing?.Packages.FirstOrDefault(entry =>
                string.Equals(entry.SoftwareId, source.SoftwareId, StringComparison.OrdinalIgnoreCase));

            try
            {
                _logger.Information(Strings.Prepare.Resolving, source.DisplayName);
                var resolved = await source.ResolveAsync(_client, cancellationToken);
                if (resolved is null)
                {
                    _logger.Error(Strings.Prepare.ResolveFailed, source.DisplayName);
                    failures++;
                    CarryPrevious(previous, payloadDirectory, packages);
                    continue;
                }

                var destination = Path.Combine(payloadDirectory, resolved.FileName);

                var cacheCandidate = previous is not null
                    && _files.FileExists(destination)
                    && string.Equals(previous.Version, resolved.Version, StringComparison.OrdinalIgnoreCase);

                if (cacheCandidate && !fresh)
                {
                    var localSha256 = await Hashing.ComputeSha256Async(destination, cancellationToken);
                    if (ShouldUseCache(resolved, previous, localSha256))
                    {
                        _logger.Information(Strings.Prepare.Skipped, resolved.FileName);
                        packages.Add(new PayloadEntry
                        {
                            SoftwareId = resolved.SoftwareId,
                            Version = resolved.Version,
                            FileName = resolved.FileName,
                            Sha256 = resolved.Sha256 ?? previous!.Sha256,
                            SourceUrl = resolved.Url,
                        });
                        continue;
                    }

                    _logger.Warning(Strings.Prepare.CacheInvalid, resolved.FileName);
                }
                else if (cacheCandidate)
                {
                    _logger.Information(Strings.Prepare.ForceRefresh, resolved.FileName);
                }

                _logger.Information(Strings.Prepare.Downloading, resolved.FileName);
                var temporaryPath = destination + ".tmp";
                await _client.DownloadFileAsync(resolved.Url, temporaryPath, null, cancellationToken);

                var sha256 = await Hashing.ComputeSha256Async(temporaryPath, cancellationToken);
                if (!string.IsNullOrWhiteSpace(resolved.Sha256) &&
                    !string.Equals(resolved.Sha256, sha256, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(temporaryPath);
                    _logger.Error(Strings.Prepare.HashMismatch, resolved.FileName);
                    failures++;
                    CarryPrevious(previous, payloadDirectory, packages);
                    continue;
                }

                File.Move(temporaryPath, destination, true);
                packages.Add(new PayloadEntry
                {
                    SoftwareId = resolved.SoftwareId,
                    Version = resolved.Version,
                    FileName = resolved.FileName,
                    Sha256 = sha256,
                    SourceUrl = resolved.Url,
                });
                _logger.Information(Strings.Prepare.Downloaded, resolved.FileName, resolved.Version);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.Error(exception, Strings.Prepare.DownloadFailed, source.DisplayName, exception.Message);
                failures++;
                CarryPrevious(previous, payloadDirectory, packages);
            }
        }

        RemoveStaleFiles(payloadDirectory, packages);

        var manifest = new PayloadManifest
        {
            SchemaVersion = 1,
            GeneratedAt = DateTimeOffset.Now,
            Packages = packages.OrderBy(entry => entry.SoftwareId, StringComparer.Ordinal).ToList(),
        };

        await using var stream = File.Create(manifestPath);
        await JsonSerializer.SerializeAsync(stream, manifest, JsonConfig.Options, cancellationToken);

        return failures == 0 ? 0 : 1;
    }

    public static bool ShouldUseCache(ResolvedPackage resolved, PayloadEntry? previous, string? localSha256)
    {
        if (previous is null || string.IsNullOrWhiteSpace(localSha256))
        {
            return false;
        }

        var expectedSha256 = string.IsNullOrWhiteSpace(resolved.Sha256) ? previous.Sha256 : resolved.Sha256;
        return !string.IsNullOrWhiteSpace(expectedSha256)
            && string.Equals(expectedSha256, localSha256, StringComparison.OrdinalIgnoreCase);
    }

    private void CarryPrevious(PayloadEntry? previous, string payloadDirectory, List<PayloadEntry> packages)
    {
        if (previous is null || !_files.FileExists(Path.Combine(payloadDirectory, previous.FileName)))
        {
            return;
        }

        packages.Add(previous);
    }

    private static void RemoveStaleFiles(string payloadDirectory, List<PayloadEntry> packages)
    {
        var keep = packages.Select(entry => entry.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.EnumerateFiles(payloadDirectory))
        {
            var name = Path.GetFileName(file);
            if (name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
            {
                TryDelete(file);
                continue;
            }

            var extension = Path.GetExtension(file);
            var isInstaller = extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".msi", StringComparison.OrdinalIgnoreCase);
            if (isInstaller && !keep.Contains(name))
            {
                TryDelete(file);
            }
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception)
        {
            // Stale payload files left behind are harmless; keep going.
        }
    }
}
