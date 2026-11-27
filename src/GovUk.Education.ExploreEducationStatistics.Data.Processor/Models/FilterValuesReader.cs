# nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

/// <summary>
/// Class responsible for up-front calculation of the column indexes to read the filter grouping
/// and filter item columns for each Filter from a given data file.
///
/// This class also keeps an efficient lookup cache for Filter Items based upon its owning Filter
/// and FilterGroup labels and its own label.
/// </summary>
public class FilterValuesReader
{
    private const string NotSpecifiedFilterItemLabel = "Not specified";

    private readonly Dictionary<Guid, int> _filterColumnIndexes;
    private readonly Dictionary<Guid, int> _filterGroupColumnIndexes;
    private readonly Dictionary<string, FilterItem> _filterItemCache;

    public FilterValuesReader(List<string> csvHeaders, List<Filter> filters)
    {
        _filterColumnIndexes = filters.ToDictionary(
            filter => filter.Id,
            filter => csvHeaders.FindIndex(h => h.Equals(filter.Name))
        );

        _filterGroupColumnIndexes = filters.ToDictionary(
            filter => filter.Id,
            filter => csvHeaders.FindIndex(h => h.Equals(filter.GroupCsvColumn))
        );

        _filterItemCache = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToDictionary(fi => $"{fi.FilterGroup.Filter.Label}_{fi.FilterGroup.Label}_{fi.Label}".ToLower(), fi => fi);
    }

    public string GetFilterItemLabel(IReadOnlyList<string> rowValues, Guid filterId)
    {
        var columnIndex = _filterColumnIndexes[filterId];

        if (columnIndex == -1)
        {
            return NotSpecifiedFilterItemLabel;
        }

        return rowValues[columnIndex].Trim().NullIfWhiteSpace() ?? NotSpecifiedFilterItemLabel;
    }

    public string GetFilterGroupLabel(IReadOnlyList<string> rowValues, Guid filterId)
    {
        var columnIndex = _filterGroupColumnIndexes[filterId];

        if (columnIndex == -1)
        {
            return FilterGroup.NotSpecifiedFilterGroupLabel;
        }

        return rowValues[columnIndex].Trim().NullIfWhiteSpace() ?? FilterGroup.NotSpecifiedFilterGroupLabel;
    }

    public FilterItem GetFilterItem(IReadOnlyList<string> rowValues, Filter filter)
    {
        var filterItemLabel = GetFilterItemLabel(rowValues, filter.Id);
        var filterGroupLabel = GetFilterGroupLabel(rowValues, filter.Id);
        return LookupCachedFilterItem(filterItemLabel, filterGroupLabel, filter.Label);
    }

    private FilterItem LookupCachedFilterItem(string filterItemLabel, string filterGroupLabel, string filterLabel)
    {
        return _filterItemCache[$"{filterLabel}_{filterGroupLabel}_{filterItemLabel}".ToLower()];
    }
}
