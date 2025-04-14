#nullable enable
using Semver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public record DataSetVersionNumber(int? Major, int? Minor, int? Patch)
{
    public static bool TryParse(string versionString, out DataSetVersionNumber? version)
    {
        version = null;
        if (versionString.Contains('*'))
        {
            return TryParseWildcard(versionString, ref version);
        }

        var successful = SemVersion.TryParse(
            versionString,
            SemVersionStyles.OptionalMinorPatch
                | SemVersionStyles.AllowWhitespace
                | SemVersionStyles.AllowLowerV,
        out var sv);

        if (!successful)
        {
            return false;
        }

        version = new DataSetVersionNumber(sv.Major, sv.Minor, sv.Patch);
        return successful;
    }

    private static bool TryParseWildcard(string versionString, [NotNullWhen(true)] ref DataSetVersionNumber? version)
    {
        int?[] parts;
        try
        {
            parts = versionString
                .Trim(' ', 'v')
                .Split('.')
                .Select(a => a != "*" ? (int?)int.Parse(a, System.Globalization.NumberStyles.None)
                : null)
                .ToArray();
        }
        catch (FormatException)
        { //reject invalid characters like '** or numbers with leading/trailing whitespaces
            return false;
        }
        if (parts.Length > 3)
        {
            return false;
        }

        var indexOfWildcard = Array.FindIndex(parts, part => part == null);
        if (indexOfWildcard != -1 && parts.Skip(indexOfWildcard + 1).Any(part => part != null))
            return false; // reject version strings like 1.*.1

        version = new DataSetVersionNumber(parts[0],
            parts.Length > 1 ? parts[1] : null,
            parts.Length > 2 ? parts[2] : null);
        return true;
    }
}
