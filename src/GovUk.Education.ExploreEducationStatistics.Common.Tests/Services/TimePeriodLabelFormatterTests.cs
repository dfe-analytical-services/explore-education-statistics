using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;
using Xunit.Abstractions;
using static GovUk.Education.ExploreEducationStatistics.Common.Database.TimePeriodLabelFormat;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.TimePeriodLabelFormatter;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class TimePeriodLabelFormatterTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const int Year = 2018;
        private static readonly string AcademicYear = $"{Year}/{(Year + 1).ToString().Substring(2)}";
        private static readonly string FiscalYear = $"{Year}-{(Year + 1).ToString().Substring(2)}";

        public TimePeriodLabelFormatterTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void FormatTimePeriodUsingAcademicYearIdentifiers()
        {
            Assert.Equal($"{AcademicYear}", Format(Year, TimeIdentifier.AcademicYear));
            Assert.Equal($"{AcademicYear} Q1", Format(Year, AcademicYearQ1));
            Assert.Equal($"{AcademicYear} Q2", Format(Year, AcademicYearQ2));
            Assert.Equal($"{AcademicYear} Q3", Format(Year, AcademicYearQ3));
            Assert.Equal($"{AcademicYear} Q4", Format(Year, AcademicYearQ4));
        }

        [Fact]
        public void FormatTimePeriodUsingCalendarYearIdentifiers()
        {
            Assert.Equal($"{Year}", Format(Year, CalendarYear));
            Assert.Equal($"{Year} Q1", Format(Year, CalendarYearQ1));
            Assert.Equal($"{Year} Q2", Format(Year, CalendarYearQ2));
            Assert.Equal($"{Year} Q3", Format(Year, CalendarYearQ3));
            Assert.Equal($"{Year} Q4", Format(Year, CalendarYearQ4));
        }

        [Fact]
        public void FormatTimePeriodUsingFinancialYearIdentifiers()
        {
            Assert.Equal($"{FiscalYear}", Format(Year, FinancialYear));
            Assert.Equal($"{FiscalYear} Q1", Format(Year, FinancialYearQ1));
            Assert.Equal($"{FiscalYear} Q2", Format(Year, FinancialYearQ2));
            Assert.Equal($"{FiscalYear} Q3", Format(Year, FinancialYearQ3));
            Assert.Equal($"{FiscalYear} Q4", Format(Year, FinancialYearQ4));
        }

        [Fact]
        public void FormatTimePeriodUsingReportingYearIdentifier()
        {
            Assert.Equal($"{Year}", Format(Year, ReportingYear));
        }

        [Fact]
        public void FormatTimePeriodUsingTaxYearIdentifiers()
        {
            Assert.Equal($"{FiscalYear}", Format(Year, TaxYear));
            Assert.Equal($"{FiscalYear} Q1", Format(Year, TaxYearQ1));
            Assert.Equal($"{FiscalYear} Q2", Format(Year, TaxYearQ2));
            Assert.Equal($"{FiscalYear} Q3", Format(Year, TaxYearQ3));
            Assert.Equal($"{FiscalYear} Q4", Format(Year, TaxYearQ4));
        }

        [Fact]
        public void FormatTimePeriodUsingMonthIdentifiers()
        {
            Assert.Equal($"{Year} January", Format(Year, January));
            Assert.Equal($"{Year} February", Format(Year, February));
            Assert.Equal($"{Year} March", Format(Year, March));
            Assert.Equal($"{Year} April", Format(Year, April));
            Assert.Equal($"{Year} May", Format(Year, May));
            Assert.Equal($"{Year} June", Format(Year, June));
            Assert.Equal($"{Year} July", Format(Year, July));
            Assert.Equal($"{Year} August", Format(Year, August));
            Assert.Equal($"{Year} September", Format(Year, September));
            Assert.Equal($"{Year} October", Format(Year, October));
            Assert.Equal($"{Year} November", Format(Year, November));
            Assert.Equal($"{Year} December", Format(Year, December));
        }

        [Fact]
        public void FormatTimePeriodUsingTermIdentifiers()
        {
            Assert.Equal($"{AcademicYear} Autumn Term", Format(Year, AutumnTerm));
            Assert.Equal($"{AcademicYear} Spring Term", Format(Year, SpringTerm));
            Assert.Equal($"{AcademicYear} Summer Term", Format(Year, SummerTerm));
            Assert.Equal($"{AcademicYear} Autumn and Spring Term", Format(Year, AutumnSpringTerm));
        }

        [Fact]
        public void FormatTimePeriodWithOverridingLabelFormats()
        {
            Assert.Equal($"{Year} Calendar Year", Format(Year, CalendarYear, FullLabel));
            Assert.Equal($"{Year}", Format(Year, CalendarYear, NoLabel));
            Assert.Equal($"{Year}", Format(Year, CalendarYear, ShortLabel));

            Assert.Equal($"{AcademicYear} Academic Year", Format(Year, TimeIdentifier.AcademicYear, FullLabel));
            Assert.Equal($"{AcademicYear}", Format(Year, TimeIdentifier.AcademicYear, NoLabel));
            Assert.Equal($"{AcademicYear}", Format(Year, TimeIdentifier.AcademicYear, ShortLabel));

            Assert.Equal($"{AcademicYear} Academic Year Q1", Format(Year, AcademicYearQ1, FullLabel));
            Assert.Equal($"{AcademicYear}", Format(Year, AcademicYearQ1, NoLabel));
            Assert.Equal($"{AcademicYear} Q1", Format(Year, AcademicYearQ1, ShortLabel));

            Assert.Equal($"{FiscalYear} Financial Year", Format(Year, FinancialYear, FullLabel));
            Assert.Equal($"{FiscalYear}", Format(Year, FinancialYear, NoLabel));
            Assert.Equal($"{FiscalYear}", Format(Year, FinancialYear, ShortLabel));
            
            Assert.Equal($"{Year} January", Format(Year, January, FullLabel));
            Assert.Equal($"{Year}", Format(Year, January, NoLabel));
            Assert.Equal($"{Year}", Format(Year, January, ShortLabel));

            Assert.Equal($"{AcademicYear} Autumn Term", Format(Year, AutumnTerm, FullLabel));
            Assert.Equal($"{AcademicYear}", Format(Year, AutumnTerm, NoLabel));
            Assert.Equal($"{AcademicYear}", Format(Year, AutumnTerm, ShortLabel));
        }

        [Fact]
        //[Fact(Skip = "Use this to debug")]
        [SuppressMessage("ReSharper", "xUnit1004")]
        public void Debug()
        {
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYear)      {Format(Year, TimeIdentifier.AcademicYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ1)    {Format(Year, AcademicYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ2)    {Format(Year, AcademicYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ3)    {Format(Year, AcademicYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ4)    {Format(Year, AcademicYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYear)      {Format(Year, CalendarYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ1)    {Format(Year, CalendarYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ2)    {Format(Year, CalendarYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ3)    {Format(Year, CalendarYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ4)    {Format(Year, CalendarYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYear)     {Format(Year, FinancialYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ1)   {Format(Year, FinancialYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ2)   {Format(Year, FinancialYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ3)   {Format(Year, FinancialYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ4)   {Format(Year, FinancialYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, ReportingYear)     {Format(Year, ReportingYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYear)           {Format(Year, TaxYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ1)         {Format(Year, TaxYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ2)         {Format(Year, TaxYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ3)         {Format(Year, TaxYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ4)         {Format(Year, TaxYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, January)           {Format(Year, January)}");
            _testOutputHelper.WriteLine($"Format({Year}, February)          {Format(Year, February)}");
            _testOutputHelper.WriteLine($"Format({Year}, March)             {Format(Year, March)}");
            _testOutputHelper.WriteLine($"Format({Year}, April)             {Format(Year, April)}");
            _testOutputHelper.WriteLine($"Format({Year}, May)               {Format(Year, May)}");
            _testOutputHelper.WriteLine($"Format({Year}, June)              {Format(Year, June)}");
            _testOutputHelper.WriteLine($"Format({Year}, July)              {Format(Year, July)}");
            _testOutputHelper.WriteLine($"Format({Year}, August)            {Format(Year, August)}");
            _testOutputHelper.WriteLine($"Format({Year}, September)         {Format(Year, September)}");
            _testOutputHelper.WriteLine($"Format({Year}, October)           {Format(Year, October)}");
            _testOutputHelper.WriteLine($"Format({Year}, November)          {Format(Year, November)}");
            _testOutputHelper.WriteLine($"Format({Year}, December)          {Format(Year, December)}");
            _testOutputHelper.WriteLine($"Format({Year}, AutumnTerm)        {Format(Year, AutumnTerm)}");
            _testOutputHelper.WriteLine($"Format({Year}, SpringTerm)        {Format(Year, SpringTerm)}");
            _testOutputHelper.WriteLine($"Format({Year}, SummerTerm)        {Format(Year, SummerTerm)}");
            _testOutputHelper.WriteLine($"Format({Year}, AutumnSpringTerm)  {Format(Year, AutumnSpringTerm)}");
        }
    }
}