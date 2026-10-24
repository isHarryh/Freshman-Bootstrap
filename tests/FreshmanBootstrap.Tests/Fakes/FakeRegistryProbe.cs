using FreshmanBootstrap.Core.Abstractions;

namespace FreshmanBootstrap.Tests.Fakes;

internal sealed class FakeRegistryProbe : IRegistryProbe
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _subKeys = new(StringComparer.OrdinalIgnoreCase);

    public List<UninstallEntry> UninstallEntries { get; } = new();

    public void SetValue(RegistryRoot root, RegistryBitness bitness, string subKeyPath, string? valueName, string value)
    {
        _values[Key(root, bitness, subKeyPath, valueName)] = value;
    }

    public void AddSubKey(RegistryRoot root, RegistryBitness bitness, string subKeyPath, string name)
    {
        var key = Key(root, bitness, subKeyPath, null);
        if (!_subKeys.TryGetValue(key, out var list))
        {
            list = new List<string>();
            _subKeys[key] = list;
        }

        list.Add(name);
    }

    public string? GetValue(RegistryRoot root, RegistryBitness bitness, string subKeyPath, string? valueName)
    {
        return _values.TryGetValue(Key(root, bitness, subKeyPath, valueName), out var value) ? value : null;
    }

    public IReadOnlyList<string> GetSubKeyNames(RegistryRoot root, RegistryBitness bitness, string subKeyPath)
    {
        return _subKeys.TryGetValue(Key(root, bitness, subKeyPath, null), out var list)
            ? list
            : Array.Empty<string>();
    }

    public IReadOnlyList<UninstallEntry> FindUninstallEntries(string displayNameContains)
    {
        return UninstallEntries
            .Where(entry => entry.DisplayName.Contains(displayNameContains, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static string Key(RegistryRoot root, RegistryBitness bitness, string subKeyPath, string? valueName)
    {
        return $"{root}|{bitness}|{subKeyPath}|{valueName ?? "<default>"}";
    }
}
