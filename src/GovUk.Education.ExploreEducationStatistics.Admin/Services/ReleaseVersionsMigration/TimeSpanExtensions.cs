#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public static class TimeSpanExtensions
{
    public static string PrettyPrint(this TimeSpan ts)
    {
        if (ts == TimeSpan.Zero)
        {
            return "0 seconds";
        }

        var parts = new List<string>();
        const char s = 's';

        var timeSpanIsNegative = ts < TimeSpan.Zero;

        // Make sure the TimeSpan is positive for further processing, but keep track of whether it was negative
        // so that a negative prefix can be added to the final result.
        if (timeSpanIsNegative && ts != TimeSpan.MinValue)
        {
            ts = ts.Negate();
        }

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

        // If TimeSpan is less than one second, treat as "0 seconds"
        var result = parts.Count == 0 ? "0 seconds" : string.Join(", ", parts);
        return timeSpanIsNegative ? $"-{result}" : result;
    }
}
