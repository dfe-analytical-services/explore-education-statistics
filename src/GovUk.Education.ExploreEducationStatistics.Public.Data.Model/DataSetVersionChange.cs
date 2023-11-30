using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonKnownTypesConverter<DataSetVersionChange>))]
public abstract class DataSetVersionChange
{
    public Guid Id { get; set; }
    public ChangeType Type { get; set; }
    public abstract MetadataType MetadataType { get; }
}

public abstract class DataSetVersionMetadataChange<T> : DataSetVersionChange where T : class
{
    public T CurrentState { get; set; } = null!;
    public T PreviousState { get; set; } = null!;
}

public class DataSetVersionFilterChange : DataSetVersionMetadataChange<DataSetVersionFilterChangeState>
{
    public override MetadataType MetadataType => MetadataType.Filter;
}

public class DataSetVersionFilterChangeState
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class DataSetVersionFilterItemChange : DataSetVersionMetadataChange<DataSetVersionFilterItemChangeState>
{
    public override MetadataType MetadataType => MetadataType.FilterItem;
}

public class DataSetVersionFilterItemChangeState
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string FilterName { get; set; } = string.Empty;
    public bool IsAggregate { get; set; }
}

public class DataSetVersionIndicatorChange : DataSetVersionMetadataChange<DataSetVersionIndicatorChangeState>
{
    public override MetadataType MetadataType => MetadataType.Indicator;
}

public class DataSetVersionIndicatorChangeState
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public byte DecimalPlaces { get; set; }
}

public class DataSetVersionLocationChange : DataSetVersionMetadataChange<DataSetVersionLocationChangeState>
{
    public override MetadataType MetadataType => MetadataType.Location;
}

public class DataSetVersionLocationChangeState
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public GeographicLevel Level { get; set; }
}

public class DataSetVersionTimePeriodChange : DataSetVersionMetadataChange<DataSetVersionTimePeriodChangeState>
{
    public override MetadataType MetadataType => MetadataType.TimePeriod;
}

public class DataSetVersionTimePeriodChangeState
{
    public TimeIdentifier Code { get; set; }
    public int Year { get; set; }
    public string Range { get; set; } = string.Empty;
}
