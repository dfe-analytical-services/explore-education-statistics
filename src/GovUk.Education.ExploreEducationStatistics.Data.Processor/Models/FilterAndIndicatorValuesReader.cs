# nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

/// <summary>
/// Class responsible for up-front calculation of the column indexes to look up particular
/// pieces of information from a given data file. This includes column indexes like the
/// various pieces of information to read filter grouping and filter item columns
/// for each Filter, and measurements for indicators.
///
/// This class also keeps an efficient lookup cache for Filter Items based upon its owning Filter
/// and FilterGroup labels and its own label.
/// </summary>
public class FilterAndIndicatorValuesReader
{
    private const string NotSpecifiedFilterItemLabel = "Not specified";

    private readonly Dictionary<Guid, int> _indicatorColumnIndexes;
    private readonly SortedList<int, Guid> _filterColumnIndexes = [];
    private readonly Dictionary<Guid, int> _filterGroupColumnIndexes;
    private readonly Dictionary<string, FilterItem> _filterItemCache;

    public FilterAndIndicatorValuesReader(List<string> csvHeaders, SubjectMeta subjectMeta)
    {
        _indicatorColumnIndexes = subjectMeta
            .Indicators
            .ToDictionary(
                indicatorMeta => indicatorMeta.Indicator.Id,
                indicatorMeta => csvHeaders.FindIndex(h => h.Equals(indicatorMeta.Column)));

        subjectMeta
            .Filters
            .ForEach(filterMeta =>
                _filterColumnIndexes.Add(
                    csvHeaders.FindIndex(h => h.Equals(filterMeta.Column)),
                    filterMeta.Filter.Id));

        _filterGroupColumnIndexes = subjectMeta
            .Filters
            .ToDictionary(
                filterMeta => filterMeta.Filter.Id,
                filterMeta => csvHeaders.FindIndex(h => h.Equals(filterMeta.Filter.GroupCsvColumn)));

        _filterItemCache = subjectMeta
            .Filters
            .Select(meta => meta.Filter)
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToDictionary(
                fi => $"{fi.FilterGroup.Filter.Label}_{fi.FilterGroup.Label}_{fi.Label}".ToLower(),
                fi => fi);
    }

    public Dictionary<Guid, string?> GetMeasures(List<string> rowValues)
    {
        return _indicatorColumnIndexes
            .ToDictionary(
                indicatorMeta => indicatorMeta.Key,
                indicatorMeta => indicatorMeta.Value != -1 ? rowValues[indicatorMeta.Value] : null);
    }

    public string GetFilterItemLabel(
        IReadOnlyList<string> rowValues,
        Guid filterId)
    {
        var indexOfFilter = _filterColumnIndexes.IndexOfValue(filterId);
        var columnIndex = _filterColumnIndexes.GetKeyAtIndex(indexOfFilter);

        return columnIndex == -1
            ? NotSpecifiedFilterItemLabel
            : rowValues[columnIndex].Trim().NullIfWhiteSpace() ?? NotSpecifiedFilterItemLabel;
    }

    public int GetFilterItemTier(Guid filterId)
    {
        return _filterColumnIndexes.IndexOfValue(filterId) + 1;
    }

    public string GetFilterGroupLabel(
        IReadOnlyList<string> rowValues,
        Guid filterId)
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
