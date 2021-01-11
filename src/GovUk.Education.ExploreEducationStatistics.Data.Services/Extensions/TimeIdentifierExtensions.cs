using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.TimeIdentifierUtil;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions
{
    public static class TimeIdentifierExtensions
    {
        public static bool IsAlike(this TimeIdentifier timeIdentifier, TimeIdentifier compare)
        {
            if (timeIdentifier.Equals(compare))
            {
                return true;
            }

            return IsAcademicQuarter(timeIdentifier) && IsAcademicQuarter(compare) ||
                   IsCalendarQuarter(timeIdentifier) && IsCalendarQuarter(compare) ||
                   IsFinancialQuarter(timeIdentifier) && IsFinancialQuarter(compare) ||
                   IsTaxQuarter(timeIdentifier) && IsTaxQuarter(compare) ||
                   IsMonth(timeIdentifier) && IsMonth(compare) ||
                   IsWeek(timeIdentifier) && IsWeek(compare) ||
                   IsTerm(timeIdentifier) && IsTerm(compare) ||
                   IsFinancialYearPart(timeIdentifier) && IsFinancialYearPart(compare);
        }

        public static bool IsYear(this TimeIdentifier timeIdentifier)
        {
            return GetYears().Contains(timeIdentifier);
        }

        public static bool IsAcademicQuarter(this TimeIdentifier timeIdentifier)
        {
            return GetAcademicQuarters().Contains(timeIdentifier);
        }

        public static bool IsCalendarQuarter(this TimeIdentifier timeIdentifier)
        {
            return GetCalendarQuarters().Contains(timeIdentifier);
        }

        public static bool IsFinancialQuarter(this TimeIdentifier timeIdentifier)
        {
            return GetFinancialQuarters().Contains(timeIdentifier);
        }

        public static bool IsTaxQuarter(this TimeIdentifier timeIdentifier)
        {
            return GetTaxQuarters().Contains(timeIdentifier);
        }

        public static bool IsMonth(this TimeIdentifier timeIdentifier)
        {
            return GetMonths().Contains(timeIdentifier);
        }

        public static bool IsWeek(this TimeIdentifier timeIdentifier)
        {
            return GetWeeks().Contains(timeIdentifier);
        }

        public static bool IsTerm(this TimeIdentifier timeIdentifier)
        {
            return GetTerms().Contains(timeIdentifier);
        }

        public static bool IsFinancialYearPart(this TimeIdentifier timeIdentifier)
        {
            return GetFinancialYearParts().Contains(timeIdentifier);
        }

        public static bool HasAssociatedRange(this TimeIdentifier timeIdentifier)
        {
            return !timeIdentifier.IsYear();
        }

        public static TimeIdentifier[] GetAssociatedRange(this TimeIdentifier timeIdentifier)
        {
            if (timeIdentifier.IsMonth())
            {
                return GetMonths();
            }

            if (timeIdentifier.IsWeek())
            {
                return GetWeeks();
            }

            if (timeIdentifier.IsAcademicQuarter())
            {
                return GetAcademicQuarters();
            }

            if (timeIdentifier.IsCalendarQuarter())
            {
                return GetCalendarQuarters();
            }

            if (timeIdentifier.IsFinancialQuarter())
            {
                return GetFinancialQuarters();
            }

            if (timeIdentifier.IsTaxQuarter())
            {
                return GetTaxQuarters();
            }

            if (timeIdentifier.IsTerm())
            {
                return GetTerms();
            }

            if (timeIdentifier.IsFinancialYearPart())
            {
                return GetFinancialYearParts();
            }

            throw new ArgumentOutOfRangeException(nameof(timeIdentifier),
                "The time identifier has no associated range");
        }
    }
}