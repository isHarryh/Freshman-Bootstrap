using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Services;

public static class PathRepairPreview
{
    // Groups the preview plans of a single repair pass into one entry list per PATH level.
    // Entries are deduplicated so that overlapping plans (for example Python's "add directories"
    // and "make python.exe first" operations) show each affected entry only once, and each
    // level lists its changes in the order add, move, remove.
    public static IReadOnlyList<PathRepairSection> Build(IReadOnlyList<PathRepairPlan> plans)
    {
        var byLevel = new Dictionary<EnvironmentLevel, List<PathChange>>();
        var seen = new Dictionary<EnvironmentLevel, HashSet<string>>();

        foreach (var plan in plans)
        {
            if (!byLevel.TryGetValue(plan.Level, out var changes))
            {
                changes = new List<PathChange>();
                byLevel[plan.Level] = changes;
                seen[plan.Level] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var keys = seen[plan.Level];
            foreach (var change in plan.Changes)
            {
                if (keys.Add(PathListLogic.NormalizeDirectory(change.Entry)))
                {
                    changes.Add(change);
                }
            }
        }

        return byLevel.Keys
            .OrderBy(level => level)
            .Select(level => new PathRepairSection(level, Sort(byLevel[level])))
            .ToList();
    }

    private static IReadOnlyList<PathChange> Sort(List<PathChange> changes)
    {
        return changes
            .OrderBy(change => change.Kind)
            .ThenBy(change => change.Entry, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
