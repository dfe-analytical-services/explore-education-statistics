#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public abstract class TimePeriodServiceTests
{
    public class GetTimePeriodLabelsTests : TimePeriodServiceTests
    {
        [Fact]
        public async Task DataSetHasQuarterlyTimePeriods_ReturnsCorrectFromAndToLabels()
        {
            var dataSet = MockDataSetWithTimePeriods([
                (2020, AcademicYearQ4),
                (2021, AcademicYearQ1),
                (2030, AcademicYearQ3),
            ]);

            var service = new TimePeriodService();

            var result = await service.GetTimePeriodLabels(dataSet.Object);

            Assert.Equal("2020/21 Q4", result.From);
            Assert.Equal("2030/31 Q3", result.To);
        }

        [Fact]
        public async Task DataSetHasWeeklyTimePeriods_ReturnsCorrectFromAndToLabels()
        {
            var dataSet = MockDataSetWithTimePeriods([(2020, Week8), (2020, Week9), (2020, Week37)]);

            var service = new TimePeriodService();

            var result = await service.GetTimePeriodLabels(dataSet.Object);

            Assert.Equal("2020 Week 8", result.From);
            Assert.Equal("2020 Week 37", result.To);
        }

        [Fact]
        public async Task DataSetHasNoTimePeriods_ReturnsEmptyLabels()
        {
            var dataSet = MockDataSetWithTimePeriods([]);

            var service = new TimePeriodService();

            var result = await service.GetTimePeriodLabels(dataSet.Object);

            Assert.Empty(result.From);
            Assert.Empty(result.To);
        }

        private static Mock<IStorageDataSet> MockDataSetWithTimePeriods(
            List<(int Year, TimeIdentifier TimeIdentifier)> timePeriods
        )
        {
            var dataSet = new Mock<IStorageDataSet>(Strict);

            dataSet.Setup(s => s.ListTimePeriods(It.IsAny<CancellationToken>())).ReturnsAsync(timePeriods);

            return dataSet;
        }
    }

    public class GetTimePeriodRangeTests : TimePeriodServiceTests
    {
        [Fact]
        public void AcademicYearRange_IncludesAllYearsInRange()
        {
            List<Observation> observations =
            [
                new() { Year = 2024, TimeIdentifier = AcademicYear },
                new() { Year = 2026, TimeIdentifier = AcademicYear },
            ];

            var service = new TimePeriodService();
            var result = service.GetTimePeriodRange(observations);

            Assert.Equal(3, result.Count);
            Assert.Equal((2024, AcademicYear), result[0]);
            Assert.Equal((2025, AcademicYear), result[1]);
            Assert.Equal((2026, AcademicYear), result[2]);
        }

        [Fact]
        public void TermRange_ExcludesTermsNotInObservations()
        {
            List<Observation> observations =
            [
                new() { Year = 2025, TimeIdentifier = AutumnTerm },
                new() { Year = 2025, TimeIdentifier = SummerTerm },
            ];

            var service = new TimePeriodService();
            var result = service.GetTimePeriodRange(observations);

            Assert.Equal(2, result.Count);
            Assert.Equal((2025, AutumnTerm), result[0]);
            Assert.Equal((2025, SummerTerm), result[1]);
        }

        [Fact]
        public void TermRangeIncludesAutumnSpringTerm_IncludesAutumnSpringTermInResult()
        {
            List<Observation> observations =
            [
                new() { Year = 2025, TimeIdentifier = AutumnTerm },
                new() { Year = 2025, TimeIdentifier = SpringTerm },
                new() { Year = 2025, TimeIdentifier = AutumnSpringTerm },
                new() { Year = 2025, TimeIdentifier = SummerTerm },
            ];

            var service = new TimePeriodService();
            var result = service.GetTimePeriodRange(observations);

            Assert.Equal(4, result.Count);
            Assert.Equal((2025, AutumnTerm), result[0]);
            Assert.Equal((2025, SpringTerm), result[1]);
            Assert.Equal((2025, AutumnSpringTerm), result[2]);
            Assert.Equal((2025, SummerTerm), result[3]);
        }

        [Fact]
        public void TermRangeSpansMultipleYears_ExcludesTermsNotInObservations()
        {
            List<Observation> observations =
            [
                new() { Year = 2024, TimeIdentifier = AutumnTerm },
                new() { Year = 2024, TimeIdentifier = SummerTerm },
                new() { Year = 2025, TimeIdentifier = AutumnTerm },
                new() { Year = 2025, TimeIdentifier = SummerTerm },
            ];

            var service = new TimePeriodService();
            var result = service.GetTimePeriodRange(observations);

            Assert.Equal(4, result.Count);
            Assert.Equal((2024, AutumnTerm), result[0]);
            Assert.Equal((2024, SummerTerm), result[1]);
            Assert.Equal((2025, AutumnTerm), result[2]);
            Assert.Equal((2025, SummerTerm), result[3]);
        }
    }
}
