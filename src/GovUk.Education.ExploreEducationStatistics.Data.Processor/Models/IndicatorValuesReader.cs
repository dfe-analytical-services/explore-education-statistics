# nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

/// <summary>
/// Class responsible for up-front calculation of the column indexes to read the measurements
/// for each Indicator from a given data file.
/// </summary>
public class IndicatorValuesReader
{
    private readonly Dictionary<Guid, int> _indicatorColumnIndexes;

    public IndicatorValuesReader(List<string> csvHeaders, List<Indicator> indicators)
    {
        _indicatorColumnIndexes = indicators.ToDictionary(
            indicator => indicator.Id,
            indicator => csvHeaders.FindIndex(h => h.Equals(indicator.Name))
        );
    }

    public Dictionary<Guid, string?> GetMeasures(List<string> rowValues)
    {
        return _indicatorColumnIndexes.ToDictionary(
            indicatorMeta => indicatorMeta.Key,
            indicatorMeta => indicatorMeta.Value != -1 ? rowValues[indicatorMeta.Value] : null
        );
    }
}
