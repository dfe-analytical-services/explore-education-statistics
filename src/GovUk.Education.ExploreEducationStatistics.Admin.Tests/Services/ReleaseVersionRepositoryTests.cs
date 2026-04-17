#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
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
            User user = _fixture.DefaultUser();
            var (releaseVersion1, releaseVersion2, releaseVersion3, releaseVersion4) = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(releaseApprovalStatus)
                // All unique publications to avoid them all just being pulled in via one publication role
                .ForIndex(
                    0,
                    s => s.SetRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .ForIndex(
                    1,
                    s => s.SetRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .ForIndex(
                    2,
                    s => s.SetRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .ForIndex(
                    3,
                    s => s.SetRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .GenerateTuple4();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .ForIndex(
                    0,
                    s => s.SetPublication(releaseVersion1.Release.Publication).SetRole(PublicationRole.Drafter)
                )
                .ForIndex(
                    1,
                    s => s.SetPublication(releaseVersion2.Release.Publication).SetRole(PublicationRole.Approver)
                )
                .ForIndex(
                    2,
                    s => s.SetPublication(releaseVersion3.Release.Publication).SetRole(PublicationRole.Approver)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(
                    releaseVersion1,
                    releaseVersion2,
                    releaseVersion3,
                    releaseVersion4
                );
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, [.. userPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.ListReleasesForUser(userId: user.Id, releaseApprovalStatus);

                Assert.Equal([releaseVersion1.Id, releaseVersion2.Id, releaseVersion3.Id], result.Select(rv => rv.Id));
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }

        [Theory]
        [InlineData(ReleaseApprovalStatus.Approved, PublicationRole.Approver)]
        [InlineData(ReleaseApprovalStatus.Approved, PublicationRole.Drafter)]
        [InlineData(ReleaseApprovalStatus.HigherLevelReview, PublicationRole.Approver)]
        [InlineData(ReleaseApprovalStatus.HigherLevelReview, PublicationRole.Drafter)]
        [InlineData(ReleaseApprovalStatus.Draft, PublicationRole.Approver)]
        [InlineData(ReleaseApprovalStatus.Draft, PublicationRole.Drafter)]
        public async Task ListReleasesForUser_IncludesAllReleaseVersionsAssociatedWithAPublication(
            ReleaseApprovalStatus releaseApprovalStatus,
            PublicationRole publicationRole
        )
        {
            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            var (releaseVersion1, releaseVersion2, releaseVersion3) = _fixture
                .DefaultReleaseVersion()
                .WithApprovalStatus(releaseApprovalStatus)
                .ForIndex(0, s => s.SetRelease(_fixture.DefaultRelease().WithPublication(publication)))
                .ForIndex(1, s => s.SetRelease(_fixture.DefaultRelease().WithPublication(publication)))
                .ForIndex(2, s => s.SetRelease(_fixture.DefaultRelease().WithPublication(publication)))
                .GenerateTuple3();

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2, releaseVersion3);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.ListReleasesForUser(userId: user.Id, releaseApprovalStatus);

                Assert.Equal([releaseVersion1.Id, releaseVersion2.Id, releaseVersion3.Id], result.Select(rv => rv.Id));
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
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

            var userPublicationRoles = otherReleaseApprovalStatus
                .Select(status =>
                    _fixture
                        .DefaultUserPublicationRole()
                        .WithUser(user)
                        .WithRole(PublicationRole.Drafter)
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
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, [.. userPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.ListReleasesForUser(userId: user.Id, releaseApprovalStatus);

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task ListReleasesForUser_ExcludesOtherUsers()
        {
            User user = _fixture.DefaultUser();

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithRole(PublicationRole.Drafter)
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
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.ListReleasesForUser(
                    userId: Guid.NewGuid(),
                    ReleaseApprovalStatus.Approved
                );

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
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
        IUserPublicationRoleRepository? userPublicationRoleRepository = null
    )
    {
        return new ReleaseVersionRepository(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>()
        );
    }
}
