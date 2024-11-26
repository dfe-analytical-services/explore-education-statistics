using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class TimePeriodRangeMetaGeneratorExtensions
{
    public static Generator<TimePeriodRangeMetaOld> DefaultTimePeriodRangeMeta(this DataFixture fixture)
        => fixture.Generator<TimePeriodRangeMetaOld>().WithDefaults();

    public static Generator<TimePeriodRangeMetaOld> WithDefaults(this Generator<TimePeriodRangeMetaOld> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static Generator<TimePeriodRangeMetaOld> WithStart(
        this Generator<TimePeriodRangeMetaOld> generator,
        string period,
        TimeIdentifier timeIdentifier)
        => generator.ForInstance(s => s.SetStart(period, timeIdentifier));

    public static Generator<TimePeriodRangeMetaOld> WithEnd(
        this Generator<TimePeriodRangeMetaOld> generator,
        string period,
        TimeIdentifier timeIdentifier)
        => generator.ForInstance(s => s.SetEnd(period, timeIdentifier));

    public static InstanceSetters<TimePeriodRangeMetaOld> SetDefaults(this InstanceSetters<TimePeriodRangeMetaOld> setters)
        => setters
            .SetStart("2000", TimeIdentifier.CalendarYear)
            .SetEnd("2001", TimeIdentifier.CalendarYear);

    public static InstanceSetters<TimePeriodRangeMetaOld> SetStart(
        this InstanceSetters<TimePeriodRangeMetaOld> setters,
        string period,
        TimeIdentifier timeIdentifier)
        => setters.Set(s => s.Start, new TimePeriodRangeBoundMetaOld
        {
            Period = period,
            TimeIdentifier = timeIdentifier,
        });

    public static InstanceSetters<TimePeriodRangeMetaOld> SetEnd(
        this InstanceSetters<TimePeriodRangeMetaOld> setters,
        string period,
        TimeIdentifier timeIdentifier)
        => setters.Set(s => s.End, new TimePeriodRangeBoundMetaOld
        {
            Period = period,
            TimeIdentifier = timeIdentifier,
        });
}
