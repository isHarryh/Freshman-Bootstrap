using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Abstractions;

public interface IPathStore
{
    PathValue Read(EnvironmentLevel level);

    void Write(EnvironmentLevel level, string value, bool expandable);
}

public interface IEnvironmentBroadcaster
{
    void BroadcastEnvironmentChanged();
}
