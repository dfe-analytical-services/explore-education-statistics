#nullable enable
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.StatsDb;

public abstract class StatisticsDbDataSetFactoryTests
{
    public class CreateTests : StatisticsDbDataSetFactoryTests
    {
        [Fact]
        public void Success_ReturnsDataSetBoundToSubject()
        {
            var subjectId = Guid.NewGuid();

            using var statisticsDbContext = InMemoryStatisticsDbContext();

            var factory = BuildStatisticsDbDataSetFactory(statisticsDbContext);

            var dataSet = factory.Create(subjectId);

            Assert.Equal(subjectId, dataSet.SubjectId);
        }
    }
}
