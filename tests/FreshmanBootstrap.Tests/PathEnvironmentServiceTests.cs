using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Tests;

public class PathEnvironmentServiceTests
{
    [Fact]
    public void Apply_WritesPlanValueAndBroadcasts()
    {
        var environment = new TestEnvironment().WithMachinePath(@"X:\Old");
        var service = environment.Services.PathEnvironment;
        var original = service.MachinePath;

        var plan = new PathRepairPlan(
            EnvironmentLevel.Machine,
            original,
            @"X:\New;X:\Old",
            new[] { new PathChange(PathChangeKind.Add, @"X:\New", PathChangeReason.MissingFromPath) });

        service.Apply(plan);

        Assert.Equal(@"X:\New;X:\Old", environment.PathStore.Machine.RawValue);
        Assert.Equal(1, environment.Broadcaster.BroadcastCount);
    }

    [Fact]
    public void Entries_SplitOnSemicolonsAndTrim()
    {
        var environment = new TestEnvironment().WithMachinePath(@" X:\A ; ;X:\B ");
        var service = environment.Services.PathEnvironment;

        Assert.Equal(new[] { @"X:\A", @"X:\B" }, service.MachineEntries);
    }
}
