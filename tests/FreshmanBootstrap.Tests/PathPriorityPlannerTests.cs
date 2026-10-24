using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Tests;

public class PathPriorityPlannerTests
{
    private static readonly Func<string, string> Identity = value => value;

    [Fact]
    public void PlanExecutableFirst_ReturnsNull_WhenTargetIsAlreadyFirst()
    {
        var environment = new TestEnvironment().WithMachinePath(@"X:\Git\cmd", @"X:\OtherGit\cmd");
        environment.Files.AddFile(@"X:\Git\cmd\git.exe");
        environment.Files.AddFile(@"X:\OtherGit\cmd\git.exe");

        var plan = PathPriorityPlanner.PlanExecutableFirst(
            @"X:\Git\cmd",
            new[] { "git.exe" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Files,
            Identity);

        Assert.Null(plan);
    }

    [Fact]
    public void PlanExecutableFirst_MovesTargetBeforeOtherCandidate()
    {
        var environment = new TestEnvironment().WithMachinePath(@"X:\OldGit\cmd", @"X:\NewGit\cmd", @"X:\Tools");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.Files.AddFile(@"X:\NewGit\cmd\git.exe");

        var plan = PathPriorityPlanner.PlanExecutableFirst(
            @"X:\NewGit\cmd",
            new[] { "git.exe" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Files,
            Identity);

        Assert.NotNull(plan);
        Assert.Equal(EnvironmentLevel.Machine, plan!.Level);
        Assert.Equal(@"X:\NewGit\cmd;X:\OldGit\cmd;X:\Tools", plan.NewValue);
        var change = Assert.Single(plan.Changes);
        Assert.Equal(PathChangeKind.Move, change.Kind);
    }

    [Fact]
    public void PlanExecutableFirst_AddsTargetToMachine_WhenMissingFromPath()
    {
        var environment = new TestEnvironment().WithMachinePath(@"X:\OldGit\cmd", @"X:\Tools");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.Files.AddFile(@"X:\NewGit\cmd\git.exe");

        var plan = PathPriorityPlanner.PlanExecutableFirst(
            @"X:\NewGit\cmd",
            new[] { "git.exe" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Files,
            Identity);

        Assert.NotNull(plan);
        Assert.Equal(EnvironmentLevel.Machine, plan!.Level);
        Assert.Equal(@"X:\NewGit\cmd;X:\OldGit\cmd;X:\Tools", plan.NewValue);
        var change = Assert.Single(plan.Changes);
        Assert.Equal(PathChangeKind.Add, change.Kind);
    }

    [Fact]
    public void PlanExecutableFirst_PromotesUserTargetToMachine_WhenMachineCandidateExists()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\OldGit\cmd")
            .WithUserPath(@"X:\NewGit\cmd");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.Files.AddFile(@"X:\NewGit\cmd\git.exe");

        var plan = PathPriorityPlanner.PlanExecutableFirst(
            @"X:\NewGit\cmd",
            new[] { "git.exe" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Files,
            Identity);

        Assert.NotNull(plan);
        Assert.Equal(EnvironmentLevel.Machine, plan!.Level);
        Assert.Equal(@"X:\NewGit\cmd;X:\OldGit\cmd", plan.NewValue);
    }

    [Fact]
    public void PlanExecutableFirst_ReordersUserPath_WhenNoMachineCandidates()
    {
        var environment = new TestEnvironment()
            .WithUserPath(@"X:\OldGit\cmd", @"X:\NewGit\cmd");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.Files.AddFile(@"X:\NewGit\cmd\git.exe");

        var plan = PathPriorityPlanner.PlanExecutableFirst(
            @"X:\NewGit\cmd",
            new[] { "git.exe" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Files,
            Identity);

        Assert.NotNull(plan);
        Assert.Equal(EnvironmentLevel.User, plan!.Level);
        Assert.Equal(@"X:\NewGit\cmd;X:\OldGit\cmd", plan.NewValue);
    }

    [Fact]
    public void PlanExecutableFirst_RemovesDuplicateTargetEntries()
    {
        var environment = new TestEnvironment().WithMachinePath(@"X:\OldGit\cmd", @"X:\NewGit\cmd", @"X:\NewGit\cmd");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.Files.AddFile(@"X:\NewGit\cmd\git.exe");

        var plan = PathPriorityPlanner.PlanExecutableFirst(
            @"X:\NewGit\cmd",
            new[] { "git.exe" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Files,
            Identity);

        Assert.NotNull(plan);
        Assert.Equal(@"X:\NewGit\cmd;X:\OldGit\cmd", plan!.NewValue);
        Assert.Contains(plan.Changes, change => change.Kind == PathChangeKind.Move);
        Assert.Contains(plan.Changes, change => change.Kind == PathChangeKind.Remove);
    }

    [Fact]
    public void PlanExecutableFirst_ExpandsEnvironmentVariablesInPathEntries()
    {
        var environment = new TestEnvironment().WithMachinePath(@"%XROOT%\OldGit\cmd", @"X:\NewGit\cmd");
        environment.Files.AddFile(@"X:\OldGit\cmd\git.exe");
        environment.Files.AddFile(@"X:\NewGit\cmd\git.exe");

        var plan = PathPriorityPlanner.PlanExecutableFirst(
            @"X:\NewGit\cmd",
            new[] { "git.exe" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Files,
            value => value.Replace("%XROOT%", "X:", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(plan);
        Assert.Equal(@"X:\NewGit\cmd;%XROOT%\OldGit\cmd", plan!.NewValue);
    }

    [Fact]
    public void PlanWindowsAppsPriority_MovesPythonBeforeWindowsApps()
    {
        var environment = new TestEnvironment()
            .WithUserPath(@"X:\Users\tester\AppData\Local\Microsoft\WindowsApps", @"X:\Python313", @"X:\Tools");

        var plan = PathPriorityPlanner.PlanWindowsAppsPriority(
            new[] { @"X:\Python313" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Services.WindowsAppsDirectory,
            Identity);

        Assert.NotNull(plan);
        Assert.Equal(EnvironmentLevel.User, plan!.Level);
        Assert.Equal(@"X:\Python313;X:\Users\tester\AppData\Local\Microsoft\WindowsApps;X:\Tools", plan.NewValue);
        var change = Assert.Single(plan.Changes);
        Assert.Equal(PathChangeReason.WindowsAppsPriority, change.Reason);
    }

    [Fact]
    public void PlanWindowsAppsPriority_ReturnsNull_WhenPythonAlreadyPrecedes()
    {
        var environment = new TestEnvironment()
            .WithUserPath(@"X:\Python313", @"X:\Users\tester\AppData\Local\Microsoft\WindowsApps");

        var plan = PathPriorityPlanner.PlanWindowsAppsPriority(
            new[] { @"X:\Python313" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Services.WindowsAppsDirectory,
            Identity);

        Assert.Null(plan);
    }

    [Fact]
    public void PlanWindowsAppsPriority_ReturnsNull_WhenPythonIsMachineLevel()
    {
        var environment = new TestEnvironment()
            .WithMachinePath(@"X:\Python313")
            .WithUserPath(@"X:\Users\tester\AppData\Local\Microsoft\WindowsApps");

        var plan = PathPriorityPlanner.PlanWindowsAppsPriority(
            new[] { @"X:\Python313" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Services.WindowsAppsDirectory,
            Identity);

        Assert.Null(plan);
    }

    [Fact]
    public void PlanWindowsAppsPriority_ReturnsNull_WhenWindowsAppsMissing()
    {
        var environment = new TestEnvironment().WithUserPath(@"X:\Python313");

        var plan = PathPriorityPlanner.PlanWindowsAppsPriority(
            new[] { @"X:\Python313" },
            environment.PathStore.Machine,
            environment.PathStore.User,
            environment.Services.WindowsAppsDirectory,
            Identity);

        Assert.Null(plan);
    }
}
