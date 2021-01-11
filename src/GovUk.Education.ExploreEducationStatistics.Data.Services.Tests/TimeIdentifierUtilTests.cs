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
        public void GetWeeksReturnsWeeks()
        {
            Assert.Equal(new[]
            {
                Week1,
                Week2,
                Week3,
                Week4,
                Week5,
                Week6,
                Week7,
                Week8,
                Week9,
                Week10,
                Week11,
                Week12,
                Week13,
                Week14,
                Week15,
                Week16,
                Week17,
                Week18,
                Week19,
                Week20,
                Week21,
                Week22,
                Week23,
                Week24,
                Week25,
                Week26,
                Week27,
                Week28,
                Week29,
                Week30,
                Week31,
                Week32,
                Week33,
                Week34,
                Week35,
                Week36,
                Week37,
                Week38,
                Week39,
                Week40,
                Week41,
                Week42,
                Week43,
                Week44,
                Week45,
                Week46,
                Week47,
                Week48,
                Week49,
                Week50,
                Week51,
                Week52
            }, TimeIdentifierUtil.GetWeeks());
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
        public void GetFinancialYearPartsReturnsFinancialYearParts()
        {
            Assert.Equal(new[]
            {
                FinancialYearPart1,
                FinancialYearPart2
            }, TimeIdentifierUtil.GetFinancialYearParts());
        }
    }
}