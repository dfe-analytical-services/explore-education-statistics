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
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserPublicationRoleRepositoryTests
{
    private readonly DataFixture _fixture = new();
    
    [Fact]
    public async Task TryCreate()
    {
        var user = new User();
        var createdBy = new User();
        var publication = new Publication();
        var publicationRole = PublicationRole.Owner;
        
        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Users.AddRangeAsync(user, createdBy);
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext);

            var result = await repository.TryCreate(
                userId: user.Id, 
                publicationId: publication.Id, 
                publicationRole: publicationRole, 
                createdById: createdBy.Id);

            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(publicationRole, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();
            var userPublicationRole = Assert.Single(userPublicationRoles);

            Assert.NotEqual(Guid.Empty, userPublicationRole.Id);
            Assert.Equal(user.Id, userPublicationRole.UserId);
            Assert.Equal(publication.Id, userPublicationRole.PublicationId);
            Assert.Equal(publicationRole, userPublicationRole.Role);
            userPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, userPublicationRole.CreatedById);
        }
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

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.AddRange(user, createdBy);
            contentDbContext.Publications.Add(publication);
            contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        var oldPublicationRole = PublicationRole.Owner;

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                oldPublicationRole,
                user.Id,
                publication.Id))
            .ReturnsAsync((PublicationRole.Drafter, PublicationRole.Approver));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await repository.TryCreate(user.Id, publication.Id, oldPublicationRole, createdBy.Id);

            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(oldPublicationRole, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
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
    }

    public class UserHasRoleOnPublicationTests : UserPublicationRoleRepositoryTests
    {
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
                var repository = CreateRepository(contentDbContext);

                Assert.True(await repository.UserHasRoleOnPublication(
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
                var repository = CreateRepository(contentDbContext);

                Assert.False(await repository.UserHasRoleOnPublication(
                    userPublicationRoleOtherPublication.UserId,
                    publication.Id,
                    PublicationRole.Owner));
            }
        }
    }

    public class GetDistinctRolesByUserTests : UserPublicationRoleRepositoryTests
    {
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
                // Roles for different publication. One duplicate role.
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetDistinctRolesByUser(user1.Id);

                // Expect only distinct roles to be returned, therefore the 2nd "PublicationRole.Owner" role is filtered out.
                Assert.Equal([PublicationRole.Owner, PublicationRole.Allower], result);
            }
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetDistinctRolesByUser(user.Id);

                Assert.Empty(result);
            }
        }
    }

    public class GetAllRolesByUserAndPublicationTests : UserPublicationRoleRepositoryTests
    {
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
                _fixture.DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication1)
                    .WithRole(PublicationRole.Allower)
                    .Generate(),
                // Different role for different publication.
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetAllRolesByUserAndPublication(
                    userId: user1.Id,
                    publicationId: publication1.Id);

                Assert.Equal([PublicationRole.Owner, PublicationRole.Allower], result);
            }
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
                // Invalid role
                _fixture.DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithPublication(publication)
                    .WithRole(PublicationRole.Approver)
                    .Generate(),
                // Invalid role
                _fixture.DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithPublication(publication)
                    .WithRole(PublicationRole.Drafter)
                    .Generate(),
                // Valid role
                _fixture.DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithPublication(publication)
                    .WithRole(PublicationRole.Allower)
                    .Generate(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetAllRolesByUserAndPublication(
                    userId: user.Id,
                    publicationId: publication.Id);

                Assert.Equal([PublicationRole.Allower], result);
            }
        }
    }

    public class RemoveTests : UserPublicationRoleRepositoryTests
    {
        [Theory]
        // Valid roles
        [InlineData(PublicationRole.Allower)]
        [InlineData(PublicationRole.Owner)]
        // Invalid roles
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task Success(PublicationRole publicationRole)
        {
            var email = "test@test.com";

            var userPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithUser(new User
                {
                    Email = email
                })
                .WithPublication(_fixture.DefaultPublication())
                .WithRole(publicationRole)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var userPublicationRoleToRemove = await contentDbContext.UserPublicationRoles
                    .IgnoreQueryFilters()
                    .SingleAsync(urr => urr.Id == userPublicationRole.Id);

                await repository.Remove(userPublicationRoleToRemove);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedPublicationRole = await contentDbContext.UserPublicationRoles
                    .SingleOrDefaultAsync(urr => urr.Id == userPublicationRole.Id);

                Assert.Null(updatedPublicationRole);
            }
        }
    }

    public class RemoveManyTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var user1 = new User { Email = "test1@test.com" };
            var publication1 = _fixture.DefaultPublication()
                .Generate();
            var userPublicationRole1 = _fixture.DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Allower)
                .Generate();
            var userPublicationInvite1 = _fixture.DefaultUserPublicationInvite()
                .WithEmail(user1.Email)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Allower)
                .Generate();

            var user2 = new User { Email = "test2@test.com" };
            var publication2 = _fixture.DefaultPublication()
                .Generate();
            var userPublicationRole2 = _fixture.DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Owner)
                .Generate();
            var userPublicationInvite2 = _fixture.DefaultUserPublicationInvite()
                .WithEmail(user2.Email)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Owner)
                .Generate();

            var user3 = new User { Email = "test3@test.com" };
            var publication3 = _fixture.DefaultPublication()
                .Generate();
            var userPublicationRole3 = _fixture.DefaultUserPublicationRole()
                .WithUser(user3)
                .WithPublication(publication3)
                .WithRole(PublicationRole.Allower)
                .Generate();
            var userPublicationInvite3 = _fixture.DefaultUserPublicationInvite()
                .WithEmail(user3.Email)
                .WithPublication(publication3)
                .WithRole(PublicationRole.Allower)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRole1, userPublicationRole2, userPublicationRole3);
                contentDbContext.UserPublicationInvites.AddRange(userPublicationInvite1, userPublicationInvite2, userPublicationInvite3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany([userPublicationRole1, userPublicationRole2]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRole = await contentDbContext.UserPublicationRoles
                    .SingleAsync();

                Assert.Equal(userPublicationRole3.Id, userPublicationRole.Id);
            }
        }
    }

    public class RemoveForUserTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task TargetUserHasRoles_RemovesTargetRoles()
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var role1 = PublicationRole.Allower;
            var role2 = PublicationRole.Owner;
            var publication1 = _fixture.DefaultPublication()
                .Generate();
            var publication2 = _fixture.DefaultPublication()
                .Generate();

            var userPublicationRoles = _fixture.DefaultUserPublicationRole()
                // These 2 roles should be removed
                .ForIndex(0, s => s.SetPublication(publication1))
                .ForIndex(0, s => s.SetUser(targetUser))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetPublication(publication2))
                .ForIndex(1, s => s.SetUser(targetUser))
                .ForIndex(1, s => s.SetRole(role2))
                // These roles are for a different email and should not be removed
                .ForIndex(2, s => s.SetPublication(publication1))
                .ForIndex(2, s => s.SetUser(otherUser))
                .ForIndex(2, s => s.SetRole(role1))
                .ForIndex(3, s => s.SetPublication(publication2))
                .ForIndex(3, s => s.SetUser(otherUser))
                .ForIndex(3, s => s.SetRole(role2))
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveForUser(targetUser.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserPublicationRoles
                    .Include(upr => upr.User)
                    .ToListAsync();

                Assert.Equal(2, remainingRoles.Count);

                Assert.Equal(publication1.Id, remainingRoles[0].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[0].User.Id);
                Assert.Equal(role1, remainingRoles[0].Role);

                Assert.Equal(publication2.Id, remainingRoles[1].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
                Assert.Equal(role2, remainingRoles[1].Role);
            }
        }

        [Fact]
        public async Task TargetUserHasNoRoles_DoesNothing()
        {
            var targetUser = new User { Email = "test1@test.com" };
            var otherUser = new User { Email = "test2@test.com" };
            var role1 = PublicationRole.Allower;
            var role2 = PublicationRole.Owner;
            var publication1 = _fixture.DefaultPublication()
                .Generate();
            var publication2 = _fixture.DefaultPublication()
                .Generate();

            var userPublicationRoles = _fixture.DefaultUserPublicationRole()
                // These roles are for a different email and should not be removed
                .ForIndex(0, s => s.SetPublication(publication1))
                .ForIndex(0, s => s.SetUser(otherUser))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetPublication(publication2))
                .ForIndex(1, s => s.SetUser(otherUser))
                .ForIndex(1, s => s.SetRole(role2))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(targetUser, otherUser);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var manager = CreateRepository(contentDbContext);

                await manager.RemoveForUser(targetUser.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserPublicationRoles
                    .Include(upr => upr.User)
                    .ToListAsync();

                Assert.Equal(2, remainingRoles.Count);

                Assert.Equal(publication1.Id, remainingRoles[0].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[0].User.Id);
                Assert.Equal(role1, remainingRoles[0].Role);

                Assert.Equal(publication2.Id, remainingRoles[1].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
                Assert.Equal(role2, remainingRoles[1].Role);
            }
        }
    }

    private static UserPublicationRoleRepository CreateRepository(
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
