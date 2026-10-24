using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Tests;

public class PathRepairPreviewTests
{
    [Fact]
    public void Build_MergesSameLevelPlansAndDeduplicatesEntries()
    {
        var plans = new[]
        {
            Plan(
                EnvironmentLevel.Machine,
                new PathChange(PathChangeKind.Add, @"C:\Python313", PathChangeReason.MissingFromPath),
                new PathChange(PathChangeKind.Add, @"C:\Python313\Scripts", PathChangeReason.MissingFromPath)),
            Plan(
                EnvironmentLevel.Machine,
                new PathChange(PathChangeKind.Add, @"C:\Python313\", PathChangeReason.MissingFromPath)),
        };

        var section = Assert.Single(PathRepairPreview.Build(plans));

        Assert.Equal(EnvironmentLevel.Machine, section.Level);
        Assert.Equal(2, section.Changes.Count);
        Assert.Contains(section.Changes, change => change.Entry == @"C:\Python313");
        Assert.Contains(section.Changes, change => change.Entry == @"C:\Python313\Scripts");
    }

    [Fact]
    public void Build_OrdersChangesByAddThenMoveThenRemove()
    {
        var plans = new[]
        {
            Plan(
                EnvironmentLevel.Machine,
                new PathChange(PathChangeKind.Remove, @"C:\dup", PathChangeReason.Duplicate),
                new PathChange(PathChangeKind.Move, @"C:\move", PathChangeReason.LowerPriority),
                new PathChange(PathChangeKind.Add, @"C:\add", PathChangeReason.MissingFromPath)),
        };

        var changes = Assert.Single(PathRepairPreview.Build(plans)).Changes;

        Assert.Equal(
            new[] { PathChangeKind.Add, PathChangeKind.Move, PathChangeKind.Remove },
            changes.Select(change => change.Kind));
    }

    [Fact]
    public void Build_SeparatesLevelsAndListsMachineFirst()
    {
        var plans = new[]
        {
            Plan(
                EnvironmentLevel.User,
                new PathChange(PathChangeKind.Move, @"C:\user", PathChangeReason.WindowsAppsPriority)),
            Plan(
                EnvironmentLevel.Machine,
                new PathChange(PathChangeKind.Add, @"C:\machine", PathChangeReason.MissingFromPath)),
        };

        var sections = PathRepairPreview.Build(plans);

        Assert.Equal(2, sections.Count);
        Assert.Equal(EnvironmentLevel.Machine, sections[0].Level);
        Assert.Equal(EnvironmentLevel.User, sections[1].Level);
    }

    [Fact]
    public void Build_ReturnsEmptyForNoPlans()
    {
        Assert.Empty(PathRepairPreview.Build(Array.Empty<PathRepairPlan>()));
    }

    private static PathRepairPlan Plan(EnvironmentLevel level, params PathChange[] changes)
    {
        return new PathRepairPlan(level, PathValue.Empty(level), string.Empty, changes);
    }
}
