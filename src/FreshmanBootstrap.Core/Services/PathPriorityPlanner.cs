using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Services;

public static class PathPriorityPlanner
{
    public static PathRepairPlan? PlanExecutableFirst(
        string targetDirectory,
        IReadOnlyList<string> executableFileNames,
        PathValue machinePath,
        PathValue userPath,
        IFileProbe files,
        Func<string, string> expandEnvironment)
    {
        var machineEntries = PathListLogic.Split(machinePath.RawValue).ToList();
        var userEntries = PathListLogic.Split(userPath.RawValue).ToList();

        bool IsTarget(string rawEntry)
        {
            return PathListLogic.DirectoryEquals(expandEnvironment(rawEntry), targetDirectory);
        }

        bool IsCandidate(string rawEntry)
        {
            var directory = expandEnvironment(rawEntry);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            foreach (var fileName in executableFileNames)
            {
                if (files.FileExists(Path.Combine(directory, fileName)))
                {
                    return true;
                }
            }

            return false;
        }

        var machineCandidates = PathListLogic.IndexesOf(machineEntries, IsCandidate);
        var userCandidates = PathListLogic.IndexesOf(userEntries, IsCandidate);
        var machineTargets = PathListLogic.IndexesOf(machineEntries, IsTarget);
        var userTargets = PathListLogic.IndexesOf(userEntries, IsTarget);

        if (machineCandidates.Count > 0 && IsTarget(machineEntries[machineCandidates[0]]))
        {
            return null;
        }

        if (machineCandidates.Count == 0 && userCandidates.Count > 0 && IsTarget(userEntries[userCandidates[0]]))
        {
            return null;
        }

        if (machineTargets.Count > 0)
        {
            var insertAt = machineCandidates.Count > 0 ? machineCandidates[0] : 0;
            return BuildMovePlan(machinePath, machineEntries, machineTargets, insertAt);
        }

        if (machineCandidates.Count == 0 && userTargets.Count > 0)
        {
            var insertAt = userCandidates.Count > 0 ? userCandidates[0] : 0;
            return BuildMovePlan(userPath, userEntries, userTargets, insertAt);
        }

        var addIndex = machineCandidates.Count > 0 ? machineCandidates[0] : 0;
        return BuildAddPlan(machinePath, machineEntries, targetDirectory, addIndex);
    }

    public static PathRepairPlan? PlanWindowsAppsPriority(
        IReadOnlyList<string> pythonDirectories,
        PathValue machinePath,
        PathValue userPath,
        string windowsAppsDirectory,
        Func<string, string> expandEnvironment)
    {
        var machineEntries = PathListLogic.Split(machinePath.RawValue).ToList();
        var entries = PathListLogic.Split(userPath.RawValue).ToList();

        bool IsWindowsApps(string entry) =>
            PathListLogic.DirectoryEquals(expandEnvironment(entry), windowsAppsDirectory);

        if (!entries.Any(IsWindowsApps))
        {
            return null;
        }

        var changes = new List<PathChange>();

        foreach (var directory in pythonDirectories)
        {
            var satisfiedByMachine = machineEntries.Any(entry =>
                PathListLogic.DirectoryEquals(expandEnvironment(entry), directory));
            if (satisfiedByMachine)
            {
                continue;
            }

            var index = entries.FindIndex(entry => PathListLogic.DirectoryEquals(expandEnvironment(entry), directory));
            if (index < 0)
            {
                continue;
            }

            var windowsAppsIndex = entries.FindIndex(IsWindowsApps);
            if (windowsAppsIndex < 0 || index < windowsAppsIndex)
            {
                continue;
            }

            var entryValue = entries[index];
            entries.RemoveAt(index);
            entries.Insert(windowsAppsIndex, entryValue);
            changes.Add(new PathChange(
                PathChangeKind.Move,
                entryValue,
                PathChangeReason.WindowsAppsPriority));
        }

        if (changes.Count == 0)
        {
            return null;
        }

        return new PathRepairPlan(EnvironmentLevel.User, userPath, PathListLogic.Join(entries), changes);
    }

    private static PathRepairPlan BuildMovePlan(
        PathValue path,
        List<string> entries,
        IReadOnlyList<int> targetIndexes,
        int insertAt)
    {
        var targetSet = targetIndexes.ToHashSet();
        var movedEntry = entries[targetIndexes[0]];
        var insertIndex = Math.Clamp(insertAt, 0, entries.Count - targetIndexes.Count);

        var result = new List<string>(entries.Count);
        for (var i = 0; i < entries.Count; i++)
        {
            if (!targetSet.Contains(i))
            {
                result.Add(entries[i]);
            }
        }

        result.Insert(insertIndex, movedEntry);

        var changes = new List<PathChange>
        {
            new(PathChangeKind.Move, movedEntry, PathChangeReason.LowerPriority),
        };

        foreach (var duplicateIndex in targetIndexes.Skip(1))
        {
            changes.Add(new PathChange(PathChangeKind.Remove, entries[duplicateIndex], PathChangeReason.Duplicate));
        }

        return new PathRepairPlan(path.Level, path, PathListLogic.Join(result), changes);
    }

    private static PathRepairPlan BuildAddPlan(
        PathValue path,
        List<string> entries,
        string directory,
        int insertAt)
    {
        var result = new List<string>(entries);
        var index = Math.Clamp(insertAt, 0, result.Count);
        result.Insert(index, directory);

        var changes = new List<PathChange>
        {
            new(PathChangeKind.Add, directory, PathChangeReason.MissingFromPath),
        };

        return new PathRepairPlan(path.Level, path, PathListLogic.Join(result), changes);
    }
}
