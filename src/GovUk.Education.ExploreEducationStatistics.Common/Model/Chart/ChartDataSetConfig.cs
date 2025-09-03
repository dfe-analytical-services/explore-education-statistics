using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;

public class ChartDataSetConfig
{
    public ChartBaseDataSet DataSet;
    public ChartDataGrouping DataGrouping;
    public long? BoundaryLevel;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ChartDataGroupingType
{
    EqualIntervals,
    Quantiles,
    Custom,
}

public class ChartDataGrouping
{
    public ChartDataGroupingType Type;
    public int NumberOfGroups;
    public List<ChartCustomDataGroup> CustomGroups = new();
}
