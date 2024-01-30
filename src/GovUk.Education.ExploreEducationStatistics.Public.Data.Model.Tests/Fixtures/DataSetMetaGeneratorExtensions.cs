using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetMetaGeneratorExtensions
{
    public static Generator<DataSetMeta> DefaultDataSetMeta(this DataFixture fixture)
        => fixture.Generator<DataSetMeta>().WithDefaults();

    public static Generator<DataSetMeta> DefaultDataSetMeta(
        this DataFixture fixture,
        int filters,
        int indicators,
        int locations,
        int timePeriods,
        int maxFilterOptions = 10,
        int maxLocationOptions = 10)
    {
        var locationMeta = fixture.DefaultLocationMeta()
            .WithOptions(() => fixture.DefaultLocationOptionMeta().GenerateRandomList(maxLocationOptions))
            .GenerateList(locations);

        return fixture.Generator<DataSetMeta>()
            .WithDefaults()
            .WithFilters(fixture.DefaultFilterMeta()
                .WithOptions(() => fixture.DefaultFilterOptionMeta().GenerateRandomList(maxFilterOptions))
                .Generate(filters))
            .WithIndicators(fixture.DefaultIndicatorMeta().Generate(indicators))
            .WithGeographicLevels(locationMeta.Select(m => m.Level))
            .WithLocations(locationMeta)
            .WithTimePeriods(fixture.DefaultTimePeriodMeta().Generate(timePeriods));
    }

    public static Generator<DataSetMeta> WithDefaults(this Generator<DataSetMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSetMeta> WithDataSetVersion(
        this Generator<DataSetMeta> generator,
        DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<DataSetMeta> WithFilters(
        this Generator<DataSetMeta> generator,
        IEnumerable<FilterMeta> filters)
        => generator.ForInstance(s => s.SetFilters(filters));

    public static Generator<DataSetMeta> WithIndicators(
        this Generator<DataSetMeta> generator,
        IEnumerable<IndicatorMeta> indicators)
        => generator.ForInstance(s => s.SetIndicators(indicators));

    public static Generator<DataSetMeta> WithTimePeriods(
        this Generator<DataSetMeta> generator,
        IEnumerable<TimePeriodMeta> timePeriods)
        => generator.ForInstance(s => s.SetTimePeriods(timePeriods));

    public static Generator<DataSetMeta> WithLocations(
        this Generator<DataSetMeta> generator,
        IEnumerable<LocationMeta> locations)
        => generator.ForInstance(s => s.SetLocations(locations));

    public static Generator<DataSetMeta> WithGeographicLevels(
        this Generator<DataSetMeta> generator,
        IEnumerable<GeographicLevel> geographicLevels)
        => generator.ForInstance(s => s.SetGeographicLevels(geographicLevels));

    public static InstanceSetters<DataSetMeta> SetDefaults(this InstanceSetters<DataSetMeta> setters)
        => setters
            .SetDefault(m => m.Id)
            .Set(m => m.Created, m => m.Date.PastOffset())
            .Set(
                m => m.Updated,
                (f, dataSet) => f.Date.SoonOffset(14, dataSet.Created)
            );

    public static InstanceSetters<DataSetMeta> SetDataSetVersion(
        this InstanceSetters<DataSetMeta> setters,
        DataSetVersion dataSetVersion)
        => setters
            .Set(
                m => m.DataSetVersion,
                (_, dsm) =>
                {
                    dataSetVersion.Meta = dsm;
                    return dataSetVersion;
                }
            )
            .Set(m => m.DataSetVersionId, dataSetVersion.Id);

    public static InstanceSetters<DataSetMeta> SetFilters(
        this InstanceSetters<DataSetMeta> setters,
        IEnumerable<FilterMeta> filters)
        => setters.Set(m => m.Filters, filters.ToList());

    public static InstanceSetters<DataSetMeta> SetIndicators(
        this InstanceSetters<DataSetMeta> setters,
        IEnumerable<IndicatorMeta> indicators)
        => setters.Set(m => m.Indicators, indicators.ToList());

    public static InstanceSetters<DataSetMeta> SetTimePeriods(
        this InstanceSetters<DataSetMeta> setters,
        IEnumerable<TimePeriodMeta> timePeriods)
        => setters.Set(m => m.TimePeriods, timePeriods.ToList());

    public static InstanceSetters<DataSetMeta> SetLocations(
        this InstanceSetters<DataSetMeta> setters,
        IEnumerable<LocationMeta> locations)
        => setters.Set(m => m.Locations, locations.ToList());

    public static InstanceSetters<DataSetMeta> SetGeographicLevels(
        this InstanceSetters<DataSetMeta> setters,
        IEnumerable<GeographicLevel> geographicLevels)
        => setters.Set(m => m.GeographicLevels, geographicLevels.ToList());
}
