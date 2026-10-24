namespace FreshmanBootstrap.Core.Services;

public static class AppPaths
{
    public const string PayloadEnvironmentVariable = "FRESHMAN_BOOTSTRAP_PAYLOAD";

    public static string DataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "FreshmanBootstrap");

    public static string LogDirectory => Path.Combine(DataDirectory, "logs");

    public static string? FindPayloadDirectory(string baseDirectory)
    {
        var environmentOverride = Environment.GetEnvironmentVariable(PayloadEnvironmentVariable);
        return PayloadLocator.Resolve(baseDirectory, environmentOverride, File.Exists);
    }
}

public static class PayloadLocator
{
    public const string ManifestFileName = "manifest.json";

    private const int MaxSearchDepth = 8;

    public static string? Resolve(string baseDirectory, string? environmentOverride, Func<string, bool> fileExists)
    {
        if (!string.IsNullOrWhiteSpace(environmentOverride) &&
            fileExists(Path.Combine(environmentOverride, ManifestFileName)))
        {
            return environmentOverride;
        }

        var current = new DirectoryInfo(baseDirectory);
        for (var depth = 0; depth < MaxSearchDepth && current is not null; depth++)
        {
            var candidate = Path.Combine(current.FullName, "payload");
            if (fileExists(Path.Combine(candidate, ManifestFileName)))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }
}
