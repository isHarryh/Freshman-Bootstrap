using FreshmanBootstrap.Core.Models;

namespace FreshmanBootstrap.Core.Abstractions;

public enum InstallOptionKind
{
    Boolean,
    Path,
}

public interface IInstallOption
{
    string Id { get; }

    string Label { get; }

    InstallOptionKind Kind { get; }

    IReadOnlyList<string> RecommendedValues { get; }

    object? Value { get; set; }

    string? Validate();
}

public interface IPostProcessOperation
{
    string Id { get; }

    string Description { get; }

    Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default);
}

public interface IPathRepairOperation : IPostProcessOperation
{
    Task<PathRepairPlan?> PlanAsync(CancellationToken cancellationToken = default);

    Task ExecuteAsync(PathRepairPlan plan, CancellationToken cancellationToken = default);
}

public interface ISoftware
{
    string Id { get; }

    string DisplayName { get; }

    string Description { get; }

    string? PackagedVersion { get; }

    Task<SoftwareInspection> InspectAsync(CancellationToken cancellationToken = default);

    IReadOnlyList<IInstallOption> CreateInstallOptions();

    IReadOnlyList<IPostProcessOperation> CreatePostProcessOperations(SoftwareInspection inspection);

    Task<InstallResult> InstallAsync(
        IReadOnlyList<IInstallOption> options,
        IProgress<InstallProgress>? progress,
        CancellationToken cancellationToken = default);
}
