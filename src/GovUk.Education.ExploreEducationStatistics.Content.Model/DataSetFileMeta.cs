#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileMeta
{
    public required List<string> GeographicLevels { get; set; }

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
    public required TimeIdentifier TimeIdentifier { get; set; }

    public required string Period { get; set; }
}

public class FilterMeta
{
    [JsonPropertyName("Id")]
    public required Guid Key { get; set; } // https://github.com/dotnet/efcore/issues/29380 maybe? @MarkFix

    public required string Label { get; set; }

    public string? Hint { get; set; }

    public required string ColumnName { get; set; }
}

public class IndicatorMeta
{
    [JsonPropertyName("Id")]
    public required Guid Key { get; set; }

    public required string Label { get; set; }

    public required string ColumnName { get; set; }
}
