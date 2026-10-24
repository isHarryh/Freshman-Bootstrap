using System.Globalization;
using System.Text.RegularExpressions;

namespace FreshmanBootstrap.Core.Models;

public readonly struct VersionNumber : IComparable<VersionNumber>, IEquatable<VersionNumber>
{
    private static readonly Regex VersionPattern = new(@"\d+(?:\.\d+)+", RegexOptions.Compiled);

    private readonly int[]? _parts;

    public VersionNumber(params int[] parts)
    {
        _parts = parts.Length == 0 ? null : parts;
    }

    public bool IsValid => _parts is { Length: > 0 };

    public IReadOnlyList<int> Parts => _parts ?? Array.Empty<int>();

    public static bool TryParse(string? text, out VersionNumber version)
    {
        version = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var match = VersionPattern.Match(text);
        if (!match.Success)
        {
            return false;
        }

        var segments = match.Value.Split('.');
        var parts = new int[segments.Length];
        for (var i = 0; i < segments.Length; i++)
        {
            if (!int.TryParse(segments[i], NumberStyles.None, CultureInfo.InvariantCulture, out parts[i]))
            {
                return false;
            }
        }

        version = new VersionNumber(parts);
        return true;
    }

    public static VersionNumber Parse(string text)
    {
        return TryParse(text, out var version) ? version : throw new FormatException($"Cannot parse a version number from '{text}'.");
    }

    public int CompareTo(VersionNumber other)
    {
        var length = Math.Max(Parts.Count, other.Parts.Count);
        for (var i = 0; i < length; i++)
        {
            var left = i < Parts.Count ? Parts[i] : 0;
            var right = i < other.Parts.Count ? other.Parts[i] : 0;
            var comparison = left.CompareTo(right);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }

    public bool Equals(VersionNumber other)
    {
        return IsValid == other.IsValid && CompareTo(other) == 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is VersionNumber other && Equals(other);
    }

    public override int GetHashCode()
    {
        var length = Parts.Count;
        while (length > 1 && Parts[length - 1] == 0)
        {
            length--;
        }

        var hash = new HashCode();
        for (var i = 0; i < length; i++)
        {
            hash.Add(Parts[i]);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return _parts is { Length: > 0 } ? string.Join('.', _parts) : string.Empty;
    }

    public static bool operator >(VersionNumber left, VersionNumber right) => left.CompareTo(right) > 0;

    public static bool operator <(VersionNumber left, VersionNumber right) => left.CompareTo(right) < 0;

    public static bool operator >=(VersionNumber left, VersionNumber right) => left.CompareTo(right) >= 0;

    public static bool operator <=(VersionNumber left, VersionNumber right) => left.CompareTo(right) <= 0;

    public static bool operator ==(VersionNumber left, VersionNumber right) => left.Equals(right);

    public static bool operator !=(VersionNumber left, VersionNumber right) => !left.Equals(right);
}
