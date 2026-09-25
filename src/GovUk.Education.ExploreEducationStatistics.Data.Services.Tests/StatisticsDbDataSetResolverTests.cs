#nullable enable
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils.StorageDataSetTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public abstract class StatisticsDbDataSetResolverTests
{
    public class ResolveTests : StatisticsDbDataSetResolverTests
    {
        [Fact]
        public async Task Success_ReturnsDataSetBoundToSubject()
        {
            var subjectId = Guid.NewGuid();

            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStatisticsDbDataSetResolver(statisticsDbContext);

            var result = await resolver.Resolve(subjectId);

            var dataSet = Assert.IsType<StatisticsDbDataSet>(result);
            Assert.Equal(subjectId, dataSet.SubjectId);
        }
    }
}
