using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TimeIdentifierUtilTests
    {
        [Fact]
        public void GetAcademicQuartersReturnsAcademicQuarters()
        {
            Assert.Equal(new[]
            {
                AcademicYearQ1,
                AcademicYearQ2,
                AcademicYearQ3,
                AcademicYearQ4
            }, TimeIdentifierUtil.GetAcademicQuarters());
        }

        [Fact]
        public void GetCalendarQuartersReturnsCalendarQuarters()
        {
            Assert.Equal(new[]
            {
                CalendarYearQ1,
                CalendarYearQ2,
                CalendarYearQ3,
                CalendarYearQ4
            }, TimeIdentifierUtil.GetCalendarQuarters());
        }

        [Fact]
        public void GetFinancialQuartersReturnsFinancialQuarters()
        {
            Assert.Equal(new[]
            {
                FinancialYearQ1,
                FinancialYearQ2,
                FinancialYearQ3,
                FinancialYearQ4
            }, TimeIdentifierUtil.GetFinancialQuarters());
        }

        [Fact]
        public void GetTaxQuartersReturnsTaxQuarters()
        {
            Assert.Equal(new[]
            {
                TaxYearQ1,
                TaxYearQ2,
                TaxYearQ3,
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
                ReportingYear
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
    }
}