namespace FreshmanBootstrap.Core.Abstractions;

public interface IFileProbe
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    long GetFileSize(string path);

    IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern);
}
