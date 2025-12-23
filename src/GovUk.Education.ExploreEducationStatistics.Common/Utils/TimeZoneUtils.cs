namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class TimeZoneUtils
{
    public static TimeZoneInfo GetUkTimeZone() => TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
}
