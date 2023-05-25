using System.Diagnostics.CodeAnalysis;
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
        private static readonly string FormattedAcademicYear = $"{Year}/{(Year + 1).ToString().Substring(2)}";
        private static readonly string FormattedAcademicCsvYear = $"{Year}{(Year + 1).ToString().Substring(2)}";
        private static readonly string FormattedFiscalYear = $"{Year}-{(Year + 1).ToString().Substring(2)}";
        private static readonly string FormattedFiscalCsvYear = $"{Year}{(Year + 1).ToString().Substring(2)}";

        public TimePeriodLabelFormatterTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void FormatTimePeriodUsingAcademicYearIdentifiers()
        {
            Assert.Equal($"{FormattedAcademicYear}", Format(Year, AcademicYear));
            Assert.Equal($"{FormattedAcademicYear} Q1", Format(Year, AcademicYearQ1));
            Assert.Equal($"{FormattedAcademicYear} Q2", Format(Year, AcademicYearQ2));
            Assert.Equal($"{FormattedAcademicYear} Q3", Format(Year, AcademicYearQ3));
            Assert.Equal($"{FormattedAcademicYear} Q4", Format(Year, AcademicYearQ4));
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
            Assert.Equal($"{FormattedFiscalYear}", Format(Year, FinancialYear));
            Assert.Equal($"{FormattedFiscalYear} Q1", Format(Year, FinancialYearQ1));
            Assert.Equal($"{FormattedFiscalYear} Q2", Format(Year, FinancialYearQ2));
            Assert.Equal($"{FormattedFiscalYear} Q3", Format(Year, FinancialYearQ3));
            Assert.Equal($"{FormattedFiscalYear} Q4", Format(Year, FinancialYearQ4));
        }

        [Fact]
        public void FormatTimePeriodUsingReportingYearIdentifier()
        {
            Assert.Equal($"{Year}", Format(Year, ReportingYear));
        }

        [Fact]
        public void FormatTimePeriodUsingTaxYearIdentifiers()
        {
            Assert.Equal($"{FormattedFiscalYear}", Format(Year, TaxYear));
            Assert.Equal($"{FormattedFiscalYear} Q1", Format(Year, TaxYearQ1));
            Assert.Equal($"{FormattedFiscalYear} Q2", Format(Year, TaxYearQ2));
            Assert.Equal($"{FormattedFiscalYear} Q3", Format(Year, TaxYearQ3));
            Assert.Equal($"{FormattedFiscalYear} Q4", Format(Year, TaxYearQ4));
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
            Assert.Equal($"{FormattedAcademicYear} Autumn term", Format(Year, AutumnTerm));
            Assert.Equal($"{FormattedAcademicYear} Spring term", Format(Year, SpringTerm));
            Assert.Equal($"{FormattedAcademicYear} Summer term", Format(Year, SummerTerm));
            Assert.Equal($"{FormattedAcademicYear} Autumn and spring term", Format(Year, AutumnSpringTerm));
        }

        [Fact]
        public void FormatTimePeriodUsingWeekIdentifiers()
        {
            Assert.Equal($"{Year} Week 1", Format(Year, Week1));
            Assert.Equal($"{Year} Week 2", Format(Year, Week2));
            Assert.Equal($"{Year} Week 3", Format(Year, Week3));
            Assert.Equal($"{Year} Week 4", Format(Year, Week4));
            Assert.Equal($"{Year} Week 5", Format(Year, Week5));
            Assert.Equal($"{Year} Week 6", Format(Year, Week6));
            Assert.Equal($"{Year} Week 7", Format(Year, Week7));
            Assert.Equal($"{Year} Week 8", Format(Year, Week8));
            Assert.Equal($"{Year} Week 9", Format(Year, Week9));
            Assert.Equal($"{Year} Week 10", Format(Year, Week10));
            Assert.Equal($"{Year} Week 11", Format(Year, Week11));
            Assert.Equal($"{Year} Week 12", Format(Year, Week12));
            Assert.Equal($"{Year} Week 13", Format(Year, Week13));
            Assert.Equal($"{Year} Week 14", Format(Year, Week14));
            Assert.Equal($"{Year} Week 15", Format(Year, Week15));
            Assert.Equal($"{Year} Week 16", Format(Year, Week16));
            Assert.Equal($"{Year} Week 17", Format(Year, Week17));
            Assert.Equal($"{Year} Week 18", Format(Year, Week18));
            Assert.Equal($"{Year} Week 19", Format(Year, Week19));
            Assert.Equal($"{Year} Week 20", Format(Year, Week20));
            Assert.Equal($"{Year} Week 21", Format(Year, Week21));
            Assert.Equal($"{Year} Week 22", Format(Year, Week22));
            Assert.Equal($"{Year} Week 23", Format(Year, Week23));
            Assert.Equal($"{Year} Week 24", Format(Year, Week24));
            Assert.Equal($"{Year} Week 25", Format(Year, Week25));
            Assert.Equal($"{Year} Week 26", Format(Year, Week26));
            Assert.Equal($"{Year} Week 27", Format(Year, Week27));
            Assert.Equal($"{Year} Week 28", Format(Year, Week28));
            Assert.Equal($"{Year} Week 29", Format(Year, Week29));
            Assert.Equal($"{Year} Week 30", Format(Year, Week30));
            Assert.Equal($"{Year} Week 31", Format(Year, Week31));
            Assert.Equal($"{Year} Week 32", Format(Year, Week32));
            Assert.Equal($"{Year} Week 33", Format(Year, Week33));
            Assert.Equal($"{Year} Week 34", Format(Year, Week34));
            Assert.Equal($"{Year} Week 35", Format(Year, Week35));
            Assert.Equal($"{Year} Week 36", Format(Year, Week36));
            Assert.Equal($"{Year} Week 37", Format(Year, Week37));
            Assert.Equal($"{Year} Week 38", Format(Year, Week38));
            Assert.Equal($"{Year} Week 39", Format(Year, Week39));
            Assert.Equal($"{Year} Week 40", Format(Year, Week40));
            Assert.Equal($"{Year} Week 41", Format(Year, Week41));
            Assert.Equal($"{Year} Week 42", Format(Year, Week42));
            Assert.Equal($"{Year} Week 43", Format(Year, Week43));
            Assert.Equal($"{Year} Week 44", Format(Year, Week44));
            Assert.Equal($"{Year} Week 45", Format(Year, Week45));
            Assert.Equal($"{Year} Week 46", Format(Year, Week46));
            Assert.Equal($"{Year} Week 47", Format(Year, Week47));
            Assert.Equal($"{Year} Week 48", Format(Year, Week48));
            Assert.Equal($"{Year} Week 49", Format(Year, Week49));
            Assert.Equal($"{Year} Week 50", Format(Year, Week50));
            Assert.Equal($"{Year} Week 51", Format(Year, Week51));
            Assert.Equal($"{Year} Week 52", Format(Year, Week52));
        }

        [Fact]
        public void FormatTimePeriodUsingFinancialYearPartIdentifiers()
        {
            Assert.Equal($"{FormattedFiscalYear} Part 1 (Apr to Sep)", Format(Year, FinancialYearPart1));
            Assert.Equal($"{FormattedFiscalYear} Part 2 (Oct to Mar)", Format(Year, FinancialYearPart2));
        }

        [Fact]
        public void FormatTimePeriodWithFullLabelFormat()
        {
            Assert.Equal($"{Year} Calendar year", Format(Year, CalendarYear, FullLabel));
            Assert.Equal($"{FormattedAcademicYear} Academic year", Format(Year, AcademicYear, FullLabel));
            Assert.Equal($"{FormattedAcademicYear} Academic year Q1", Format(Year, AcademicYearQ1, FullLabel));
            Assert.Equal($"{FormattedFiscalYear} Financial year", Format(Year, FinancialYear, FullLabel));
            Assert.Equal($"{FormattedFiscalYear} Tax year", Format(Year, TaxYear, FullLabel));
            Assert.Equal($"{Year} Reporting year", Format(Year, ReportingYear, FullLabel));
            Assert.Equal($"{Year} January", Format(Year, January, FullLabel));
            Assert.Equal($"{FormattedAcademicYear} Autumn term", Format(Year, AutumnTerm, FullLabel));
            Assert.Equal($"{FormattedFiscalYear} Part 1 (April to September)", Format(Year, FinancialYearPart1, FullLabel));
            Assert.Equal($"{FormattedFiscalYear} Part 2 (October to March)", Format(Year, FinancialYearPart2, FullLabel));
        }

        [Fact]
        public void FormatTimePeriodWithFullLabelBeforeYearFormat()
        {
            Assert.Equal($"Calendar year {Year}", Format(Year, CalendarYear, FullLabelBeforeYear));
            Assert.Equal($"Academic year {FormattedAcademicYear}", Format(Year, AcademicYear, FullLabelBeforeYear));
            Assert.Equal($"Academic year Q1 {FormattedAcademicYear}", Format(Year, AcademicYearQ1, FullLabelBeforeYear));
            Assert.Equal($"Financial year {FormattedFiscalYear}", Format(Year, FinancialYear, FullLabelBeforeYear));
            Assert.Equal($"Tax year {FormattedFiscalYear}", Format(Year, TaxYear, FullLabelBeforeYear));
            Assert.Equal($"Reporting year {Year}", Format(Year, ReportingYear, FullLabelBeforeYear));
            Assert.Equal($"January {Year}", Format(Year, January, FullLabelBeforeYear));
            Assert.Equal($"Autumn term {FormattedAcademicYear}", Format(Year, AutumnTerm, FullLabelBeforeYear));
            Assert.Equal($"Part 1 (April to September) {FormattedFiscalYear}", Format(Year, FinancialYearPart1, FullLabelBeforeYear));
            Assert.Equal($"Part 2 (October to March) {FormattedFiscalYear}", Format(Year, FinancialYearPart2, FullLabelBeforeYear));
        }

        [Fact]
        public void FormatTimePeriodWithNoLabelFormat()
        {
            Assert.Equal($"{Year}", Format(Year, CalendarYear, NoLabel));
            Assert.Equal($"{FormattedAcademicYear}", Format(Year, AcademicYear, NoLabel));
            Assert.Equal($"{FormattedAcademicYear}", Format(Year, AcademicYearQ1, NoLabel));
            Assert.Equal($"{FormattedFiscalYear}", Format(Year, FinancialYear, NoLabel));
            Assert.Equal($"{FormattedFiscalYear}", Format(Year, TaxYear, NoLabel));
            Assert.Equal($"{Year}", Format(Year, ReportingYear, NoLabel));
            Assert.Equal($"{Year}", Format(Year, January, NoLabel));
            Assert.Equal($"{FormattedAcademicYear}", Format(Year, AutumnTerm, NoLabel));
            Assert.Equal($"{FormattedFiscalYear}", Format(Year, FinancialYearPart1, NoLabel));
        }

        [Fact]
        public void FormatTimePeriodWithShortLabelFormat()
        {
            Assert.Equal($"{Year}", Format(Year, CalendarYear, ShortLabel));
            Assert.Equal($"{FormattedAcademicYear}", Format(Year, AcademicYear, ShortLabel));
            Assert.Equal($"{FormattedAcademicYear} Q1", Format(Year, AcademicYearQ1, ShortLabel));
            Assert.Equal($"{FormattedFiscalYear}", Format(Year, FinancialYear, ShortLabel));
            Assert.Equal($"{FormattedFiscalYear}", Format(Year, TaxYear, ShortLabel));
            Assert.Equal($"{Year}", Format(Year, ReportingYear, ShortLabel));
            Assert.Equal($"{Year}", Format(Year, January, ShortLabel));
            Assert.Equal($"{FormattedAcademicYear}", Format(Year, AutumnTerm, ShortLabel));
            Assert.Equal($"{FormattedFiscalYear} Part 1 (Apr to Sep)", Format(Year, FinancialYearPart1, ShortLabel));
            Assert.Equal($"{FormattedFiscalYear} Part 2 (Oct to Mar)", Format(Year, FinancialYearPart2, ShortLabel));
        }

        [Fact]
        public void FormatCsvYear_AcademicYearFormat()
        {
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, AcademicYear));
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, AcademicYearQ1));
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, AcademicYearQ2));
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, AcademicYearQ3));
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, AcademicYearQ4));

            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, AutumnTerm));
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, SpringTerm));
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, SummerTerm));
            Assert.Equal(FormattedAcademicCsvYear, FormatCsvYear(Year, AutumnSpringTerm));
        }

        [Fact]
        public void FormatCsvYear_FiscalYearFormat()
        {
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, FinancialYear));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, FinancialYearQ1));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, FinancialYearQ2));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, FinancialYearQ3));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, FinancialYearQ4));

            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, TaxYear));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, TaxYearQ1));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, TaxYearQ2));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, TaxYearQ3));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, TaxYearQ4));

            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, FinancialYearPart1));
            Assert.Equal(FormattedFiscalCsvYear, FormatCsvYear(Year, FinancialYearPart2));
        }

        [Fact]
        public void FormatCsvYear_DefaultFormat()
        {
            Assert.Equal($"{Year}", FormatCsvYear(Year, CalendarYear));
            Assert.Equal($"{Year}", FormatCsvYear(Year, CalendarYearQ1));
            Assert.Equal($"{Year}", FormatCsvYear(Year, CalendarYearQ2));
            Assert.Equal($"{Year}", FormatCsvYear(Year, CalendarYearQ3));
            Assert.Equal($"{Year}", FormatCsvYear(Year, CalendarYearQ4));

            Assert.Equal($"{Year}", FormatCsvYear(Year, ReportingYear));

            Assert.Equal($"{Year}", FormatCsvYear(Year, January));
            Assert.Equal($"{Year}", FormatCsvYear(Year, February));
            Assert.Equal($"{Year}", FormatCsvYear(Year, March));
            Assert.Equal($"{Year}", FormatCsvYear(Year, April));
            Assert.Equal($"{Year}", FormatCsvYear(Year, May));
            Assert.Equal($"{Year}", FormatCsvYear(Year, June));
            Assert.Equal($"{Year}", FormatCsvYear(Year, July));
            Assert.Equal($"{Year}", FormatCsvYear(Year, August));
            Assert.Equal($"{Year}", FormatCsvYear(Year, September));
            Assert.Equal($"{Year}", FormatCsvYear(Year, October));
            Assert.Equal($"{Year}", FormatCsvYear(Year, November));
            Assert.Equal($"{Year}", FormatCsvYear(Year, December));

            Assert.Equal($"{Year}", FormatCsvYear(Year, Week1));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week2));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week3));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week4));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week5));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week6));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week7));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week8));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week9));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week10));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week11));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week12));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week13));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week14));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week15));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week16));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week17));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week18));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week19));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week20));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week21));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week22));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week23));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week24));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week25));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week26));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week27));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week28));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week29));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week30));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week31));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week32));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week33));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week34));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week35));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week36));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week37));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week38));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week39));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week40));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week41));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week42));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week43));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week44));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week45));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week46));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week47));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week48));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week49));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week50));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week51));
            Assert.Equal($"{Year}", FormatCsvYear(Year, Week52));
        }

        [Fact(Skip = "Use this to debug")]
        [SuppressMessage("ReSharper", "xUnit1004")]
        public void Debug()
        {
            _testOutputHelper.WriteLine($"Format({Year}, AcademicYear)      {Format(Year, AcademicYear)}");
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
            _testOutputHelper.WriteLine($"Format({Year}, Week1)             {Format(Year, Week1)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week2)             {Format(Year, Week2)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week3)             {Format(Year, Week3)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week4)             {Format(Year, Week4)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week5)             {Format(Year, Week5)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week6)             {Format(Year, Week6)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week7)             {Format(Year, Week7)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week8)             {Format(Year, Week8)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week9)             {Format(Year, Week9)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week10)            {Format(Year, Week10)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week11)            {Format(Year, Week11)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week12)            {Format(Year, Week12)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week13)            {Format(Year, Week13)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week14)            {Format(Year, Week14)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week15)            {Format(Year, Week15)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week16)            {Format(Year, Week16)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week17)            {Format(Year, Week17)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week18)            {Format(Year, Week18)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week19)            {Format(Year, Week19)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week20)            {Format(Year, Week20)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week21)            {Format(Year, Week21)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week22)            {Format(Year, Week22)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week23)            {Format(Year, Week23)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week24)            {Format(Year, Week24)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week25)            {Format(Year, Week25)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week26)            {Format(Year, Week26)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week27)            {Format(Year, Week27)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week28)            {Format(Year, Week28)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week29)            {Format(Year, Week29)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week30)            {Format(Year, Week30)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week31)            {Format(Year, Week31)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week32)            {Format(Year, Week32)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week33)            {Format(Year, Week33)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week34)            {Format(Year, Week34)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week35)            {Format(Year, Week35)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week36)            {Format(Year, Week36)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week37)            {Format(Year, Week37)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week38)            {Format(Year, Week38)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week39)            {Format(Year, Week39)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week40)            {Format(Year, Week40)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week41)            {Format(Year, Week41)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week42)            {Format(Year, Week42)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week43)            {Format(Year, Week43)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week44)            {Format(Year, Week44)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week45)            {Format(Year, Week45)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week46)            {Format(Year, Week46)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week47)            {Format(Year, Week47)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week48)            {Format(Year, Week48)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week49)            {Format(Year, Week49)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week50)            {Format(Year, Week50)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week51)            {Format(Year, Week51)}");
            _testOutputHelper.WriteLine($"Format({Year}, Week52)            {Format(Year, Week52)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearPart1)     {Format(Year, FinancialYearPart1)}");
            _testOutputHelper.WriteLine($"Format({Year}, FinancialYearPart2)     {Format(Year, FinancialYearPart2)}");
        }
    }
}
