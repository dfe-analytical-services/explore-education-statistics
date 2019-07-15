using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using static GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TimeIdentifierUtil;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions
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
                   IsTerm(timeIdentifier) && IsTerm(compare);
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

        public static bool IsNumberOfTerms(this TimeIdentifier timeIdentifier)
        {
            return GetNumberOfTerms().Contains(timeIdentifier);
        }

        public static bool IsTerm(this TimeIdentifier timeIdentifier)
        {
            return GetTerms().Contains(timeIdentifier);
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

            throw new ArgumentOutOfRangeException(nameof(timeIdentifier),
                "The time identifier has no associated range");
        }
    }
}