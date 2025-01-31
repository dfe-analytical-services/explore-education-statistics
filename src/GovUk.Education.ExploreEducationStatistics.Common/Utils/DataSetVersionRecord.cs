#nullable enable
using Semver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public record DataSetVersionRecord(int? Major, int? Minor, int? Patch)
{
    public static bool TryParse(string versionString, out DataSetVersionRecord? version)
    {
        version = null;
        if (versionString.Contains("*"))
            return TryParseWildCard(versionString, ref version);

        var successful = SemVersion.TryParse(
            versionString,
            SemVersionStyles.OptionalMinorPatch
                | SemVersionStyles.AllowWhitespace
                | SemVersionStyles.AllowLowerV,
        out var sv);

        if (!successful)
            return false;

        version = new DataSetVersionRecord(sv.Major, sv.Minor, sv.Patch);
        return successful;
    }

    static bool TryParseWildCard(string versionString, [NotNullWhen(true)] ref DataSetVersionRecord? version)
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
            return false;

        var indexOfWildcard = Array.FindIndex(parts, part => part == null);
        if (indexOfWildcard != -1 && parts.Skip(indexOfWildcard + 1).Any(part => part != null))
            return false; // reject version strings like 1.*.1

        version = new DataSetVersionRecord(parts[0],
            parts.Length > 1 ? parts[1] : null,
            parts.Length > 2 ? parts[2] : null);
        return true;
    }
}
