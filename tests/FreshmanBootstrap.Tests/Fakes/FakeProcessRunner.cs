using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Tests.Fakes;

internal sealed class FakeProcessRunner : IProcessRunner
{
    private readonly Dictionary<string, string> _outputs = new(StringComparer.OrdinalIgnoreCase);

    public void SetOutput(string executablePath, string standardOutput)
    {
        _outputs[executablePath] = standardOutput;
    }

    public Task<ProcessResult> RunAsync(
        string executablePath,
        string arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var output = _outputs.TryGetValue(executablePath, out var value) ? value : string.Empty;
        var exitCode = _outputs.ContainsKey(executablePath) ? 0 : 1;
        return Task.FromResult(new ProcessResult(exitCode, output, string.Empty, false));
    }
}
