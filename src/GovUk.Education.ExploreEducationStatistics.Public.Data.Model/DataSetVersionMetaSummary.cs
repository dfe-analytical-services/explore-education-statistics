using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersionMetaSummary
{
    public required TimePeriodRange TimePeriodRange { get; set; }

    public required List<string> Filters { get; set; } = [];

    public required List<string> Indicators { get; set; } = [];

    public required List<GeographicLevel> GeographicLevels { get; set; } = [];

    public static DataSetVersionMetaSummary Create(DataSetVersion dataSetVersion)
    {
        if (dataSetVersion.TimePeriodMetas.Count < 2)
        {
            throw new IndexOutOfRangeException(
                "Must have at least two time periods for meta summary"
            );
        }

        var timePeriods = dataSetVersion
            .TimePeriodMetas.OrderBy(t => t.Period)
            .ThenBy(t => t.Code)
            .ToList();

        return new DataSetVersionMetaSummary
        {
            Filters = dataSetVersion.FilterMetas.Select(f => f.Label).ToList(),
            Indicators = dataSetVersion.IndicatorMetas.Select(i => i.Label).ToList(),
            GeographicLevels = dataSetVersion.GeographicLevelMeta!.Levels,
            TimePeriodRange = new TimePeriodRange
            {
                Start = TimePeriodRangeBound.Create(timePeriods[0]),
                End = TimePeriodRangeBound.Create(timePeriods[^1]),
            },
        };
    }
}
