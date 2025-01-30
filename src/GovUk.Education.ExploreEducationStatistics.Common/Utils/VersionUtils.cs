#nullable enable
using AngleSharp.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Semver;
using System;
using System.Linq;

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
}
public record WildcardVersion(uint? Major, uint? Minor, uint? Patch)
{
    public static bool TryParse(string versionString, out WildcardVersion? version)
    {
        var parts = versionString.Trim(' ', 'v').Split('.');

        if (parts.Length == 1 && (parts[0].IsNullOrEmpty() || parts[0] == "*"))
        {
            version = new WildcardVersion(null, null, null);
            return true;
        }

        version = null;
        var containsInvalidItems = !parts.All(part => part.Is("*") || uint.TryParse(part, System.Globalization.NumberStyles.None, null, out _));
        if (parts.Length > 3 || containsInvalidItems)
            return false;

        var indexOfWildcard = Array.FindIndex(parts, part => part == "*");
        if (indexOfWildcard != -1 && parts.Skip(indexOfWildcard + 1).Any(part => part != "*"))
            return false; // reject version strings like 1.*.1

        uint? versionMajor = parts[0] == "*" ? null : uint.Parse(parts[0]);
        uint? versionMinor = (parts.Length > 1 && parts[1] != "*") ? uint.Parse(parts[1]) : null;
        uint? versionPatch = (parts.Length > 2 && parts[2] != "*") ? uint.Parse(parts[2]) : null;

        version = new WildcardVersion(versionMajor, versionMinor, versionPatch);
        return true;
    }
}
