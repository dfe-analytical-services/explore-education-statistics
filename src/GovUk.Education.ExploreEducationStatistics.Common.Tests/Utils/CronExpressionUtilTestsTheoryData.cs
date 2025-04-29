using System;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class CronExpressionUtilTestsTheoryData
{
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
    /// <term>significance</term>
    /// <description>A description of the significance of the reference time.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static readonly TheoryData<string, string, string, string> UtcTimeZoneTestData =
        new()
        {
            { "2025-01-01T00:00:00 +00:00", "2025-01-01T09:30:00 +00:00", "30 9 * * *", "Beginning of scheduled day" },
            { "2025-01-01T08:30:00 +00:00", "2025-01-01T09:30:00 +00:00", "30 9 * * *", "One hour before schedule" },
            { "2025-01-01T09:29:59 +00:00", "2025-01-01T09:30:00 +00:00", "30 9 * * *", "One second before schedule" },
            { "2025-01-01T09:30:00 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "On the schedule" },
            { "2025-01-01T09:30:01 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "One second after schedule" },
            { "2025-01-01T10:30:00 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "One hour after schedule" },
            { "2025-01-01T23:59:59 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "End of scheduled day" },
            { "2025-03-30T00:00:00 +00:00", "2025-03-30T09:30:00 +00:00", "30 9 * * *", "One hour before BST starts" },
            { "2025-03-30T00:59:59 +00:00", "2025-03-30T09:30:00 +00:00", "30 9 * * *", "One second before BST starts" },
            { "2025-03-30T01:00:00 +00:00", "2025-03-30T09:30:00 +00:00", "30 9 * * *", "BST started" },
            { "2025-03-30T01:00:01 +00:00", "2025-03-30T09:30:00 +00:00", "30 9 * * *", "One second after BST started" },
            { "2025-03-30T02:00:00 +00:00", "2025-03-30T09:30:00 +00:00", "30 9 * * *", "One hour after BST started" },
            { "2025-10-26T00:00:00 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One hour before BST ends" },
            { "2025-10-26T00:59:59 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One second before BST ends" },
            { "2025-10-26T01:00:00 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "BST ended" },
            { "2025-10-26T01:00:01 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One second after BST ended" },
            { "2025-10-26T02:00:00 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One hour after BST ended" }
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
    /// <term>significance</term>
    /// <description>A description of the significance of the reference time.</description>
    /// </item>
    /// </list>
    /// <para>For readability, the times are expressed using the UK time zone offset.</para>
    /// </remarks>
    public static readonly TheoryData<string, string, string, string> UkTimeZoneTestData =
        new()
        {
            { "2025-01-01T00:00:00 +00:00", "2025-01-01T09:30:00 +00:00", "30 9 * * *", "Beginning of scheduled day" },
            { "2025-01-01T08:30:00 +00:00", "2025-01-01T09:30:00 +00:00", "30 9 * * *", "One hour before schedule" },
            { "2025-01-01T09:29:59 +00:00", "2025-01-01T09:30:00 +00:00", "30 9 * * *", "One second before schedule" },
            { "2025-01-01T09:30:00 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "On the schedule" },
            { "2025-01-01T09:30:01 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "One second after schedule" },
            { "2025-01-01T10:30:00 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "One hour after schedule" },
            { "2025-01-01T10:30:00 +00:00", "2025-01-02T09:30:00 +00:00", "30 9 * * *", "End of scheduled day" },
            { "2025-03-30T00:00:00 +00:00", "2025-03-30T09:30:00 +01:00", "30 9 * * *", "One hour before BST starts" },
            { "2025-03-30T00:59:59 +00:00", "2025-03-30T09:30:00 +01:00", "30 9 * * *", "One second before BST starts" },
            { "2025-03-30T02:00:00 +01:00", "2025-03-30T09:30:00 +01:00", "30 9 * * *", "BST started" },
            { "2025-03-30T02:00:01 +01:00", "2025-03-30T09:30:00 +01:00", "30 9 * * *", "One second after BST started" },
            { "2025-03-30T03:00:00 +01:00", "2025-03-30T09:30:00 +01:00", "30 9 * * *", "One hour after BST started" },
            { "2025-10-26T01:00:00 +01:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One hour before BST ends" },
            { "2025-10-26T01:59:59 +01:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One second before BST ends" },
            { "2025-10-26T01:00:00 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "BST ended" },
            { "2025-10-26T01:00:01 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One second after BST ended" },
            { "2025-10-26T02:00:00 +00:00", "2025-10-26T09:30:00 +00:00", "30 9 * * *", "One hour after BST ended" }
        };

    internal static (DateTimeOffset From, DateTimeOffset ExpectedNextOccurrence) ParseTheoryData(
        string from,
        string expectedNextOccurrence) =>
        (DateTimeOffset.Parse(from), DateTimeOffset.Parse(expectedNextOccurrence));
}
