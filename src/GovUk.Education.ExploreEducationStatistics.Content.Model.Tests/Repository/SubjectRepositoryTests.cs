using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class SubjectRepositoryTests
{
    private readonly DataFixture _fixture = new();

    public class FindPublicationIdForSubjectTests : SubjectRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var subjectId = Guid.NewGuid();

            ReleaseFile releaseFile = _fixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subjectId));

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext);

                var result = await service.FindPublicationIdForSubject(subjectId);
                Assert.Equal(releaseFile.ReleaseVersion.Release.PublicationId, result);
            }
        }

        [Fact]
        public async Task SubjectDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();
            var service = BuildService(contentDbContext);

            Assert.Null(await service.FindPublicationIdForSubject(Guid.NewGuid()));
        }
    }

    private static SubjectRepository BuildService(ContentDbContext contentDbContext)
    {
        return new SubjectRepository(contentDbContext);
    }
}
