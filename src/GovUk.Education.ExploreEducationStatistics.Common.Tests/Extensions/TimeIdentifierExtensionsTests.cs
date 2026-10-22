using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public class TimeIdentifierExtensionsTests
{
    private readonly IEnumerable<TimeIdentifier> _allTimeIdentifiers = Enum.GetValues(typeof(TimeIdentifier))
        .Cast<TimeIdentifier>()
        .ToList();

    [Fact]
    public void TimeIdentifiersAreAlikeWhenTheyAreEqual()
    {
        Assert.True(AcademicYear.IsAlike(AcademicYear));
        Assert.True(CalendarYear.IsAlike(CalendarYear));
        Assert.True(FinancialYear.IsAlike(FinancialYear));
        Assert.True(TaxYear.IsAlike(TaxYear));
        Assert.True(AcademicYearQ1.IsAlike(AcademicYearQ1));
        Assert.True(CalendarYearQ1.IsAlike(CalendarYearQ1));
        Assert.True(FinancialYearQ1.IsAlike(FinancialYearQ1));
        Assert.True(TaxYearQ1.IsAlike(TaxYearQ1));
        Assert.True(January.IsAlike(January));
        Assert.True(SpringTerm.IsAlike(SpringTerm));
        Assert.True(ReportingYear.IsAlike(ReportingYear));
        Assert.True(Week1.IsAlike(Week1));
        Assert.True(FinancialYearPart1.IsAlike(FinancialYearPart1));
    }

    [Fact]
    public void AcademicQuartersAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetAcademicQuarters());
    }

    [Fact]
    public void CalendarQuartersAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetCalendarQuarters());
    }

    [Fact]
    public void FinancialQuartersAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetFinancialQuarters());
    }

    [Fact]
    public void TaxQuartersAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetTaxQuarters());
    }

    [Fact]
    public void MonthsAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetMonths());
    }

    [Fact]
    public void WeeksAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetWeeks());
    }

    [Fact]
    public void TermsAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetTerms());
    }

    [Fact]
    public void FinancialYearPartsAreAlike()
    {
        AssertTimeIdentifiersAreAlike(TimeIdentifierUtils.GetFinancialYearParts());
    }

    [Fact]
    public void TimeIdentifiersAreAcademicQuarters()
    {
        AssertTimeIdentifiersMeetCondition(
            identifier => identifier.IsAcademicQuarter(),
            TimeIdentifierUtils.GetAcademicQuarters()
        );
    }

    [Fact]
    public void TimeIdentifiersAreCalendarQuarters()
    {
        AssertTimeIdentifiersMeetCondition(
            identifier => identifier.IsCalendarQuarter(),
            TimeIdentifierUtils.GetCalendarQuarters()
        );
    }

    [Fact]
    public void TimeIdentifiersAreFinancialQuarters()
    {
        AssertTimeIdentifiersMeetCondition(
            identifier => identifier.IsFinancialQuarter(),
            TimeIdentifierUtils.GetFinancialQuarters()
        );
    }

    [Fact]
    public void TimeIdentifiersAreTaxQuarters()
    {
        AssertTimeIdentifiersMeetCondition(
            identifier => identifier.IsTaxQuarter(),
            TimeIdentifierUtils.GetTaxQuarters()
        );
    }

    [Fact]
    public void TimeIdentifiersAreYears()
    {
        AssertTimeIdentifiersMeetCondition(identifier => identifier.IsYear(), TimeIdentifierUtils.GetYears());
    }

    [Fact]
    public void TimeIdentifiersAreMonths()
    {
        AssertTimeIdentifiersMeetCondition(identifier => identifier.IsMonth(), TimeIdentifierUtils.GetMonths());
    }

    [Fact]
    public void TimeIdentifiersAreWeeks()
    {
        AssertTimeIdentifiersMeetCondition(identifier => identifier.IsWeek(), TimeIdentifierUtils.GetWeeks());
    }

    [Fact]
    public void TimeIdentifiersAreTerms()
    {
        AssertTimeIdentifiersMeetCondition(identifier => identifier.IsTerm(), TimeIdentifierUtils.GetTerms());
    }

    [Fact]
    public void TimeIdentifiersAreFinancialYearParts()
    {
        AssertTimeIdentifiersMeetCondition(
            identifier => identifier.IsFinancialYearPart(),
            TimeIdentifierUtils.GetFinancialYearParts()
        );
    }

    [Fact]
    public void TimeIdentifiersHaveAssociatedRanges()
    {
        AssertTimeIdentifiersMeetCondition(
            identifier => identifier.HasAssociatedRange(),
            _allTimeIdentifiers.Except(TimeIdentifierUtils.GetYears())
        );
    }

    [Fact]
    public void GetAssociatedRangeForIdentifierWithoutRangeThrowsException()
    {
        foreach (var identifier in TimeIdentifierUtils.GetYears())
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => identifier.GetAssociatedRange());
        }
    }

    [Fact]
    public void GetAssociatedRangeForAcademicQuarterReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetAcademicQuarters(), AcademicYearQ1.GetAssociatedRange());
    }

    [Fact]
    public void GetAssociatedRangeForCalendarQuarterReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetCalendarQuarters(), CalendarYearQ1.GetAssociatedRange());
    }

    [Fact]
    public void GetAssociatedRangeForFinancialQuarterReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetFinancialQuarters(), FinancialYearQ1.GetAssociatedRange());
    }

    [Fact]
    public void GetAssociatedRangeForTaxQuarterReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetTaxQuarters(), TaxYearQ1.GetAssociatedRange());
    }

    [Fact]
    public void GetAssociatedRangeForMonthReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetMonths(), January.GetAssociatedRange());
    }

    [Fact]
    public void GetAssociatedRangeForWeeksReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetWeeks(), Week1.GetAssociatedRange());
    }

    [Fact]
    public void GetAssociatedRangeForTermsReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetTerms(), AutumnTerm.GetAssociatedRange());
    }

    [Fact]
    public void GetAssociatedRangeForFinancialYearPartsReturnsAssociatedRange()
    {
        Assert.Equal(TimeIdentifierUtils.GetFinancialYearParts(), FinancialYearPart1.GetAssociatedRange());
    }

    private void AssertTimeIdentifiersMeetCondition(
        Func<TimeIdentifier, bool> condition,
        IEnumerable<TimeIdentifier> expectedTrue
    )
    {
        foreach (var identifier in expectedTrue)
        {
            Assert.True(condition.Invoke(identifier));
        }

        foreach (var identifier in _allTimeIdentifiers.Except(expectedTrue))
        {
            Assert.False(condition.Invoke(identifier));
        }
    }

    private void AssertTimeIdentifiersAreAlike(IEnumerable<TimeIdentifier> expectedTrue)
    {
        foreach (var identifier in expectedTrue)
        {
            Assert.True(expectedTrue.First().IsAlike(identifier));
        }

        foreach (var identifier in _allTimeIdentifiers.Except(expectedTrue))
        {
            Assert.False(expectedTrue.First().IsAlike(identifier));
        }
    }
}
