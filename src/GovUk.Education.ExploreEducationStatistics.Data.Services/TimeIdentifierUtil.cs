using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Category = GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifierCategory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public static class TimeIdentifierUtil
    {
        
        public static TimeIdentifier[] GetMonths()
        {
            return Category.Month.GetTimeIdentifiers();
        }

        public static TimeIdentifier[] GetYears()
        {
            return new[]
            {
                TimeIdentifier.AcademicYear,
                TimeIdentifier.CalendarYear,
                TimeIdentifier.FinancialYear,
                TimeIdentifier.TaxYear,
                TimeIdentifier.EndOfMarch,
                TimeIdentifier.FiveHalfTerms,
                TimeIdentifier.SixHalfTerms
            };
        }

        public static TimeIdentifier[] GetTerms()
        {
            return Category.Term.GetTimeIdentifiers();
        }

        public static TimeIdentifier[] GetNumberOfTerms()
        {
            return new[]
            {
                TimeIdentifier.FiveHalfTerms,
                TimeIdentifier.SixHalfTerms
            };
        }

        public static TimeIdentifier[] GetAcademicQuarters()
        {
            return new[]
            {
                TimeIdentifier.AcademicYearQ1,
                TimeIdentifier.AcademicYearQ1Q2,
                TimeIdentifier.AcademicYearQ1Q3,
                TimeIdentifier.AcademicYearQ1Q4,
                TimeIdentifier.AcademicYearQ2,
                TimeIdentifier.AcademicYearQ2Q3,
                TimeIdentifier.AcademicYearQ2Q4,
                TimeIdentifier.AcademicYearQ3,
                TimeIdentifier.AcademicYearQ3Q4,
                TimeIdentifier.AcademicYearQ4
            };
        }

        public static TimeIdentifier[] GetCalendarQuarters()
        {
            return new[]
            {
                TimeIdentifier.CalendarYearQ1,
                TimeIdentifier.CalendarYearQ1Q2,
                TimeIdentifier.CalendarYearQ1Q3,
                TimeIdentifier.CalendarYearQ1Q4,
                TimeIdentifier.CalendarYearQ2,
                TimeIdentifier.CalendarYearQ2Q3,
                TimeIdentifier.CalendarYearQ2Q4,
                TimeIdentifier.CalendarYearQ3,
                TimeIdentifier.CalendarYearQ3Q4,
                TimeIdentifier.CalendarYearQ4
            };
        }

        public static TimeIdentifier[] GetFinancialQuarters()
        {
            return new[]
            {
                TimeIdentifier.FinancialYearQ1,
                TimeIdentifier.FinancialYearQ1Q2,
                TimeIdentifier.FinancialYearQ1Q3,
                TimeIdentifier.FinancialYearQ1Q4,
                TimeIdentifier.FinancialYearQ2,
                TimeIdentifier.FinancialYearQ2Q3,
                TimeIdentifier.FinancialYearQ2Q4,
                TimeIdentifier.FinancialYearQ3,
                TimeIdentifier.FinancialYearQ3Q4,
                TimeIdentifier.FinancialYearQ4
            };
        }

        public static TimeIdentifier[] GetTaxQuarters()
        {
            return new[]
            {
                TimeIdentifier.TaxYearQ1,
                TimeIdentifier.TaxYearQ1Q2,
                TimeIdentifier.TaxYearQ1Q3,
                TimeIdentifier.TaxYearQ1Q4,
                TimeIdentifier.TaxYearQ2,
                TimeIdentifier.TaxYearQ2Q3,
                TimeIdentifier.TaxYearQ2Q4,
                TimeIdentifier.TaxYearQ3,
                TimeIdentifier.TaxYearQ3Q4,
                TimeIdentifier.TaxYearQ4
            };
        }
    }
}