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
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseVersionRepositoryTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task ListReleasesForUser_ReleaseRole_Approved()
        {
            var userId = Guid.NewGuid();
            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = userId,
                ReleaseVersion = new ReleaseVersion
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
                ReleaseVersion = new ReleaseVersion
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
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);
                Assert.Single(result);
                Assert.Equal(userReleaseRole1.ReleaseVersionId, result[0].Id);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_ReleaseRole_Draft()
        {
            var userId = Guid.NewGuid();
            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = userId,
                ReleaseVersion = new ReleaseVersion
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
                ReleaseVersion = new ReleaseVersion
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
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId,
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
                    ReleaseVersions = new List<ReleaseVersion>
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
                ReleaseVersions = new List<ReleaseVersion>
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
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);
                Assert.Single(result);
                Assert.Equal(userPublicationRole1.Publication.ReleaseVersions[0].Id, result[0].Id);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_PublicationRole_Owner_Draft()
        {
            var userId = Guid.NewGuid();
            var userPublicationRole1 = new UserPublicationRole
            {
                UserId = userId,
                Publication = new Publication
                {
                    Title = "Test publication 1",
                    Slug = "test-publication-1",
                    ReleaseVersions = new List<ReleaseVersion>
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
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListReleasesForUser_PublicationRole_Approver_Approved()
        {
            var userId = Guid.NewGuid();
            var userPublicationRole1 = new UserPublicationRole
            {
                UserId = userId,
                Publication = new Publication
                {
                    Title = "Test publication 1",
                    Slug = "test-publication-1",
                    ReleaseVersions = new List<ReleaseVersion>
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
                ReleaseVersions = new List<ReleaseVersion>
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
                var repository = BuildRepository(contentDbContext);
                var result = await repository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);

                var resultRelease = Assert.Single(result);
                Assert.Equal(userPublicationRole1.Publication.ReleaseVersions[0].Id, resultRelease.Id);
            }
        }

        public class CreateStatisticsDbReleaseVersionAndSubjectHierarchy : ReleaseVersionRepositoryTests
        {
            [Fact]
            public async Task StatsReleaseDoesNotExist_CreatesStatsReleaseAndReleaseSubject()
            {
                ReleaseVersion releaseVersion = _fixture
                    .DefaultReleaseVersion()
                    .WithPublication(_fixture
                        .DefaultPublication());

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
                    .WithPublication(_fixture
                        .DefaultPublication());

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
}
