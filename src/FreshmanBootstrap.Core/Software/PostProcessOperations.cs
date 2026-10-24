using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;

namespace FreshmanBootstrap.Core.Software;

public abstract class PathRepairOperationBase : IPathRepairOperation
{
    protected PathRepairOperationBase(CoreServices services, string id, string description)
    {
        Services = services;
        Id = id;
        Description = description;
    }

    protected CoreServices Services { get; }

    public string Id { get; }

    public string Description { get; }

    public abstract Task<PathRepairPlan?> PlanAsync(CancellationToken cancellationToken = default);

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return await PlanAsync(cancellationToken) is null;
    }

    public Task ExecuteAsync(PathRepairPlan plan, CancellationToken cancellationToken = default)
    {
        Services.PathEnvironment.Apply(plan);
        return Task.CompletedTask;
    }
}

public sealed class EnsureExecutableFirstOperation : PathRepairOperationBase
{
    private readonly string _targetDirectory;
    private readonly IReadOnlyList<string> _executableFileNames;

    public EnsureExecutableFirstOperation(
        CoreServices services,
        string id,
        string description,
        string targetDirectory,
        IReadOnlyList<string> executableFileNames)
        : base(services, id, description)
    {
        _targetDirectory = targetDirectory;
        _executableFileNames = executableFileNames;
    }

    public override Task<PathRepairPlan?> PlanAsync(CancellationToken cancellationToken = default)
    {
        var plan = PathPriorityPlanner.PlanExecutableFirst(
            _targetDirectory,
            _executableFileNames,
            Services.PathEnvironment.MachinePath,
            Services.PathEnvironment.UserPath,
            Services.Files,
            Services.PathEnvironment.Expand);

        return Task.FromResult(plan);
    }
}

public sealed class EnsurePathEntriesOperation : PathRepairOperationBase
{
    private readonly IReadOnlyList<string> _directories;

    public EnsurePathEntriesOperation(
        CoreServices services,
        string id,
        string description,
        IReadOnlyList<string> directories)
        : base(services, id, description)
    {
        _directories = directories;
    }

    public override Task<PathRepairPlan?> PlanAsync(CancellationToken cancellationToken = default)
    {
        var machinePath = Services.PathEnvironment.MachinePath;
        var machineEntries = PathListLogic.Split(machinePath.RawValue).ToList();
        var userEntries = Services.PathEnvironment.UserEntries;

        var missing = _directories
            .Where(directory => !ContainsEntry(machineEntries, directory) && !ContainsEntry(userEntries, directory))
            .ToList();

        if (missing.Count == 0)
        {
            return Task.FromResult<PathRepairPlan?>(null);
        }

        var changes = new List<PathChange>();
        var insertAt = 0;
        foreach (var directory in missing)
        {
            machineEntries.Insert(insertAt, directory);
            changes.Add(new PathChange(PathChangeKind.Add, directory, PathChangeReason.MissingFromPath));
            insertAt++;
        }

        var plan = new PathRepairPlan(machinePath.Level, machinePath, PathListLogic.Join(machineEntries), changes);
        return Task.FromResult<PathRepairPlan?>(plan);
    }

    private bool ContainsEntry(IReadOnlyList<string> entries, string directory)
    {
        return entries.Any(entry => PathListLogic.DirectoryEquals(Services.PathEnvironment.Expand(entry), directory));
    }
}

public sealed class WindowsAppsPriorityOperation : PathRepairOperationBase
{
    private readonly IReadOnlyList<string> _pythonDirectories;

    public WindowsAppsPriorityOperation(
        CoreServices services,
        string id,
        string description,
        IReadOnlyList<string> pythonDirectories)
        : base(services, id, description)
    {
        _pythonDirectories = pythonDirectories;
    }

    public override Task<PathRepairPlan?> PlanAsync(CancellationToken cancellationToken = default)
    {
        var plan = PathPriorityPlanner.PlanWindowsAppsPriority(
            _pythonDirectories,
            Services.PathEnvironment.MachinePath,
            Services.PathEnvironment.UserPath,
            Services.WindowsAppsDirectory,
            Services.PathEnvironment.Expand);

        return Task.FromResult(plan);
    }
}
