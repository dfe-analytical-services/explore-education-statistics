using Xunit;
using static System.TimeZoneInfo;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class CronExpressionUtilTestsTheoryData
{
    private static readonly TimeZoneInfo UkTimeZone = FindSystemTimeZoneById("Europe/London");

    /// <summary>
    /// Test data to verify calculating the next occurrence of a Cron expression in the UTC time zone.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader>
    /// <term>parameter</term>
    /// <description>description</description>
    /// </listheader>
    /// <item>
    /// <term>from</term>
    /// <description>The reference time to start the calculation.</description>
    /// </item>
    /// <item>
    /// <term>expectedNextOccurrence</term>
    /// <description>The expected next occurrence of the Cron expression.</description>
    /// </item>
    /// <item>
    /// <term>cronExpression</term>
    /// <description>The cron expression</description>
    /// </item>
    /// <item>
    /// <term>timeZone</term>
    /// <description>The time zone which the next occurrence should be evaluated in.</description>
    /// </item>
    /// <item>
    /// <term>significance</term>
    /// <description>A description of the significance of the reference time.</description>
    /// </item>
    /// </list>
    /// <para>The data includes cases for the start and end of BST. Since this data is intended to verify calculating
    /// the next occurrence of the Cron expression in the UTC time zone, expected results remain in UTC (+00:00).
    /// </para>
    /// </remarks>
    public static readonly TheoryData<DateTimeOffset, DateTimeOffset, string, TimeZoneInfo, string>
        UtcTimeZoneTestData =
            new()
            {
            // @formatter:off
            { Dt("2025-01-01T00:00:00 +00:00"), Dt("2025-01-01T09:30:00 +00:00"), "30 9 * * *", Utc, "Beginning of scheduled day" },
            { Dt("2025-01-01T08:30:00 +00:00"), Dt("2025-01-01T09:30:00 +00:00"), "30 9 * * *", Utc, "One hour before schedule" },
            { Dt("2025-01-01T09:29:59 +00:00"), Dt("2025-01-01T09:30:00 +00:00"), "30 9 * * *", Utc, "One second before schedule" },
            { Dt("2025-01-01T09:30:00 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", Utc, "On the schedule" },
            { Dt("2025-01-01T09:30:01 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", Utc, "One second after schedule" },
            { Dt("2025-01-01T10:30:00 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", Utc, "One hour after schedule" },
            { Dt("2025-01-01T23:59:59 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", Utc, "End of scheduled day" },
            { Dt("2025-03-30T00:00:00 +00:00"), Dt("2025-03-30T09:30:00 +00:00"), "30 9 * * *", Utc, "One hour before BST starts" },
            { Dt("2025-03-30T00:59:59 +00:00"), Dt("2025-03-30T09:30:00 +00:00"), "30 9 * * *", Utc, "One second before BST starts" },
            { Dt("2025-03-30T01:00:00 +00:00"), Dt("2025-03-30T09:30:00 +00:00"), "30 9 * * *", Utc, "BST started" },
            { Dt("2025-03-30T01:00:01 +00:00"), Dt("2025-03-30T09:30:00 +00:00"), "30 9 * * *", Utc, "One second after BST started" },
            { Dt("2025-03-30T02:00:00 +00:00"), Dt("2025-03-30T09:30:00 +00:00"), "30 9 * * *", Utc, "One hour after BST started" },
            { Dt("2025-10-26T00:00:00 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", Utc, "One hour before BST ends" },
            { Dt("2025-10-26T00:59:59 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", Utc, "One second before BST ends" },
            { Dt("2025-10-26T01:00:00 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", Utc, "BST ended" },
            { Dt("2025-10-26T01:00:01 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", Utc, "One second after BST ended" },
            { Dt("2025-10-26T02:00:00 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", Utc, "One hour after BST ended" }
            // @formatter:on
            };

    /// <summary>
    /// Test data to verify calculating the next occurrence of a Cron expression in the UK time zone.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader>
    /// <term>parameter</term>
    /// <description>description</description>
    /// </listheader>
    /// <item>
    /// <term>from</term>
    /// <description>The reference time to start the calculation.</description>
    /// </item>
    /// <item>
    /// <term>expectedNextOccurrence</term>
    /// <description>The expected next occurrence of the Cron expression.</description>
    /// </item>
    /// <item>
    /// <term>cronExpression</term>
    /// <description>The cron expression</description>
    /// </item>
    /// <item>
    /// <term>timeZone</term>
    /// <description>The time zone which the next occurrence should be evaluated in.</description>
    /// </item>
    /// <item>
    /// <term>significance</term>
    /// <description>A description of the significance of the reference time.</description>
    /// </item>
    /// </list>
    /// <para>The data includes cases for the start and end of BST. Since this data is intended to verify calculating
    /// the next occurrence of the Cron expression in the UK time zone, expected results are expressed as UK local time.
    /// For each test within BST, the <c>from</c> times are expressed as both UTC and UK local times to ensure that
    /// the calculations handle the transition correctly regardless of the time zone representation.
    /// </para>
    /// </remarks>
    public static readonly TheoryData<DateTimeOffset, DateTimeOffset, string, TimeZoneInfo, string> UkTimeZoneTestData =
        new()
        {
            // @formatter:off
            { Dt("2025-01-01T00:00:00 +00:00"), Dt("2025-01-01T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "Beginning of scheduled day" },
            { Dt("2025-01-01T08:30:00 +00:00"), Dt("2025-01-01T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One hour before schedule" },
            { Dt("2025-01-01T09:29:59 +00:00"), Dt("2025-01-01T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One second before schedule" },
            { Dt("2025-01-01T09:30:00 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "On the schedule" },
            { Dt("2025-01-01T09:30:01 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One second after schedule" },
            { Dt("2025-01-01T10:30:00 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One hour after schedule" },
            { Dt("2025-01-01T23:59:59 +00:00"), Dt("2025-01-02T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "End of scheduled day" },
            { Dt("2025-03-30T00:00:00 +00:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "One hour before BST starts" },
            { Dt("2025-03-30T00:59:59 +00:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "One second before BST starts" },
            { Dt("2025-03-30T01:00:00 +00:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "BST started (expressed in +00:00)" },
            { Dt("2025-03-30T02:00:00 +01:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "BST started (expressed in +01:00)" },
            { Dt("2025-03-30T01:00:01 +00:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "One second after BST started (expressed as +00:00)" },
            { Dt("2025-03-30T02:00:01 +01:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "One second after BST started (expressed as +01:00)" },
            { Dt("2025-03-30T02:00:00 +00:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "One hour after BST started (expressed as +00:00)" },
            { Dt("2025-03-30T03:00:00 +01:00"), Dt("2025-03-30T09:30:00 +01:00"), "30 9 * * *", UkTimeZone, "One hour after BST started (expressed as +01:00)" },
            { Dt("2025-10-26T00:00:00 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One hour before BST ends (expressed as +00:00)" },
            { Dt("2025-10-26T01:00:00 +01:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One hour before BST ends (expressed as +01:00)" },
            { Dt("2025-10-26T00:59:59 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One second before BST ends (expressed as +00:00)" },
            { Dt("2025-10-26T01:59:59 +01:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One second before BST ends (expressed as +01:00)" },
            { Dt("2025-10-26T01:00:00 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "BST ended" },
            { Dt("2025-10-26T01:00:01 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One second after BST ended" },
            { Dt("2025-10-26T02:00:00 +00:00"), Dt("2025-10-26T09:30:00 +00:00"), "30 9 * * *", UkTimeZone, "One hour after BST ended" }
            // @formatter:on
        };

    private static DateTimeOffset Dt(string input)
    {
        return DateTimeOffset.Parse(input);
    }
}
