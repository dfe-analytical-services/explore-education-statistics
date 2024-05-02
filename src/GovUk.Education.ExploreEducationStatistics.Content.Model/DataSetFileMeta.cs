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
    public TimeIdentifier TimeIdentifier { get; set; }

    public List<int> Years { get; set; } = new();

    public List<FilterMeta> Filters { get; set; } = new();

    public List<IndicatorMeta> Indicators { get; set; } = new();
}

public class FilterMeta
{
    public Guid Id { get; set; }
    public string Label { get; set; }

    public string? Hint { get; set; }

    public string ColumnName { get; set; }
}

public class IndicatorMeta
{
    public Guid Id { get; set; }
    public string Label { get; set; }

    public string ColumnName { get; set; }
}

public class TimePeriodMeta
{
    public int Year;
    public TimeIdentifier TimeIdentifier;
}

