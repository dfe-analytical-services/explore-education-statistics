#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseVersionRepositoryTests
{
    private readonly DataFixture _fixture = new();

    public class ListReleasesForUserTests : ReleaseVersionRepositoryTests
    {
        [Theory]
        [InlineData(ReleaseApprovalStatus.Approved)]
        [InlineData(ReleaseApprovalStatus.HigherLevelReview)]
        [InlineData(ReleaseApprovalStatus.Draft)]
        public async Task ListReleasesForUser_Success(ReleaseApprovalStatus releaseApprovalStatus)
        {
            var user = _fixture.DefaultUser()
                .Generate();

            var userReleaseRoles = _fixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                    .WithApprovalStatus(releaseApprovalStatus)
                    .WithRelease(_fixture.DefaultRelease()
                        .WithPublication(_fixture.DefaultPublication())))
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(2);

            UserPublicationRole userPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithRole(PublicationRole.Owner)
                .WithPublication(_fixture.DefaultPublication()
                    .WithReleases([
                        _fixture.DefaultRelease()
                            .WithVersions([
                                _fixture.DefaultReleaseVersion()
                                    .WithApprovalStatus(releaseApprovalStatus)
                            ])
                    ]));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId: user.Id,
                    releaseApprovalStatus);
                Assert.Equal([
                        userReleaseRoles[0].ReleaseVersionId,
                        userPublicationRole.Publication.Releases[0].Versions[0].Id
                    ],
                    result.Select(rv => rv.Id));
            }
        }

        [Theory]
        [InlineData(ReleaseApprovalStatus.Approved)]
        [InlineData(ReleaseApprovalStatus.HigherLevelReview)]
        [InlineData(ReleaseApprovalStatus.Draft)]
        public async Task ListReleasesForUser_ExcludesOtherReleaseApprovalStatuses(
            ReleaseApprovalStatus releaseApprovalStatus)
        {
            var user = _fixture.DefaultUser()
                .Generate();

            var otherReleaseApprovalStatus = Enum.GetValues<ReleaseApprovalStatus>()
                .Except([releaseApprovalStatus])
                .ToList();

            var userReleaseRoles = otherReleaseApprovalStatus
                .Select(status => _fixture.DefaultUserReleaseRole()
                    .WithUser(user)
                    .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                        .WithApprovalStatus(status)
                        .WithRelease(_fixture.DefaultRelease()
                            .WithPublication(_fixture.DefaultPublication())))
                    .WithRole(ReleaseRole.Contributor)
                    .Generate())
                .ToList();

            var userPublicationRoles = otherReleaseApprovalStatus
                .Select(status => _fixture.DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithRole(PublicationRole.Owner)
                    .WithPublication(_fixture.DefaultPublication()
                        .WithReleases(_ =>
                        [
                            _fixture.DefaultRelease()
                                .WithVersions(_ =>
                                [
                                    _fixture.DefaultReleaseVersion()
                                        .WithApprovalStatus(status)
                                ])
                        ]))
                    .Generate())
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId: user.Id,
                    releaseApprovalStatus);
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_ExcludesOtherUsers()
        {
            var user = _fixture.DefaultUser()
                .Generate();

            UserReleaseRole userReleaseRole = _fixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                    .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                    .WithRelease(_fixture.DefaultRelease()
                        .WithPublication(_fixture.DefaultPublication())))
                .WithRole(ReleaseRole.Contributor);

            UserPublicationRole userPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithRole(PublicationRole.Owner)
                .WithPublication(_fixture.DefaultPublication()
                    .WithReleases([
                        _fixture.DefaultRelease()
                            .WithVersions([
                                _fixture.DefaultReleaseVersion()
                                    .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                            ])
                    ]));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId: Guid.NewGuid(),
                    ReleaseApprovalStatus.Approved);
                Assert.Empty(result);
            }
        }
    }

    public class CreateStatisticsDbReleaseVersionAndSubjectHierarchy : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task StatsReleaseDoesNotExist_CreatesStatsReleaseAndReleaseSubject()
        {
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture
                        .DefaultPublication()));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            Guid? createdSubjectId;
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var repository = BuildRepository(contentDbContext, statisticsDbContext);
                createdSubjectId = await repository.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statsReleaseVersion = Assert.Single(statisticsDbContext.ReleaseVersion);

                Assert.Equal(releaseVersion.Id, statsReleaseVersion.Id);
                Assert.Equal(releaseVersion.PublicationId, statsReleaseVersion.PublicationId);

                var releaseSubject = Assert.Single(statisticsDbContext.ReleaseSubject);

                Assert.Equal(releaseVersion.Id, releaseSubject.ReleaseVersionId);
                Assert.Equal(createdSubjectId, releaseSubject.SubjectId);
            }
        }

        [Fact]
        public async Task StatsReleaseExists_CreatesReleaseSubject()
        {
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture
                        .DefaultPublication()));

            Data.Model.ReleaseVersion existingStatsReleaseVersion = _fixture
                .DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .WithPublicationId(releaseVersion.PublicationId);

            ReleaseSubject existingReleaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithReleaseVersion(existingStatsReleaseVersion)
                .WithSubject(_fixture
                    .DefaultSubject());

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.Add(existingStatsReleaseVersion);
                statisticsDbContext.ReleaseSubject.Add(existingReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            Guid? createdSubjectId;
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var repository = BuildRepository(contentDbContext, statisticsDbContext);
                createdSubjectId = await repository.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statsReleaseVersion = Assert.Single(statisticsDbContext.ReleaseVersion);

                Assert.Equal(releaseVersion.Id, statsReleaseVersion.Id);
                Assert.Equal(releaseVersion.PublicationId, statsReleaseVersion.PublicationId);

                Assert.Equal(2, await statisticsDbContext.ReleaseSubject.CountAsync());

                Assert.True(await statisticsDbContext.ReleaseSubject
                    .AnyAsync(rs => rs.ReleaseVersionId == releaseVersion.Id
                                    && rs.SubjectId == existingReleaseSubject.SubjectId));

                Assert.True(await statisticsDbContext.ReleaseSubject
                    .AnyAsync(rs => rs.ReleaseVersionId == releaseVersion.Id
                                    && rs.SubjectId == createdSubjectId));
            }
        }
    }

    private static ReleaseVersionRepository BuildRepository(
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null
    )
    {
        return new ReleaseVersionRepository(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>());
    }
}
