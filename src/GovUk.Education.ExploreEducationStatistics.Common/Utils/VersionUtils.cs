#nullable enable
using Semver;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static partial class VersionUtils
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

    public static bool TryParseWildcard(string versionString,
        [NotNullWhen(true)] out WildcardVersion? version)
    {
        versionString = versionString.Trim();

        var successful = SemVersionRange.TryParse(versionString,
            SemVersionRangeOptions.OptionalMinorPatch
            | SemVersionRangeOptions.AllowLowerV,
            out var versionRange);

        if (successful)
        {
            version = new WildcardVersion(versionString);
            return true;
        }
        version = null;
        return false;

    }
}
public record WildcardVersion(uint? VersionMajor, uint? VersionMinor, uint? VersionPatch)
{
    public WildcardVersion(string wildcardedVersion) : this(null, null, null)
    {
        var parts = wildcardedVersion.Trim('v').Split('.');
        VersionMajor = parts[0] == "*" ? null : uint.Parse(parts[0]);
        VersionMinor = (parts.Length > 1 && parts[1] != "*") ? uint.Parse(parts[1]) : null;
        VersionPatch = (parts.Length > 2 && parts[2] != "*") ? uint.Parse(parts[2]) : null;
    }
}
