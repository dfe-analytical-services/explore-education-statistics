#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserPublicationInviteRepositoryTests
{
    private readonly DataFixture _fixture = new();

    public class CreateManyIfNotExistsTests : UserPublicationInviteRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var createdById = Guid.NewGuid();
            var userPublicationRole1 = new UserPublicationRoleCreateRequest
            {
                PublicationId = Guid.NewGuid(),
                PublicationRole = PublicationRole.Owner,
            };
            var userPublicationRole2 = new UserPublicationRoleCreateRequest
            {
                PublicationId = Guid.NewGuid(),
                PublicationRole = PublicationRole.Allower,
            };
            var existingPublicationInvite = new UserPublicationInvite
            {
                Email = "test@test.com",
                PublicationId = Guid.NewGuid(),
                Role = PublicationRole.Owner,
                CreatedById = createdById,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(existingPublicationInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);
                await repository.CreateManyIfNotExists(
                    userPublicationRoles: ListOf(
                        userPublicationRole1,
                        userPublicationRole2,
                        new UserPublicationRoleCreateRequest
                        {
                            PublicationId = existingPublicationInvite.PublicationId,
                            PublicationRole = existingPublicationInvite.Role,
                        }),
                    email: "test@test.com",
                    createdById: createdById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationInvites = await contentDbContext.UserPublicationInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Equal(3, userPublicationInvites.Count);

                Assert.Equal(existingPublicationInvite.Id, userPublicationInvites[0].Id);
                Assert.Equal(existingPublicationInvite.PublicationId, userPublicationInvites[0].PublicationId);
                Assert.Equal(existingPublicationInvite.Role, userPublicationInvites[0].Role);
                Assert.Equal("test@test.com", userPublicationInvites[0].Email);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userPublicationInvites[0].CreatedById);

                Assert.Equal(userPublicationRole1.PublicationId, userPublicationInvites[1].PublicationId);
                Assert.Equal(userPublicationRole1.PublicationRole, userPublicationInvites[1].Role);
                Assert.Equal("test@test.com", userPublicationInvites[1].Email);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[1].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userPublicationInvites[1].CreatedById);

                Assert.Equal(userPublicationRole2.PublicationId, userPublicationInvites[2].PublicationId);
                Assert.Equal(userPublicationRole2.PublicationRole, userPublicationInvites[2].Role);
                Assert.Equal("test@test.com", userPublicationInvites[2].Email);
                Assert.InRange(DateTime.UtcNow.Subtract(userPublicationInvites[2].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userPublicationInvites[2].CreatedById);
            }
        }
    }

    public class RemoveTests : UserPublicationInviteRepositoryTests
    {
        [Fact]
        public async Task TargetInvitesExist_RemovesTargetedInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var targetRole = PublicationRole.Owner;
            var otherRole = PublicationRole.Allower;
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
                .Generate();

            var userPublicationInvites = _fixture.DefaultUserPublicationInvite()
                // These 2 invites should be removed
                .ForIndex(0, s => s.SetPublication(targetPublication))
                .ForIndex(0, s => s.SetEmail(targetEmail))
                .ForIndex(0, s => s.SetRole(targetRole))
                .ForIndex(1, s => s.SetPublication(targetPublication))
                .ForIndex(1, s => s.SetEmail(targetEmail))
                .ForIndex(1, s => s.SetRole(targetRole))
                // This invite is for a different publication and should not be removed
                .ForIndex(2, s => s.SetPublication(otherPublication))
                .ForIndex(2, s => s.SetEmail(targetEmail))
                .ForIndex(2, s => s.SetRole(targetRole))
                // This invite is for a different email and should not be removed
                .ForIndex(3, s => s.SetPublication(targetPublication))
                .ForIndex(3, s => s.SetEmail(otherEmail))
                .ForIndex(3, s => s.SetRole(targetRole))
                // This invite is for a different role and should not be removed
                .ForIndex(4, s => s.SetPublication(targetPublication))
                .ForIndex(4, s => s.SetEmail(targetEmail))
                .ForIndex(4, s => s.SetRole(otherRole))
                .GenerateList(5);

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationInvites.AddRange(userPublicationInvites);
            await contentDbContext.SaveChangesAsync();

            var repository = CreateRepository(contentDbContext);

            await repository.Remove(
                publicationId: targetPublication.Id,
                email: targetEmail,
                role: targetRole);

            var remainingInvites = await contentDbContext.UserPublicationInvites
                .ToListAsync();

            Assert.Equal(3, remainingInvites.Count);

            Assert.Equal(otherPublication.Id, remainingInvites[0].PublicationId);
            Assert.Equal(targetEmail, remainingInvites[0].Email);
            Assert.Equal(targetRole, remainingInvites[0].Role);

            Assert.Equal(targetPublication.Id, remainingInvites[1].PublicationId);
            Assert.Equal(otherEmail, remainingInvites[1].Email);
            Assert.Equal(targetRole, remainingInvites[1].Role);

            Assert.Equal(targetPublication.Id, remainingInvites[2].PublicationId);
            Assert.Equal(targetEmail, remainingInvites[2].Email);
            Assert.Equal(otherRole, remainingInvites[2].Role);
        }

        [Fact]
        public async Task NoInvitesExist_DoesNothing()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await repository.Remove(
                publicationId: Guid.NewGuid(),
                email: "test1@test.com",
                role: PublicationRole.Owner);

            var remainingInvites = await contentDbContext.UserPublicationInvites
                .ToListAsync();

            Assert.Empty(remainingInvites);
        }
    }

    public class RemoveManyTests : UserPublicationInviteRepositoryTests
    {
        [Fact]
        public async Task TargetInvitesExist_RemovesTargetedInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var targetRole = PublicationRole.Owner;
            var otherRole = PublicationRole.Allower;
            var targetPublication = _fixture.DefaultPublication()
                .Generate();
            var otherPublication = _fixture.DefaultPublication()
                .Generate();

            var userPublicationInvites = _fixture.DefaultUserPublicationInvite()
                // These 2 invites should be removed
                .ForIndex(0, s => s.SetPublication(targetPublication))
                .ForIndex(0, s => s.SetEmail(targetEmail))
                .ForIndex(0, s => s.SetRole(targetRole))
                .ForIndex(1, s => s.SetPublication(targetPublication))
                .ForIndex(1, s => s.SetEmail(targetEmail))
                .ForIndex(1, s => s.SetRole(targetRole))
                // This invite is for a different publication and should not be removed
                .ForIndex(2, s => s.SetPublication(otherPublication))
                .ForIndex(2, s => s.SetEmail(targetEmail))
                .ForIndex(2, s => s.SetRole(targetRole))
                // This invite is for a different email and should not be removed
                .ForIndex(3, s => s.SetPublication(targetPublication))
                .ForIndex(3, s => s.SetEmail(otherEmail))
                .ForIndex(3, s => s.SetRole(targetRole))
                // This invite is for a different role and should not be removed
                .ForIndex(4, s => s.SetPublication(targetPublication))
                .ForIndex(4, s => s.SetEmail(targetEmail))
                .ForIndex(4, s => s.SetRole(otherRole))
                .GenerateList(5);

            var userPublicationInvitesToRemove = new[]
            {
                userPublicationInvites[0],
                userPublicationInvites[1],
            };

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationInvites.AddRange(userPublicationInvites);
            await contentDbContext.SaveChangesAsync();

            var repository = CreateRepository(contentDbContext);

            await repository.RemoveMany(userPublicationInvitesToRemove);

            var remainingInvites = await contentDbContext.UserPublicationInvites
                .ToListAsync();

            Assert.Equal(3, remainingInvites.Count);

            Assert.Equal(otherPublication.Id, remainingInvites[0].PublicationId);
            Assert.Equal(targetEmail, remainingInvites[0].Email);
            Assert.Equal(targetRole, remainingInvites[0].Role);

            Assert.Equal(targetPublication.Id, remainingInvites[1].PublicationId);
            Assert.Equal(otherEmail, remainingInvites[1].Email);
            Assert.Equal(targetRole, remainingInvites[1].Role);

            Assert.Equal(targetPublication.Id, remainingInvites[2].PublicationId);
            Assert.Equal(targetEmail, remainingInvites[2].Email);
            Assert.Equal(otherRole, remainingInvites[2].Role);
        }

        [Fact]
        public async Task TargetInviteDoesNotExist_ThrowsException()
        {
            var existingUserPublicationInvite = _fixture.DefaultUserPublicationInvite()
                .WithPublication(_fixture.DefaultPublication())
                .WithEmail("test@test.com")
                .WithRole(PublicationRole.Owner)
                .Generate();

            var targetUserPublicationInvite = _fixture.DefaultUserPublicationInvite()
                .WithPublication(_fixture.DefaultPublication())
                .WithEmail("test@test.com")
                .WithRole(PublicationRole.Owner)
                .Generate();

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationInvites.Add(existingUserPublicationInvite);
            await contentDbContext.SaveChangesAsync();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
                await repository.RemoveMany([targetUserPublicationInvite]));
        }

        [Fact]
        public async Task EmptyList_DoesNothing()
        {
            var existingUserPublicationInvite = _fixture.DefaultUserPublicationInvite()
                .WithPublication(_fixture.DefaultPublication())
                .WithEmail("test@test.com")
                .WithRole(PublicationRole.Owner)
                .Generate();

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationInvites.Add(existingUserPublicationInvite);
            await contentDbContext.SaveChangesAsync();

            var repository = CreateRepository(contentDbContext);

            await repository.RemoveMany([]);

            var remainingInvites = await contentDbContext.UserPublicationInvites
                .ToListAsync();

            var remainingInvite = Assert.Single(remainingInvites);

            Assert.Equal(existingUserPublicationInvite.PublicationId, remainingInvite.PublicationId);
            Assert.Equal(existingUserPublicationInvite.Email, remainingInvite.Email);
            Assert.Equal(existingUserPublicationInvite.Role, remainingInvite.Role);
        }
    }

    public class RemoveByUserTests : UserPublicationInviteRepositoryTests
    {
        [Fact]
        public async Task TargetUserHasInvites_RemovesTargetInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var role1 = PublicationRole.Owner;
            var role2 = PublicationRole.Allower;
            var publication1 = _fixture.DefaultPublication()
                .Generate();
            var publication2 = _fixture.DefaultPublication()
                .Generate();

            var userPublicationInvites = _fixture.DefaultUserPublicationInvite()
                // These 2 invites should be removed
                .ForIndex(0, s => s.SetPublication(publication1))
                .ForIndex(0, s => s.SetEmail(targetEmail))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetPublication(publication2))
                .ForIndex(1, s => s.SetEmail(targetEmail))
                .ForIndex(1, s => s.SetRole(role2))
                // These invites are for a different email and should not be removed
                .ForIndex(2, s => s.SetPublication(publication1))
                .ForIndex(2, s => s.SetEmail(otherEmail))
                .ForIndex(2, s => s.SetRole(role1))
                .ForIndex(3, s => s.SetPublication(publication2))
                .ForIndex(3, s => s.SetEmail(otherEmail))
                .ForIndex(3, s => s.SetRole(role2))
                .GenerateList(4);

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationInvites.AddRange(userPublicationInvites);
            await contentDbContext.SaveChangesAsync();

            var repository = CreateRepository(contentDbContext);

            await repository.RemoveByUser(targetEmail);

            var remainingInvites = await contentDbContext.UserPublicationInvites
                .ToListAsync();

            Assert.Equal(2, remainingInvites.Count);

            Assert.Equal(publication1.Id, remainingInvites[0].PublicationId);
            Assert.Equal(otherEmail, remainingInvites[0].Email);
            Assert.Equal(role1, remainingInvites[0].Role);

            Assert.Equal(publication2.Id, remainingInvites[1].PublicationId);
            Assert.Equal(otherEmail, remainingInvites[1].Email);
            Assert.Equal(role2, remainingInvites[1].Role);
        }

        [Fact]
        public async Task TargetUserHasNoInvites_DoesNothing()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var role1 = PublicationRole.Owner;
            var role2 = PublicationRole.Allower;
            var publication1 = _fixture.DefaultPublication()
                .Generate();
            var publication2 = _fixture.DefaultPublication()
                .Generate();

            var userPublicationInvites = _fixture.DefaultUserPublicationInvite()
                // These invites are for a different email and should not be removed
                .ForIndex(0, s => s.SetPublication(publication1))
                .ForIndex(0, s => s.SetEmail(otherEmail))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetPublication(publication2))
                .ForIndex(1, s => s.SetEmail(otherEmail))
                .ForIndex(1, s => s.SetRole(role2))
                .GenerateList(2);

            await using var contentDbContext = InMemoryApplicationDbContext();

            contentDbContext.UserPublicationInvites.AddRange(userPublicationInvites);
            await contentDbContext.SaveChangesAsync();

            var repository = CreateRepository(contentDbContext);

            await repository.RemoveByUser(targetEmail);

            var remainingInvites = await contentDbContext.UserPublicationInvites
                .ToListAsync();

            Assert.Equal(2, remainingInvites.Count);

            Assert.Equal(publication1.Id, remainingInvites[0].PublicationId);
            Assert.Equal(otherEmail, remainingInvites[0].Email);
            Assert.Equal(role1, remainingInvites[0].Role);

            Assert.Equal(publication2.Id, remainingInvites[1].PublicationId);
            Assert.Equal(otherEmail, remainingInvites[1].Email);
            Assert.Equal(role2, remainingInvites[1].Role);
        }
    }

    private static UserPublicationInviteRepository CreateRepository(ContentDbContext contentDbContext)
    {
        return new UserPublicationInviteRepository(contentDbContext);
    }
}
