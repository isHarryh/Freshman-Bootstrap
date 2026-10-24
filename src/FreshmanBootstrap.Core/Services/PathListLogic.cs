namespace FreshmanBootstrap.Core.Services;

public static class PathListLogic
{
    public static IReadOnlyList<string> Split(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    public static string Join(IEnumerable<string> entries) => string.Join(';', entries);

    public static string NormalizeDirectory(string path) => path.Trim().TrimEnd('\\', '/');

    public static bool DirectoryEquals(string left, string right)
    {
        return string.Equals(NormalizeDirectory(left), NormalizeDirectory(right), StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<int> IndexesOf(IReadOnlyList<string> entries, Func<string, bool> predicate)
    {
        var indexes = new List<int>();
        for (var i = 0; i < entries.Count; i++)
        {
            if (predicate(entries[i]))
            {
                indexes.Add(i);
            }
        }

        return indexes;
    }
}
