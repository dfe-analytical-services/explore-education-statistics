#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserReleaseRoleRepositoryTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Create()
    {
        var user = new User();

        var createdByUser = new User();

        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Users.AddRange(user, createdByUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            var result = await service.Create(userId: user.Id,
                resourceId: releaseVersion.Id,
                ReleaseRole.Contributor,
                createdById: createdByUser.Id);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, result.Role);
            Assert.Equal(createdByUser.Id, result.CreatedById);
            result.Created.AssertUtcNow();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseRole = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .SingleAsync();

            Assert.NotEqual(Guid.Empty, userReleaseRole.Id);
            Assert.Equal(user.Id, userReleaseRole.UserId);
            Assert.Equal(releaseVersion.Id, userReleaseRole.ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRole.Role);
            Assert.Equal(createdByUser.Id, userReleaseRole.CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRole.Created!.Value).Milliseconds,
                0, 1500);
        }
    }

    [Fact]
    public async Task CreateIfNotExists_DoesNotExist()
    {
        var userId = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            var result = await service.CreateIfNotExists(userId: userId,
                resourceId: releaseVersionId,
                ReleaseRole.Contributor,
                createdById: createdById);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(releaseVersionId, result.ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, result.Role);
            Assert.Equal(createdById, result.CreatedById);
            result.Created.AssertUtcNow();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseRole = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .SingleAsync();

            Assert.NotEqual(Guid.Empty, userReleaseRole.Id);
            Assert.Equal(userId, userReleaseRole.UserId);
            Assert.Equal(releaseVersionId, userReleaseRole.ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRole.Role);
            Assert.Equal(createdById, userReleaseRole.CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRole.Created!.Value).Milliseconds,
                0, 1500);
        }
    }

    [Fact]
    public async Task CreateIfNotExists_AlreadyExists()
    {
        var userReleaseRole = new UserReleaseRole
        {
            UserId = Guid.NewGuid(),
            ReleaseVersionId = Guid.NewGuid(),
            Role = ReleaseRole.Approver,
            CreatedById = Guid.NewGuid(),
            Created = new DateTime(2021, 12, 25),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(userReleaseRole);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            var result = await service.CreateIfNotExists(userId: userReleaseRole.UserId,
                resourceId: userReleaseRole.ReleaseVersionId,
                userReleaseRole.Role,
                createdById: userReleaseRole.CreatedById.Value);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(userReleaseRole.UserId, result.UserId);
            Assert.Equal(userReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
            Assert.Equal(userReleaseRole.Role, result.Role);
            Assert.Equal(userReleaseRole.CreatedById, result.CreatedById);
            Assert.Equal(userReleaseRole.Created, result.Created);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbUserReleaseRole = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .SingleAsync();

            Assert.NotEqual(Guid.Empty, dbUserReleaseRole.Id);
            Assert.Equal(dbUserReleaseRole.UserId, dbUserReleaseRole.UserId);
            Assert.Equal(dbUserReleaseRole.ReleaseVersionId, dbUserReleaseRole.ReleaseVersionId);
            Assert.Equal(dbUserReleaseRole.Role, dbUserReleaseRole.Role);
            Assert.Equal(dbUserReleaseRole.CreatedById, dbUserReleaseRole.CreatedById);
            Assert.Equal(dbUserReleaseRole.Created, dbUserReleaseRole.Created);
        }
    }

    [Fact]
    public async Task CreateManyIfNotExists_Users()
    {
        var releaseVersion = new ReleaseVersion();

        var user1 = new User();
        var user1ReleaseRole1 = new UserReleaseRole
        {
            ReleaseVersion = releaseVersion,
            User = user1,
            Role = ReleaseRole.Contributor,
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
                user1ReleaseRole1, user2, user3, user4, createdByUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            await service.CreateManyIfNotExists(
                userIds: ListOf(user1.Id, user2.Id, user3.Id, user4.Id),
                resourceId: releaseVersion.Id,
                role: ReleaseRole.Contributor,
                createdById: createdByUser.Id);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseRoles = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .ToListAsync();
            Assert.Equal(4, userReleaseRoles.Count);

            Assert.Equal(user1.Id, userReleaseRoles[0].UserId);
            Assert.Equal(releaseVersion.Id, userReleaseRoles[0].ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[0].Role);
            Assert.Equal(user1ReleaseRole1.CreatedById, userReleaseRoles[0].CreatedById);
            Assert.Equal(user1ReleaseRole1.Created, userReleaseRoles[0].Created);

            Assert.Equal(user2.Id, userReleaseRoles[1].UserId);
            Assert.Equal(releaseVersion.Id, userReleaseRoles[1].ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[1].Role);
            Assert.Equal(createdByUser.Id, userReleaseRoles[1].CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Created!.Value).Milliseconds,
                0, 1500);

            Assert.Equal(user3.Id, userReleaseRoles[2].UserId);
            Assert.Equal(releaseVersion.Id, userReleaseRoles[2].ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[2].Role);
            Assert.Equal(createdByUser.Id, userReleaseRoles[2].CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[2].Created!.Value).Milliseconds,
                0, 1500);

            Assert.Equal(user4.Id, userReleaseRoles[3].UserId);
            Assert.Equal(releaseVersion.Id, userReleaseRoles[3].ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[3].Role);
            Assert.Equal(createdByUser.Id, userReleaseRoles[3].CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[3].Created!.Value).Milliseconds,
                0, 1500);
        }
    }

    [Fact]
    public async Task CreateManyIfNotExists_Releases()
    {
        var release1 = new ReleaseVersion();

        var user = new User();
        var createdByUser = new User();

        var userRelease1Role = new UserReleaseRole
        {
            ReleaseVersion = release1,
            User = user,
            Role = ReleaseRole.Contributor,
            CreatedBy = createdByUser,
            Created = new DateTime(2000, 12, 25),
        };
        var release2 = new ReleaseVersion();
        var release3 = new ReleaseVersion();
        var release4 = new ReleaseVersion();


        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(release1, release2, release3, release4);
            contentDbContext.UserReleaseRoles.Add(userRelease1Role);
            contentDbContext.Users.Add(createdByUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            await service.CreateManyIfNotExists(
                userId: user.Id,
                resourceIds: ListOf(release1.Id, release2.Id, release3.Id, release4.Id),
                role: ReleaseRole.Contributor,
                createdById: createdByUser.Id);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseRoles = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .ToListAsync();
            Assert.Equal(4, userReleaseRoles.Count);

            Assert.Equal(userRelease1Role.UserId, userReleaseRoles[0].UserId);
            Assert.Equal(userRelease1Role.ReleaseVersionId, userReleaseRoles[0].ReleaseVersionId);
            Assert.Equal(userRelease1Role.Role, userReleaseRoles[0].Role);
            Assert.Equal(userRelease1Role.CreatedById, userReleaseRoles[0].CreatedById);
            Assert.Equal(userRelease1Role.Created, userReleaseRoles[0].Created);

            Assert.Equal(user.Id, userReleaseRoles[1].UserId);
            Assert.Equal(release2.Id, userReleaseRoles[1].ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[1].Role);
            Assert.Equal(createdByUser.Id, userReleaseRoles[1].CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Created!.Value).Milliseconds,
                0, 1500);

            Assert.Equal(user.Id, userReleaseRoles[2].UserId);
            Assert.Equal(release3.Id, userReleaseRoles[2].ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[2].Role);
            Assert.Equal(createdByUser.Id, userReleaseRoles[2].CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[2].Created!.Value).Milliseconds,
                0, 1500);

            Assert.Equal(user.Id, userReleaseRoles[3].UserId);
            Assert.Equal(release4.Id, userReleaseRoles[3].ReleaseVersionId);
            Assert.Equal(ReleaseRole.Contributor, userReleaseRoles[3].Role);
            Assert.Equal(createdByUser.Id, userReleaseRoles[3].CreatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[3].Created!.Value).Milliseconds,
                0, 1500);
        }
    }

    [Fact]
    public async Task CreateMany_NoUsersToAdd()
    {
        var releaseVersion = new ReleaseVersion();
        var createdByUser = new User();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Users.Add(createdByUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            await service.CreateManyIfNotExists(new List<Guid>(),
                releaseVersion.Id, ReleaseRole.Contributor, createdByUser.Id);
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
    public async Task RemoveRoleAndInvite()
    {
        var userReleaseRole = _fixture.DefaultUserReleaseRole()
            .WithUser(new User())
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.Approver)
            .Generate();

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.Add(userReleaseRole);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserReleaseRoleRepository(contentDbContext);
        await service.RemoveRoleAndInvite(userReleaseRole);

        var updatedReleaseRole = contentDbContext.UserReleaseRoles
            .SingleOrDefault(urr => urr.Id == userReleaseRole.Id);

        Assert.Null(updatedReleaseRole);
    }

    [Fact]
    public async Task RemoveRoleAndInvite_IgnoreQueryFilters()
    {
        var userReleaseRole = _fixture.DefaultUserReleaseRole()
            .WithUser(new User())
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.Approver)
            .Generate();

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.Add(userReleaseRole);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserReleaseRoleRepository(contentDbContext);
        await service.RemoveRoleAndInvite(userReleaseRole);

        var updatedReleaseRole = contentDbContext.UserReleaseRoles
            .IgnoreQueryFilters()
            .SingleOrDefault(urr => urr.Id == userReleaseRole.Id);

        Assert.Null(updatedReleaseRole);
    }

    [Fact]
    public async Task RemoveManyRolesAndInvites()
    {
        var userReleaseRole1 = _fixture.DefaultUserReleaseRole()
            .WithUser(new User { Email = "test1@test.com" })
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.Contributor)
            .Generate();

        var userReleaseRole2 = _fixture.DefaultUserReleaseRole()
            .WithUser(new User { Email = "test2@test.com" })
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.PrereleaseViewer)
            .Generate();

        var userReleaseRoleToRemain = _fixture.DefaultUserReleaseRole()
            .WithUser(new User { Email = "test3@test.com" })
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.Approver)
            .Generate();

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.AddRange(userReleaseRole1, userReleaseRole2, userReleaseRoleToRemain);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserReleaseRoleRepository(contentDbContext);
        await service.RemoveManyRolesAndInvites([userReleaseRole1, userReleaseRole2]);

        var userReleaseRole = await contentDbContext.UserReleaseRoles
            .SingleAsync();

        Assert.Equal(userReleaseRoleToRemain.Id, userReleaseRole.Id);
    }

    [Fact]
    public async Task RemoveManyRolesAndInvites_IgnoreQueryFilters()
    {
        var userReleaseRole1 = _fixture.DefaultUserReleaseRole()
            .WithUser(new User{ Email = "test1@test.com" })
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.Contributor)
            .Generate();

        var userReleaseRole2 = _fixture.DefaultUserReleaseRole()
            .WithUser(new User{ Email = "test2@test.com" })
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.PrereleaseViewer)
            .Generate();

        var userReleaseRoleToRemain = _fixture.DefaultUserReleaseRole()
            .WithUser(new User{ Email = "test3@test.com" })
            .WithReleaseVersion(_fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication())))
            .WithRole(ReleaseRole.Approver)
            .Generate();

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.AddRange(userReleaseRole1, userReleaseRole2, userReleaseRoleToRemain);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserReleaseRoleRepository(contentDbContext);
        await service.RemoveManyRolesAndInvites([userReleaseRole1, userReleaseRole2]);

        var userReleaseRole = await contentDbContext.UserReleaseRoles
            .SingleAsync();

        Assert.Equal(userReleaseRoleToRemain.Id, userReleaseRole.Id);
        Assert.Equal(userReleaseRoleToRemain.ReleaseVersionId, userReleaseRole.ReleaseVersionId);
        Assert.Equal(userReleaseRoleToRemain.Role, userReleaseRole.Role);
    }

    [Fact]
    public async Task RemoveAllRolesAndInvitesForPublication()
    {
        var targetUser = new User();

        var targetPublication = _fixture.DefaultPublication()
            .Generate();

        var releaseVersionsForTargetPublication = _fixture.DefaultReleaseVersion()
            .ForIndex(0, s => s.SetRelease(_fixture.DefaultRelease()
                .WithPublication(targetPublication)))
            .ForIndex(1, s => s.SetRelease(_fixture.DefaultRelease()
                .WithPublication(targetPublication)))
            .ForIndex(2, s => s.SetRelease(_fixture.DefaultRelease()
                .WithPublication(targetPublication)))
            .GenerateList(3);

        var releaseVersionForNonTargetPublication = _fixture.DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease()
                .WithPublication(_fixture.DefaultPublication()))
            .Generate();

        var userReleaseRole1 = _fixture.DefaultUserReleaseRole()
            .WithUser(targetUser)
            .WithReleaseVersion(releaseVersionsForTargetPublication[0])
            .WithRole(ReleaseRole.Contributor)
            .Generate();
        var userReleaseRole2 = _fixture.DefaultUserReleaseRole()
            .WithUser(targetUser)
            .WithReleaseVersion(releaseVersionsForTargetPublication[1])
            .WithRole(ReleaseRole.Contributor)
            .Generate();

        var userReleaseRoleWithNonTargetRole = _fixture.DefaultUserReleaseRole()
            .WithUser(targetUser)
            .WithReleaseVersion(releaseVersionsForTargetPublication[2])
            .WithRole(ReleaseRole.PrereleaseViewer)
            .Generate();
        var userReleaseRoleWithNonTargetPublication = _fixture.DefaultUserReleaseRole()
            .WithUser(targetUser)
            .WithReleaseVersion(releaseVersionForNonTargetPublication)
            .WithRole(ReleaseRole.Contributor)
            .Generate();
        var userReleaseRoleWithNonTargetUser = _fixture.DefaultUserReleaseRole()
            .WithUser(new User())
            .WithReleaseVersion(releaseVersionsForTargetPublication[2])
            .WithRole(ReleaseRole.Contributor)
            .Generate();

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.UserReleaseRoles.AddRange(
            userReleaseRole1,
            userReleaseRole2,
            userReleaseRoleWithNonTargetRole,
            userReleaseRoleWithNonTargetPublication,
            userReleaseRoleWithNonTargetUser);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserReleaseRoleRepository(contentDbContext);
        await service.RemoveAllRolesAndInvitesForPublication(
            publicationId: targetPublication.Id,
            userId: targetUser.Id,
            rolesToInclude: ReleaseRole.Contributor);

        var remainingUserReleaseRoles = await contentDbContext.UserReleaseRoles
            .ToListAsync();

        Assert.Equal(3, remainingUserReleaseRoles.Count);

        Assert.Equal(userReleaseRoleWithNonTargetRole.Id, remainingUserReleaseRoles[0].Id);
        Assert.Equal(userReleaseRoleWithNonTargetPublication.Id, remainingUserReleaseRoles[1].Id);
        Assert.Equal(userReleaseRoleWithNonTargetUser.Id, remainingUserReleaseRoles[2].Id);
    }

    [Fact]
    public async Task GetAllRolesByUserAndRelease()
    {
        var user = new User();
        var releaseVersion = new ReleaseVersion();

        var userReleaseRoles = new List<UserReleaseRole>
        {
            new()
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Contributor
            },
            new()
            {
                User = user,
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.PrereleaseViewer
            }
        };

        var otherUserReleaseRoles = new List<UserReleaseRole>
        {
            // Role for different release
            new()
            {
                User = user,
                ReleaseVersion = new ReleaseVersion(),
                Role = ReleaseRole.Approver
            },
            // Role for different user
            new()
            {
                User = new User(),
                ReleaseVersion = releaseVersion,
                Role = ReleaseRole.Approver
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
            contentDbContext.UserReleaseRoles.AddRange(otherUserReleaseRoles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            var result = await service.GetAllRolesByUserAndRelease(userId: user.Id,
                releaseVersionId: releaseVersion.Id);

            Assert.Equal(2, result.Count);
            Assert.Equal(ReleaseRole.Contributor, result[0]);
            Assert.Equal(ReleaseRole.PrereleaseViewer, result[1]);
        }
    }

    [Fact]
    public async Task GetAllRolesByUserAndPublication()
    {
        var user = new User();
        var publication = new Publication();

        var userReleaseRolesForPublication = new List<UserReleaseRole>
        {
            new()
            {
                User = user,
                ReleaseVersion = new ReleaseVersion
                {
                    Publication = publication
                },
                Role = ReleaseRole.Contributor
            },
            new()
            {
                User = user,
                ReleaseVersion = new ReleaseVersion
                {
                    Publication = publication
                },
                Role = ReleaseRole.Approver
            },
            // Add a duplicate ReleaseRole to ensure duplicates are removed by the method under test
            new()
            {
                User = user,
                ReleaseVersion = new ReleaseVersion
                {
                    Publication = publication
                },
                Role = ReleaseRole.Approver
            }
        };

        var otherPublication = new Publication();
        var otherUserReleaseRoles = new List<UserReleaseRole>
        {
            // Role for different Publication
            new()
            {
                User = user,
                ReleaseVersion = new ReleaseVersion
                {
                    Publication = otherPublication
                },
                Role = ReleaseRole.PrereleaseViewer
            },
            // Role for same Publication but different user
            new()
            {
                User = new User(),
                ReleaseVersion = new ReleaseVersion
                {
                    Publication = publication
                },
                Role = ReleaseRole.PrereleaseViewer
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Users.AddRangeAsync(user);
            await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRolesForPublication);
            await contentDbContext.UserReleaseRoles.AddRangeAsync(otherUserReleaseRoles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            var result = await service.GetAllRolesByUserAndPublication(user.Id, publication.Id);

            Assert.Equal(2, result.Count);
            Assert.Equal(ReleaseRole.Contributor, result[0]);
            Assert.Equal(ReleaseRole.Approver, result[1]);
        }
    }

    [Fact]
    public async Task GetDistinctRolesByUser()
    {
        var user = new User();
        var release1 = new ReleaseVersion();
        var release2 = new ReleaseVersion();

        var userReleaseRoles = new List<UserReleaseRole>
        {
            new()
            {
                User = user,
                ReleaseVersion = release1,
                Role = ReleaseRole.Contributor
            },
            new()
            {
                User = user,
                ReleaseVersion = release1,
                Role = ReleaseRole.PrereleaseViewer
            },
            new()
            {
                User = user,
                ReleaseVersion = release2,
                Role = ReleaseRole.Approver,
            },
            new()
            {
                User = user,
                ReleaseVersion = release2,
                Role = ReleaseRole.PrereleaseViewer
            }
        };

        var otherUserReleaseRoles = new List<UserReleaseRole>
        {
            // Role for different release
            new()
            {
                User = user,
                ReleaseVersion = new ReleaseVersion(),
                Role = ReleaseRole.Approver
            },
            // Role for different user
            new()
            {
                User = new User(),
                ReleaseVersion = release1,
                Role = ReleaseRole.Approver
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.ReleaseVersions.AddRange(release1, release2);
            contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
            contentDbContext.UserReleaseRoles.AddRange(otherUserReleaseRoles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            var result = await service.GetDistinctRolesByUser(user.Id);

            // Expect 3 distinct results.  The 4th duplicate "ReleaseRole.PrereleaseViewer" role is filtered out.
            Assert.Equal(3, result.Count);
            Assert.Equal(ReleaseRole.Contributor, result[0]);
            Assert.Equal(ReleaseRole.PrereleaseViewer, result[1]);
            Assert.Equal(ReleaseRole.Approver, result[2]);
        }
    }

    [Fact]
    public async Task GetUserReleaseRole_ReturnRoleIfExists()
    {
        var userReleaseRole = new UserReleaseRole
        {
            User = new User(),
            ReleaseVersion = new ReleaseVersion(),
            Role = ReleaseRole.Contributor
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

            var result = await service.GetUserReleaseRole(
                userId: userReleaseRole.UserId,
                releaseVersionId: userReleaseRole.ReleaseVersionId,
                ReleaseRole.Contributor);

            Assert.NotNull(result);
            Assert.Equal(userReleaseRole.UserId, result!.UserId);
            Assert.Equal(userReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
            Assert.Equal(userReleaseRole.Role, result.Role);
        }
    }

    [Fact]
    public async Task GetUserReleaseRole_NullIfNotExists()
    {
        var user = new User();
        var releaseVersion = new ReleaseVersion();

        // Setup a role but for a different release to make sure it has no influence
        var userReleaseRoleOtherRelease = new UserReleaseRole
        {
            User = user,
            ReleaseVersion = new ReleaseVersion(),
            Role = ReleaseRole.Contributor
        };

        // Setup a different role on the release to make sure it has no influence
        var userReleaseRoleDifferentRole = new UserReleaseRole
        {
            User = user,
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.Approver
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.UserReleaseRoles.AddRange(
                userReleaseRoleOtherRelease,
                userReleaseRoleDifferentRole);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserReleaseRoleRepository(contentDbContext);

            var result = await service.GetUserReleaseRole(
                userId: user.Id,
                releaseVersionId: releaseVersion.Id,
                ReleaseRole.Contributor);

            Assert.Null(result);
        }
    }

    private static UserReleaseRoleAndInviteManager SetupUserReleaseRoleRepository(ContentDbContext contentDbContext)
    {
        return new(
            contentDbContext: contentDbContext,
            userReleaseInviteRepository: new UserReleaseInviteRepository(contentDbContext),
            userRepository: new UserRepository(contentDbContext));
    }
}
