using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Tests.Fakes;

internal sealed class FakePathStore : IPathStore
{
    public PathValue Machine { get; set; } = PathValue.Empty(EnvironmentLevel.Machine);

    public PathValue User { get; set; } = PathValue.Empty(EnvironmentLevel.User);

    public PathValue Read(EnvironmentLevel level) => level == EnvironmentLevel.Machine ? Machine : User;

    public void Write(EnvironmentLevel level, string value, bool expandable)
    {
        var pathValue = new PathValue(level, value, expandable);
        if (level == EnvironmentLevel.Machine)
        {
            Machine = pathValue;
        }
        else
        {
            User = pathValue;
        }
    }
}

internal sealed class FakeEnvironmentBroadcaster : IEnvironmentBroadcaster
{
    public int BroadcastCount { get; private set; }

    public void BroadcastEnvironmentChanged() => BroadcastCount++;
}
