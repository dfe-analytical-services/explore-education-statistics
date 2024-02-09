#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseRepositoryTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task ListReleasesForUser_ReleaseRole_Approved()
        {
            var userId = Guid.NewGuid();
            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = userId,
                Release = new Release
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    ReleaseName = "2001",
                    Publication = new Publication
                    {
                        Title = "Test publication 1",
                        Slug = "test-publication-1"
                    },
                },
                Role = ReleaseRole.Lead,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                UserId = userId,
                Release = new Release
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    ReleaseName = "2001",
                    Publication = new Publication
                    {
                        Title = "Test publication 2",
                        Slug = "test-publication-2"
                    },
                },
                Role = ReleaseRole.PrereleaseViewer,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userReleaseRole1, userReleaseRole2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseRepository = BuildRepository(contentDbContext);
                var result =
                    await releaseRepository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);
                Assert.Single(result);
                Assert.Equal(userReleaseRole1.ReleaseId, result[0].Id);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_ReleaseRole_Draft()
        {
            var userId = Guid.NewGuid();
            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = userId,
                Release = new Release
                {
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    ReleaseName = "2001",
                    Publication = new Publication
                    {
                        Title = "Test publication 1",
                        Slug = "test-publication-1"
                    },
                },
                Role = ReleaseRole.Lead,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                UserId = userId,
                Release = new Release
                {
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    ReleaseName = "2001",
                    Publication = new Publication
                    {
                        Title = "Test publication 2",
                        Slug = "test-publication-2"
                    },
                },
                Role = ReleaseRole.PrereleaseViewer,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userReleaseRole1, userReleaseRole2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseRepository = BuildRepository(contentDbContext);
                var result =
                    await releaseRepository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_PublicationRole_Owner_Approved()
        {
            var userId = Guid.NewGuid();
            var userPublicationRole1 = new UserPublicationRole
            {
                UserId = userId,
                Publication = new Publication
                {
                    Title = "Test publication 1",
                    Slug = "test-publication-1",
                    Releases = new List<Release>
                    {
                        new()
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            TimePeriodCoverage = TimeIdentifier.AcademicYear,
                            ReleaseName = "2001",
                        },
                    }
                },
                Role = PublicationRole.Owner,
            };

            var otherPublication = new Publication
            {
                Title = "Test publication 2",
                Slug = "test-publication-2",
                Releases = new List<Release>
                {
                    new()
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        ReleaseName = "2001",
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userPublicationRole1, otherPublication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseRepository = BuildRepository(contentDbContext);
                var result =
                    await releaseRepository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);
                Assert.Single(result);
                Assert.Equal(userPublicationRole1.Publication.Releases[0].Id, result[0].Id);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_PublicationRole_Owner_Draft()
        {
            var userId = Guid.NewGuid();
            var userPublicationRole1 = new UserPublicationRole()
            {
                UserId = userId,
                Publication = new Publication
                {
                    Title = "Test publication 1",
                    Slug = "test-publication-1",
                    Releases = new List<Release>
                    {
                        new()
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Draft,
                            TimePeriodCoverage = TimeIdentifier.AcademicYear,
                            ReleaseName = "2001",
                        },
                    }
                },
                Role = PublicationRole.Owner,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userPublicationRole1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseRepository = BuildRepository(contentDbContext);
                var result =
                    await releaseRepository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_PublicationRole_Approver_Approved()
        {
            var userId = Guid.NewGuid();
            var userPublicationRole1 = new UserPublicationRole()
            {
                UserId = userId,
                Publication = new Publication
                {
                    Title = "Test publication 1",
                    Slug = "test-publication-1",
                    Releases = new List<Release>
                    {
                        new()
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            TimePeriodCoverage = TimeIdentifier.AcademicYear,
                            ReleaseName = "2001",
                        },
                    }
                },
                Role = PublicationRole.Approver,
            };

            var otherPublication = new Publication
            {
                Title = "Test publication 2",
                Slug = "test-publication-2",
                Releases = new List<Release>
                {
                    new()
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        ReleaseName = "2001",
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userPublicationRole1, otherPublication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseRepository = BuildRepository(contentDbContext);
                var result =
                    await releaseRepository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);

                var resultRelease = Assert.Single(result);
                Assert.Equal(userPublicationRole1.Publication.Releases[0].Id, resultRelease.Id);
            }
        }

        public class CreateStatisticsDbReleaseAndSubjectHierarchy : ReleaseRepositoryTests
        {
            [Fact]
            public async Task StatsReleaseDoesNotExist_CreatesStatsReleaseAndReleaseSubject()
            {
                Release release = _fixture
                    .DefaultRelease()
                    .WithPublication(_fixture
                        .DefaultPublication());

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.Releases.Add(release);
                    await contentDbContext.SaveChangesAsync();
                }

                Guid? createdSubjectId;
                var statisticsDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var releaseRepository = BuildRepository(contentDbContext, statisticsDbContext);
                    createdSubjectId = await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(release.Id);
                }

                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var statsRelease = Assert.Single(statisticsDbContext.Release);

                    Assert.Equal(release.Id, statsRelease.Id);
                    Assert.Equal(release.PublicationId, statsRelease.PublicationId);

                    var releaseSubject = Assert.Single(statisticsDbContext.ReleaseSubject);

                    Assert.Equal(release.Id, releaseSubject.ReleaseId);
                    Assert.Equal(createdSubjectId, releaseSubject.SubjectId);
                }
            }

            [Fact]
            public async Task StatsReleaseExists_CreatesReleaseSubject()
            {
                Release release = _fixture
                    .DefaultRelease()
                    .WithPublication(_fixture
                        .DefaultPublication());

                Data.Model.Release existingStatsRelease = _fixture
                    .DefaultStatsRelease()
                    .WithId(release.Id)
                    .WithPublicationId(release.PublicationId);

                ReleaseSubject existingReleaseSubject = _fixture
                    .DefaultReleaseSubject()
                    .WithRelease(existingStatsRelease)
                    .WithSubject(_fixture
                        .DefaultSubject());

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.Releases.Add(release);
                    await contentDbContext.SaveChangesAsync();
                }

                var statisticsDbContextId = Guid.NewGuid().ToString();
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    statisticsDbContext.Release.Add(existingStatsRelease);
                    statisticsDbContext.ReleaseSubject.Add(existingReleaseSubject);
                    await statisticsDbContext.SaveChangesAsync();
                }

                Guid? createdSubjectId;
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var releaseRepository = BuildRepository(contentDbContext, statisticsDbContext);
                    createdSubjectId = await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(release.Id);
                }

                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var statsRelease = Assert.Single(statisticsDbContext.Release);

                    Assert.Equal(release.Id, statsRelease.Id);
                    Assert.Equal(release.PublicationId, statsRelease.PublicationId);

                    Assert.Equal(2, await statisticsDbContext.ReleaseSubject.CountAsync());

                    Assert.True(await statisticsDbContext.ReleaseSubject
                        .AnyAsync(rs => rs.ReleaseId == release.Id
                                        && rs.SubjectId == existingReleaseSubject.SubjectId));

                    Assert.True(await statisticsDbContext.ReleaseSubject
                        .AnyAsync(rs => rs.ReleaseId == release.Id
                                        && rs.SubjectId == createdSubjectId));
                }
            }
        }

        private static ReleaseRepository BuildRepository(
            ContentDbContext? contentDbContext = null,
            StatisticsDbContext? statisticsDbContext = null
        )
        {
            return new ReleaseRepository(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>());
        }
    }
}
