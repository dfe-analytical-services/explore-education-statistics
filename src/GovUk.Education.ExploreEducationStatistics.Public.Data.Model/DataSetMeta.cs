using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonKnownTypesConverter<DataSetChange>))]
public abstract class DataSetMeta<TMeta> : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset>
    where TMeta : class
{
    public Guid Id { get; set; }

    public Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public abstract DataSetMetaType Type { get; }

    public TMeta Meta { get; set; } = null!;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }
}

public class DataSetMetaFilters : DataSetMeta<List<FilterMeta>>
{
    public override DataSetMetaType Type => DataSetMetaType.Filters;
}

public class DataSetMetaIndicators : DataSetMeta<List<IndicatorMeta>>
{
    public override DataSetMetaType Type => DataSetMetaType.Indicators;
}

public class DataSetMetaGeographicLevels : DataSetMeta<List<GeographicLevel>>
{
    public override DataSetMetaType Type => DataSetMetaType.GeographicLevels;
}

public class DataSetMetaLocations : DataSetMeta<List<LocationMeta>>
{
    public override DataSetMetaType Type => DataSetMetaType.Locations;
}

public class DataSetMetaTimePeriods : DataSetMeta<List<TimePeriodMeta>>
{
    public override DataSetMetaType Type => DataSetMetaType.TimePeriods;
}
