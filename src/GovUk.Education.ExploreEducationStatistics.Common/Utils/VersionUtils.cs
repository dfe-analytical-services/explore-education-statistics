using SemanticVersioning;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class VersionUtils
{
    public static bool TryParse(string versionString, out int major, out int minor, out int patch)
    {
        if (!SemVersion.TryParse(
            versionString,
            SemVersionStyles.OptionalMinorPatch
                | SemVersionStyles.AllowWhitespace
                | SemVersionStyles.AllowLeadingWhitespace,
            out SemVersion version))
        {
            major = default;
            minor = default;
            patch = default;

            return false;
        }

        major = version.Major;
        minor = version.Minor;
        patch = version.Patch;

        return true;
    }
}
