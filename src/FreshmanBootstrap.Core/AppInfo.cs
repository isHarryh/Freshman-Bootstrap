using System.Reflection;

namespace FreshmanBootstrap.Core;

public static class AppInfo
{
    public const string DisplayName = "萌新秒装";
    public const string EnglishName = "Freshman Bootstrap";
    public const string Author = "Harry Huang";
    public const string CopyrightText = "Copyright © Harry Huang";
    public const string PrepareArgument = "--prepare";
    public const string FreshArgument = "--fresh";

    public const string GitHubUrl = "https://github.com/isHarryh/Freshman-Bootstrap";

    public static string VersionText
    {
        get
        {
            var assembly = Assembly.GetEntryAssembly() ?? typeof(AppInfo).Assembly;
            var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrWhiteSpace(informational))
            {
                var plusIndex = informational.IndexOf('+');
                return plusIndex >= 0 ? informational[..plusIndex] : informational;
            }

            return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        }
    }
}
