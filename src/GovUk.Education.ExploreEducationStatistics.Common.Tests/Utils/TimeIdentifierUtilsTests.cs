using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class TimeIdentifierUtilTests
{
    [Fact]
    public void GetIdentifiersUsingAcademicQuartersReturnsAcademicQuarters()
    {
        Assert.Equal(
            new[] { AcademicYearQ1, AcademicYearQ2, AcademicYearQ3, AcademicYearQ4 },
            TimeIdentifierUtils.GetIdentifiersUsingAcademicQuarters()
        );
    }

    [Fact]
    public void GetIdentifiersUsingCalendarQuartersReturnsCalendarQuarters()
    {
        Assert.Equal(
            new[] { CalendarYearQ1, CalendarYearQ2, CalendarYearQ3, CalendarYearQ4 },
            TimeIdentifierUtils.GetIdentifiersUsingCalendarQuarters()
        );
    }

    [Fact]
    public void GetIdentifiersUsingFinancialQuartersReturnsFinancialQuarters()
    {
        Assert.Equal(
            new[] { FinancialYearQ1, FinancialYearQ2, FinancialYearQ3, FinancialYearQ4 },
            TimeIdentifierUtils.GetIdentifiersUsingFinancialQuarters()
        );
    }

    [Fact]
    public void GetIdentifiersUsingTaxQuartersReturnsTaxQuarters()
    {
        Assert.Equal(
            new[] { TaxYearQ1, TaxYearQ2, TaxYearQ3, TaxYearQ4 },
            TimeIdentifierUtils.GetIdentifiersUsingTaxQuarters()
        );
    }

    [Fact]
    public void GetIdentifiersUsingMonthsReturnsMonths()
    {
        Assert.Equal(
            new[] { January, February, March, April, May, June, July, August, September, October, November, December },
            TimeIdentifierUtils.GetIdentifiersUsingMonths()
        );
    }

    [Fact]
    public void GetIdentifiersUsingYearsReturnsYears()
    {
        Assert.Equal(
            new[] { AcademicYear, CalendarYear, FinancialYear, TaxYear, ReportingYear },
            TimeIdentifierUtils.GetIdentifiersUsingYears()
        );
    }

    [Fact]
    public void GetIdentifiersUsingWeeksReturnsWeeks()
    {
        Assert.Equal(
            new[]
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
                Week52,
            },
            TimeIdentifierUtils.GetIdentifiersUsingWeeks()
        );
    }

    [Fact]
    public void GetIdentifiersUsingTermsReturnsTerms()
    {
        Assert.Equal(
            new[] { AutumnTerm, SpringTerm, AutumnSpringTerm, SummerTerm },
            TimeIdentifierUtils.GetIdentifiersUsingTerms()
        );
    }

    [Fact]
    public void GetIdentifiersUsingFinancialYearPartsReturnsFinancialYearParts()
    {
        Assert.Equal(
            new[] { FinancialYearPart1, FinancialYearPart2 },
            TimeIdentifierUtils.GetIdentifiersUsingFinancialYearParts()
        );
    }
}
