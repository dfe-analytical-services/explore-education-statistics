# nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

public class DataFileReader
{
    private const string DefaultFilterGroupLabel = "Default";
    private const string DefaultFilterItemLabel = "Not specified";
    
    private static readonly EnumToEnumLabelConverter<TimeIdentifier> TimeIdentifierLookup = new();
    private static readonly EnumToEnumLabelConverter<GeographicLevel> GeographicLevelLookup = new();
    
    private readonly int _timeIdentifierColumnIndex;
    private readonly int _yearColumnIndex;
    private readonly int _geographicLevelColumnIndex;
    private readonly Dictionary<Indicator, int>? _indicatorColumnIndexes;
    private readonly Dictionary<Filter, int>? _filterColumnIndexes;
    private readonly Dictionary<Filter, int>? _filterGroupColumnIndexes;

    public DataFileReader(List<string> csvHeaders, SubjectMeta? subjectMeta = null)
    {
        _timeIdentifierColumnIndex = csvHeaders.FindIndex(h => h.Equals("time_identifier"));
        _yearColumnIndex = csvHeaders.FindIndex(h => h.Equals("time_period"));
        _geographicLevelColumnIndex = csvHeaders.FindIndex(h => h.Equals("geographic_level"));

        if (subjectMeta != null)
        {
            _indicatorColumnIndexes = subjectMeta
                .Indicators
                .ToDictionary(
                    indicatorMeta => indicatorMeta.Indicator,
                    indicatorMeta => csvHeaders.FindIndex(h => h.Equals(indicatorMeta.Column)));

            _filterColumnIndexes = subjectMeta
                .Filters
                .ToDictionary(
                    filterMeta => filterMeta.Filter,
                    filterMeta => csvHeaders.FindIndex(h => h.Equals(filterMeta.Column)));

            _filterGroupColumnIndexes = subjectMeta
                .Filters
                .ToDictionary(
                    filterMeta => filterMeta.Filter,
                    filterMeta => csvHeaders.FindIndex(h => h.Equals(filterMeta.FilterGroupingColumn)));
        }
    }


    public TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> rowValues)
    {
        var value = rowValues[_timeIdentifierColumnIndex];
            
        try
        {
            return (TimeIdentifier) TimeIdentifierLookup.ConvertFromProvider.Invoke(value)!;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new InvalidTimeIdentifierException(value);
        }
    }

    public int GetYear(IReadOnlyList<string> rowValues)
    {
        var year = rowValues[_yearColumnIndex];
            
        if (year == null)
        {
            throw new InvalidTimePeriodException(null);
        }

        return int.Parse(year.Substring(0, 4));
    }
    
    public GeographicLevel GetGeographicLevel(IReadOnlyList<string> rowValues)
    {
        var value = rowValues[_geographicLevelColumnIndex];

        try
        {
            return (GeographicLevel) GeographicLevelLookup.ConvertFromProvider.Invoke(value)!;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new InvalidGeographicLevelException(value!);
        }
    }

    public Dictionary<Guid, string> GetMeasures(List<string> rowValues)
    {
        return _indicatorColumnIndexes!
            .ToDictionary(
                indicatorMeta => indicatorMeta.Key.Id,
                indicatorMeta => rowValues[indicatorMeta.Value]);
    }

    public string GetFilterItemValue(
        IReadOnlyList<string> rowValues,
        Filter filter)
    {
        return rowValues[_filterColumnIndexes![filter]].Trim().NullIfWhiteSpace() ?? DefaultFilterItemLabel;
    }

    public string GetFilterGroupValue(
        IReadOnlyList<string> rowValues,
        Filter filter)
    {
        var value = _filterGroupColumnIndexes![filter] != -1 ? rowValues[_filterGroupColumnIndexes[filter]] : null;
        return value?.Trim().NullIfWhiteSpace() ?? DefaultFilterGroupLabel;
    }
}