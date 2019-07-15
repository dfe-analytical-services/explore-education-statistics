using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Model.Service;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Model.Service.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Extensions
{
    public class TimeIdentifierExtensionsTests
    {
        private readonly IEnumerable<TimeIdentifier> _allTimeIdentifiers =
            Enum.GetValues(typeof(TimeIdentifier)).Cast<TimeIdentifier>().ToList();

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
            Assert.True(EndOfMarch.IsAlike(EndOfMarch));
            Assert.True(FiveHalfTerms.IsAlike(FiveHalfTerms));
            Assert.True(SixHalfTerms.IsAlike(SixHalfTerms));
            Assert.True(January.IsAlike(January));
            Assert.True(SpringTerm.IsAlike(SpringTerm));
        }

        [Fact]
        public void AcademicQuartersAreAlike()
        {
            AssertTimeIdentifiersAreAlike(TimeIdentifierUtil.GetAcademicQuarters());
        }

        [Fact]
        public void CalendarQuartersAreAlike()
        {
            AssertTimeIdentifiersAreAlike(TimeIdentifierUtil.GetCalendarQuarters());
        }

        [Fact]
        public void FinancialQuartersAreAlike()
        {
            AssertTimeIdentifiersAreAlike(TimeIdentifierUtil.GetFinancialQuarters());
        }

        [Fact]
        public void TaxQuartersAreAlike()
        {
            AssertTimeIdentifiersAreAlike(TimeIdentifierUtil.GetTaxQuarters());
        }

        [Fact]
        public void MonthsAreAlike()
        {
            AssertTimeIdentifiersAreAlike(TimeIdentifierUtil.GetMonths());
        }

        [Fact]
        public void TermsAreAlike()
        {
            AssertTimeIdentifiersAreAlike(TimeIdentifierUtil.GetTerms());
        }

        [Fact]
        public void TimeIdentifiersAreAcademicQuarters()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsAcademicQuarter(),
                TimeIdentifierUtil.GetAcademicQuarters());
        }

        [Fact]
        public void TimeIdentifiersAreCalendarQuarters()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsCalendarQuarter(),
                TimeIdentifierUtil.GetCalendarQuarters());
        }

        [Fact]
        public void TimeIdentifiersAreFinancialQuarters()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsFinancialQuarter(),
                TimeIdentifierUtil.GetFinancialQuarters());
        }

        [Fact]
        public void TimeIdentifiersAreTaxQuarters()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsTaxQuarter(),
                TimeIdentifierUtil.GetTaxQuarters());
        }

        [Fact]
        public void TimeIdentifiersAreYears()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsYear(), TimeIdentifierUtil.GetYears());
        }

        [Fact]
        public void TimeIdentifiersAreMonths()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsMonth(), TimeIdentifierUtil.GetMonths());
        }

        [Fact]
        public void TimeIdentifiersAreNumberOfTerms()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsNumberOfTerms(),
                TimeIdentifierUtil.GetNumberOfTerms());
        }

        [Fact]
        public void TimeIdentifiersAreTerms()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.IsTerm(), TimeIdentifierUtil.GetTerms());
        }

        [Fact]
        public void TimeIdentifiersHaveAssociatedRanges()
        {
            AssertTimeIdentifiersMeetCondition(identifier => identifier.HasAssociatedRange(),
                _allTimeIdentifiers.Except(TimeIdentifierUtil.GetYears()));
        }

        [Fact]
        public void GetAssociatedRangeForIdentifierWithoutRangeThrowsException()
        {
            foreach (var identifier in TimeIdentifierUtil.GetYears())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => identifier.GetAssociatedRange());
            }
        }

        [Fact]
        public void GetAssociatedRangeForAcademicQuarterReturnsAssociatedRange()
        {
            Assert.Equal(TimeIdentifierUtil.GetAcademicQuarters(), AcademicYearQ1.GetAssociatedRange());
        }

        [Fact]
        public void GetAssociatedRangeForCalendarQuarterReturnsAssociatedRange()
        {
            Assert.Equal(TimeIdentifierUtil.GetCalendarQuarters(), CalendarYearQ1.GetAssociatedRange());
        }

        [Fact]
        public void GetAssociatedRangeForFinancialQuarterReturnsAssociatedRange()
        {
            Assert.Equal(TimeIdentifierUtil.GetFinancialQuarters(), FinancialYearQ1.GetAssociatedRange());
        }

        [Fact]
        public void GetAssociatedRangeForTaxQuarterReturnsAssociatedRange()
        {
            Assert.Equal(TimeIdentifierUtil.GetTaxQuarters(), TaxYearQ1.GetAssociatedRange());
        }

        [Fact]
        public void GetAssociatedRangeForMonthReturnsAssociatedRange()
        {
            Assert.Equal(TimeIdentifierUtil.GetMonths(), January.GetAssociatedRange());
        }

        [Fact]
        public void GetAssociatedRangeForTermReturnsAssociatedRange()
        {
            Assert.Equal(TimeIdentifierUtil.GetTerms(), AutumnTerm.GetAssociatedRange());
        }

        private void AssertTimeIdentifiersMeetCondition(Func<TimeIdentifier, bool> condition,
            IEnumerable<TimeIdentifier> expectedTrue)
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
}