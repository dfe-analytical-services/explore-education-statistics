using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class TimeIdentifierUtilTests
    {
        [Fact]
        public void GetAcademicQuartersReturnsAcademicQuarters()
        {
            Assert.Equal(new[]
            {
                AcademicYearQ1,
                AcademicYearQ1Q2,
                AcademicYearQ1Q3,
                AcademicYearQ1Q4,
                AcademicYearQ2,
                AcademicYearQ2Q3,
                AcademicYearQ2Q4,
                AcademicYearQ3,
                AcademicYearQ3Q4,
                AcademicYearQ4
            }, TimeIdentifierUtil.GetAcademicQuarters());
        }

        [Fact]
        public void GetCalendarQuartersReturnsCalendarQuarters()
        {
            Assert.Equal(new[]
            {
                CalendarYearQ1,
                CalendarYearQ1Q2,
                CalendarYearQ1Q3,
                CalendarYearQ1Q4,
                CalendarYearQ2,
                CalendarYearQ2Q3,
                CalendarYearQ2Q4,
                CalendarYearQ3,
                CalendarYearQ3Q4,
                CalendarYearQ4
            }, TimeIdentifierUtil.GetCalendarQuarters());
        }

        [Fact]
        public void GetFinancialQuartersReturnsFinancialQuarters()
        {
            Assert.Equal(new[]
            {
                FinancialYearQ1,
                FinancialYearQ1Q2,
                FinancialYearQ1Q3,
                FinancialYearQ1Q4,
                FinancialYearQ2,
                FinancialYearQ2Q3,
                FinancialYearQ2Q4,
                FinancialYearQ3,
                FinancialYearQ3Q4,
                FinancialYearQ4
            }, TimeIdentifierUtil.GetFinancialQuarters());
        }

        [Fact]
        public void GetTaxQuartersReturnsTaxQuarters()
        {
            Assert.Equal(new[]
            {
                TaxYearQ1,
                TaxYearQ1Q2,
                TaxYearQ1Q3,
                TaxYearQ1Q4,
                TaxYearQ2,
                TaxYearQ2Q3,
                TaxYearQ2Q4,
                TaxYearQ3,
                TaxYearQ3Q4,
                TaxYearQ4
            }, TimeIdentifierUtil.GetTaxQuarters());
        }

        [Fact]
        public void GetMonthsReturnsMonths()
        {
            Assert.Equal(new[]
            {
                January,
                February,
                March,
                April,
                May,
                June,
                July,
                August,
                September,
                October,
                November,
                December
            }, TimeIdentifierUtil.GetMonths());
        }

        [Fact]
        public void GetYearsReturnsYears()
        {
            Assert.Equal(new[]
            {
                AcademicYear,
                CalendarYear,
                FinancialYear,
                TaxYear,
                EndOfMarch
            }, TimeIdentifierUtil.GetYears());
        }

        [Fact]
        public void GetTermsReturnsTerms()
        {
            Assert.Equal(new[]
            {
                AutumnTerm,
                AutumnSpringTerm,
                SpringTerm,
                SummerTerm
            }, TimeIdentifierUtil.GetTerms());
        }

        [Fact]
        public void GetNumberOfTermsReturnsNumberOfTerms()
        {
            Assert.Equal(new[]
            {
                FiveHalfTerms,
                SixHalfTerms
            }, TimeIdentifierUtil.GetNumberOfTerms());
        }
    }
}