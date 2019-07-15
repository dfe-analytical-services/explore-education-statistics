using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using Assert = Xunit.Assert;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class TimePeriodUtilTests
    {
        private readonly IEnumerable<TimeIdentifier> _allTimeIdentifiers =
            Enum.GetValues(typeof(TimeIdentifier)).Cast<TimeIdentifier>().ToList();

        [Fact]
        public void RangeFailsWithoutYears()
        {
            Assert.Throws<ArgumentNullException>(() => TimePeriodUtil.Range(
                new TimePeriodQuery
                {
                    StartCode = CalendarYear,
                    EndYear = 2019,
                    EndCode = CalendarYear
                }));

            Assert.Throws<ArgumentNullException>(() => TimePeriodUtil.Range(
                new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = CalendarYear,
                    EndCode = CalendarYear
                }));
        }

        [Fact]
        public void RangeFailsIfStartYearIsAfterEndYear()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2019, CalendarYear, 2018, CalendarYear)));
        }

        [Fact]
        public void RangeFailsIfYearsAreInvalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(12345, CalendarYear, 2019, CalendarYear)));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2019, CalendarYear, 12345, CalendarYear)));
        }

        [Fact]
        public void RangeFailsIfTimeIdentifiersAreNotAlike()
        {
            var calendarQuarterIdentifiers = new[]
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
            };

            var monthIdentifiers = new[]
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
            };

            var termIdentifiers = new[]
            {
                AutumnTerm,
                AutumnSpringTerm,
                SpringTerm,
                SummerTerm
            };

            foreach (var identifier in _allTimeIdentifiers.Except(calendarQuarterIdentifiers))
            {
                Assert.Throws<ArgumentException>(() =>
                    TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYearQ1, 2019, identifier)));
            }

            foreach (var identifier in _allTimeIdentifiers.Except(monthIdentifiers))
            {
                Assert.Throws<ArgumentException>(() =>
                    TimePeriodUtil.Range(new TimePeriodQuery(2018, January, 2019, identifier)));
            }

            foreach (var identifier in _allTimeIdentifiers.Except(termIdentifiers))
            {
                Assert.Throws<ArgumentException>(() =>
                    TimePeriodUtil.Range(new TimePeriodQuery(2018, AutumnTerm, 2019, identifier)));
            }

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, AcademicYear)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, TaxYear)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, FinancialYear)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, EndOfMarch)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, FiveHalfTerms)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, FiveHalfTerms, 2019, SixHalfTerms)));
        }

        [Fact]
        public void RangeIsGeneratedForAcademicYearQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, AcademicYear),
                (2019, AcademicYear)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, AcademicYear, 2019, AcademicYear)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForCalendarYearQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, CalendarYear),
                (2019, CalendarYear)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, CalendarYear)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForFinancialYearQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, FinancialYear),
                (2019, FinancialYear)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, FinancialYear, 2019, FinancialYear)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForTaxYearQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, TaxYear),
                (2019, TaxYear)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, TaxYear, 2019, TaxYear)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForEndOfMarchQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, EndOfMarch),
                (2019, EndOfMarch)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, EndOfMarch, 2019, EndOfMarch)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForFiveHalfTermsQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, FiveHalfTerms),
                (2019, FiveHalfTerms)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, FiveHalfTerms, 2019, FiveHalfTerms)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForSixHalfTermsQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, SixHalfTerms),
                (2019, SixHalfTerms)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, SixHalfTerms, 2019, SixHalfTerms)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForNumberOfTermsQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, FiveHalfTerms),
                (2019, FiveHalfTerms),
                (2018, SixHalfTerms),
                (2019, SixHalfTerms)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.RangeForNumberOfTerms(2018, 2019).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForMonthQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, February)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, February, 2019, February)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, January),
                    (2019, February),
                    (2019, March),
                    (2019, April),
                    (2019, May),
                    (2019, June),
                    (2019, July),
                    (2019, August),
                    (2019, September),
                    (2019, October),
                    (2019, November),
                    (2019, December)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, January, 2019, December)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, March),
                    (2019, April),
                    (2019, May),
                    (2019, June),
                    (2019, July),
                    (2019, August),
                    (2019, September),
                    (2019, October)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, March, 2019, October)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, April),
                    (2018, May),
                    (2018, June),
                    (2018, July),
                    (2018, August),
                    (2018, September),
                    (2018, October),
                    (2018, November),
                    (2018, December),
                    (2019, January),
                    (2019, February),
                    (2019, March),
                    (2019, April),
                    (2019, May),
                    (2019, June),
                    (2019, July),
                    (2019, August),
                    (2019, September),
                    (2019, October),
                    (2019, November),
                    (2019, December),
                    (2020, January),
                    (2020, February),
                    (2020, March)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2018, April, 2020, March)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForAcademicQuarterQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, AcademicYearQ3)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, AcademicYearQ3, 2019, AcademicYearQ3)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, AcademicYearQ3),
                    (2019, AcademicYearQ3Q4),
                    (2019, AcademicYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, AcademicYearQ3, 2019, AcademicYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, AcademicYearQ4),
                    (2019, AcademicYearQ1),
                    (2019, AcademicYearQ1Q2),
                    (2019, AcademicYearQ1Q3),
                    (2019, AcademicYearQ1Q4),
                    (2019, AcademicYearQ2),
                    (2019, AcademicYearQ2Q3),
                    (2019, AcademicYearQ2Q4),
                    (2019, AcademicYearQ3),
                    (2019, AcademicYearQ3Q4),
                    (2019, AcademicYearQ4),
                    (2020, AcademicYearQ1),
                    (2020, AcademicYearQ1Q2),
                    (2020, AcademicYearQ1Q3),
                    (2020, AcademicYearQ1Q4),
                    (2020, AcademicYearQ2)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2018, AcademicYearQ4, 2020, AcademicYearQ2)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForCalendarQuarterQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYearQ3)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, CalendarYearQ3, 2019, CalendarYearQ3)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYearQ3),
                    (2019, CalendarYearQ3Q4),
                    (2019, CalendarYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, CalendarYearQ3, 2019, CalendarYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, CalendarYearQ4),
                    (2019, CalendarYearQ1),
                    (2019, CalendarYearQ1Q2),
                    (2019, CalendarYearQ1Q3),
                    (2019, CalendarYearQ1Q4),
                    (2019, CalendarYearQ2),
                    (2019, CalendarYearQ2Q3),
                    (2019, CalendarYearQ2Q4),
                    (2019, CalendarYearQ3),
                    (2019, CalendarYearQ3Q4),
                    (2019, CalendarYearQ4),
                    (2020, CalendarYearQ1),
                    (2020, CalendarYearQ1Q2),
                    (2020, CalendarYearQ1Q3),
                    (2020, CalendarYearQ1Q4),
                    (2020, CalendarYearQ2)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYearQ4, 2020, CalendarYearQ2)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForFinancialQuarterQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, FinancialYearQ3)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, FinancialYearQ3, 2019, FinancialYearQ3)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, FinancialYearQ3),
                    (2019, FinancialYearQ3Q4),
                    (2019, FinancialYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, FinancialYearQ3, 2019, FinancialYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, FinancialYearQ4),
                    (2019, FinancialYearQ1),
                    (2019, FinancialYearQ1Q2),
                    (2019, FinancialYearQ1Q3),
                    (2019, FinancialYearQ1Q4),
                    (2019, FinancialYearQ2),
                    (2019, FinancialYearQ2Q3),
                    (2019, FinancialYearQ2Q4),
                    (2019, FinancialYearQ3),
                    (2019, FinancialYearQ3Q4),
                    (2019, FinancialYearQ4),
                    (2020, FinancialYearQ1),
                    (2020, FinancialYearQ1Q2),
                    (2020, FinancialYearQ1Q3),
                    (2020, FinancialYearQ1Q4),
                    (2020, FinancialYearQ2)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2018, FinancialYearQ4, 2020, FinancialYearQ2)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForTaxQuarterQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, TaxYearQ3)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, TaxYearQ3, 2019, TaxYearQ3)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, TaxYearQ3),
                    (2019, TaxYearQ3Q4),
                    (2019, TaxYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, TaxYearQ3, 2019, TaxYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, TaxYearQ4),
                    (2019, TaxYearQ1),
                    (2019, TaxYearQ1Q2),
                    (2019, TaxYearQ1Q3),
                    (2019, TaxYearQ1Q4),
                    (2019, TaxYearQ2),
                    (2019, TaxYearQ2Q3),
                    (2019, TaxYearQ2Q4),
                    (2019, TaxYearQ3),
                    (2019, TaxYearQ3Q4),
                    (2019, TaxYearQ4),
                    (2020, TaxYearQ1),
                    (2020, TaxYearQ1Q2),
                    (2020, TaxYearQ1Q3),
                    (2020, TaxYearQ1Q4),
                    (2020, TaxYearQ2)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2018, TimeIdentifier.TaxYearQ4, 2020, TaxYearQ2)).ToList());
        }

        [Fact]
        public void RangeIsGeneratedForTermQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, AutumnTerm)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, AutumnTerm, 2019, AutumnTerm)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, AutumnTerm),
                    (2019, AutumnSpringTerm),
                    (2019, SpringTerm),
                    (2019, SummerTerm)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, AutumnTerm, 2019, SummerTerm)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, SpringTerm),
                    (2018, SummerTerm),
                    (2019, AutumnTerm),
                    (2019, AutumnSpringTerm),
                    (2019, SpringTerm),
                    (2019, SummerTerm),
                    (2020, AutumnTerm)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2018, SpringTerm, 2020, AutumnTerm)).ToList());
        }
    }
}