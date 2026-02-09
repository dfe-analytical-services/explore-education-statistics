#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public static class TimeSpanExtensions
{
    public static string PrettyPrint(this TimeSpan ts)
    {
        var parts = new List<string>();
        const char s = 's';

        if (ts.Days > 0)
        {
            parts.Add($"{ts.Days} day{(ts.Days != 1 ? s : string.Empty)}");
        }

        if (ts.Hours > 0)
        {
            parts.Add($"{ts.Hours} hour{(ts.Hours != 1 ? s : string.Empty)}");
        }

        if (ts.Minutes > 0)
        {
            parts.Add($"{ts.Minutes} minute{(ts.Minutes != 1 ? s : string.Empty)}");
        }

        if (ts.Seconds > 0)
        {
            parts.Add($"{ts.Seconds} second{(ts.Seconds != 1 ? s : string.Empty)}");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "0 seconds";
    }
}
