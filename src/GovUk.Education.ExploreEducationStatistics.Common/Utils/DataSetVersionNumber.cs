using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public record DataSetVersionNumber(BigInteger? Major, BigInteger? Minor, BigInteger? Patch)
{
    public static bool TryParse(string versionString, [NotNullWhen(true)] out DataSetVersionNumber? version)
    {
        version = null;
        if (DataSetVersionWildcardHelper.ContainsWildcard(versionString))
        {
            return DataSetVersionWildcardHelper.TryParseWildcard(versionString, out version);
        }

        var successful = SemVersion.TryParse(
            versionString,
            SemVersionStyles.OptionalMinorPatch | SemVersionStyles.AllowWhitespace | SemVersionStyles.AllowLowerV,
            out var sv
        );

        if (!successful)
        {
            return false;
        }

        version = new DataSetVersionNumber(sv?.Major, sv?.Minor, sv?.Patch);
        return successful;
    }
}

public static class DataSetVersionWildcardHelper
{
    public static bool ContainsWildcard(string versionString)
    {
        return versionString.Contains('*');
    }

    public static bool TryParseWildcard(string versionString, [NotNullWhen(true)] out DataSetVersionNumber? version)
    {
        version = null;
        int?[] parts;
        try
        {
            parts = versionString
                .Trim(' ', 'v')
                .Split('.')
                .Select(a => a != "*" ? (int?)int.Parse(a, NumberStyles.None) : null)
                .ToArray();
        }
        catch (FormatException)
        {
            return false;
        }
        if (parts.Length > 3)
        {
            return false;
        }

        var indexOfWildcard = Array.FindIndex(parts, part => part == null);
        if (indexOfWildcard != -1 && parts.Skip(indexOfWildcard + 1).Any(part => part != null))
            return false; // reject version strings like 1.*.1

        version = new DataSetVersionNumber(
            // ints implicitly converted to BigIntegers
            parts[0],
            parts.Length > 1 ? parts[1] : null,
            parts.Length > 2 ? parts[2] : null
        );

        return true;
    }
}
