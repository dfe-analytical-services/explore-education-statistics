namespace GovUk.Education.ExploreEducationStatistics.Common.Cache;

public static class CronSchedules
{
    public const string HourlyExpirySchedule = "0 * * * *";
    public const string HalfHourlyExpirySchedule = "*/30 * * * *";
}