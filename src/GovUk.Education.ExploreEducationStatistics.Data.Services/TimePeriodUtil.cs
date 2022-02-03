using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public static class TimePeriodUtil
    {
        public static IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(
            (int Year, TimeIdentifier TimeIdentifier) start,
            (int Year, TimeIdentifier TimeIdentifier) end)
        {
            return Range(start.Year, start.TimeIdentifier, end.Year, end.TimeIdentifier);
        }

        public static IList<(int Year, TimeIdentifier TimeIdentifier)> Range(TimePeriodQuery timePeriodQuery)
        {
            return Range(timePeriodQuery.StartYear,
                timePeriodQuery.StartCode,
                timePeriodQuery.EndYear,
                timePeriodQuery.EndCode);
        }

        private static IList<(int Year, TimeIdentifier TimeIdentifier)> Range(int startYear,
            TimeIdentifier startCode, int endYear, TimeIdentifier endCode)
        {
            if (startYear <= 0)
            {
                throw new ArgumentNullException(nameof(startYear),
                    "The time period StartYear must be specified");
            }

            if (endYear <= 0)
            {
                throw new ArgumentNullException(nameof(endYear),
                    "The time period EndYear must be specified");
            }

            if (startYear > endYear)
            {
                throw new ArgumentOutOfRangeException(nameof(startYear),
                    "The time period StartYear cannot be greater than the EndYear");
            }

            var startYearLength = startYear.ToString().Length;
            var endYearLength = endYear.ToString().Length;

            if (startYearLength != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(startYear),
                    "The time period StartYear must be four digits");
            }

            if (endYearLength != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(endYear),
                    "The time period EndYear must be four digits");
            }

            if (!startCode.IsAlike(endCode))
            {
                throw new ArgumentException(
                    "The time period StartCode and EndCode must be the same or alike to generate a range");
            }

            if (startCode.IsYear())
            {
                return GetYearsForTimeIdentifier(startYear, endYear, startCode);
            }

            if (startCode.HasAssociatedRange())
            {
                return GetYearsForTimeIdentifierRange(startYear, endYear, startCode, endCode,
                    startCode.GetAssociatedRange());
            }

            throw new ArgumentException(
                "The time period StartCode and EndCode must either represent a year or be part of an associated range");
        }

        private static IList<(int Year, TimeIdentifier TimeIdentifier)> GetYearsForTimeIdentifier(int startYear,
            int endYear, TimeIdentifier identifier)
        {
            return Enumerable.Range(startYear, endYear - startYear + 1).Select(year => (year, identifier)).ToList();
        }

        private static IList<(int Year, TimeIdentifier TimeIdentifier)> GetYearsForTimeIdentifierRange(
            int startYear, int endYear, TimeIdentifier startCode, TimeIdentifier endCode, TimeIdentifier[] range)
        {
            var indexOfStart = Array.IndexOf(range, startCode);
            var indexOfEnd = Array.IndexOf(range, endCode);

            var result = new List<(int Year, TimeIdentifier TimeIdentifier)>();

            if (startYear == endYear)
            {
                return range
                    .Skip(indexOfStart).Take(indexOfEnd - indexOfStart + 1)
                    .Select(identifier => (startYear, identifier))
                    .ToList();
            }

            for (var i = startYear; i <= endYear; i++)
            {
                if (i == startYear)
                {
                    result.AddRange(range.Skip(indexOfStart).Select(identifier => (i, identifier)));
                }
                else if (i == endYear)
                {
                    result.AddRange(range.Take(indexOfEnd + 1).Select(identifier => (i, identifier)));
                }
                else
                {
                    result.AddRange(range.Select(identifier => (i, identifier)));
                }
            }

            return result;
        }
    }
}