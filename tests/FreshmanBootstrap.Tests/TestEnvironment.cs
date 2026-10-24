using FreshmanBootstrap.Core.Models;
using FreshmanBootstrap.Core.Services;
using FreshmanBootstrap.Tests.Fakes;
using Serilog;

namespace FreshmanBootstrap.Tests;

internal sealed class TestEnvironment
{
    public TestEnvironment()
    {
        Services = new CoreServices
        {
            ProcessRunner = ProcessRunner,
            Files = Files,
            Registry = Registry,
            PathEnvironment = new PathEnvironmentService(PathStore, Broadcaster),
            Payloads = new PayloadProvider(null),
            Logger = new LoggerConfiguration().CreateLogger(),
            ProgramFilesDirectory = @"X:\Program Files",
            ProgramFilesX86Directory = @"X:\Program Files (x86)",
            LocalAppDataDirectory = @"X:\Users\tester\AppData\Local",
            WindowsAppsDirectory = @"X:\Users\tester\AppData\Local\Microsoft\WindowsApps",
        };
    }

    public FakeProcessRunner ProcessRunner { get; } = new();

    public FakeFileProbe Files { get; } = new();

    public FakeRegistryProbe Registry { get; } = new();

    public FakePathStore PathStore { get; } = new();

    public FakeEnvironmentBroadcaster Broadcaster { get; } = new();

    public CoreServices Services { get; }

    public TestEnvironment WithMachinePath(params string[] entries)
    {
        PathStore.Machine = new PathValue(EnvironmentLevel.Machine, string.Join(';', entries), true);
        return this;
    }

    public TestEnvironment WithUserPath(params string[] entries)
    {
        PathStore.User = new PathValue(EnvironmentLevel.User, string.Join(';', entries), true);
        return this;
    }

    public TestEnvironment WithProgram(string executablePath, string versionOutput)
    {
        Files.AddFile(executablePath);
        ProcessRunner.SetOutput(executablePath, versionOutput);
        return this;
    }
}
