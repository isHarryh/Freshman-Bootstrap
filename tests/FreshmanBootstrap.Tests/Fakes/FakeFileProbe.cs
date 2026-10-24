using System.Text.RegularExpressions;
using FreshmanBootstrap.Core.Abstractions;

namespace FreshmanBootstrap.Tests.Fakes;

internal sealed class FakeFileProbe : IFileProbe
{
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, long> _sizes = new(StringComparer.OrdinalIgnoreCase);

    public void AddFile(string path, long size = 1)
    {
        _files.Add(Normalize(path));
        _sizes[Normalize(path)] = size;
    }

    public void AddDirectory(string path)
    {
        _directories.Add(Normalize(path));
    }

    public bool FileExists(string path) => _files.Contains(Normalize(path));

    public bool DirectoryExists(string path) => _directories.Contains(Normalize(path));

    public long GetFileSize(string path) => _sizes.TryGetValue(Normalize(path), out var size) ? size : 0;

    public IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern)
    {
        var normalized = Normalize(path);
        var regex = new Regex(
            "^" + Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$",
            RegexOptions.IgnoreCase);

        return _directories
            .Where(directory => string.Equals(Path.GetDirectoryName(directory), normalized, StringComparison.OrdinalIgnoreCase))
            .Where(directory => regex.IsMatch(Path.GetFileName(directory)))
            .ToList();
    }

    private static string Normalize(string path) => path.Replace('/', '\\').TrimEnd('\\');
}
