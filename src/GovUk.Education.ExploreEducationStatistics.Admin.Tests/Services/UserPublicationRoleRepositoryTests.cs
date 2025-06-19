#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserPublicationRoleRepositoryTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task TryCreate()
    {
        var user = new User();
        var createdBy = new User();
        var publication = new Publication();
        var publicationRole = PublicationRole.Owner;

        using var contentDbContext = InMemoryApplicationDbContext();

        await contentDbContext.Users.AddRangeAsync(user, createdBy);
        await contentDbContext.Publications.AddAsync(publication);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserPublicationRoleRepository(contentDbContext);

        var result = await service.TryCreate(user.Id, publication.Id, publicationRole, createdBy.Id);

        Assert.NotNull(result);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(publication.Id, result.PublicationId);
        Assert.Equal(publicationRole, result.Role);
        result.Created.AssertUtcNow();
        Assert.Equal(createdBy.Id, result.CreatedById);

        var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();
        Assert.Single(userPublicationRoles);

        Assert.NotEqual(Guid.Empty, userPublicationRoles[0].Id);
        Assert.Equal(user.Id, userPublicationRoles[0].UserId);
        Assert.Equal(publication.Id, userPublicationRoles[0].PublicationId);
        Assert.Equal(publicationRole, userPublicationRoles[0].Role);
        userPublicationRoles[0].Created.AssertUtcNow();
        Assert.Equal(createdBy.Id, userPublicationRoles[0].CreatedById);
    }

    [Fact]
    public async Task TryCreate_NewPermissionsSystemPublicationRolesToRemoveAndCreateFromOldRole()
    {
        var user = new User();
        var createdBy = new User();
        var publication = _fixture.DefaultPublication()
            .Generate();
        var existingPublicationRole = _fixture.DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .WithRole(PublicationRole.Drafter)
            .Generate();

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.Users.AddRange(user, createdBy);
        contentDbContext.Publications.Add(publication);
        contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
        await contentDbContext.SaveChangesAsync();

        var oldPublicationRole = PublicationRole.Owner;

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                oldPublicationRole,
                user.Id,
                publication.Id))
            .ReturnsAsync((PublicationRole.Drafter, PublicationRole.Approver));

        var service = SetupUserPublicationRoleRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

        var result = await service.TryCreate(user.Id, publication.Id, oldPublicationRole, createdBy.Id);

        Assert.NotNull(result);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(publication.Id, result.PublicationId);
        Assert.Equal(oldPublicationRole, result.Role);
        result.Created.AssertUtcNow();
        Assert.Equal(createdBy.Id, result.CreatedById);

        var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

        // Should be 2 as the 'Drafter` role has been removed and replaced with the `Approver` role,
        // and the `Owner` role has been created.
        Assert.Equal(2, userPublicationRoles.Count);

        Assert.NotEqual(Guid.Empty, userPublicationRoles[0].Id);
        Assert.Equal(user.Id, userPublicationRoles[0].UserId);
        Assert.Equal(publication.Id, userPublicationRoles[0].PublicationId);
        Assert.Equal(oldPublicationRole, userPublicationRoles[0].Role);
        userPublicationRoles[0].Created.AssertUtcNow();
        Assert.Equal(createdBy.Id, userPublicationRoles[0].CreatedById);

        Assert.NotEqual(Guid.Empty, userPublicationRoles[1].Id);
        Assert.Equal(user.Id, userPublicationRoles[1].UserId);
        Assert.Equal(publication.Id, userPublicationRoles[1].PublicationId);
        Assert.Equal(PublicationRole.Approver, userPublicationRoles[1].Role);
        userPublicationRoles[1].Created.AssertUtcNow();
        Assert.Equal(createdBy.Id, userPublicationRoles[1].CreatedById);
    }

    [Fact]
    public async Task UserHasRoleOnPublication_TrueIfRoleExists()
    {
        var userPublicationRole = new UserPublicationRole
        {
            User = new User(),
            Publication = new Publication(),
            Role = PublicationRole.Owner
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
                PublicationRole.Owner));
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
            Role = PublicationRole.Owner
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
                PublicationRole.Owner));
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
                .WithRole(PublicationRole.Owner)
                .Generate(),
            // Roles for different pulication. One duplicate role.
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Owner)
                .Generate(),
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Allower)
                .Generate(),
            // Role for different user
            _fixture.DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Owner)
                .Generate(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await contentDbContext.AddRangeAsync(userPublicationRoles);
        await contentDbContext.SaveChangesAsync();

        var service = SetupUserPublicationRoleRepository(contentDbContext);

        var result = await service.GetDistinctRolesByUser(user1.Id);

        // Expect only distinct roles to be returned, therefore the 2nd "PublicationRole.Owner" role is filtered out.
        Assert.Equal(2, result.Count);
        Assert.Equal(PublicationRole.Owner, result[0]);
        Assert.Equal(PublicationRole.Allower, result[1]);
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
                .WithRole(PublicationRole.Approver)
                .Generate(),
            _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter)
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
                .WithRole(PublicationRole.Owner)
                .Generate(),
            // Different role for different pulication.
            _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Allower)
                .Generate(),
            // Different role for different user
            _fixture.DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Allower)
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
        Assert.Equal(PublicationRole.Owner, role);
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
                .WithRole(PublicationRole.Approver)
                .Generate(),
            _fixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Drafter)
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
        ContentDbContext contentDbContext,
        INewPermissionsSystemHelper? newPermissionsSystemHelper = null)
    {
        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                It.IsAny<PublicationRole>(), 
                It.IsAny<Guid>(),
                It.IsAny<Guid>()))
            .ReturnsAsync((null, null));

        return new(
            contentDbContext,
            newPermissionsSystemHelper ?? newPermissionsSystemHelperMock.Object);
    }
}
