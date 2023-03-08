#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;

public static class TimePeriodExtensions
{
    private static readonly EnumToEnumValueConverter<TimeIdentifier> TimeIdentifierLookup = new();

    public static string GetTimePeriod(this (int Year, TimeIdentifier TimeIdentifier) tuple)
    {
        return $"{tuple.Year}_{tuple.TimeIdentifier.GetEnumValue()}";
    }

    public static string GetTimePeriod(this Observation observation)
    {
        return $"{observation.Year}_{observation.TimeIdentifier.GetEnumValue()}";
    }

    public static (int Year, TimeIdentifier TimeIdentifier) GetTimePeriodTuple(this ObservationViewModel observation)
    {
        var parts = observation.TimePeriod.Split('_');

        if (parts.Length == 2
            && int.TryParse(parts[0], out var year)
            && TryParseTimeIdentifier(parts[1], out var timeIdentifier))
        {
            return (year, timeIdentifier);
        }

        throw new ArgumentException(
            $"Time period '{observation.TimePeriod}' is invalid and could not be parsed");
    }

    private static bool TryParseTimeIdentifier(string input, out TimeIdentifier timeIdentifier)
    {
        try
        {
            timeIdentifier = (TimeIdentifier)TimeIdentifierLookup.ConvertFromProvider.Invoke(input)!;
            return true;
        }
        catch (Exception)
        {
            timeIdentifier = default;
            return false;
        }
    }
}
