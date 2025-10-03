using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class TimePeriodRangeMetaGeneratorExtensions
{
    public static Generator<TimePeriodRangeMeta> DefaultTimePeriodRangeMeta(this DataFixture fixture) =>
        fixture.Generator<TimePeriodRangeMeta>().WithDefaults();

    public static Generator<TimePeriodRangeMeta> WithDefaults(this Generator<TimePeriodRangeMeta> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static Generator<TimePeriodRangeMeta> WithStart(
        this Generator<TimePeriodRangeMeta> generator,
        string period,
        TimeIdentifier timeIdentifier
    ) => generator.ForInstance(s => s.SetStart(period, timeIdentifier));

    public static Generator<TimePeriodRangeMeta> WithEnd(
        this Generator<TimePeriodRangeMeta> generator,
        string period,
        TimeIdentifier timeIdentifier
    ) => generator.ForInstance(s => s.SetEnd(period, timeIdentifier));

    public static InstanceSetters<TimePeriodRangeMeta> SetDefaults(this InstanceSetters<TimePeriodRangeMeta> setters) =>
        setters.SetStart("2000", TimeIdentifier.CalendarYear).SetEnd("2001", TimeIdentifier.CalendarYear);

    public static InstanceSetters<TimePeriodRangeMeta> SetStart(
        this InstanceSetters<TimePeriodRangeMeta> setters,
        string period,
        TimeIdentifier timeIdentifier
    ) => setters.Set(s => s.Start, new TimePeriodRangeBoundMeta { Period = period, TimeIdentifier = timeIdentifier });

    public static InstanceSetters<TimePeriodRangeMeta> SetEnd(
        this InstanceSetters<TimePeriodRangeMeta> setters,
        string period,
        TimeIdentifier timeIdentifier
    ) => setters.Set(s => s.End, new TimePeriodRangeBoundMeta { Period = period, TimeIdentifier = timeIdentifier });
}
