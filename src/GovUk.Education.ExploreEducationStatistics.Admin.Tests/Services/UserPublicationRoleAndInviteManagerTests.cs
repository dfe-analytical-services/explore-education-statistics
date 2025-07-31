#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserPublicationRoleAndInviteManagerTests
{
    private readonly DataFixture _fixture = new();

    public class CreateTests : UserPublicationRoleAndInviteManagerTests
    {
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
                var service = SetupUserPublicationRoleAndInviteManager(contentDbContext);

                var result = await service.Create(user.Id, publication.Id, PublicationRole.Owner, createdBy.Id);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(PublicationRole.Owner, result.Role);
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
                Assert.Equal(PublicationRole.Owner, userPublicationRole.Role);
                userPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, userPublicationRole.CreatedById);
            }
        }
    }

    public class UserHasRoleOnPublication_TrueIfRoleExistsTests : UserPublicationRoleAndInviteManagerTests
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
                var service = SetupUserPublicationRoleAndInviteManager(contentDbContext);

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
                var service = SetupUserPublicationRoleAndInviteManager(contentDbContext);

                Assert.False(await service.UserHasRoleOnPublication(
                    userPublicationRoleOtherPublication.UserId,
                    publication.Id,
                    PublicationRole.Owner));
            }
        }
    }

    public class GetDistinctRolesByUserTests : UserPublicationRoleAndInviteManagerTests
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

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            await contentDbContext.AddRangeAsync(userPublicationRoles);
            await contentDbContext.SaveChangesAsync();

            var service = SetupUserPublicationRoleAndInviteManager(contentDbContext);

            var result = await service.GetDistinctRolesByUser(user1.Id);

            // Expect only distinct roles to be returned, therefore the 2nd "Owner" role is filtered out.
            Assert.Equal([PublicationRole.Owner, PublicationRole.Allower], result);
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

            var service = SetupUserPublicationRoleAndInviteManager(contentDbContext);

            var result = await service.GetDistinctRolesByUser(user.Id);

            Assert.Empty(result);
        }
    }

    public class GetAllRolesByUserAndPublicationTests : UserPublicationRoleAndInviteManagerTests
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

            await using var contentDbContext = InMemoryApplicationDbContext();
            await contentDbContext.AddRangeAsync(userPublicationRoles);
            await contentDbContext.SaveChangesAsync();

            var service = SetupUserPublicationRoleAndInviteManager(contentDbContext);

            var result = await service.GetAllRolesByUserAndPublication(
                userId: user1.Id,
                publicationId: publication1.Id);

            Assert.Equal([PublicationRole.Owner, PublicationRole.Allower], result);
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

            await using var contentDbContext = InMemoryApplicationDbContext();
            await contentDbContext.AddRangeAsync(userPublicationRoles);
            await contentDbContext.SaveChangesAsync();

            var service = SetupUserPublicationRoleAndInviteManager(contentDbContext);

            var result = await service.GetAllRolesByUserAndPublication(
                userId: user.Id,
                publicationId: publication.Id);

            Assert.Equal([PublicationRole.Allower], result);
        }
    }

    public class RemoveRoleAndInviteTests : UserPublicationRoleAndInviteManagerTests
    {
        [Theory]
        // Valid roles
        [InlineData(PublicationRole.Allower, true)]
        [InlineData(PublicationRole.Allower, false)]
        // Invalid roles
        [InlineData(PublicationRole.Approver, true)]
        [InlineData(PublicationRole.Approver, false)]
        public async Task Success(PublicationRole publicationRole, bool ignoreQueryFilters)
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

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.Add(userPublicationRole);
            await contentDbContext.SaveChangesAsync();

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(MockBehavior.Strict);
            userPublicationInviteRepository
                .Setup(m => m.Remove(
                    userPublicationRole.PublicationId,
                    email,
                    userPublicationRole.Role,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = SetupUserPublicationRoleAndInviteManager(
                contentDbContext: contentDbContext,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            await service.RemoveRoleAndInvite(userPublicationRole);

            MockUtils.VerifyAllMocks(userPublicationInviteRepository);

            var query = contentDbContext.UserPublicationRoles.AsQueryable();

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            var updatedPublicationRole = query
                .SingleOrDefault(urr => urr.Id == userPublicationRole.Id);

            Assert.Null(updatedPublicationRole);
        }
    }

    public class RemoveRolesAndInvitesTests : UserPublicationRoleAndInviteManagerTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Success(bool ignoreQueryFilters)
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

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationRoles.AddRange(userPublicationRole1, userPublicationRole2, userPublicationRole3);
            contentDbContext.UserPublicationInvites.AddRange(userPublicationInvite1, userPublicationInvite2, userPublicationInvite3);
            await contentDbContext.SaveChangesAsync();

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(MockBehavior.Strict);
            userPublicationInviteRepository
                .Setup(m => m.RemoveMany(
                    new[] { userPublicationInvite1, userPublicationInvite2 },
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = SetupUserPublicationRoleAndInviteManager(
                contentDbContext: contentDbContext,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            await service.RemoveRolesAndInvites([userPublicationRole1, userPublicationRole2]);

            MockUtils.VerifyAllMocks(userPublicationInviteRepository);

            var userPublicationRolesQuery = contentDbContext.UserPublicationRoles.AsQueryable();

            if (ignoreQueryFilters)
            {
                userPublicationRolesQuery = userPublicationRolesQuery.IgnoreQueryFilters();
            }

            var userPublicationRole = await userPublicationRolesQuery
                .SingleAsync();

            Assert.Equal(userPublicationRole3.Id, userPublicationRole.Id);
        }
    }

    public class RemoveAllRolesAndInvitesForUserTests : UserPublicationRoleAndInviteManagerTests
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

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
            await contentDbContext.SaveChangesAsync();

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(MockBehavior.Strict);
            userPublicationInviteRepository
                .Setup(m => m.RemoveByUserEmail(
                    targetUser.Email,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var manager = SetupUserPublicationRoleAndInviteManager(
                contentDbContext: contentDbContext,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            await manager.RemoveAllRolesAndInvitesForUser(targetUser.Id);

            MockUtils.VerifyAllMocks(userPublicationInviteRepository);

            var remainingRoles = await contentDbContext.UserPublicationRoles
                .Include(upr => upr.User)
                .ToListAsync();

            Assert.Equal(2, remainingRoles.Count);

            Assert.Equal(publication1.Id, remainingRoles[0].PublicationId);
            Assert.Equal(otherUser, remainingRoles[0].User);
            Assert.Equal(role1, remainingRoles[0].Role);

            Assert.Equal(publication2.Id, remainingRoles[1].PublicationId);
            Assert.Equal(otherUser, remainingRoles[1].User);
            Assert.Equal(role2, remainingRoles[1].Role);
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

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.Users.AddRange(targetUser, otherUser);
            contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
            await contentDbContext.SaveChangesAsync();

            var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(MockBehavior.Strict);
            userPublicationInviteRepository
                .Setup(m => m.RemoveByUserEmail(
                    targetUser.Email,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var manager = SetupUserPublicationRoleAndInviteManager(
                contentDbContext: contentDbContext,
                userPublicationInviteRepository: userPublicationInviteRepository.Object);

            await manager.RemoveAllRolesAndInvitesForUser(targetUser.Id);

            MockUtils.VerifyAllMocks(userPublicationInviteRepository);

            var remainingRoles = await contentDbContext.UserPublicationRoles
                .Include(upr => upr.User)
                .ToListAsync();

            Assert.Equal(2, remainingRoles.Count);

            Assert.Equal(publication1.Id, remainingRoles[0].PublicationId);
            Assert.Equal(otherUser, remainingRoles[0].User);
            Assert.Equal(role1, remainingRoles[0].Role);

            Assert.Equal(publication2.Id, remainingRoles[1].PublicationId);
            Assert.Equal(otherUser, remainingRoles[1].User);
            Assert.Equal(role2, remainingRoles[1].Role);
        }
    }

    private static UserPublicationRoleAndInviteManager SetupUserPublicationRoleAndInviteManager(
        ContentDbContext contentDbContext,
        IUserPublicationInviteRepository? userPublicationInviteRepository = null)
    {
        return new(
            contentDbContext: contentDbContext,
            userPublicationInviteRepository: userPublicationInviteRepository ?? Mock.Of<IUserPublicationInviteRepository>(),
            userRepository: new UserRepository(contentDbContext));
    }
}
