using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

public sealed class SoftwareCatalog
{
    public SoftwareCatalog(CoreServices services)
    {
        All = new ISoftware[]
        {
            new GitSoftware(services),
            new PowerShellSoftware(services),
            new VSCodeSoftware(services),
            new PythonSoftware(services),
            new NodeJsSoftware(services),
        };
    }

    public IReadOnlyList<ISoftware> All { get; }
}
