#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;

public class SubjectRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    public class FindPublicationIdForSubjectTests : SubjectRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            ReleaseSubject releaseSubject = _dataFixture
                .DefaultReleaseSubject()
                .WithReleaseVersion(_dataFixture.DefaultStatsReleaseVersion());

            var contextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                statisticsDbContext.ReleaseSubject.Add(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildService(statisticsDbContext);

                var result = await service.FindPublicationIdForSubject(releaseSubject.SubjectId);
                Assert.Equal(releaseSubject.ReleaseVersion.PublicationId, result);
            }
        }

        [Fact]
        public async Task ReleaseSubjectDoesNotExist_ReturnsNull()
        {
            Subject subject = _dataFixture.DefaultSubject();

            var contextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                statisticsDbContext.Subject.Add(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildService(statisticsDbContext);

                Assert.Null(await service.FindPublicationIdForSubject(subject.Id));
            }
        }

        [Fact]
        public async Task SubjectDoesNotExist_ReturnsNull()
        {
            await using var statisticsDbContext = InMemoryStatisticsDbContext();
            var service = BuildService(statisticsDbContext);

            Assert.Null(await service.FindPublicationIdForSubject(Guid.NewGuid()));
        }
    }

    private static SubjectRepository BuildService(StatisticsDbContext statisticsDbContext)
    {
        return new SubjectRepository(statisticsDbContext);
    }
}
