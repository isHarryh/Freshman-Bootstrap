namespace FreshmanBootstrap.Core.Models;

public static class VersionUpgradePolicy
{
    // Only non-major and non-minor upgrades are allowed, so the installed and target versions must
    // share the same major and minor components while the target is strictly newer. Changes in the
    // third slot or any deeper slot (Git's revision, for example) count as non-major and non-minor.
    // Versions may carry any number of components, so missing slots are treated as zero.
    public static bool CanUpgrade(VersionNumber installed, VersionNumber target)
    {
        if (!installed.IsValid || !target.IsValid || target <= installed)
        {
            return false;
        }

        return Component(installed, 0) == Component(target, 0)
            && Component(installed, 1) == Component(target, 1);
    }

    private static int Component(VersionNumber version, int index)
    {
        return index < version.Parts.Count ? version.Parts[index] : 0;
    }
}
