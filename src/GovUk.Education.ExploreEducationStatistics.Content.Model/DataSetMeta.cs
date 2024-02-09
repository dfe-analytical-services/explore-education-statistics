#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetMeta : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public File File { get; set; }
    public List<CsvColumn> DataCsvColumns { get; set; }
    public List<CsvData> DataCsvSample { get; set; }
    public List<GeographicLevelMeta> GeographicLevels { get; set; }
    public List<LocationMeta> Locations { get; set; }
    public List<TimePeriodMeta> TimePeriods { get; set; }
    public List<FilterMeta> Filters { get; set; }
    public List<IndicatorMeta> Indicators { get; set; }
    public DateTime Created { get; set; }
}

public class CsvColumn
{
    public string Name { get; set; }
    public string Label { get; set; }
}

public class CsvData
{
    public string Data { get; set; }
    public int Row { get; set; }
}

public class GeographicLevelMeta
{
    public string Label { get; set; }
    public List<LocationMeta> Locations { get; set; }
}

public class LocationMeta
{
    public Guid Id { get; set; }
    public string Label { get; set; }
}

public class TimePeriodMeta
{
    public string Label { get; set; }
    public string Code { get; set; }
    public int Year { get; set; }
}

public class FilterMeta
{
    public string Label { get; set; }
    public List<FilterItemMeta> FilterItems { get; set; }
}

public class FilterItemMeta
{
    public Guid Id { get; set; }
    public string Label { get; set; }
}

public class IndicatorMeta
{
    public Guid Id { get; set; }
    public string Label { get; set; }
}


