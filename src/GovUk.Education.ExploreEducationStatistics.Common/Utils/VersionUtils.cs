#nullable enable
using Semver;
using System.Reflection.Metadata.Ecma335;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class VersionUtils
{
    public static bool TryParse(string versionString, out SemVersion version)
    {
        var successful = SemVersion.TryParse(
            versionString,
            SemVersionStyles.OptionalMinorPatch
                | SemVersionStyles.AllowWhitespace
                | SemVersionStyles.AllowLowerV,
            out var sv);

        version = sv;

        return successful;
    }
    public static bool TryParseWildcard(string versionString, out SemVersionRange version)
    {
        var successful = SemVersionRange.TryParse(
            versionString,
            SemVersionRangeOptions.OptionalMinorPatch
            | SemVersionRangeOptions.AllowLowerV,
            out var sv);

        version = sv;

        return successful;
    }
}
