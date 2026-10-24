using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FreshmanBootstrap.Core.Abstractions;

namespace FreshmanBootstrap.Core.Services;

public sealed class HttpDownloadClient : IDownloadClient, IDisposable
{
    private static readonly string UserAgent = $"FreshmanBootstrap/{AppInfo.VersionText}";

    private readonly HttpClient _httpClient;
    private readonly string? _githubToken;

    public HttpDownloadClient(string? githubToken = null)
    {
        _githubToken = string.IsNullOrWhiteSpace(githubToken) ? null : githubToken;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(30),
        };
    }

    public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(url);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(url);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength;
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = File.Create(destinationPath);

        var buffer = new byte[81920];
        long received = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            received += read;
            if (total is > 0)
            {
                progress?.Report((double)received / total.Value);
            }
        }
    }

    public void Dispose() => _httpClient.Dispose();

    private HttpRequestMessage CreateRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd(UserAgent);

        if (_githubToken is not null && new Uri(url).Host.Equals("api.github.com", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
        }

        return request;
    }
}

public static class Hashing
{
    public static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        var hash = await System.Security.Cryptography.SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
