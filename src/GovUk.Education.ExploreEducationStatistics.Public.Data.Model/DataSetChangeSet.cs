using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class DataSetChangeSet<TChanges> : ICreatedTimestamp<DateTimeOffset>
{
    public Guid Id { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public Guid DataSetVersionId { get; set; }

    public abstract List<TChanges> Changes { get; set; }

    public DateTimeOffset Created { get; set; }
}

public class DataSetChange<TState>
{
    [JsonPropertyName("Id")]
    public Guid Identifier { get; set; }

    [NotMapped]
    public TState? CurrentState { get; set; } = default;

    [NotMapped]
    public TState? PreviousState { get; set; } = default;

    public DataSetChangeType Type { get; set; }
}

public class DataSetChangeFilter : DataSetChangeSet<DataSetChange<FilterChangeState>>
{
    public override List<DataSetChange<FilterChangeState>> Changes { get; set; } = new();
}

[Keyless]
public class FilterChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Hint { get; set; } = string.Empty;
}

public class DataSetChangeFilterOption : DataSetChangeSet<DataSetChange<FilterOptionChangeState>>
{
    public override List<DataSetChange<FilterOptionChangeState>> Changes { get; set; } = new();
}

public class FilterOptionChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string FilterId { get; set; } = string.Empty;

    public bool? IsAggregate { get; set; } = null!;
}

public class DataSetChangeIndicator : DataSetChangeSet<DataSetChange<IndicatorChangeState>>
{
    public override List<DataSetChange<IndicatorChangeState>> Changes { get; set; } = new();
}

public class IndicatorChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public byte DecimalPlaces { get; set; }
}

public class DataSetChangeLocation : DataSetChangeSet<DataSetChange<LocationChangeState>>
{
    public override List<DataSetChange<LocationChangeState>> Changes { get; set; } = new();
}

public class LocationChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public GeographicLevel Level { get; set; }
}

public class DataSetChangeTimePeriod : DataSetChangeSet<DataSetChange<TimePeriodChangeState>>
{
    public override List<DataSetChange<TimePeriodChangeState>> Changes { get; set; } = new();
}

public class TimePeriodChangeState
{
    public string Id { get; set; } = string.Empty;

    public TimeIdentifier Code { get; set; }

    public int Year { get; set; }
}
