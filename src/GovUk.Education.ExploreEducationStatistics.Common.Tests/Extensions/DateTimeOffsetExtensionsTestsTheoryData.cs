using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

// ReSharper disable once ClassNeverInstantiated.Global
public class DateTimeOffsetExtensionsTestsTheoryData
{
    public class GetUkStartOfDayUtcUTheoryData
    {
        /// <summary>
        /// Test data of UTC <see cref="DateTimeOffset"/> values used to verify start-of-day calculation.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader>
        /// <term>parameter</term>
        /// <description>description</description>
        /// </listheader>
        /// <item>
        /// <term>dateTimeOffset</term>
        /// <description>The input <see cref="DateTimeOffset"/> in UTC.</description>
        /// </item>
        /// <item>
        /// <term>expectedDateTimeOffset</term>
        /// <description>The expected <see cref="DateTimeOffset"/> result in UTC.</description>
        /// </item>
        /// <item>
        /// <term>description</term>
        /// <description>A description of the test case printed on failure.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static readonly TheoryData<DateTimeOffset, DateTimeOffset, string> UtcZoneData = new()
        {
            // csharpier-ignore-start
            { Dt("2025-01-01T00:00:00 +00:00"), Dt("2025-01-01T00:00:00 +00:00"), "Start of UK local day (00:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-01-01T12:00:00 +00:00"), Dt("2025-01-01T00:00:00 +00:00"), "Noon UK local time (12:00:00 GMT, 12:00:00 UTC)" },
            { Dt("2025-01-01T23:59:59 +00:00"), Dt("2025-01-01T00:00:00 +00:00"), "End of UK local day (23:59:59 GMT, 23:59:59 UTC)" },

            { Dt("2025-05-31T23:00:00 +00:00"), Dt("2025-05-31T23:00:00 +00:00"), "Start of UK local day (00:00:00 BST, 23:00:00 UTC previous day)" },
            { Dt("2025-06-01T11:00:00 +00:00"), Dt("2025-05-31T23:00:00 +00:00"), "Noon UK local time (12:00:00 BST, 11:00:00 UTC)" },
            { Dt("2025-06-01T22:59:59 +00:00"), Dt("2025-05-31T23:00:00 +00:00"), "End of UK local day (23:59:59 BST, 22:59:59 UTC)" },

            { Dt("2025-03-30T00:00:00 +00:00"), Dt("2025-03-30T00:00:00 +00:00"), "One hour before BST starts (01:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-03-30T00:59:59 +00:00"), Dt("2025-03-30T00:00:00 +00:00"), "One second before BST starts (01:59:59 GMT, 00:59:59 UTC)" },
            { Dt("2025-03-30T01:00:00 +00:00"), Dt("2025-03-30T00:00:00 +00:00"), "BST starts at this instant (01:00:00 GMT -> 02:00:00 BST, 01:00:00 UTC)" },
            { Dt("2025-03-30T01:00:01 +00:00"), Dt("2025-03-30T00:00:00 +00:00"), "One second after BST started (02:00:01 BST, 01:00:01 UTC)" },
            { Dt("2025-03-30T02:00:00 +00:00"), Dt("2025-03-30T00:00:00 +00:00"), "One hour after BST started (03:00:00 BST, 02:00:00 UTC)" },

            { Dt("2025-10-26T00:00:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "One hour before BST ends (01:00:00 BST, 00:00:00 UTC)" },
            { Dt("2025-10-26T00:30:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "01:30:00 local time before BST ends (01:30:00 BST, 00:30:00 UTC)" },
            { Dt("2025-10-26T00:59:59 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "One second before BST ends (01:59:59 BST, 00:59:59 UTC)" },
            { Dt("2025-10-26T01:00:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "BST ends at this instant (02:00:00 BST -> 01:00:00 GMT, 01:00:00 UTC)" },
            { Dt("2025-10-26T01:00:01 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "One second after BST ended (01:00:01 GMT, 01:00:01 UTC)" },
            { Dt("2025-10-26T01:30:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "01:30:00 local time after BST ended (01:30:00 GMT, 01:30:00 UTC)" },
            { Dt("2025-10-26T02:00:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "One hour after BST ended (02:00:00 GMT, 02:00:00 UTC)" }
            // csharpier-ignore-end
        };

        /// <summary>
        /// Test data of UK time zone <see cref="DateTimeOffset"/> values used to verify start-of-day calculation.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader>
        /// <term>parameter</term>
        /// <description>description</description>
        /// </listheader>
        /// <item>
        /// <term>dateTimeOffset</term>
        /// <description>The input <see cref="DateTimeOffset"/> with offset for UK time zone.</description>
        /// </item>
        /// <item>
        /// <term>expectedDateTimeOffset</term>
        /// <description>The expected <see cref="DateTimeOffset"/> result in UTC.</description>
        /// </item>
        /// <item>
        /// <term>description</term>
        /// <description>A description of the test case printed on failure.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static readonly TheoryData<DateTimeOffset, DateTimeOffset, string> UkZoneData = new()
        {
            // csharpier-ignore-start
            { Dt("2025-01-01T00:00:00 +00:00"), Dt("2025-01-01T00:00:00 +00:00"), "Start of UK local day (00:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-01-01T12:00:00 +00:00"), Dt("2025-01-01T00:00:00 +00:00"), "Noon UK local time (12:00:00 GMT, 12:00:00 UTC)" },
            { Dt("2025-01-01T23:59:59 +00:00"), Dt("2025-01-01T00:00:00 +00:00"), "End of UK local day (23:59:59 GMT, 23:59:59 UTC)" },

            { Dt("2025-06-01T00:00:00 +01:00"), Dt("2025-05-31T23:00:00 +00:00"), "Start of UK local day (00:00:00 BST, 23:00:00 UTC previous day)" },
            { Dt("2025-06-01T12:00:00 +01:00"), Dt("2025-05-31T23:00:00 +00:00"), "Noon UK local time (12:00:00 BST, 11:00:00 UTC)" },
            { Dt("2025-06-01T23:59:59 +01:00"), Dt("2025-05-31T23:00:00 +00:00"), "End of UK local day (23:59:59 BST, 22:59:59 UTC)" },

            { Dt("2025-03-30T00:00:00 +00:00"), Dt("2025-03-30T00:00:00 +00:00"), "One hour before BST starts (01:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-03-30T00:59:59 +00:00"), Dt("2025-03-30T00:00:00 +00:00"), "One second before BST starts (01:59:59 GMT, 00:59:59 UTC)" },
            { Dt("2025-03-30T02:00:00 +01:00"), Dt("2025-03-30T00:00:00 +00:00"), "BST starts at this instant (01:00:00 GMT -> 02:00:00 BST, 01:00:00 UTC)" },
            { Dt("2025-03-30T02:00:01 +01:00"), Dt("2025-03-30T00:00:00 +00:00"), "One second after BST started (02:00:01 BST, 01:00:01 UTC)" },
            { Dt("2025-03-30T03:00:00 +01:00"), Dt("2025-03-30T00:00:00 +00:00"), "One hour after BST started (03:00:00 BST, 02:00:00 UTC)" },

            { Dt("2025-10-26T01:00:00 +01:00"), Dt("2025-10-25T23:00:00 +00:00"), "One hour before BST ends (01:00:00 BST, 00:00:00 UTC)" },
            { Dt("2025-10-26T01:30:00 +01:00"), Dt("2025-10-25T23:00:00 +00:00"), "01:30:00 local time before BST ends (01:30:00 BST, 00:30:00 UTC)" },
            { Dt("2025-10-26T01:59:59 +01:00"), Dt("2025-10-25T23:00:00 +00:00"), "One second before BST ends (01:59:59 BST, 00:59:59 UTC)" },
            { Dt("2025-10-26T01:00:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "BST ends at this instant (02:00:00 BST -> 01:00:00 GMT, 01:00:00 UTC)" },
            { Dt("2025-10-26T01:00:01 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "One second after BST ended (01:00:01 GMT, 01:00:01 UTC)" },
            { Dt("2025-10-26T01:30:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "01:30:00 local time after BST ended (01:30:00 GMT, 01:30:00 UTC)" },
            { Dt("2025-10-26T02:00:00 +00:00"), Dt("2025-10-25T23:00:00 +00:00"), "One hour after BST ended (02:00:00 GMT, 02:00:00 UTC)" }
            // csharpier-ignore-end
        };
    }

    public class GetUkEndOfDayUtcUTheoryData
    {
        /// <summary>
        /// Test data of UTC <see cref="DateTimeOffset"/> values used to verify end-of-day calculation.
        /// </summary>
        public static readonly TheoryData<DateTimeOffset, DateTimeOffset, string> UtcZoneData = new()
        {
            // csharpier-ignore-start
            // Winter (GMT) - End of day is 23:59:59 UTC
            { Dt("2025-01-01T00:00:00 +00:00"), Dt("2025-01-01T23:59:59 +00:00"), "Start of UK local day (Result: 23:59:59 GMT)" },
            { Dt("2025-01-01T12:00:00 +00:00"), Dt("2025-01-01T23:59:59 +00:00"), "Noon UK local time (Result: 23:59:59 GMT)" },
            { Dt("2025-01-01T23:59:59 +00:00"), Dt("2025-01-01T23:59:59 +00:00"), "End of UK local day (Result: 23:59:59 GMT)" },

            // Summer (BST) - End of day is 23:59:59 BST (which is 22:59:59 UTC)
            { Dt("2025-05-31T23:00:00 +00:00"), Dt("2025-06-01T22:59:59 +00:00"), "Start of UK local day (Result: 22:59:59 UTC)" },
            { Dt("2025-06-01T11:00:00 +00:00"), Dt("2025-06-01T22:59:59 +00:00"), "Noon UK local time (Result: 22:59:59 UTC)" },
            { Dt("2025-06-01T22:59:59 +00:00"), Dt("2025-06-01T22:59:59 +00:00"), "End of UK local day (Result: 22:59:59 UTC)" },

            // --- SPRING TRANSITION (GMT to BST) ---
            { Dt("2025-03-29T12:00:00 +00:00"), Dt("2025-03-29T23:59:59 +00:00"), "Day before Spring transition (Result: 23:59:59 GMT)" },
            { Dt("2025-03-30T00:00:00 +00:00"), Dt("2025-03-30T22:59:59 +00:00"), "Midnight GMT before spring forward" },
            { Dt("2025-03-30T12:00:00 +00:00"), Dt("2025-03-30T22:59:59 +00:00"), "Noon BST (11:00 UTC) on transition day" },
            { Dt("2025-03-31T12:00:00 +00:00"), Dt("2025-03-31T22:59:59 +00:00"), "Day after Spring transition (Result: 22:59:59 UTC / 23:59:59 BST)" },

            // --- AUTUMN TRANSITION (BST to GMT) ---
            { Dt("2025-10-25T12:00:00 +00:00"), Dt("2025-10-25T22:59:59 +00:00"), "Day before Autumn transition (Result: 22:59:59 UTC / 23:59:59 BST)" },
            { Dt("2025-10-26T00:00:00 +00:00"), Dt("2025-10-26T23:59:59 +00:00"), "Midnight BST before fall back" },
            { Dt("2025-10-26T12:00:00 +00:00"), Dt("2025-10-26T23:59:59 +00:00"), "Noon GMT on transition day" },
            { Dt("2025-10-27T12:00:00 +00:00"), Dt("2025-10-27T23:59:59 +00:00"), "Day after Autumn transition (Result: 23:59:59 GMT)" }
            // csharpier-ignore-end
        };

        /// <summary>
        /// Test data of UK time zone <see cref="DateTimeOffset"/> values used to verify end-of-day calculation.
        /// </summary>
        public static readonly TheoryData<DateTimeOffset, DateTimeOffset, string> UkZoneData = new()
        {
            // csharpier-ignore-start
            // Winter (GMT)
            { Dt("2025-01-01T12:00:00 +00:00"), Dt("2025-01-01T23:59:59 +00:00"), "Noon GMT" },

            // Summer (BST)
            { Dt("2025-06-01T12:00:00 +01:00"), Dt("2025-06-01T22:59:59 +00:00"), "Noon BST (22:59 UTC)" },

            // Complex Transition: Spring Forward (Clocks skip 01:00 to 02:00)
            { Dt("2025-03-30T00:30:00 +00:00"), Dt("2025-03-30T22:59:59 +00:00"), "Inside the last GMT hour of spring transition day" },
            { Dt("2025-03-30T02:30:00 +01:00"), Dt("2025-03-30T22:59:59 +00:00"), "Inside the first BST hour of spring transition day" },

            // Complex Transition: Fall Back (01:00 to 02:00 happens twice)
            { Dt("2025-10-26T01:30:00 +01:00"), Dt("2025-10-26T23:59:59 +00:00"), "The first 01:30 (BST) on fall back day" },
            { Dt("2025-10-26T01:30:00 +00:00"), Dt("2025-10-26T23:59:59 +00:00"), "The second 01:30 (GMT) on fall back day" }
            // csharpier-ignore-end
        };
    }

    public class ToUkDateOnlyTheoryData
    {
        /// <summary>
        /// Test data of UTC <see cref="DateTimeOffset"/> values used to verify date-only calculation.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader>
        /// <term>parameter</term>
        /// <description>description</description>
        /// </listheader>
        /// <item>
        /// <term>dateTimeOffset</term>
        /// <description>The input <see cref="DateTimeOffset"/> in UTC.</description>
        /// </item>
        /// <item>
        /// <term>expectedDateOnly</term>
        /// <description>The expected <see cref="DateOnly"/> result.</description>
        /// </item>
        /// <item>
        /// <term>description</term>
        /// <description>A description of the test case printed on failure.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static readonly TheoryData<DateTimeOffset, DateOnly, string> UtcZoneData = new()
        {
            // csharpier-ignore-start
            { Dt("2025-01-01T00:00:00 +00:00"), Do("2025-01-01"), "Start of UK local day (00:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-01-01T12:00:00 +00:00"), Do("2025-01-01"), "Noon UK local time (12:00:00 GMT, 12:00:00 UTC)" },
            { Dt("2025-01-01T23:59:59 +00:00"), Do("2025-01-01"), "End of UK local day (23:59:59 GMT, 23:59:59 UTC)" },

            { Dt("2025-05-31T23:00:00 +00:00"), Do("2025-06-01"), "Start of UK local day (00:00:00 BST, 23:00:00 UTC previous day)" },
            { Dt("2025-06-01T11:00:00 +00:00"), Do("2025-06-01"), "Noon UK local time (12:00:00 BST, 11:00:00 UTC)" },
            { Dt("2025-06-01T22:59:59 +00:00"), Do("2025-06-01"), "End of UK local day (23:59:59 BST, 22:59:59 UTC)" },

            { Dt("2025-03-30T00:00:00 +00:00"), Do("2025-03-30"), "One hour before BST starts (01:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-03-30T00:59:59 +00:00"), Do("2025-03-30"), "One second before BST starts (01:59:59 GMT, 00:59:59 UTC)" },
            { Dt("2025-03-30T01:00:00 +00:00"), Do("2025-03-30"), "BST starts at this instant (01:00:00 GMT -> 02:00:00 BST, 01:00:00 UTC)" },
            { Dt("2025-03-30T01:00:01 +00:00"), Do("2025-03-30"), "One second after BST started (02:00:01 BST, 01:00:01 UTC)" },
            { Dt("2025-03-30T02:00:00 +00:00"), Do("2025-03-30"), "One hour after BST started (03:00:00 BST, 02:00:00 UTC)" },

            { Dt("2025-10-26T00:00:00 +00:00"), Do("2025-10-26"), "One hour before BST ends (01:00:00 BST, 00:00:00 UTC)" },
            { Dt("2025-10-26T00:30:00 +00:00"), Do("2025-10-26"), "01:30:00 local time before BST ends (01:30:00 BST, 00:30:00 UTC)" },
            { Dt("2025-10-26T00:59:59 +00:00"), Do("2025-10-26"), "One second before BST ends (01:59:59 BST, 00:59:59 UTC)" },
            { Dt("2025-10-26T01:00:00 +00:00"), Do("2025-10-26"), "BST ends at this instant (02:00:00 BST -> 01:00:00 GMT, 01:00:00 UTC)" },
            { Dt("2025-10-26T01:00:01 +00:00"), Do("2025-10-26"), "One second after BST ended (01:00:01 GMT, 01:00:01 UTC)" },
            { Dt("2025-10-26T01:30:00 +00:00"), Do("2025-10-26"), "01:30:00 local time after BST ended (01:30:00 GMT, 01:30:00 UTC)" },
            { Dt("2025-10-26T02:00:00 +00:00"), Do("2025-10-26"), "One hour after BST ended (02:00:00 GMT, 02:00:00 UTC)" }
            // csharpier-ignore-end
        };

        /// <summary>
        /// Test data of UK time zone <see cref="DateTimeOffset"/> values used to verify date-only calculation.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader>
        /// <term>parameter</term>
        /// <description>description</description>
        /// </listheader>
        /// <item>
        /// <term>dateTimeOffset</term>
        /// <description>The input <see cref="DateTimeOffset"/> with offset for UK time zone.</description>
        /// </item>
        /// <item>
        /// <term>expectedDateOnly</term>
        /// <description>The expected <see cref="DateOnly"/> result.</description>
        /// </item>
        /// <item>
        /// <term>description</term>
        /// <description>A description of the test case printed on failure.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static readonly TheoryData<DateTimeOffset, DateOnly, string> UkZoneData = new()
        {
            // csharpier-ignore-start
            { Dt("2025-01-01T00:00:00 +00:00"), Do("2025-01-01"), "Start of UK local day (00:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-01-01T12:00:00 +00:00"), Do("2025-01-01"), "Noon UK local time (12:00:00 GMT, 12:00:00 UTC)" },
            { Dt("2025-01-01T23:59:59 +00:00"), Do("2025-01-01"), "End of UK local day (23:59:59 GMT, 23:59:59 UTC)" },

            { Dt("2025-06-01T00:00:00 +01:00"), Do("2025-06-01"), "Start of UK local day (00:00:00 BST, 23:00:00 UTC previous day)" },
            { Dt("2025-06-01T12:00:00 +01:00"), Do("2025-06-01"), "Noon UK local time (12:00:00 BST, 11:00:00 UTC)" },
            { Dt("2025-06-01T23:59:59 +01:00"), Do("2025-06-01"), "End of UK local day (23:59:59 BST, 22:59:59 UTC)" },

            { Dt("2025-03-30T00:00:00 +00:00"), Do("2025-03-30"), "One hour before BST starts (01:00:00 GMT, 00:00:00 UTC)" },
            { Dt("2025-03-30T00:59:59 +00:00"), Do("2025-03-30"), "One second before BST starts (01:59:59 GMT, 00:59:59 UTC)" },
            { Dt("2025-03-30T02:00:00 +01:00"), Do("2025-03-30"), "BST starts at this instant (01:00:00 GMT -> 02:00:00 BST, 01:00:00 UTC)" },
            { Dt("2025-03-30T02:00:01 +01:00"), Do("2025-03-30"), "One second after BST started (02:00:01 BST, 01:00:01 UTC)" },
            { Dt("2025-03-30T03:00:00 +01:00"), Do("2025-03-30"), "One hour after BST started (03:00:00 BST, 02:00:00 UTC)" },

            { Dt("2025-10-26T01:00:00 +01:00"), Do("2025-10-26"), "One hour before BST ends (01:00:00 BST, 00:00:00 UTC)" },
            { Dt("2025-10-26T01:30:00 +01:00"), Do("2025-10-26"), "01:30:00 local time before BST ends (01:30:00 BST, 00:30:00 UTC)" },
            { Dt("2025-10-26T01:59:59 +01:00"), Do("2025-10-26"), "One second before BST ends (01:59:59 BST, 00:59:59 UTC)" },
            { Dt("2025-10-26T01:00:00 +00:00"), Do("2025-10-26"), "BST ends at this instant (02:00:00 BST -> 01:00:00 GMT, 01:00:00 UTC)" },
            { Dt("2025-10-26T01:00:01 +00:00"), Do("2025-10-26"), "One second after BST ended (01:00:01 GMT, 01:00:01 UTC)" },
            { Dt("2025-10-26T01:30:00 +00:00"), Do("2025-10-26"), "01:30:00 local time after BST ended (01:30:00 GMT, 01:30:00 UTC)" },
            { Dt("2025-10-26T02:00:00 +00:00"), Do("2025-10-26"), "One hour after BST ended (02:00:00 GMT, 02:00:00 UTC)" }
            // csharpier-ignore-end
        };
    }

    private static DateTimeOffset Dt(string input) => DateTimeOffset.Parse(input);

    private static DateOnly Do(string input) => DateOnly.Parse(input);
}
