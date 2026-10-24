using FreshmanBootstrap.Core.Abstractions;
using Microsoft.Win32;

namespace FreshmanBootstrap.App.Services;

public sealed class WindowsRegistryProbe : IRegistryProbe
{
    private const string UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    public string? GetValue(RegistryRoot root, RegistryBitness bitness, string subKeyPath, string? valueName)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(ToHive(root), ToView(bitness));
            using var key = baseKey.OpenSubKey(subKeyPath);
            return key?.GetValue(valueName) as string;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public IReadOnlyList<string> GetSubKeyNames(RegistryRoot root, RegistryBitness bitness, string subKeyPath)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(ToHive(root), ToView(bitness));
            using var key = baseKey.OpenSubKey(subKeyPath);
            return key?.GetSubKeyNames() ?? Array.Empty<string>();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    public IReadOnlyList<UninstallEntry> FindUninstallEntries(string displayNameContains)
    {
        return LoadUninstallEntries()
            .Where(entry => entry.DisplayName.Contains(displayNameContains, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static IReadOnlyList<UninstallEntry> LoadUninstallEntries()
    {
        var entries = new List<UninstallEntry>();

        foreach (var root in new[] { RegistryRoot.LocalMachine, RegistryRoot.CurrentUser })
        {
            foreach (var bitness in new[] { RegistryBitness.X64, RegistryBitness.X86 })
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(ToHive(root), ToView(bitness));
                    using var uninstallKey = baseKey.OpenSubKey(UninstallKeyPath);
                    if (uninstallKey is null)
                    {
                        continue;
                    }

                    foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                    {
                        using var subKey = uninstallKey.OpenSubKey(subKeyName);
                        var displayName = subKey?.GetValue("DisplayName") as string;
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            continue;
                        }

                        entries.Add(new UninstallEntry(
                            displayName,
                            subKey?.GetValue("DisplayVersion") as string,
                            subKey?.GetValue("InstallLocation") as string,
                            root,
                            bitness));
                    }
                }
                catch (Exception)
                {
                    // Individual registry views can be unavailable; skip them.
                }
            }
        }

        return entries;
    }

    private static RegistryHive ToHive(RegistryRoot root)
    {
        return root == RegistryRoot.LocalMachine ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
    }

    private static RegistryView ToView(RegistryBitness bitness)
    {
        return bitness == RegistryBitness.X64 ? RegistryView.Registry64 : RegistryView.Registry32;
    }
}
