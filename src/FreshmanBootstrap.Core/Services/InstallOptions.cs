using FreshmanBootstrap.Core.Abstractions;

namespace FreshmanBootstrap.Core.Services;

public sealed class BoolInstallOption : IInstallOption
{
    private readonly bool _recommended;

    public BoolInstallOption(string id, string label, bool recommendedValue)
    {
        Id = id;
        Label = label;
        _recommended = recommendedValue;
        Value = recommendedValue;
    }

    public string Id { get; }

    public string Label { get; }

    public InstallOptionKind Kind => InstallOptionKind.Boolean;

    public IReadOnlyList<string> RecommendedValues => new[] { _recommended ? "true" : "false" };

    public object? Value { get; set; }

    public bool BoolValue => Value is bool value ? value : _recommended;

    public string? Validate() => null;
}

public sealed class PathInstallOption : IInstallOption
{
    private readonly string _recommendedDirectory;

    public PathInstallOption(string id, string label, string recommendedDirectory)
    {
        Id = id;
        Label = label;
        _recommendedDirectory = recommendedDirectory;
        Value = recommendedDirectory;
    }

    public string Id { get; }

    public string Label { get; }

    public InstallOptionKind Kind => InstallOptionKind.Path;

    public IReadOnlyList<string> RecommendedValues => new[] { _recommendedDirectory };

    public object? Value { get; set; }

    public string PathValue => Value as string ?? string.Empty;

    public string? Validate()
    {
        var value = PathValue.Trim();
        if (value.Length == 0)
        {
            return Strings.InstallOptions.PathRequired;
        }

        if (!Path.IsPathFullyQualified(value))
        {
            return Strings.InstallOptions.PathMustBeAbsolute;
        }

        if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return Strings.InstallOptions.PathInvalid;
        }

        var root = Path.GetPathRoot(value);
        if (!string.IsNullOrEmpty(root) && PathListLogic.DirectoryEquals(root, value))
        {
            return Strings.InstallOptions.PathIsRoot;
        }

        if (File.Exists(value))
        {
            return Strings.InstallOptions.PathIsFile;
        }

        return null;
    }
}
