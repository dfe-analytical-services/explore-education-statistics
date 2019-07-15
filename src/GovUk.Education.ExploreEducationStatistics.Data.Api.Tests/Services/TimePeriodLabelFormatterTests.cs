using Xunit;
using Xunit.Abstractions;
using static GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TimePeriodLabelFormatter;
using static GovUk.Education.ExploreEducationStatistics.Model.Service.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class TimePeriodLabelFormatterTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const int Year = 2018;
        private readonly string AcademicOrFiscalYear = $"{Year}/{(Year + 1).ToString().Substring(2)}";

        public TimePeriodLabelFormatterTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void FormatTimePeriodUsingAcademicYearIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear}", Format(Year, AcademicYear));
            Assert.Equal($"{AcademicOrFiscalYear} Q1", Format(Year, AcademicYearQ1));
            Assert.Equal($"{AcademicOrFiscalYear} Q2", Format(Year, AcademicYearQ2));
            Assert.Equal($"{AcademicOrFiscalYear} Q3", Format(Year, AcademicYearQ3));
            Assert.Equal($"{AcademicOrFiscalYear} Q4", Format(Year, AcademicYearQ4));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q2", Format(Year, AcademicYearQ1Q2));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q3", Format(Year, AcademicYearQ1Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q4", Format(Year, AcademicYearQ1Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q3", Format(Year, AcademicYearQ2Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q4", Format(Year, AcademicYearQ2Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q3-Q4", Format(Year, AcademicYearQ3Q4));
        }

        [Fact]
        public void FormatTimePeriodUsingCalendarYearIdentifiers()
        {
            Assert.Equal($"{Year}", Format(Year, CalendarYear));
            Assert.Equal($"{Year} Q1", Format(Year, CalendarYearQ1));
            Assert.Equal($"{Year} Q2", Format(Year, CalendarYearQ2));
            Assert.Equal($"{Year} Q3", Format(Year, CalendarYearQ3));
            Assert.Equal($"{Year} Q4", Format(Year, CalendarYearQ4));
            Assert.Equal($"{Year} Q1-Q2", Format(Year, CalendarYearQ1Q2));
            Assert.Equal($"{Year} Q1-Q3", Format(Year, CalendarYearQ1Q3));
            Assert.Equal($"{Year} Q1-Q4", Format(Year, CalendarYearQ1Q4));
            Assert.Equal($"{Year} Q2-Q3", Format(Year, CalendarYearQ2Q3));
            Assert.Equal($"{Year} Q2-Q4", Format(Year, CalendarYearQ2Q4));
            Assert.Equal($"{Year} Q3-Q4", Format(Year, CalendarYearQ3Q4));
        }

        [Fact]
        public void FormatTimePeriodUsingFinancialYearIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear}", Format(Year, FinancialYear));
            Assert.Equal($"{AcademicOrFiscalYear} Q1", Format(Year, FinancialYearQ1));
            Assert.Equal($"{AcademicOrFiscalYear} Q2", Format(Year, FinancialYearQ2));
            Assert.Equal($"{AcademicOrFiscalYear} Q3", Format(Year, FinancialYearQ3));
            Assert.Equal($"{AcademicOrFiscalYear} Q4", Format(Year, FinancialYearQ4));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q2", Format(Year, FinancialYearQ1Q2));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q3", Format(Year, FinancialYearQ1Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q4", Format(Year, FinancialYearQ1Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q3", Format(Year, FinancialYearQ2Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q4", Format(Year, FinancialYearQ2Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q3-Q4", Format(Year, FinancialYearQ3Q4));
        }

        [Fact]
        public void FormatTimePeriodUsingTaxYearIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear}", Format(Year, TaxYear));
            Assert.Equal($"{AcademicOrFiscalYear} Q1", Format(Year, TaxYearQ1));
            Assert.Equal($"{AcademicOrFiscalYear} Q2", Format(Year, TaxYearQ2));
            Assert.Equal($"{AcademicOrFiscalYear} Q3", Format(Year, TaxYearQ3));
            Assert.Equal($"{AcademicOrFiscalYear} Q4", Format(Year, TaxYearQ4));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q2", Format(Year, TaxYearQ1Q2));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q3", Format(Year, TaxYearQ1Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q1-Q4", Format(Year, TaxYearQ1Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q3", Format(Year, TaxYearQ2Q3));
            Assert.Equal($"{AcademicOrFiscalYear} Q2-Q4", Format(Year, TaxYearQ2Q4));
            Assert.Equal($"{AcademicOrFiscalYear} Q3-Q4", Format(Year, TaxYearQ3Q4));
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
        public void FormatTimePeriodUsingEndOfMarchIdentifier()
        {
            Assert.Equal($"{Year} Up Until 31st March", Format(Year, EndOfMarch));
        }

        [Fact]
        public void FormatTimePeriodUsingNumberOfTermIdentifiers()
        {
            Assert.Equal(AcademicOrFiscalYear, Format(Year, FiveHalfTerms));
            Assert.Equal(AcademicOrFiscalYear, Format(Year, SixHalfTerms));
        }

        [Fact]
        public void FormatTimePeriodUsingTermIdentifiers()
        {
            Assert.Equal($"{AcademicOrFiscalYear} Autumn Term", Format(Year, AutumnTerm));
            Assert.Equal($"{AcademicOrFiscalYear} Spring Term", Format(Year, SpringTerm));
            Assert.Equal($"{AcademicOrFiscalYear} Summer Term", Format(Year, SummerTerm));
            Assert.Equal($"{AcademicOrFiscalYear} Autumn and Spring Term", Format(Year, AutumnSpringTerm));
        }

        [Fact(Skip = "Use this to debug")]
        public void Debug()
        {
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYear)      {Format(Year, AcademicYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ1)    {Format(Year, AcademicYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ2)    {Format(Year, AcademicYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ3)    {Format(Year, AcademicYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ4)    {Format(Year, AcademicYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ1Q2)  {Format(Year, AcademicYearQ1Q2)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ1Q3)  {Format(Year, AcademicYearQ1Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ1Q4)  {Format(Year, AcademicYearQ1Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ2Q3)  {Format(Year, AcademicYearQ2Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ2Q4)  {Format(Year, AcademicYearQ2Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYearQ3Q4)  {Format(Year, AcademicYearQ3Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYear)      {Format(Year, CalendarYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ1)    {Format(Year, CalendarYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ2)    {Format(Year, CalendarYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ3)    {Format(Year, CalendarYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ4)    {Format(Year, CalendarYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ1Q2)  {Format(Year, CalendarYearQ1Q2)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ1Q3)  {Format(Year, CalendarYearQ1Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ1Q4)  {Format(Year, CalendarYearQ1Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ2Q3)  {Format(Year, CalendarYearQ2Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ2Q4)  {Format(Year, CalendarYearQ2Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, CalendarYearQ3Q4)  {Format(Year, CalendarYearQ3Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYear)     {Format(Year, FinancialYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ1)   {Format(Year, FinancialYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ2)   {Format(Year, FinancialYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ3)   {Format(Year, FinancialYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ4)   {Format(Year, FinancialYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ1Q2) {Format(Year, FinancialYearQ1Q2)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ1Q3) {Format(Year, FinancialYearQ1Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ1Q4) {Format(Year, FinancialYearQ1Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ2Q3) {Format(Year, FinancialYearQ2Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ2Q4) {Format(Year, FinancialYearQ2Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearQ3Q4) {Format(Year, FinancialYearQ3Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYear)           {Format(Year, TaxYear)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ1)         {Format(Year, TaxYearQ1)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ2)         {Format(Year, TaxYearQ2)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ3)         {Format(Year, TaxYearQ3)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ4)         {Format(Year, TaxYearQ4)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ1Q2)       {Format(Year, TaxYearQ1Q2)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ1Q3)       {Format(Year, TaxYearQ1Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ1Q4)       {Format(Year, TaxYearQ1Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ2Q3)       {Format(Year, TaxYearQ2Q3)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ2Q4)       {Format(Year, TaxYearQ2Q4)}");
            _testOutputHelper.WriteLine($"Format({Year}, TaxYearQ3Q4)       {Format(Year, TaxYearQ3Q4)}");
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
            _testOutputHelper.WriteLine($"Format({Year}, EndOfMarch)        {Format(Year, EndOfMarch)}");
            _testOutputHelper.WriteLine($"Format({Year}, FiveHalfTerms)     {Format(Year, FiveHalfTerms)}");
            _testOutputHelper.WriteLine($"Format({Year}, SixHalfTerms)      {Format(Year, SixHalfTerms)}");
            _testOutputHelper.WriteLine($"Format({Year}, AutumnTerm)        {Format(Year, AutumnTerm)}");
            _testOutputHelper.WriteLine($"Format({Year}, SpringTerm)        {Format(Year, SpringTerm)}");
            _testOutputHelper.WriteLine($"Format({Year}, SummerTerm)        {Format(Year, SummerTerm)}");
            _testOutputHelper.WriteLine($"Format({Year}, AutumnSpringTerm)  {Format(Year, AutumnSpringTerm)}");
         }
    }
}