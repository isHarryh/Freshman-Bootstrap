using System.Runtime.InteropServices;
using FreshmanBootstrap.Core.Abstractions;
using FreshmanBootstrap.Core.Models;
using Microsoft.Win32;

namespace FreshmanBootstrap.App.Services;

public sealed class RegistryPathStore : IPathStore
{
    private const string MachineEnvironmentKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
    private const string UserEnvironmentKeyPath = "Environment";

    public PathValue Read(EnvironmentLevel level)
    {
        using var baseKey = RegistryKey.OpenBaseKey(ToHive(level), RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(ToKeyPath(level));
        if (key is null)
        {
            return PathValue.Empty(level);
        }

        if (key.GetValue("Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames) is not string raw)
        {
            return PathValue.Empty(level);
        }

        var expandable = key.GetValueKind("Path") == RegistryValueKind.ExpandString;
        return new PathValue(level, raw, expandable);
    }

    public void Write(EnvironmentLevel level, string value, bool expandable)
    {
        using var baseKey = RegistryKey.OpenBaseKey(ToHive(level), RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(ToKeyPath(level), writable: true)
            ?? throw new InvalidOperationException($"Cannot open environment registry key for {level}.");
        key.SetValue("Path", value, expandable ? RegistryValueKind.ExpandString : RegistryValueKind.String);
    }

    private static RegistryHive ToHive(EnvironmentLevel level)
    {
        return level == EnvironmentLevel.Machine ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
    }

    private static string ToKeyPath(EnvironmentLevel level)
    {
        return level == EnvironmentLevel.Machine ? MachineEnvironmentKeyPath : UserEnvironmentKeyPath;
    }
}

public sealed class NativeEnvironmentBroadcaster : IEnvironmentBroadcaster
{
    private const uint WmSettingChange = 0x001A;
    private const uint SmtoAbortIfHung = 0x0002;
    private static readonly IntPtr HwndBroadcast = new(0xffff);
    private static readonly IntPtr SmtoTimeout = new(5000);

    public void BroadcastEnvironmentChanged()
    {
        SendMessageTimeout(
            HwndBroadcast,
            WmSettingChange,
            IntPtr.Zero,
            "Environment",
            SmtoAbortIfHung,
            (uint)SmtoTimeout,
            out _);
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        string lParam,
        uint fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult);
}
