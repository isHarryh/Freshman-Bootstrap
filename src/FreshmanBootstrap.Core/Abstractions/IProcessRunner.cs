using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Abstractions;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string executablePath,
        string arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
