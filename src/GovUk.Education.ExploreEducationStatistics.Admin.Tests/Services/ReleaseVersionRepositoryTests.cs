#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
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
            User user = _fixture.DefaultUser();
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(releaseApprovalStatus)
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(releaseApprovalStatus)
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion3 = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(releaseApprovalStatus)
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion4 = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(releaseApprovalStatus)
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion1)
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Approver))
                // This release version is double-counted to test distinctness
                .ForIndex(2, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                // This one should be filtered out
                .ForIndex(3, s => s.SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(4);

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .ForIndex(0, s => s.SetPublication(releaseVersion4.Release.Publication).SetRole(PublicationRole.Owner))
                .ForIndex(
                    1,
                    s => s.SetPublication(releaseVersion4.Release.Publication).SetRole(PublicationRole.Allower)
                )
                // This release version is double-counted to test distinctness
                .ForIndex(
                    2,
                    s => s.SetPublication(releaseVersion2.Release.Publication).SetRole(PublicationRole.Allower)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange([
                    releaseVersion1,
                    releaseVersion2,
                    releaseVersion3,
                    releaseVersion4,
                ]);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
                .Returns(userPublicationRoles.BuildMock());

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
                .Returns(userReleaseRoles.BuildMock());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await repository.ListReleasesForUser(userId: user.Id, releaseApprovalStatus);

                Assert.Equal([releaseVersion1.Id, releaseVersion2.Id, releaseVersion4.Id], result.Select(rv => rv.Id));
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Theory]
        [InlineData(ReleaseApprovalStatus.Approved)]
        [InlineData(ReleaseApprovalStatus.HigherLevelReview)]
        [InlineData(ReleaseApprovalStatus.Draft)]
        public async Task ListReleasesForUser_ExcludesOtherReleaseApprovalStatuses(
            ReleaseApprovalStatus releaseApprovalStatus
        )
        {
            User user = _fixture.DefaultUser();

            var otherReleaseApprovalStatus = Enum.GetValues<ReleaseApprovalStatus>()
                .Except([releaseApprovalStatus])
                .ToList();

            var userReleaseRoles = otherReleaseApprovalStatus
                .Select(status =>
                    _fixture
                        .DefaultUserReleaseRole()
                        .WithUser(user)
                        .WithReleaseVersion(
                            _fixture
                                .DefaultReleaseVersion()
                                .WithApprovalStatus(status)
                                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                        )
                        .WithRole(ReleaseRole.Contributor)
                        .Generate()
                )
                .ToList();

            var userPublicationRoles = otherReleaseApprovalStatus
                .Select(status =>
                    _fixture
                        .DefaultUserPublicationRole()
                        .WithUser(user)
                        .WithRole(PublicationRole.Owner)
                        .WithPublication(
                            _fixture
                                .DefaultPublication()
                                .WithReleases(_ =>
                                    [
                                        _fixture
                                            .DefaultRelease()
                                            .WithVersions(_ =>
                                                [_fixture.DefaultReleaseVersion().WithApprovalStatus(status)]
                                            ),
                                    ]
                                )
                        )
                        .Generate()
                )
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
                .Returns(userPublicationRoles.BuildMock());

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
                .Returns(userReleaseRoles.BuildMock());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await repository.ListReleasesForUser(userId: user.Id, releaseApprovalStatus);

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task ListReleasesForUser_ExcludesOtherUsers()
        {
            User user = _fixture.DefaultUser();

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.Contributor);

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithRole(PublicationRole.Owner)
                .WithPublication(
                    _fixture
                        .DefaultPublication()
                        .WithReleases([
                            _fixture
                                .DefaultRelease()
                                .WithVersions([
                                    _fixture.DefaultReleaseVersion().WithApprovalStatus(ReleaseApprovalStatus.Approved),
                                ]),
                        ])
                );

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange([
                    userReleaseRole.ReleaseVersion,
                    userPublicationRole.Publication.Releases[0].Versions[0],
                ]);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
                .Returns(new[] { userPublicationRole }.BuildMock());

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
                .Returns(new[] { userReleaseRole }.BuildMock());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await repository.ListReleasesForUser(
                    userId: Guid.NewGuid(),
                    ReleaseApprovalStatus.Approved
                );

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
        }
    }

    public class CreateStatisticsDbReleaseVersionAndSubjectHierarchy : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task StatsReleaseDoesNotExist_CreatesStatsReleaseAndReleaseSubject()
        {
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

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
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            Data.Model.ReleaseVersion existingStatsReleaseVersion = _fixture
                .DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .WithPublicationId(releaseVersion.PublicationId);

            ReleaseSubject existingReleaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithReleaseVersion(existingStatsReleaseVersion)
                .WithSubject(_fixture.DefaultSubject());

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

                Assert.True(
                    await statisticsDbContext.ReleaseSubject.AnyAsync(rs =>
                        rs.ReleaseVersionId == releaseVersion.Id && rs.SubjectId == existingReleaseSubject.SubjectId
                    )
                );

                Assert.True(
                    await statisticsDbContext.ReleaseSubject.AnyAsync(rs =>
                        rs.ReleaseVersionId == releaseVersion.Id && rs.SubjectId == createdSubjectId
                    )
                );
            }
        }
    }

    private static ReleaseVersionRepository BuildRepository(
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null
    )
    {
        return new ReleaseVersionRepository(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>()
        );
    }
}
