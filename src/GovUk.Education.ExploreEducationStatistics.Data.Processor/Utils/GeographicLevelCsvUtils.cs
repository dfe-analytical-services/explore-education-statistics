#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;

public static class GeographicLevelCsvUtils
{
    private static readonly EnumToEnumLabelConverter<GeographicLevel> Lookups = new();

    public static GeographicLevel GetGeographicLevel(IReadOnlyList<string> rowValues, List<string> colValues)
    {
        var value = CsvUtils.Value(rowValues, colValues, "geographic_level");

        try
        {
            return (GeographicLevel) Lookups.ConvertFromProvider.Invoke(value)!;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new InvalidGeographicLevelException(value!);
        }
    }
}
