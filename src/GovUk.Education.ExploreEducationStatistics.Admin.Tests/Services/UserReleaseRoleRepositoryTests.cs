#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

            var createdByUser = new User();

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(user, createdByUser, release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.Create(user.Id, release.Id, Contributor, createdByUser.Id);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(release.Id, result.ReleaseId);
                Assert.Equal(Contributor, result.Role);
                Assert.Equal(createdByUser.Id, result.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created!.Value).Milliseconds, 0, 1500);
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
                Assert.Equal(createdByUser.Id, userReleaseRoles[0].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[0].Created!.Value).Milliseconds,
                    0, 1500);
            }
        }

        [Fact]
        public async Task CreateAll()
        {
            var release = new Release();

            var user1 = new User();
            var userReleaseRole1 = new UserReleaseRole
            {
                Release = release,
                User = user1,
                Role = Contributor,
                CreatedById = Guid.NewGuid(),
                Created = new DateTime(2000, 12, 25),
            };

            var user2 = new User();
            var user3 = new User();
            var user4 = new User();

            var createdByUser = new User();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    userReleaseRole1, user2, user3, user4, createdByUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.CreateAll(new List<Guid>{user1.Id, user2.Id, user3.Id, user4.Id},
                    release.Id, Contributor, createdByUser.Id);

                Assert.Equal(Unit.Instance, result);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(4, userReleaseRoles.Count);

                Assert.Equal(user1.Id, userReleaseRoles[0].UserId);
                Assert.Equal(release.Id, userReleaseRoles[0].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[0].Role);
                Assert.Equal(userReleaseRole1.CreatedById, userReleaseRoles[0].CreatedById);
                Assert.Equal(userReleaseRole1.Created, userReleaseRoles[0].Created);

                Assert.Equal(user2.Id, userReleaseRoles[1].UserId);
                Assert.Equal(release.Id, userReleaseRoles[1].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[1].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[1].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Created!.Value).Milliseconds,
                    0, 1500);

                Assert.Equal(user3.Id, userReleaseRoles[2].UserId);
                Assert.Equal(release.Id, userReleaseRoles[2].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[2].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[2].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[2].Created!.Value).Milliseconds,
                    0, 1500);

                Assert.Equal(user4.Id, userReleaseRoles[3].UserId);
                Assert.Equal(release.Id, userReleaseRoles[3].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[3].Role);
                Assert.Equal(createdByUser.Id, userReleaseRoles[3].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[3].Created!.Value).Milliseconds,
                    0, 1500);
            }
        }

        [Fact]
        public async Task CreateAll_NoUsersToAdd()
        {
            var release = new Release();
            var createdByUser = new User();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, createdByUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserReleaseRoleRepository(contentDbContext);

                var result = await service.CreateAll(new List<Guid>(),
                    release.Id, Contributor, createdByUser.Id);
                Assert.Equal(Unit.Instance, result);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);
            }
        }

        [Fact]
        public async Task Remove()
        {
            var deletedById = Guid.NewGuid();
            var userReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Lead,
                Deleted = null,
                DeletedById = null,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = new UserReleaseRoleRepository(contentDbContext);
                await service.Remove(userReleaseRole, deletedById);

                var updatedReleaseRole = contentDbContext.UserReleaseRoles
                    .SingleOrDefault(urr => urr.Id == userReleaseRole.Id);

                Assert.Null(updatedReleaseRole);
            }
        }

        [Fact]
        public async Task Remove_IgnoreQueryFilters()
        {
            var deletedById = Guid.NewGuid();
            var userReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Lead,
                Deleted = null,
                DeletedById = null,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = new UserReleaseRoleRepository(contentDbContext);
                await service.Remove(userReleaseRole, deletedById);

                var updatedReleaseRole = contentDbContext.UserReleaseRoles
                    .IgnoreQueryFilters()
                    .SingleOrDefault(urr => urr.Id == userReleaseRole.Id);

                Assert.NotNull(updatedReleaseRole);
                Assert.Equal(userReleaseRole.ReleaseId, updatedReleaseRole!.ReleaseId);
                Assert.Equal(userReleaseRole.Role, updatedReleaseRole.Role);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedReleaseRole.Deleted!.Value).Milliseconds, 0, 1500);
                Assert.Equal(deletedById, updatedReleaseRole.DeletedById);
            }
        }

        [Fact]
        public async Task RemoveAll()
        {
            var deletedById = Guid.NewGuid();
            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Lead,
                Deleted = null,
                DeletedById = null,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
                Deleted = null,
                DeletedById = null,
            };
            var userReleaseRole3 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = PrereleaseViewer,
                Deleted = null,
                DeletedById = null,
            };
            var notDeletedUserReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Approver,
                Deleted = null,
                DeletedById = null,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userReleaseRole1, userReleaseRole2, userReleaseRole3,
                    notDeletedUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = new UserReleaseRoleRepository(contentDbContext);
                await service.RemoveAll(
                    new List<UserReleaseRole>{userReleaseRole1, userReleaseRole2, userReleaseRole3},
                    deletedById);

                var userReleaseRoles = contentDbContext.UserReleaseRoles
                    .ToList();
                Assert.Single(userReleaseRoles);
                Assert.Equal(notDeletedUserReleaseRole.Id, userReleaseRoles[0].Id);
            }
        }

        [Fact]
        public async Task RemoveAll_IgnoreQueryFilters()
        {
            var deletedById = Guid.NewGuid();
            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Lead,
                Deleted = null,
                DeletedById = null,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
                Deleted = null,
                DeletedById = null,
            };
            var userReleaseRole3 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = PrereleaseViewer,
                Deleted = null,
                DeletedById = null,
            };
            var notDeletedUserReleaseRole = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                Role = Approver,
                Deleted = null,
                DeletedById = null,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userReleaseRole1, userReleaseRole2, userReleaseRole3,
                    notDeletedUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = new UserReleaseRoleRepository(contentDbContext);
                await service.RemoveAll(
                    new List<UserReleaseRole>{userReleaseRole1, userReleaseRole2, userReleaseRole3},
                    deletedById);

                var userReleaseRoles = contentDbContext.UserReleaseRoles
                    .IgnoreQueryFilters()
                    .ToList();
                Assert.Equal(4, userReleaseRoles.Count);

                Assert.Equal(userReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal(userReleaseRole1.ReleaseId, userReleaseRoles[0].ReleaseId);
                Assert.Equal(userReleaseRole1.Role, userReleaseRoles[0].Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[0].Deleted!.Value).Milliseconds, 0, 1500);
                Assert.Equal(deletedById, userReleaseRoles[0].DeletedById);

                Assert.Equal(userReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal(userReleaseRole2.ReleaseId, userReleaseRoles[1].ReleaseId);
                Assert.Equal(userReleaseRole2.Role, userReleaseRoles[1].Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Deleted!.Value).Milliseconds, 0, 1500);
                Assert.Equal(deletedById, userReleaseRoles[1].DeletedById);

                Assert.Equal(userReleaseRole3.Id, userReleaseRoles[2].Id);
                Assert.Equal(userReleaseRole3.ReleaseId, userReleaseRoles[2].ReleaseId);
                Assert.Equal(userReleaseRole3.Role, userReleaseRoles[2].Role);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[2].Deleted!.Value).Milliseconds, 0, 1500);
                Assert.Equal(deletedById, userReleaseRoles[2].DeletedById);

                Assert.Equal(notDeletedUserReleaseRole.Id, userReleaseRoles[3].Id);
                Assert.Equal(notDeletedUserReleaseRole.ReleaseId, userReleaseRoles[3].ReleaseId);
                Assert.Equal(notDeletedUserReleaseRole.Role, userReleaseRoles[3].Role);
                Assert.Null(userReleaseRoles[3].Deleted);
                Assert.Null(userReleaseRoles[3].DeletedById);
            }
        }

        [Fact]
        public async Task RemoveAllUserReleaseRolesForPublication()
        {
            var user = new User();
            var deletedById = Guid.NewGuid();

            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    new() { Id = Guid.NewGuid(), },
                    new() { Id = Guid.NewGuid(), },
                    new() { Id = Guid.NewGuid(), },
                }
            };
            var publication2 = new Publication
            {
                Releases = new List<Release>
                {
                    new() { Id = Guid.NewGuid(), }
                }
            };

            var userReleaseRole1 = new UserReleaseRole
            {
                User = user,
                Release = publication.Releases[0],
                Role = Contributor,
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                User = user,
                Release = publication.Releases[2],
                Role = Contributor,
            };
            var notDeletedUserReleaseRole1 = new UserReleaseRole
            {
                User = user,
                Release = publication.Releases[0],
                Role = PrereleaseViewer,
            };
            var notDeletedUserReleaseRole2 = new UserReleaseRole
            {
                User = user,
                Release = publication2.Releases[0],
                Role = Contributor,
            };
            var notDeletedUserReleaseRole3 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                Release = publication.Releases[0],
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication, publication2,
                    userReleaseRole1, userReleaseRole2,
                    notDeletedUserReleaseRole1, notDeletedUserReleaseRole2, notDeletedUserReleaseRole3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = new UserReleaseRoleRepository(contentDbContext);
                await service.RemoveAllUserReleaseRolesForPublication(user.Id, publication, Contributor,
                    deletedById);

                var userReleaseRoles = contentDbContext.UserReleaseRoles
                    .ToList();
                Assert.Equal(3, userReleaseRoles.Count);

                Assert.Equal(notDeletedUserReleaseRole1.Id, userReleaseRoles[0].Id);
                Assert.Equal(notDeletedUserReleaseRole2.Id, userReleaseRoles[1].Id);
                Assert.Equal(notDeletedUserReleaseRole3.Id, userReleaseRoles[2].Id);
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
