namespace FreshmanBootstrap.Core.Models;

public enum EnvironmentLevel
{
    Machine,
    User,
}

public sealed record PathValue(EnvironmentLevel Level, string RawValue, bool IsExpandable)
{
    public static PathValue Empty(EnvironmentLevel level) => new(level, string.Empty, true);
}

public enum PathChangeKind
{
    Add,
    Move,
    Remove,
}

public enum PathChangeReason
{
    MissingFromPath,
    LowerPriority,
    WindowsAppsPriority,
    Duplicate,
}

public sealed record PathChange(
    PathChangeKind Kind,
    string Entry,
    PathChangeReason Reason);

public sealed record PathRepairPlan(
    EnvironmentLevel Level,
    PathValue Original,
    string NewValue,
    IReadOnlyList<PathChange> Changes);
