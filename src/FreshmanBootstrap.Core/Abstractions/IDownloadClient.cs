using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Abstractions;

public interface IDownloadClient
{
    Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default);

    Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface IPackageSource
{
    string SoftwareId { get; }

    string DisplayName { get; }

    Task<ResolvedPackage?> ResolveAsync(IDownloadClient client, CancellationToken cancellationToken = default);
}
