using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonKnownTypesConverter<DataSetChange>))]
public abstract class DataSetChange
{
    public Guid Id { get; set; }

    public DataSetChangeType Type { get; set; }

    public abstract DataSetChangeMetaType MetaType { get; }
}

public abstract class DataSetChangeMeta<TState> : DataSetChange where TState : class
{
    public TState? CurrentState { get; set; } = null!;

    public TState? PreviousState { get; set; } = null!;
}

public class DataSetChangeFilter : DataSetChangeMeta<FilterChangeState>
{
    public override DataSetChangeMetaType MetaType => DataSetChangeMetaType.Filter;
}

public class FilterChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Hint { get; set; } = string.Empty;
}

public class DataSetChangeFilterOption : DataSetChangeMeta<FilterOptionChangeState>
{
    public override DataSetChangeMetaType MetaType => DataSetChangeMetaType.FilterOption;
}

public class FilterOptionChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string FilterId { get; set; } = string.Empty;

    public bool? IsAggregate { get; set; } = null!;
}

public class DataSetChangeIndicator : DataSetChangeMeta<IndicatorChangeState>
{
    public override DataSetChangeMetaType MetaType => DataSetChangeMetaType.Indicator;
}

public class IndicatorChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public byte DecimalPlaces { get; set; }
}

public class DataSetChangeLocation : DataSetChangeMeta<LocationChangeState>
{
    public override DataSetChangeMetaType MetaType => DataSetChangeMetaType.Location;
}

public class LocationChangeState
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public GeographicLevel Level { get; set; }
}

public class DataSetChangeTimePeriod : DataSetChangeMeta<TimePeriodChangeState>
{
    public override DataSetChangeMetaType MetaType => DataSetChangeMetaType.TimePeriod;
}

public class TimePeriodChangeState
{
    public TimeIdentifier Code { get; set; }

    public int Year { get; set; }
}
