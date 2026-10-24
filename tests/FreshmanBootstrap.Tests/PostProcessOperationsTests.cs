using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Software;

namespace FreshmanBootstrap.Tests;

public class PostProcessOperationsTests
{
    [Fact]
    public async Task EnsurePathEntries_IsSatisfied_WhenDirectoryExistsInUserPath()
    {
        var environment = new TestEnvironment().WithUserPath(@"X:\Python313");
        var operation = new EnsurePathEntriesOperation(
            environment.Services,
            "python.path.add",
            "test",
            new[] { @"X:\Python313" });

        Assert.True(await operation.IsSatisfiedAsync());
    }

    [Fact]
    public async Task EnsurePathEntries_PlansAndAppliesMachineAdditions()
    {
        var environment = new TestEnvironment().WithMachinePath(@"X:\Tools");
        var operation = new EnsurePathEntriesOperation(
            environment.Services,
            "python.path.add",
            "test",
            new[] { @"X:\Python313", @"X:\Python313\Scripts" });

        Assert.False(await operation.IsSatisfiedAsync());

        var plan = await operation.PlanAsync();
        Assert.NotNull(plan);
        Assert.Equal(@"X:\Python313;X:\Python313\Scripts;X:\Tools", plan!.NewValue);
        Assert.Equal(2, plan.Changes.Count);
        Assert.All(plan.Changes, change => Assert.Equal(PathChangeKind.Add, change.Kind));

        await operation.ExecuteAsync(plan);

        Assert.Equal(plan.NewValue, environment.PathStore.Machine.RawValue);
        Assert.Equal(1, environment.Broadcaster.BroadcastCount);
    }

    [Fact]
    public async Task EnsureExecutableFirst_IsSatisfied_AfterReordering()
    {
        var environment = new TestEnvironment().WithMachinePath(@"X:\OldGit\cmd", @"X:\NewGit\cmd");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.Files.AddFile(@"X:\NewGit\cmd\git.exe");

        var operation = new EnsureExecutableFirstOperation(
            environment.Services,
            "git.path.first",
            "test",
            @"X:\NewGit\cmd",
            new[] { "git.exe" });

        Assert.False(await operation.IsSatisfiedAsync());

        var plan = await operation.PlanAsync();
        Assert.NotNull(plan);

        await operation.ExecuteAsync(plan!);

        Assert.True(await operation.IsSatisfiedAsync());
        Assert.Equal(@"X:\NewGit\cmd;X:\OldGit\cmd", environment.PathStore.Machine.RawValue);
    }

    [Fact]
    public async Task WindowsAppsPriority_AppliesUserLevelRepair()
    {
        var environment = new TestEnvironment()
            .WithUserPath(
                @"X:\Users\tester\AppData\Local\Microsoft\WindowsApps",
                @"X:\Python313");

        var operation = new WindowsAppsPriorityOperation(
            environment.Services,
            "python.path.windowsapps",
            "test",
            new[] { @"X:\Python313" });

        Assert.False(await operation.IsSatisfiedAsync());

        var plan = await operation.PlanAsync();
        Assert.NotNull(plan);
        Assert.Equal(EnvironmentLevel.User, plan!.Level);

        await operation.ExecuteAsync(plan);

        Assert.True(await operation.IsSatisfiedAsync());
        Assert.StartsWith(@"X:\Python313", environment.PathStore.User.RawValue);
    }
}
