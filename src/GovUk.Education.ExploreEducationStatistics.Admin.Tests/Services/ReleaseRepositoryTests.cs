#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseRepositoryTests
    {
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
                        Slug = "test-publication-1",
                        Contact = new Contact(),
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
                        Slug = "test-publication-2",
                        Contact = new Contact(),
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
                var releaseRepository = BuildReleaseRepository(contentDbContext);
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
                        Slug = "test-publication-1",
                        Contact = new Contact(),
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
                        Slug = "test-publication-2",
                        Contact = new Contact(),
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
                var releaseRepository = BuildReleaseRepository(contentDbContext);
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
            var userPublicationRole1 = new UserPublicationRole()
            {
                UserId = userId,
                Publication = new Publication
                {
                    Title = "Test publication 1",
                    Slug = "test-publication-1",
                    Contact = new Contact(),
                    Releases = new List<Release>
                    {
                        new Release
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
                Contact = new Contact(),
                Releases = new List<Release>
                {
                    new Release
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
                var releaseRepository = BuildReleaseRepository(contentDbContext);
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
                    Contact = new Contact(),
                    Releases = new List<Release>
                    {
                        new Release
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
                var releaseRepository = BuildReleaseRepository(contentDbContext);
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
                    Contact = new Contact(),
                    Releases = new List<Release>
                    {
                        new Release
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
                Contact = new Contact(),
                Releases = new List<Release>
                {
                    new Release
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
                var releaseRepository = BuildReleaseRepository(contentDbContext);
                var result =
                    await releaseRepository.ListReleasesForUser(userId,
                        ReleaseApprovalStatus.Approved);

                var resultRelease = Assert.Single(result);
                Assert.Equal(userPublicationRole1.Publication.Releases[0].Id, resultRelease.Id);
            }
        }

        [Fact]
        public async Task ReleaseHierarchyCreated()
        {
            var release = new Release
            {
                Published = DateTime.Now,
                PreviousVersionId = Guid.NewGuid(),
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Title = "Test theme"
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseRepository = BuildReleaseRepository(contentDbContext, statisticsDbContext);
                await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(
                    release.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statsRelease = await statisticsDbContext.Release.FindAsync(release.Id);
                Assert.NotNull(statsRelease);
                Assert.Equal(release.Id, statsRelease!.Id);
                Assert.Equal(release.PublicationId, statsRelease!.PublicationId);
            }
            
        }
        
        [Fact]
        public async Task ReleaseHierarchyUpdated()
        {
            var release = new Release
            {
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week5,
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Title = "Test theme"
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(new Data.Model.Release
                {
                    Id = release.Id,
                    PublicationId = release.Publication.Id
                });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseRepository = BuildReleaseRepository(contentDbContext, statisticsDbContext);
                await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(
                    release.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statsRelease = await statisticsDbContext.Release.FindAsync(release.Id);
                Assert.NotNull(statsRelease);
                Assert.Equal(release.Id, statsRelease!.Id);
                Assert.Equal(release.PublicationId, statsRelease!.PublicationId);
            }
        }
        
        [Fact]
        public async Task SubjectHierarchyCreated()
        {
            var release = new Release
            {
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.April,
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Title = "Test theme"
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseRepository = BuildReleaseRepository(contentDbContext, statisticsDbContext);
                await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(
                    release.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var subjects = statisticsDbContext.Subject.ToList();
                Assert.Single(subjects);

                var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
                Assert.Single(releaseSubjects);
                Assert.Equal(subjects[0].Id, releaseSubjects[0].SubjectId);
                Assert.Equal(release.Id, releaseSubjects[0].ReleaseId);
            }
        }

        [Fact]
        public async Task GetAllReleaseVersionIds()
        {
            var originalRelease = new Release();

            var amendedRelease1 = new Release
            {
                PreviousVersion = originalRelease,
            };

            var amendedRelease2 = new Release
            {
                PreviousVersion = amendedRelease1,
            };

            var ignoredRelease = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    originalRelease, amendedRelease1, amendedRelease2, ignoredRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseRepository = BuildReleaseRepository(contentDbContext: contentDbContext);
                var result = await releaseRepository.GetAllReleaseVersionIds(amendedRelease2);
                Assert.Equal(3, result.Count);

                Assert.Contains(originalRelease.Id, result);
                Assert.Contains(amendedRelease1.Id, result);
                Assert.Contains(amendedRelease2.Id, result);
            }
        }

        [Fact]
        public async Task GetAllReleaseVersionIds_SingleResult()
        {
            var originalRelease = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(originalRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseRepository = BuildReleaseRepository(contentDbContext: contentDbContext);
                var result = await releaseRepository.GetAllReleaseVersionIds(originalRelease);
                Assert.Single(result);

                Assert.Contains(originalRelease.Id, result);
            }
        }

        private ReleaseRepository BuildReleaseRepository(
            ContentDbContext? contentDbContext = null,
            StatisticsDbContext? statisticsDbContext = null
        )
        {
            return new ReleaseRepository(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                AdminMapper());
        }
    }
}
