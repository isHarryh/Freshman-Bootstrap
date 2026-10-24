using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Services;

public sealed class PathEnvironmentService
{
    private readonly IPathStore _store;
    private readonly IEnvironmentBroadcaster _broadcaster;

    public PathEnvironmentService(IPathStore store, IEnvironmentBroadcaster broadcaster)
    {
        _store = store;
        _broadcaster = broadcaster;
    }

    public PathValue MachinePath => _store.Read(EnvironmentLevel.Machine);

    public PathValue UserPath => _store.Read(EnvironmentLevel.User);

    public IReadOnlyList<string> MachineEntries => PathListLogic.Split(MachinePath.RawValue);

    public IReadOnlyList<string> UserEntries => PathListLogic.Split(UserPath.RawValue);

    public string Expand(string entry) => Environment.ExpandEnvironmentVariables(entry);

    public void Apply(PathRepairPlan plan)
    {
        _store.Write(plan.Level, plan.NewValue, plan.Original.IsExpandable);
        _broadcaster.BroadcastEnvironmentChanged();
    }
}
