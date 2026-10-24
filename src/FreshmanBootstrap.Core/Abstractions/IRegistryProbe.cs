namespace FreshmanBootstrap.Core.Abstractions;

public enum RegistryRoot
{
    LocalMachine,
    CurrentUser,
}

public enum RegistryBitness
{
    X64,
    X86,
}

public sealed record UninstallEntry(
    string DisplayName,
    string? DisplayVersion,
    string? InstallLocation,
    RegistryRoot Root,
    RegistryBitness Bitness);

public interface IRegistryProbe
{
    string? GetValue(RegistryRoot root, RegistryBitness bitness, string subKeyPath, string? valueName);

    IReadOnlyList<string> GetSubKeyNames(RegistryRoot root, RegistryBitness bitness, string subKeyPath);

    IReadOnlyList<UninstallEntry> FindUninstallEntries(string displayNameContains);
}
