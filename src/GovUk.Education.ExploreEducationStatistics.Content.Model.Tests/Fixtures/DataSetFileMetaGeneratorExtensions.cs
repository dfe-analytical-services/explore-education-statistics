using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataSetFileMetaGeneratorExtensions
{
    public static Generator<DataSetFileMeta> DefaultDataSetFileMeta(this DataFixture fixture)
        => fixture.Generator<DataSetFileMeta>().WithDefaults();

    public static Generator<DataSetFileMeta> WithDefaults(this Generator<DataSetFileMeta> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<DataSetFileMeta> SetDefaults(this InstanceSetters<DataSetFileMeta> setters)
        => setters
            .SetGeographicLevels(new List<string> { "National " })
            .SetTimeIdentifier(TimeIdentifier.AcademicYear)
            .SetYears(new List<int> { 2000, 2001 })
            .SetFilters(new List<FilterMeta> { new() { Label = "Filter 1" }, })
            .SetIndicators(new List<IndicatorMeta> { new() { Label = "Indicator 1" }, });

    public static Generator<DataSetFileMeta> WithGeographicLevels(
        this Generator<DataSetFileMeta> generator,
        List<string> geographicLevels)
        => generator.ForInstance(s => s.SetGeographicLevels(geographicLevels));

    public static Generator<DataSetFileMeta> WithTimeIdentifier(
        this Generator<DataSetFileMeta> generator,
        TimeIdentifier timeIdentifier)
        => generator.ForInstance(s => s.SetTimeIdentifier(timeIdentifier));

    public static Generator<DataSetFileMeta> WithYears(
        this Generator<DataSetFileMeta> generator,
        List<int> years)
        => generator.ForInstance(s => s.SetYears(years));

    public static Generator<DataSetFileMeta> WithFilters(
        this Generator<DataSetFileMeta> generator,
        List<FilterMeta> filters)
        => generator.ForInstance(s => s.SetFilters(filters));

    public static Generator<DataSetFileMeta> WithIndicators(
        this Generator<DataSetFileMeta> generator,
        List<IndicatorMeta> indicators)
        => generator.ForInstance(s => s.SetIndicators(indicators));

    public static InstanceSetters<DataSetFileMeta> SetGeographicLevels(
        this InstanceSetters<DataSetFileMeta> setters,
        List<string> geographicLevels)
        => setters.Set(s => s.GeographicLevels, geographicLevels);

    public static InstanceSetters<DataSetFileMeta> SetTimeIdentifier(
        this InstanceSetters<DataSetFileMeta> setters,
        TimeIdentifier timeIdentifier)
        => setters.Set(s => s.TimeIdentifier, timeIdentifier);

    public static InstanceSetters<DataSetFileMeta> SetYears(
        this InstanceSetters<DataSetFileMeta> setters,
        List<int> years)
        => setters.Set(s => s.Years, years);

    public static InstanceSetters<DataSetFileMeta> SetFilters(
        this InstanceSetters<DataSetFileMeta> setters,
        List<FilterMeta> filters)
        => setters.Set(s => s.Filters, filters);

    public static InstanceSetters<DataSetFileMeta> SetIndicators(
        this InstanceSetters<DataSetFileMeta> setters,
        List<IndicatorMeta> indicators)
        => setters.Set(s => s.Indicators, indicators);
}
