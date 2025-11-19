#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserPublicationRoleRepositoryTests
{
    private readonly DataFixture _fixture = new();

    public class CreateTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Create()
        {
            User user = _fixture.DefaultUser();

            User createdBy = _fixture.DefaultUser();

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
                var repository = CreateRepository(contentDbContext);

                var result = await repository.Create(user.Id, publication.Id, PublicationRole.Owner, createdBy.Id);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(PublicationRole.Owner, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
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
                Assert.Null(userPublicationRole.EmailSent);
            }
        }
    }

    public class UserHasRoleOnPublication_TrueIfRoleExistsTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task UserHasRoleOnPublication_TrueIfRoleExists()
        {
            var userPublicationRole = new UserPublicationRole
            {
                User = _fixture.DefaultUser().Generate(),
                Publication = new Publication(),
                Role = PublicationRole.Owner,
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

                Assert.True(
                    await repository.UserHasRoleOnPublication(
                        userPublicationRole.UserId,
                        userPublicationRole.PublicationId,
                        PublicationRole.Owner
                    )
                );
            }
        }

        [Fact]
        public async Task UserHasRoleOnPublication_FalseIfRoleDoesNotExist()
        {
            var publication = new Publication();

            // Setup a role but for a different publication to make sure it has no influence
            var userPublicationRoleOtherPublication = new UserPublicationRole
            {
                User = _fixture.DefaultUser().Generate(),
                Publication = new Publication(),
                Role = PublicationRole.Owner,
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

                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userPublicationRoleOtherPublication.UserId,
                        publication.Id,
                        PublicationRole.Owner
                    )
                );
            }
        }
    }

    public class ListDistinctRolesByUserTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();

            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var userPublicationRoles = new List<UserPublicationRole>
            {
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication1)
                    .WithRole(PublicationRole.Owner)
                    .Generate(),
                // Roles for different publication. One duplicate role.
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication2)
                    .WithRole(PublicationRole.Owner)
                    .Generate(),
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication2)
                    .WithRole(PublicationRole.Allower)
                    .Generate(),
                // Role for different user
                _fixture
                    .DefaultUserPublicationRole()
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

                var result = await repository.ListDistinctRolesByUser(user1.Id);

                // Expect only distinct roles to be returned, therefore the 2nd "Owner" role is filtered out.
                Assert.Equal([PublicationRole.Owner, PublicationRole.Allower], result);
            }
        }

        // This test will be changed when we start introducing the use of the NEW publication roles in the
        // UI, in STEP 9 (EES-6196) of the Permissions Rework. For now, we want to
        // filter out any usage of the NEW roles.
        [Fact]
        public async Task InvalidRolesNotReturned()
        {
            User user = _fixture.DefaultUser();

            Publication publication = _fixture.DefaultPublication();

            var userPublicationRoles = new List<UserPublicationRole>
            {
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithPublication(publication)
                    .WithRole(PublicationRole.Approver)
                    .Generate(),
                _fixture
                    .DefaultUserPublicationRole()
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

                var result = await repository.ListDistinctRolesByUser(user.Id);

                Assert.Empty(result);
            }
        }
    }

    public class ListRolesByUserAndPublicationTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();

            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var userPublicationRoles = new List<UserPublicationRole>
            {
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication1)
                    .WithRole(PublicationRole.Owner)
                    .Generate(),
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication1)
                    .WithRole(PublicationRole.Allower)
                    .Generate(),
                // Different role for different publication.
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication2)
                    .WithRole(PublicationRole.Allower)
                    .Generate(),
                // Different role for different user
                _fixture
                    .DefaultUserPublicationRole()
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

                var result = await repository.ListRolesByUserAndPublication(
                    userId: user1.Id,
                    publicationId: publication1.Id
                );

                Assert.Equal([PublicationRole.Owner, PublicationRole.Allower], result);
            }
        }

        // This test will be changed when we start introducing the use of the NEW publication roles in the
        // UI, in STEP 9 (EES-6196) of the Permissions Rework. For now, we want to
        // filter out any usage of the NEW roles.
        [Fact]
        public async Task InvalidRolesNotReturned()
        {
            User user = _fixture.DefaultUser();

            Publication publication = _fixture.DefaultPublication();

            var userPublicationRoles = new List<UserPublicationRole>
            {
                // Invalid role
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithPublication(publication)
                    .WithRole(PublicationRole.Approver)
                    .Generate(),
                // Invalid role
                _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithPublication(publication)
                    .WithRole(PublicationRole.Drafter)
                    .Generate(),
                // Valid role
                _fixture
                    .DefaultUserPublicationRole()
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

                var result = await repository.ListRolesByUserAndPublication(
                    userId: user.Id,
                    publicationId: publication.Id
                );

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

            var userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(_fixture.DefaultUser().WithEmail(email).Generate())
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

                var userPublicationRoleToRemove = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .SingleAsync(urr => urr.Id == userPublicationRole.Id);

                await repository.Remove(userPublicationRoleToRemove);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedPublicationRole = await contentDbContext.UserPublicationRoles.SingleOrDefaultAsync(urr =>
                    urr.Id == userPublicationRole.Id
                );

                Assert.Null(updatedPublicationRole);
            }
        }
    }

    public class RemoveManyTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            User user1 = _fixture.DefaultUser().WithEmail("test1@test.com");
            Publication publication1 = _fixture.DefaultPublication();
            UserPublicationRole userPublicationRole1 = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user1)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Allower);
            UserPublicationInvite userPublicationInvite1 = _fixture
                .DefaultUserPublicationInvite()
                .WithEmail(user1.Email)
                .WithPublication(publication1)
                .WithRole(PublicationRole.Allower);

            User user2 = _fixture.DefaultUser().WithEmail("test2@test.com");
            Publication publication2 = _fixture.DefaultPublication();
            UserPublicationRole userPublicationRole2 = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Owner);
            UserPublicationInvite userPublicationInvite2 = _fixture
                .DefaultUserPublicationInvite()
                .WithEmail(user2.Email)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Owner);

            User user3 = _fixture.DefaultUser().WithEmail("test3@test.com");
            Publication publication3 = _fixture.DefaultPublication();
            UserPublicationRole userPublicationRole3 = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user3)
                .WithPublication(publication3)
                .WithRole(PublicationRole.Allower);
            UserPublicationInvite userPublicationInvite3 = _fixture
                .DefaultUserPublicationInvite()
                .WithEmail(user3.Email)
                .WithPublication(publication3)
                .WithRole(PublicationRole.Allower);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(
                    userPublicationRole1,
                    userPublicationRole2,
                    userPublicationRole3
                );
                contentDbContext.UserPublicationInvites.AddRange(
                    userPublicationInvite1,
                    userPublicationInvite2,
                    userPublicationInvite3
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany([userPublicationRole1, userPublicationRole2]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRole = await contentDbContext.UserPublicationRoles.SingleAsync();

                Assert.Equal(userPublicationRole3.Id, userPublicationRole.Id);
            }
        }
    }

    public class RemoveForUserTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task TargetUserHasRoles_RemovesTargetRoles()
        {
            User targetUser = _fixture.DefaultUser().WithEmail("test1@test.com");
            User otherUser = _fixture.DefaultUser().WithEmail("test2@test.com");
            var role1 = PublicationRole.Allower;
            var role2 = PublicationRole.Owner;
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
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
                var remainingRoles = await contentDbContext.UserPublicationRoles.Include(upr => upr.User).ToListAsync();

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
            User targetUser = _fixture.DefaultUser().WithEmail("test1@test.com");
            User otherUser = _fixture.DefaultUser().WithEmail("test2@test.com");
            var role1 = PublicationRole.Allower;
            var role2 = PublicationRole.Owner;
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
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
                var remainingRoles = await contentDbContext.UserPublicationRoles.Include(upr => upr.User).ToListAsync();

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

    public class MarkEmailAsSentTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task UnsuppliedEmailSentDate_UpdatesEmailSentDateToUtcNow()
        {
            UserPublicationRole userPublicationRole = _fixture.DefaultUserPublicationRole();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(
                    userId: userPublicationRole.UserId,
                    publicationId: userPublicationRole.PublicationId,
                    role: userPublicationRole.Role
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserPublicationRole = await contentDbContext.UserPublicationRoles.SingleAsync();

                Assert.Equal(userPublicationRole.Id, updatedUserPublicationRole.Id);
                updatedUserPublicationRole.EmailSent.AssertUtcNow();
            }
        }

        [Fact]
        public async Task SuppliedEmailSentDate_UpdatesEmailSentDateToSuppliedDate()
        {
            UserPublicationRole userPublicationRole = _fixture.DefaultUserPublicationRole();

            var emailSentDate = DateTimeOffset.UtcNow.AddDays(-2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(
                    userId: userPublicationRole.UserId,
                    publicationId: userPublicationRole.PublicationId,
                    role: userPublicationRole.Role,
                    emailSent: emailSentDate
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserPublicationRole = await contentDbContext.UserPublicationRoles.SingleAsync();

                Assert.Equal(userPublicationRole.Id, updatedUserPublicationRole.Id);
                updatedUserPublicationRole.EmailSent.AssertEqual(emailSentDate);
            }
        }

        [Fact]
        public async Task SuppliedEmailSentDateNotInUtc_EmailSentDateIsStoredAsUniversalTime()
        {
            UserPublicationRole userPublicationRole = _fixture.DefaultUserPublicationRole();

            // Supply a local date with a +2 hour offset (not UTC)
            var emailSentDateWithOffset = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.FromHours(2));
            var expectedUtcDate = emailSentDateWithOffset.ToUniversalTime();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(
                    userId: userPublicationRole.UserId,
                    publicationId: userPublicationRole.PublicationId,
                    role: userPublicationRole.Role,
                    emailSent: emailSentDateWithOffset
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserPublicationRole = await contentDbContext.UserPublicationRoles.SingleAsync();

                Assert.Equal(userPublicationRole.Id, updatedUserPublicationRole.Id);

                // The stored value should be in UTC
                Assert.Equal(expectedUtcDate, updatedUserPublicationRole.EmailSent);
                Assert.Equal(TimeSpan.Zero, updatedUserPublicationRole.EmailSent!.Value.Offset);
            }
        }

        [Fact]
        public async Task UserPublicationRoleDoesNotExist_Throws()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.MarkEmailAsSent(
                    userId: Guid.NewGuid(),
                    publicationId: Guid.NewGuid(),
                    role: PublicationRole.Allower
                )
            );
        }
    }

    private static UserPublicationRoleRepository CreateRepository(ContentDbContext contentDbContext)
    {
        return new(contentDbContext);
    }
}
