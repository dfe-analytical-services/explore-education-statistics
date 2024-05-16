#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileMeta
{
    public List<string> GeographicLevels { get; set; } = new();

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeIdentifier? TimeIdentifier { get; set; } // EES-4918 to remove

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<int>? Years { get; set; } = new(); // EES-4918 to remove

    public TimePeriodRangeMeta TimePeriodRange { get; set; } = null!;

    public List<FilterMeta> Filters { get; set; } = new();

    public List<IndicatorMeta> Indicators { get; set; } = new();
}

public class TimePeriodRangeMeta
{
    public TimePeriodRangeBoundMeta Start { get; set; } = null!;

    public TimePeriodRangeBoundMeta End { get; set; } = null!;
}

public class TimePeriodRangeBoundMeta
{
    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier TimeIdentifier { get; set; }

    public int Year { get; set; }
}

public class FilterMeta
{
    public Guid Id { get; set; }

    public string Label { get; set; } = string.Empty;

    public string? Hint { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;
}

public class IndicatorMeta
{
    public Guid Id { get; set; }

    public string Label { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;
}

public class TimePeriodMeta
{
    public int Year;
    public TimeIdentifier TimeIdentifier;
}

