#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileMeta
{
    public required int NumDataFileRows { get; set; }

    // NOTE: GeographicLevels aren't in DataSetFileMeta JSON because they need to queryable
    // So that meta data lives in the DataSetFileVersionGeographicLevels table

    public required TimePeriodRangeMeta TimePeriodRange { get; set; }

    public required List<FilterMeta> Filters { get; set; }

    public required List<IndicatorMeta> Indicators { get; set; }
}

public class TimePeriodRangeMeta
{
    public required TimePeriodRangeBoundMeta Start { get; set; }

    public required TimePeriodRangeBoundMeta End { get; set; }
}

public class TimePeriodRangeBoundMeta
{
    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public required TimeIdentifier TimeIdentifier { get; set; }

    public required string Period { get; set; }
}

public class FilterMeta
{
    public required Guid Id { get; set; }

    public required string Label { get; set; }

    public string? Hint { get; set; }

    public required string ColumnName { get; set; }

    [JsonIgnore]
    public string? ParentFilter { get; set; }
}

public class IndicatorMeta
{
    public required Guid Id { get; set; }

    public required string Label { get; set; }

    public required string ColumnName { get; set; }
}
