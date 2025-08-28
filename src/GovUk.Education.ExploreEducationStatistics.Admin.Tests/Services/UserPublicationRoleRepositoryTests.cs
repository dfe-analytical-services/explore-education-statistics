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
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserPublicationRoleRepositoryTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task TryCreate_OldRole_NoNewPermissionsSystemPublicationRoleChanges()
    {
        var oldPublicationRoleToCreate = PublicationRole.Allower;
        var newPublicationRoleToRemain = PublicationRole.Approver;

        var user = new User();
        var createdBy = new User();
        var publication = _fixture.DefaultPublication()
            .Generate();
        var existingPublicationRole = _fixture.DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .WithRole(newPublicationRoleToRemain)
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                oldPublicationRoleToCreate,
                user.Id,
                publication.Id))
            .ReturnsAsync((null, null));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await repository.TryCreate(user.Id, publication.Id, oldPublicationRoleToCreate, createdBy.Id);

            // Should be the OLD `Allower` role which has been created that is returned
            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(oldPublicationRoleToCreate, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
            // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
            // when we stop using the OLD roles.
            var userPublicationRoles = await contentDbContext.AllUserPublicationRoles.ToListAsync();

            // Should be 2 as the 'Approver` role should be untouched,
            // and the `Owner` role has been created.
            Assert.Equal(2, userPublicationRoles.Count);

            var createdOldPublicationRole =
                Assert.Single(userPublicationRoles, upr => upr.Role == oldPublicationRoleToCreate);

            Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
            Assert.Equal(user.Id, createdOldPublicationRole.UserId);
            Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
            Assert.Equal(oldPublicationRoleToCreate, createdOldPublicationRole.Role);
            createdOldPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);

            var existingNewPublicationRole =
                Assert.Single(userPublicationRoles, upr => upr.Role == newPublicationRoleToRemain);

            Assert.NotEqual(Guid.Empty, existingNewPublicationRole.Id);
            Assert.Equal(user.Id, existingNewPublicationRole.UserId);
            Assert.Equal(publication.Id, existingNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToRemain, existingNewPublicationRole.Role);
            existingNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, existingNewPublicationRole.CreatedById);
        }
    }

    [Fact]
    public async Task TryCreate_OldRole_NewPermissionsSystemPublicationRolesToRemoveAndCreateFromOldRole()
    {
        var oldPublicationRoleToCreate = PublicationRole.Allower;
        var newPublicationRoleToCreate = PublicationRole.Approver;
        var newPublicationRoleToRemove = PublicationRole.Drafter;

        var user = new User();
        var createdBy = new User();
        var publication = _fixture.DefaultPublication()
            .Generate();
        var existingPublicationRole = _fixture.DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .WithRole(newPublicationRoleToRemove)
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                oldPublicationRoleToCreate,
                user.Id,
                publication.Id))
            .ReturnsAsync((newPublicationRoleToRemove, newPublicationRoleToCreate));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await repository.TryCreate(user.Id, publication.Id, oldPublicationRoleToCreate, createdBy.Id);

            // Should be the OLD `Owner` role which has been created that is returned
            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(oldPublicationRoleToCreate, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }
        
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
            // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
            // when we stop using the OLD roles.
            var userPublicationRoles = await contentDbContext.AllUserPublicationRoles.ToListAsync();

            // Should be 2 as the 'Drafter` role has been removed and replaced with the `Approver` role,
            // and the `Owner` role has been created.
            Assert.Equal(2, userPublicationRoles.Count);

            var createdOldPublicationRole =
                Assert.Single(userPublicationRoles, upr => upr.Role == oldPublicationRoleToCreate);

            Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
            Assert.Equal(user.Id, createdOldPublicationRole.UserId);
            Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
            Assert.Equal(oldPublicationRoleToCreate, createdOldPublicationRole.Role);
            createdOldPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);

            var createdNewPublicationRole =
                Assert.Single(userPublicationRoles, upr => upr.Role == newPublicationRoleToCreate);

            Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
            Assert.Equal(user.Id, createdNewPublicationRole.UserId);
            Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, createdNewPublicationRole.Role);
            createdNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
        }
    }

    [Fact]
    public async Task TryCreate_OldRole_NewPermissionsSystemPublicationRoleToCreate()
    {
        var oldPublicationRoleToCreate = PublicationRole.Owner;
        var newPublicationRoleToCreate = PublicationRole.Drafter;

        var user = new User();
        var createdBy = new User();
        var publication = _fixture.DefaultPublication()
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.AddRange(user, createdBy);
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                oldPublicationRoleToCreate,
                user.Id,
                publication.Id))
            .ReturnsAsync((null, newPublicationRoleToCreate));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await repository.TryCreate(user.Id, publication.Id, oldPublicationRoleToCreate, createdBy.Id);

            // Should be the OLD `Owner` role which has been created that is returned
            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(oldPublicationRoleToCreate, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }
        
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
            // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
            // when we stop using the OLD roles.
            var userPublicationRoles = await contentDbContext.AllUserPublicationRoles.ToListAsync();

            // Should be 2 as the 'Drafter` role has been created,
            // and the `Owner` role has been created.
            Assert.Equal(2, userPublicationRoles.Count);

            var createdOldPublicationRole =
                Assert.Single(userPublicationRoles, upr => upr.Role == oldPublicationRoleToCreate);

            Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
            Assert.Equal(user.Id, createdOldPublicationRole.UserId);
            Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
            Assert.Equal(oldPublicationRoleToCreate, createdOldPublicationRole.Role);
            createdOldPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);

            var createdNewPublicationRole =
                Assert.Single(userPublicationRoles, upr => upr.Role == newPublicationRoleToCreate);

            Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
            Assert.Equal(user.Id, createdNewPublicationRole.UserId);
            Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, createdNewPublicationRole.Role);
            createdNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
        }
    }

    [Fact]
    public async Task TryCreate_NewRole_NoNewPermissionsSystemPublicationRoleChanges()
    {
        var newPublicationRoleToCreate = PublicationRole.Drafter;
        var newPublicationRoleToRemain = PublicationRole.Approver;

        var user = new User();
        var createdBy = new User();
        var publication = new Publication();
        var existingPublicationRole = _fixture.DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .WithRole(newPublicationRoleToRemain)
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                newPublicationRoleToCreate,
                user.Id,
                publication.Id))
            .ReturnsAsync((null, null));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await repository.TryCreate(user.Id, publication.Id, newPublicationRoleToCreate, createdBy.Id);

            // Should not return anything, as no OLD should be created,
            // AND no NEW role should be created.
            Assert.Null(result);
        }
        
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
            // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
            // when we stop using the OLD roles.
            var userPublicationRoles = await contentDbContext.AllUserPublicationRoles.ToListAsync();

            // Should only be the 'Approver` role which remains,
            // and the `Drafter` role should NOT be created (i.e. NO changes).
            var existingNewPublicationRole = Assert.Single(userPublicationRoles);

            Assert.NotEqual(Guid.Empty, existingNewPublicationRole.Id);
            Assert.Equal(user.Id, existingNewPublicationRole.UserId);
            Assert.Equal(publication.Id, existingNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToRemain, existingNewPublicationRole.Role);
            existingNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, existingNewPublicationRole.CreatedById);
        }
    }

    [Fact]
    public async Task TryCreate_NewRole_NewPermissionsSystemPublicationRolesToRemoveAndCreateFromOldRole()
    {
        var newPublicationRoleToCreate = PublicationRole.Approver;
        var newPublicationRoleToRemove = PublicationRole.Drafter;

        var user = new User();
        var createdBy = new User();
        var publication = _fixture.DefaultPublication()
            .Generate();
        var existingPublicationRole = _fixture.DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .WithRole(newPublicationRoleToRemove)
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                newPublicationRoleToRemove,
                user.Id,
                publication.Id))
            .ReturnsAsync((newPublicationRoleToRemove, newPublicationRoleToCreate));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await repository.TryCreate(user.Id, publication.Id, newPublicationRoleToRemove, createdBy.Id);

            Assert.NotNull(result);

            // Should be the NEW `Approver` role which has been created that is returned
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }
        
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
            // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
            // when we stop using the OLD roles.
            var userPublicationRoles = await contentDbContext.AllUserPublicationRoles.ToListAsync();

            // Should only be the 'Approver` role that exists, as the `Drafter` role should be removed,
            // and the `Approver` should be created.
            var createdNewPublicationRole = Assert.Single(userPublicationRoles);

            Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
            Assert.Equal(user.Id, createdNewPublicationRole.UserId);
            Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, createdNewPublicationRole.Role);
            createdNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
        }
    }

    [Fact]
    public async Task TryCreate_NewRole_NewPermissionsSystemPublicationRoleToCreate()
    {
        var newPublicationRoleToCreate = PublicationRole.Drafter;

        var user = new User();
        var createdBy = new User();
        var publication = _fixture.DefaultPublication()
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.AddRange(user, createdBy);
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
        newPermissionsSystemHelperMock
            .Setup(rvr => rvr.DetermineNewPermissionsSystemChanges(
                newPublicationRoleToCreate,
                user.Id,
                publication.Id))
            .ReturnsAsync((null, newPublicationRoleToCreate));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var repository = CreateRepository(contentDbContext, newPermissionsSystemHelperMock.Object);

            var result = await repository.TryCreate(user.Id, publication.Id, newPublicationRoleToCreate, createdBy.Id);

            // Should be the NEW `Drafter` role which has been created that is returned
            Assert.NotNull(result);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(publication.Id, result.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, result.Role);
            result.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, result.CreatedById);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
            // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
            // when we stop using the OLD roles.
            var userPublicationRoles = await contentDbContext.AllUserPublicationRoles.ToListAsync();

            // Should only be the 'Drafter` role which has been created.
            // No other role should exist.
            Assert.Single(userPublicationRoles);

            var createdNewPublicationRole =
                Assert.Single(userPublicationRoles, upr => upr.Role == newPublicationRoleToCreate);

            Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
            Assert.Equal(user.Id, createdNewPublicationRole.UserId);
            Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
            Assert.Equal(newPublicationRoleToCreate, createdNewPublicationRole.Role);
            createdNewPublicationRole.Created.AssertUtcNow();
            Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
        }
    }

    public class UserHasRoleOnPublicationTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task UserHasRoleOnPublication_TrueIfRoleExists()
        {
            var userPublicationRole = new UserPublicationRole
            {
                User = new User(), Publication = new Publication(), Role = PublicationRole.Owner
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
                User = new User(), Publication = new Publication(), Role = PublicationRole.Owner
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

            var userPublicationRoles = new List<UserPublicationRole>
            {
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

            var userPublicationRoles = new List<UserPublicationRole>
            {
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

            var userPublicationRoles = new List<UserPublicationRole>
            {
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

            var userPublicationRoles = new List<UserPublicationRole>
            {
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
        [Fact]
        public async Task Remove_OldRole_NoNewPermissionsSystemPublicationRoleToRemove()
        {
            var oldPublicationRoleToDelete = PublicationRole.Owner;

            var user = new User();
            var publication = _fixture.DefaultPublication()
                .Generate();
            var oldUserPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToDelete)
                .WithUser(user)
                .WithPublication(publication)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(oldUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
            newPermissionsSystemHelperMock
                .Setup(rvr => rvr.DetermineNewPermissionsSystemRoleToDelete(
                    oldUserPublicationRole))
                .ReturnsAsync((UserPublicationRole?)null);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    newPermissionsSystemHelper: newPermissionsSystemHelperMock.Object);

                await repository.Remove(oldUserPublicationRole);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
                // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
                // when we stop using the OLD roles.
                var updatedPublicationRolesIgnoringQueryFilters = await contentDbContext.AllUserPublicationRoles
                    .IgnoreQueryFilters()
                    .ToListAsync();

                // The existing role should have been hard-deleted
                Assert.Empty(updatedPublicationRolesIgnoringQueryFilters);
            }
        }

        [Fact]
        public async Task Remove_OldRole_NewPermissionsSystemPublicationRoleToRemove()
        {
            var oldPublicationRoleToDelete = PublicationRole.Owner;
            var newPublicationRoleToDelete = PublicationRole.Drafter;

            var user = new User();
            var publication = _fixture.DefaultPublication()
                .Generate();
            var oldUserPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToDelete)
                .WithUser(user)
                .WithPublication(publication)
                .Generate();
            var newUserPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToDelete)
                .WithUser(user)
                .WithPublication(publication)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(oldUserPublicationRole, newUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();
            newPermissionsSystemHelperMock
                .Setup(rvr => rvr.DetermineNewPermissionsSystemRoleToDelete(
                    oldUserPublicationRole))
                .ReturnsAsync(newUserPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    newPermissionsSystemHelper: newPermissionsSystemHelperMock.Object);

                await repository.Remove(oldUserPublicationRole);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
                // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
                // when we stop using the OLD roles.
                var updatedPublicationRolesIgnoringQueryFilters = await contentDbContext.AllUserPublicationRoles
                    .IgnoreQueryFilters()
                    .ToListAsync();

                // Both roles should have been hard-deleted
                Assert.Empty(updatedPublicationRolesIgnoringQueryFilters);
            }
        }

        [Fact]
        public async Task Remove_NewRole()
        {
            var newPublicationRoleToDelete = PublicationRole.Drafter;

            var user = new User();
            var publication = _fixture.DefaultPublication()
                .Generate();
            var newUserPublicationRole = _fixture.DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToDelete)
                .WithUser(user)
                .WithPublication(publication)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(newUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            var newPermissionsSystemHelperMock = new Mock<INewPermissionsSystemHelper>();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    newPermissionsSystemHelper: newPermissionsSystemHelperMock.Object);

                await repository.Remove(newUserPublicationRole);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking 'AllUserPublicationRoles', which includes the NEW and OLD permissions system
                // publication roles. This will likely be changed back to just 'UserPublicationRoles' in EES-6196 
                // when we stop using the OLD roles.
                var updatedPublicationRolesIgnoringQueryFilters = await contentDbContext.AllUserPublicationRoles
                    .IgnoreQueryFilters()
                    .ToListAsync();

                // The existing new role should have been hard-deleted
                Assert.Empty(updatedPublicationRolesIgnoringQueryFilters);
            }

            // Should have exited early and not called the NewPermissionsSystemHelper
            newPermissionsSystemHelperMock
                .Verify(mock => mock.DetermineNewPermissionsSystemRoleToDelete(
                    It.IsAny<UserPublicationRole>()), Times.Never);
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
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRole1, userPublicationRole2,
                    userPublicationRole3);
                contentDbContext.UserPublicationInvites.AddRange(userPublicationInvite1, userPublicationInvite2,
                    userPublicationInvite3);
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
