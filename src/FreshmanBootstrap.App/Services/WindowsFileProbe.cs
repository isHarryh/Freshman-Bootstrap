using System.IO;
using FreshmanBootstrap.Core.Abstractions;

namespace FreshmanBootstrap.App.Services;

public sealed class WindowsFileProbe : IFileProbe
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public long GetFileSize(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern)
    {
        try
        {
            return Directory.GetDirectories(path, searchPattern);
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }
}
