using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataSetFileMetaGeneratorExtensions
{
    public static Generator<DataSetFileMetaOld> DefaultDataSetFileMeta(this DataFixture fixture)
        => fixture.Generator<DataSetFileMetaOld>().WithDefaults();

    public static Generator<DataSetFileMetaOld> WithDefaults(this Generator<DataSetFileMetaOld> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<DataSetFileMetaOld> SetDefaults(this InstanceSetters<DataSetFileMetaOld> setters)
        => setters
            .SetGeographicLevels([GeographicLevel.Country])
            .SetTimePeriodRange(new TimePeriodRangeMetaOld
            {
                Start = new TimePeriodRangeBoundMetaOld { TimeIdentifier = TimeIdentifier.CalendarYear, Period = "2000", },
                End = new TimePeriodRangeBoundMetaOld { TimeIdentifier = TimeIdentifier.CalendarYear, Period = "2001", },
            })
            .SetFilters([ new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter 1",
                    ColumnName = "filter_1",
                },
            ])
            .SetIndicators([ new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator 1",
                    ColumnName = "indicator_1",
                },
            ]);

    public static Generator<DataSetFileMetaOld> WithGeographicLevels(
        this Generator<DataSetFileMetaOld> generator,
        List<GeographicLevel> geographicLevels)
        => generator.ForInstance(s => s.SetGeographicLevels(geographicLevels));

    public static Generator<DataSetFileMetaOld> WithTimePeriodRange(
        this Generator<DataSetFileMetaOld> generator,
        TimePeriodRangeMetaOld timePeriodRange)
        => generator.ForInstance(s => s.SetTimePeriodRange(timePeriodRange));

    public static Generator<DataSetFileMetaOld> WithFilters(
        this Generator<DataSetFileMetaOld> generator,
        List<FilterMetaOld> filters)
        => generator.ForInstance(s => s.SetFilters(filters));

    public static Generator<DataSetFileMetaOld> WithIndicators(
        this Generator<DataSetFileMetaOld> generator,
        List<IndicatorMetaOld> indicators)
        => generator.ForInstance(s => s.SetIndicators(indicators));

    public static InstanceSetters<DataSetFileMetaOld> SetGeographicLevels(
        this InstanceSetters<DataSetFileMetaOld> setters,
        List<GeographicLevel> geographicLevels)
        => setters.Set(s => s.GeographicLevels, geographicLevels);

    public static InstanceSetters<DataSetFileMetaOld> SetTimePeriodRange(
        this InstanceSetters<DataSetFileMetaOld> setters,
        TimePeriodRangeMetaOld timePeriodRange)
        => setters.Set(s => s.TimePeriodRange, timePeriodRange);

    public static InstanceSetters<DataSetFileMetaOld> SetFilters(
        this InstanceSetters<DataSetFileMetaOld> setters,
        List<FilterMetaOld> filters)
        => setters.Set(s => s.Filters, filters);

    public static InstanceSetters<DataSetFileMetaOld> SetIndicators(
        this InstanceSetters<DataSetFileMetaOld> setters,
        List<IndicatorMetaOld> indicators)
        => setters.Set(s => s.Indicators, indicators);
}
