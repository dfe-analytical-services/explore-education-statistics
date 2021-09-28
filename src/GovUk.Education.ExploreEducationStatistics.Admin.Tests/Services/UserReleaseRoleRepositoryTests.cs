#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task Create()
        {
            var user = new User();

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.Create(user.Id, release.Id, Contributor);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(release.Id, result.ReleaseId);
                Assert.Equal(Contributor, result.Role);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, userReleaseRoles[0].Id);
                Assert.Equal(user.Id, userReleaseRoles[0].UserId);
                Assert.Equal(release.Id, userReleaseRoles[0].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[0].Role);
            }
        }

        [Fact]
        public async Task GetAllRolesByUser()
        {
            var user = new User();
            var release = new Release();

            var userReleaseRoles = new List<UserReleaseRole>
            {
                new()
                {
                    User = user,
                    Release = release,
                    Role = Contributor
                },
                new()
                {
                    User = user,
                    Release = release,
                    Role = Lead
                }
            };

            var otherUserReleaseRoles = new List<UserReleaseRole>
            {
                // Role for different release
                new()
                {
                    User = user,
                    Release = new Release(),
                    Role = Approver
                },
                // Role for different user
                new()
                {
                    User = new User(),
                    Release = release,
                    Role = Approver
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(user);
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(userReleaseRoles);
                await contentDbContext.AddRangeAsync(otherUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.GetAllRolesByUser(user.Id, release.Id);

                Assert.Equal(2, result.Count);
                Assert.Equal(Contributor, result[0]);
                Assert.Equal(Lead, result[1]);
            }
        }

        [Fact]
        public async Task IsUserApproverOnLatestRelease_TrueIfUserHasRole()
        {
            var publication = new Publication();

            var olderRelease = new Release
            {
                ApprovalStatus = Approved,
                Publication = publication,
                ReleaseName = "2019",
                TimePeriodCoverage = CalendarYear
            };

            var latestPublishedRelease = new Release
            {
                Id = Guid.NewGuid(),
                ApprovalStatus = Approved,
                Publication = publication,
                Published = DateTime.UtcNow,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                Version = 0
            };

            var latestRelease = new Release
            {
                ApprovalStatus = Draft,
                Publication = publication,
                Published = null,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                PreviousVersionId = latestPublishedRelease.Id,
                Version = 1
            };

            // Assign the Approver role to the latest release
            var userReleaseRole = new UserReleaseRole
            {
                User = new User(),
                Release = latestRelease,
                Role = Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(
                    olderRelease,
                    latestPublishedRelease,
                    latestRelease);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.True(await service.IsUserApproverOnLatestRelease(
                    userReleaseRole.UserId,
                    publication.Id));
            }
        }

        [Fact]
        public async Task IsUserApproverOnLatestRelease_FalseWithoutRole()
        {
            var publication = new Publication();
            var user = new User();

            var olderRelease = new Release
            {
                ApprovalStatus = Approved,
                Publication = publication,
                ReleaseName = "2019",
                TimePeriodCoverage = CalendarYear
            };

            var latestPublishedRelease = new Release
            {
                Id = Guid.NewGuid(),
                ApprovalStatus = Approved,
                Publication = publication,
                Published = DateTime.UtcNow,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                Version = 0
            };

            var latestRelease = new Release
            {
                ApprovalStatus = Draft,
                Publication = publication,
                Published = null,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                PreviousVersionId = latestPublishedRelease.Id,
                Version = 1
            };

            var latestReleaseOtherPublication = new Release
            {
                Publication = new Publication(),
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear
            };

            // Assign the Approver role to all releases except the latest release
            var userReleaseRoles = ListOf(
                    olderRelease, latestPublishedRelease, latestReleaseOtherPublication)
                .Select(release => new UserReleaseRole
                {
                    User = user,
                    Release = release,
                    Role = Approver
                })
                .ToList();

            // Also assign a different role to the latest release to check it has no influence
            userReleaseRoles.Add(new UserReleaseRole
            {
                User = user,
                Release = latestRelease,
                Role = Lead
            });

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(
                    olderRelease,
                    latestPublishedRelease,
                    latestRelease,
                    latestReleaseOtherPublication);
                await contentDbContext.Users.AddRangeAsync(user);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.False(await service.IsUserApproverOnLatestRelease(
                    user.Id,
                    publication.Id));
            }
        }

        [Fact]
        public async Task IsUserEditorOrApproverOnLatestRelease_TrueIfUserHasRole()
        {
            var publication = new Publication();

            var olderRelease = new Release
            {
                ApprovalStatus = Approved,
                Publication = publication,
                ReleaseName = "2019",
                TimePeriodCoverage = CalendarYear
            };

            var latestPublishedRelease = new Release
            {
                Id = Guid.NewGuid(),
                ApprovalStatus = Approved,
                Publication = publication,
                Published = DateTime.UtcNow,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                Version = 0
            };

            var latestRelease = new Release
            {
                ApprovalStatus = Draft,
                Publication = publication,
                Published = null,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                PreviousVersionId = latestPublishedRelease.Id,
                Version = 1
            };

            await ListOf(Approver, Contributor, Lead).ForEachAsync(async role =>
            {
                // Assign the role to the latest release
                var userReleaseRole = new UserReleaseRole
                {
                    User = new User(),
                    Release = latestRelease,
                    Role = role
                };

                var contentDbContextId = Guid.NewGuid().ToString();

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.Releases.AddRangeAsync(
                        olderRelease,
                        latestPublishedRelease,
                        latestRelease);
                    await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupUserReleaseRoleRepository(contentDbContext);

                    Assert.True(await service.IsUserEditorOrApproverOnLatestRelease(
                        userReleaseRole.UserId,
                        publication.Id));
                }
            });
        }

        [Fact]
        public async Task IsUserEditorOrApproverOnLatestRelease_FalseWithoutRole()
        {
            var publication = new Publication();
            var user = new User();

            var olderRelease = new Release
            {
                ApprovalStatus = Approved,
                Publication = publication,
                ReleaseName = "2019",
                TimePeriodCoverage = CalendarYear
            };

            var latestPublishedRelease = new Release
            {
                Id = Guid.NewGuid(),
                ApprovalStatus = Approved,
                Publication = publication,
                Published = DateTime.UtcNow,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                Version = 0
            };

            var latestRelease = new Release
            {
                ApprovalStatus = Draft,
                Publication = publication,
                Published = null,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                PreviousVersionId = latestPublishedRelease.Id,
                Version = 1
            };

            var latestReleaseOtherPublication = new Release
            {
                Publication = new Publication(),
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear
            };

            // Assign the Contributor role to all releases except the latest release
            var userReleaseRoles = ListOf(
                    olderRelease, latestPublishedRelease, latestReleaseOtherPublication)
                .Select(release => new UserReleaseRole
                {
                    User = user,
                    Release = release,
                    Role = Contributor
                })
                .ToList();

            // Also assign a different role to the latest release to check it has no influence
            userReleaseRoles.Add(new UserReleaseRole
            {
                User = user,
                Release = latestRelease,
                Role = Viewer
            });

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(
                    olderRelease,
                    latestPublishedRelease,
                    latestRelease,
                    latestReleaseOtherPublication);
                await contentDbContext.Users.AddRangeAsync(user);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.False(await service.IsUserEditorOrApproverOnLatestRelease(
                    user.Id,
                    publication.Id));
            }
        }

        [Fact]
        public async Task UserHasRoleOnRelease_TrueIfRoleExists()
        {
            var userReleaseRole = new UserReleaseRole
            {
                User = new User(),
                Release = new Release(),
                Role = Contributor
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.True(await service.UserHasRoleOnRelease(
                    userReleaseRole.UserId,
                    userReleaseRole.ReleaseId,
                    Contributor));
            }
        }

        [Fact]
        public async Task UserHasRoleOnRelease_FalseIfRoleDoesNotExist()
        {
            var user = new User();
            var release = new Release();

            // Setup a role but for a different release to make sure it has no influence
            var userReleaseRoleOtherRelease = new UserReleaseRole
            {
                User = user,
                Release = new Release(),
                Role = Contributor
            };

            // Setup a different role on the release to make sure it has no influence
            var userReleaseRoleDifferentRole = new UserReleaseRole
            {
                User = user,
                Release = release,
                Role = Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(
                    userReleaseRoleOtherRelease,
                    userReleaseRoleDifferentRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.False(await service.UserHasRoleOnRelease(
                    user.Id,
                    release.Id,
                    Contributor));
            }
        }

        [Fact]
        public async Task UserHasAnyOfRolesOnLatestRelease_TrueIfUserHasRoleOnLatestRelease()
        {
            var publication = new Publication();

            var olderRelease = new Release
            {
                ApprovalStatus = Approved,
                Publication = publication,
                ReleaseName = "2019",
                TimePeriodCoverage = CalendarYear
            };

            var latestPublishedRelease = new Release
            {
                Id = Guid.NewGuid(),
                ApprovalStatus = Approved,
                Publication = publication,
                Published = DateTime.UtcNow,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                Version = 0
            };

            var latestRelease = new Release
            {
                ApprovalStatus = Draft,
                Publication = publication,
                Published = null,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                PreviousVersionId = latestPublishedRelease.Id,
                Version = 1
            };

            // Assign one of the user roles being tested on the latest release
            var userReleaseRole = new UserReleaseRole
            {
                User = new User(),
                Release = latestRelease,
                Role = Approver
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(
                    olderRelease,
                    latestPublishedRelease,
                    latestRelease);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.True(await service.UserHasAnyOfRolesOnLatestRelease(
                    userReleaseRole.UserId,
                    publication.Id,
                    ListOf(Contributor, Approver)));
            }
        }

        [Fact]
        public async Task UserHasAnyOfRolesOnLatestRelease_PublicationHasNoReleases()
        {
            var publication = new Publication();

            // Assign a user role but to a release not connected with the publication
            var userReleaseRole = new UserReleaseRole
            {
                User = new User(),
                Release = new Release(),
                Role = Contributor
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.False(await service.UserHasAnyOfRolesOnLatestRelease(
                    userReleaseRole.UserId,
                    publication.Id,
                    ListOf(Contributor)));
            }
        }

        [Fact]
        public async Task UserHasAnyOfRolesOnLatestRelease_FalseIfUserHasNoRoleOnLatestRelease()
        {
            var publication = new Publication();
            var user = new User();

            var olderRelease = new Release
            {
                ApprovalStatus = Approved,
                Publication = publication,
                ReleaseName = "2019",
                TimePeriodCoverage = CalendarYear
            };

            var latestPublishedRelease = new Release
            {
                Id = Guid.NewGuid(),
                ApprovalStatus = Approved,
                Publication = publication,
                Published = DateTime.UtcNow,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                Version = 0
            };

            var latestRelease = new Release
            {
                ApprovalStatus = Draft,
                Publication = publication,
                Published = null,
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear,
                PreviousVersionId = latestPublishedRelease.Id,
                Version = 1
            };

            var latestReleaseOtherPublication = new Release
            {
                Publication = new Publication(),
                ReleaseName = "2020",
                TimePeriodCoverage = CalendarYear
            };

            // Assign the user role being tested to all releases except the latest release
            var userReleaseRoles = ListOf(
                    olderRelease, latestPublishedRelease, latestReleaseOtherPublication)
                .Select(release => new UserReleaseRole
                {
                    User = user,
                    Release = release,
                    Role = Contributor
                })
                .ToList();

            // Also assign a different role to the latest release to check it has no influence
            userReleaseRoles.Add(new UserReleaseRole
            {
                User = user,
                Release = latestRelease,
                Role = Approver
            });

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(
                    olderRelease,
                    latestPublishedRelease,
                    latestRelease,
                    latestReleaseOtherPublication);
                await contentDbContext.Users.AddRangeAsync(user);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                Assert.False(await service.UserHasAnyOfRolesOnLatestRelease(
                    user.Id,
                    publication.Id,
                    ListOf(Contributor)));
            }
        }

        private static UserReleaseRoleRepository SetupUserReleaseRoleRepository(
            ContentDbContext contentDbContext)
        {
            return new(contentDbContext);
        }
    }
}
