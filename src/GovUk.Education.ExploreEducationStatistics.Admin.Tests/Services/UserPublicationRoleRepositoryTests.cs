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
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserPublicationRoleRepositoryTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Create()
    {
        var user = new User();

        var createdBy = new User();

        var publication = new Publication();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Users.AddRangeAsync(user, createdBy);
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserPublicationRoleRepository(contentDbContext);

            var result = await service.Create(user.Id, publication.Id, Owner, createdBy.Id);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(Owner, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();
            Assert.Single(userPublicationRoles);

            Assert.NotEqual(Guid.Empty, userPublicationRoles[0].Id);
            Assert.Equal(user.Id, userPublicationRoles[0].UserId);
            Assert.Equal(publication.Id, userPublicationRoles[0].PublicationId);
            Assert.Equal(Owner, userPublicationRoles[0].Role);
            userPublicationRoles[0].Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, userPublicationRoles[0].CreatedById);
        }
    }

    [Fact]
    public async Task UserHasRoleOnPublication_TrueIfRoleExists()
    {
        var userPublicationRole = new UserPublicationRole
        {
            User = new User(),
            Publication = new Publication(),
            Role = Owner
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserPublicationRoleRepository(contentDbContext);

            Assert.True(await service.UserHasRoleOnPublication(
                userPublicationRole.UserId,
                userPublicationRole.PublicationId,
                Owner));
        }
    }

    [Fact]
    public async Task UserHasRoleOnPublication_FalseIfRoleDoesNotExist()
    {
        var publication = new Publication();

        // Setup a role but for a different publication to make sure it has no influence
        var userPublicationRoleOtherPublication = new UserPublicationRole
        {
            User = new User(),
            Publication = new Publication(),
            Role = Owner
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRoleOtherPublication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupUserPublicationRoleRepository(contentDbContext);

            Assert.False(await service.UserHasRoleOnPublication(
                userPublicationRoleOtherPublication.UserId,
                publication.Id,
                Owner));
        }
    }

    [Fact]
    public async Task GetDistinctRolesByUser()
    {
        var user1 = new User();
        var user2 = new User();

        var publication1 = _fixture.DefaultPublication()
            .Generate();
        var publication2 = _fixture.DefaultPublication()
            .Generate();

        var userPublicationRoles = new List<UserPublicationRole> {
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication1)
                .WithRole(Owner)
                .Generate(),
            // Roles for different pulication. One duplicate role.
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication2)
                .WithRole(Owner)
                .Generate(),
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication2)
                .WithRole(Allower)
                .Generate(),
            // Role for different user
            _fixture.DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication1)
                .WithRole(Owner)
                .Generate(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await contentDbContext.AddRangeAsync(userPublicationRoles);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserPublicationRoleRepository(contentDbContext);

        var result = await service.GetDistinctRolesByUser(user1.Id);

        // Expect only distinct roles to be returned, therefore the 2nd "Owner" role is filtered out.
        Assert.Equal(2, result.Count);
        Assert.Equal(Owner, result[0]);
        Assert.Equal(Allower, result[1]);
    }

    // This test will be changed when we start introducing the use of the NEW publication roles in the 
    // UI, in STEP 9 (EES-6196) of the Permissions Rework. For now, we want to
    // filter out any usage of the NEW roles.
    [Fact]
    public async Task GetDistinctRolesByUser_InvalidRolesNotReturned()
    {
        var user = new User();

        var publication = _fixture.DefaultPublication()
            .Generate();

        var userPublicationRoles = new List<UserPublicationRole> {
            _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(Approver)
                .Generate(),
            _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(Drafter)
                .Generate(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await contentDbContext.AddRangeAsync(userPublicationRoles);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserPublicationRoleRepository(contentDbContext);

        var result = await service.GetDistinctRolesByUser(user.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllRolesByUserAndPublication()
    {
        var user1 = new User();
        var user2 = new User();

        var publication1 = _fixture.DefaultPublication()
            .Generate();
        var publication2 = _fixture.DefaultPublication()
            .Generate();

        var userPublicationRoles = new List<UserPublicationRole> {
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication1)
                .WithRole(Owner)
                .Generate(),
            // Different role for different pulication.
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication2)
                .WithRole(Allower)
                .Generate(),
            // Different role for different user
            _fixture.DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication1)
                .WithRole(Allower)
                .Generate(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await contentDbContext.AddRangeAsync(userPublicationRoles);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserPublicationRoleRepository(contentDbContext);

        var result = await service.GetAllRolesByUserAndPublication(
            userId: user1.Id,
            publicationId: publication1.Id);

        var role = Assert.Single(result);
        Assert.Equal(Owner, role);
    }

    // This test will be changed when we start introducing the use of the NEW publication roles in the 
    // UI, in STEP 9 (EES-6196) of the Permissions Rework. For now, we want to
    // filter out any usage of the NEW roles.
    [Fact]
    public async Task GetAllRolesByUserAndPublication_InvalidRolesNotReturned()
    {
        var user = new User();

        var publication = _fixture.DefaultPublication()
            .Generate();

        var userPublicationRoles = new List<UserPublicationRole> {
            _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(Approver)
                .Generate(),
            _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(Drafter)
                .Generate(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await contentDbContext.AddRangeAsync(userPublicationRoles);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserPublicationRoleRepository(contentDbContext);

        var result = await service.GetAllRolesByUserAndPublication(
            userId: user.Id,
            publicationId: publication.Id);

        Assert.Empty(result);
    }

    private static UserPublicationRoleRepository SetupUserPublicationRoleRepository(
        ContentDbContext contentDbContext)
    {
        return new(contentDbContext);
    }
}
