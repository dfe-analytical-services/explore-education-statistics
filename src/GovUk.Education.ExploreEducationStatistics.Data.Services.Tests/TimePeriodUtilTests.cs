using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using Assert = Xunit.Assert;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
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
                CalendarYearQ2,
                CalendarYearQ3,
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

            var weekIdentifiers = new[]
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
            };

            var termIdentifiers = new[]
            {
                AutumnTerm,
                AutumnSpringTerm,
                SpringTerm,
                SummerTerm
            };

            var financialYearPartIdentifiers = new[]
            {
                FinancialYearPart1,
                FinancialYearPart2
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

            foreach (var identifier in _allTimeIdentifiers.Except(weekIdentifiers))
            {
                Assert.Throws<ArgumentException>(() =>
                    TimePeriodUtil.Range(new TimePeriodQuery(2018, Week1, 2019, identifier)));
            }

            foreach (var identifier in _allTimeIdentifiers.Except(termIdentifiers))
            {
                Assert.Throws<ArgumentException>(() =>
                    TimePeriodUtil.Range(new TimePeriodQuery(2018, AutumnTerm, 2019, identifier)));
            }

            foreach (var identifier in _allTimeIdentifiers.Except(financialYearPartIdentifiers))
            {
                Assert.Throws<ArgumentException>(() =>
                    TimePeriodUtil.Range(new TimePeriodQuery(2018, FinancialYearPart1, 2019, identifier)));
            }

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, AcademicYear)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, TaxYear)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, FinancialYear)));

            Assert.Throws<ArgumentException>(() =>
                TimePeriodUtil.Range(new TimePeriodQuery(2018, CalendarYear, 2019, ReportingYear)));
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
        public void RangeIsGeneratedForReportingYearQuery()
        {
            var expected = new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2018, ReportingYear),
                (2019, ReportingYear)
            };

            CollectionAssert.AreEquivalent(expected,
                TimePeriodUtil.Range(new TimePeriodQuery(2018, ReportingYear, 2019, ReportingYear)).ToList());
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
        public void RangeIsGeneratedForWeekQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2020, Week17)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2020, Week17, 2020, Week17)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2020, Week1),
                    (2020, Week2),
                    (2020, Week3),
                    (2020, Week4),
                    (2020, Week5),
                    (2020, Week6),
                    (2020, Week7),
                    (2020, Week8),
                    (2020, Week9),
                    (2020, Week10),
                    (2020, Week11),
                    (2020, Week12),
                    (2020, Week13),
                    (2020, Week14),
                    (2020, Week15),
                    (2020, Week16),
                    (2020, Week17),
                    (2020, Week18),
                    (2020, Week19),
                    (2020, Week20),
                    (2020, Week21),
                    (2020, Week22),
                    (2020, Week23),
                    (2020, Week24),
                    (2020, Week25),
                    (2020, Week26),
                    (2020, Week27),
                    (2020, Week28),
                    (2020, Week29),
                    (2020, Week30),
                    (2020, Week31),
                    (2020, Week32),
                    (2020, Week33),
                    (2020, Week34),
                    (2020, Week35),
                    (2020, Week36),
                    (2020, Week37),
                    (2020, Week38),
                    (2020, Week39),
                    (2020, Week40),
                    (2020, Week41),
                    (2020, Week42),
                    (2020, Week43),
                    (2020, Week44),
                    (2020, Week45),
                    (2020, Week46),
                    (2020, Week47),
                    (2020, Week48),
                    (2020, Week49),
                    (2020, Week50),
                    (2020, Week51),
                    (2020, Week52)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2020, Week1, 2020, Week52)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2020, Week17),
                    (2020, Week18),
                    (2020, Week19),
                    (2020, Week20)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2020, Week17, 2020, Week20)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, Week51),
                    (2019, Week52),
                    (2020, Week1),
                    (2020, Week2)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, Week51, 2020, Week2)).ToList());
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
                    (2019, AcademicYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, AcademicYearQ3, 2019, AcademicYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, AcademicYearQ4),
                    (2019, AcademicYearQ1),
                    (2019, AcademicYearQ2),
                    (2019, AcademicYearQ3),
                    (2019, AcademicYearQ4),
                    (2020, AcademicYearQ1),
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
                    (2019, CalendarYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, CalendarYearQ3, 2019, CalendarYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, CalendarYearQ4),
                    (2019, CalendarYearQ1),
                    (2019, CalendarYearQ2),
                    (2019, CalendarYearQ3),
                    (2019, CalendarYearQ4),
                    (2020, CalendarYearQ1),
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
                    (2019, FinancialYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, FinancialYearQ3, 2019, FinancialYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, FinancialYearQ4),
                    (2019, FinancialYearQ1),
                    (2019, FinancialYearQ2),
                    (2019, FinancialYearQ3),
                    (2019, FinancialYearQ4),
                    (2020, FinancialYearQ1),
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
                    (2019, TaxYearQ4)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2019, TaxYearQ3, 2019, TaxYearQ4)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2018, TaxYearQ4),
                    (2019, TaxYearQ1),
                    (2019, TaxYearQ2),
                    (2019, TaxYearQ3),
                    (2019, TaxYearQ4),
                    (2020, TaxYearQ1),
                    (2020, TaxYearQ2)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2018, TaxYearQ4, 2020, TaxYearQ2)).ToList());
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

        [Fact]
        public void RangeIsGeneratedForFinancialYearPartQuery()
        {
            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2021, FinancialYearPart1)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2021, FinancialYearPart1, 2021, FinancialYearPart1)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2021, FinancialYearPart1),
                    (2021, FinancialYearPart2)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2021, FinancialYearPart1, 2021, FinancialYearPart2)).ToList());

            CollectionAssert.AreEquivalent(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2020, FinancialYearPart1),
                    (2020, FinancialYearPart2),
                    (2021, FinancialYearPart1)
                },
                TimePeriodUtil.Range(new TimePeriodQuery(2020, FinancialYearPart1, 2021, FinancialYearPart1)).ToList());
        }
    }
}