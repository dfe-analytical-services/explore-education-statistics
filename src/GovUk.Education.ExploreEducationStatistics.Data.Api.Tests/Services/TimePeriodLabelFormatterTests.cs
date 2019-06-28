using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TimePeriodLabelFormatter;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class TimePeriodLabelFormatterTests
    {
        private const int Year = 2018;
        private readonly string AcademicOrFiscalYear = $"{Year}/{(Year + 1).ToString().Substring(2)}";

        [Fact]
        public void FormatTimePeriodUsingAcademicYearIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear}", Format(Year, TimeIdentifier.AcademicYear));
            Assert.Equal($"{AcademicOrFiscalYear} Q1", Format(Year, TimeIdentifier.AcademicYearQ1));
            Assert.Equal($"{AcademicOrFiscalYear} Q2", Format(Year, TimeIdentifier.AcademicYearQ2));
            Assert.Equal($"{AcademicOrFiscalYear} Q3", Format(Year, TimeIdentifier.AcademicYearQ3));
            Assert.Equal($"{AcademicOrFiscalYear} Q4", Format(Year, TimeIdentifier.AcademicYearQ4));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q2", Format(Year, TimeIdentifier.AcademicYearQ1Q2));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q3", Format(Year, TimeIdentifier.AcademicYearQ1Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q4", Format(Year, TimeIdentifier.AcademicYearQ1Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q3", Format(Year, TimeIdentifier.AcademicYearQ2Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q4", Format(Year, TimeIdentifier.AcademicYearQ2Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q3-Q4", Format(Year, TimeIdentifier.AcademicYearQ3Q4));
        }

        [Fact]
        public void FormatTimePeriodUsingCalendarYearIdentifiers()
        {
            Assert.Equal($"{Year}", Format(Year, TimeIdentifier.CalendarYear));
            Assert.Equal($"{Year} Q1", Format(Year, TimeIdentifier.CalendarYearQ1));
            Assert.Equal($"{Year} Q2", Format(Year, TimeIdentifier.CalendarYearQ2));
            Assert.Equal($"{Year} Q3", Format(Year, TimeIdentifier.CalendarYearQ3));
            Assert.Equal($"{Year} Q4", Format(Year, TimeIdentifier.CalendarYearQ4));
            Assert.Equal($"{Year} Q1-Q2", Format(Year, TimeIdentifier.CalendarYearQ1Q2));
            Assert.Equal($"{Year} Q1-Q3", Format(Year, TimeIdentifier.CalendarYearQ1Q3));
            Assert.Equal($"{Year} Q1-Q4", Format(Year, TimeIdentifier.CalendarYearQ1Q4));
            Assert.Equal($"{Year} Q2-Q3", Format(Year, TimeIdentifier.CalendarYearQ2Q3));
            Assert.Equal($"{Year} Q2-Q4", Format(Year, TimeIdentifier.CalendarYearQ2Q4));
            Assert.Equal($"{Year} Q3-Q4", Format(Year, TimeIdentifier.CalendarYearQ3Q4));
        }

        [Fact]
        public void FormatTimePeriodUsingFinancialYearIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear}", Format(Year, TimeIdentifier.FinancialYear));
            Assert.Equal($"{AcademicOrFiscalYear} Q1", Format(Year, TimeIdentifier.FinancialYearQ1));
            Assert.Equal($"{AcademicOrFiscalYear} Q2", Format(Year, TimeIdentifier.FinancialYearQ2));
            Assert.Equal($"{AcademicOrFiscalYear} Q3", Format(Year, TimeIdentifier.FinancialYearQ3));
            Assert.Equal($"{AcademicOrFiscalYear} Q4", Format(Year, TimeIdentifier.FinancialYearQ4));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q2", Format(Year, TimeIdentifier.FinancialYearQ1Q2));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q3", Format(Year, TimeIdentifier.FinancialYearQ1Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q4", Format(Year, TimeIdentifier.FinancialYearQ1Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q3", Format(Year, TimeIdentifier.FinancialYearQ2Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q4", Format(Year, TimeIdentifier.FinancialYearQ2Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q3-Q4", Format(Year, TimeIdentifier.FinancialYearQ3Q4));
        }

        [Fact]
        public void FormatTimePeriodUsingTaxYearIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear}", Format(Year, TimeIdentifier.TaxYear));
            Assert.Equal($"{AcademicOrFiscalYear} Q1", Format(Year, TimeIdentifier.TaxYearQ1));
            Assert.Equal($"{AcademicOrFiscalYear} Q2", Format(Year, TimeIdentifier.TaxYearQ2));
            Assert.Equal($"{AcademicOrFiscalYear} Q3", Format(Year, TimeIdentifier.TaxYearQ3));
            Assert.Equal($"{AcademicOrFiscalYear} Q4", Format(Year, TimeIdentifier.TaxYearQ4));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q2", Format(Year, TimeIdentifier.TaxYearQ1Q2));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q3", Format(Year, TimeIdentifier.TaxYearQ1Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q4", Format(Year, TimeIdentifier.TaxYearQ1Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q3", Format(Year, TimeIdentifier.TaxYearQ2Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q4", Format(Year, TimeIdentifier.TaxYearQ2Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q3-Q4", Format(Year, TimeIdentifier.TaxYearQ3Q4));
        }

        [Fact]
        public void FormatTimePeriodUsingMonthIdentifiers()
        {
            Assert.Equal($"{Year} January", Format(Year, TimeIdentifier.January));
            Assert.Equal($"{Year} February", Format(Year, TimeIdentifier.February));
            Assert.Equal($"{Year} March", Format(Year, TimeIdentifier.March));
            Assert.Equal($"{Year} April", Format(Year, TimeIdentifier.April));
            Assert.Equal($"{Year} May", Format(Year, TimeIdentifier.May));
            Assert.Equal($"{Year} June", Format(Year, TimeIdentifier.June));
            Assert.Equal($"{Year} July", Format(Year, TimeIdentifier.July));
            Assert.Equal($"{Year} August", Format(Year, TimeIdentifier.August));
            Assert.Equal($"{Year} September", Format(Year, TimeIdentifier.September));
            Assert.Equal($"{Year} October", Format(Year, TimeIdentifier.October));
            Assert.Equal($"{Year} November", Format(Year, TimeIdentifier.November));
            Assert.Equal($"{Year} December", Format(Year, TimeIdentifier.December));
        }

        [Fact]
        public void FormatTimePeriodUsingEndOfMarchIdentifier()
        {
            Assert.Equal($"{Year} Up Until 31st March", Format(Year, TimeIdentifier.EndOfMarch));
        }

        [Fact]
        public void FormatTimePeriodUsingNumberOfTermIdentifiers()
        {
            Assert.Equal(AcademicOrFiscalYear, Format(Year, TimeIdentifier.FiveHalfTerms));
            Assert.Equal(AcademicOrFiscalYear, Format(Year, TimeIdentifier.SixHalfTerms));
        }

        [Fact]
        public void FormatTimePeriodUsingTermIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear} Autumn Term", Format(Year, TimeIdentifier.AutumnTerm));
            Assert.Equal($"{AcademicOrFiscalYear} Spring Term", Format(Year, TimeIdentifier.SpringTerm));
            Assert.Equal($"{AcademicOrFiscalYear} Summer Term", Format(Year, TimeIdentifier.SummerTerm));
            Assert.Equal($"{AcademicOrFiscalYear} Autumn and Spring Term",
                Format(Year, TimeIdentifier.AutumnSpringTerm));
        }
    }
}