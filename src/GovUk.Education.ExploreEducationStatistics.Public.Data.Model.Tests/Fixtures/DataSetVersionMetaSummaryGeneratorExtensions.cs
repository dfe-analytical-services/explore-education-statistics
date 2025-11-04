using Bogus;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetVersionMetaSummaryGeneratorExtensions
{
    public static Generator<DataSetVersionMetaSummary> DefaultDataSetVersionMetaSummary(this DataFixture fixture) =>
        fixture.Generator<DataSetVersionMetaSummary>().WithDefaults();

    public static Generator<DataSetVersionMetaSummary> WithDefaults(
        this Generator<DataSetVersionMetaSummary> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSetVersionMetaSummary> WithTimePeriodRange(
        this Generator<DataSetVersionMetaSummary> generator,
        TimePeriodRange timePeriodRange
    ) => generator.ForInstance(s => s.SetTimePeriodRange(timePeriodRange));

    public static Generator<DataSetVersionMetaSummary> WithFilters(
        this Generator<DataSetVersionMetaSummary> generator,
        IEnumerable<string> indicators
    ) => generator.ForInstance(s => s.SetFilters(indicators));

    public static Generator<DataSetVersionMetaSummary> WithIndicators(
        this Generator<DataSetVersionMetaSummary> generator,
        IEnumerable<string> indicators
    ) => generator.ForInstance(s => s.SetIndicators(indicators));

    public static Generator<DataSetVersionMetaSummary> WithGeographicLevels(
        this Generator<DataSetVersionMetaSummary> generator,
        IEnumerable<GeographicLevel> geographicLevels
    ) => generator.ForInstance(s => s.SetGeographicLevels(geographicLevels));

    public static InstanceSetters<DataSetVersionMetaSummary> SetDefaults(
        this InstanceSetters<DataSetVersionMetaSummary> setters
    ) =>
        setters
            .Set(ms => ms.TimePeriodRange, DefaultTimePeriodRange)
            .SetDefault(ms => ms.Filters)
            .SetDefault(ms => ms.Indicators)
            .SetDefault(ms => ms.GeographicLevels);

    public static InstanceSetters<DataSetVersionMetaSummary> SetTimePeriodRange(
        this InstanceSetters<DataSetVersionMetaSummary> setters,
        TimePeriodRange timePeriodRange
    ) => setters.Set(ms => ms.TimePeriodRange, timePeriodRange);

    public static InstanceSetters<DataSetVersionMetaSummary> SetFilters(
        this InstanceSetters<DataSetVersionMetaSummary> setters,
        IEnumerable<string> filters
    ) => setters.Set(ms => ms.Filters, filters.ToList);

    public static InstanceSetters<DataSetVersionMetaSummary> SetIndicators(
        this InstanceSetters<DataSetVersionMetaSummary> setters,
        IEnumerable<string> indicators
    ) => setters.Set(ms => ms.Indicators, indicators.ToList);

    public static InstanceSetters<DataSetVersionMetaSummary> SetGeographicLevels(
        this InstanceSetters<DataSetVersionMetaSummary> setters,
        IEnumerable<GeographicLevel> geographicLevels
    ) => setters.Set(ms => ms.GeographicLevels, geographicLevels.ToList);

    private static TimePeriodRange DefaultTimePeriodRange(Faker faker)
    {
        const TimeIdentifier code = TimeIdentifier.CalendarYear;

        var startYear = 2000 + faker.Random.Int(0, 12);
        var endYear = startYear + faker.Random.Int(0, 12);

        return new TimePeriodRange
        {
            Start = new TimePeriodRangeBound { Code = code, Period = startYear.ToString() },
            End = new TimePeriodRangeBound { Code = code, Period = endYear.ToString() },
        };
    }
}
