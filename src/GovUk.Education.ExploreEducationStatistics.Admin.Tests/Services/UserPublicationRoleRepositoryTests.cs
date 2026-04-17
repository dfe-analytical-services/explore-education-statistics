#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPublicationRoleRepository;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserPublicationRoleRepositoryTests
{
    private readonly DataFixture _fixture = new();

    public static readonly TheoryData<Func<DataFixture, User>> AllTypesOfUser =
    [
        // Active User
        fixture => fixture.DefaultUser(),
        // User with Pending Invite
        fixture => fixture.DefaultUserWithPendingInvite(),
        // User with Expired Invite
        fixture => fixture.DefaultUserWithExpiredInvite(),
        // Soft Deleted User (These ones shouldn't ever have associated roles, as they
        // get removed when the user is soft deleted. But is added here for completeness)
        fixture => fixture.DefaultSoftDeletedUser(),
    ];

    public class CreateTests : UserPublicationRoleRepositoryTests
    {
        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task NoPublicationRoleChanges(Func<DataFixture, User> userFactory)
        {
            var publicationRoleToRemain = PublicationRole.Approver;

            User user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRoleToRemain);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.Create(user.Id, publication.Id, publicationRoleToRemain, createdBy.Id);

                // Should not have created a new role, as the same role already exists for the user/publication combination
                Assert.Null(result);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                var unchangedPublicationRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, unchangedPublicationRole.Id);
                Assert.Equal(user.Id, unchangedPublicationRole.UserId);
                Assert.Equal(publication.Id, unchangedPublicationRole.PublicationId);
                Assert.Equal(publicationRoleToRemain, unchangedPublicationRole.Role);
                unchangedPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, unchangedPublicationRole.CreatedById);
                Assert.Null(unchangedPublicationRole.EmailSent);
            }
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task PublicationRolesToRemoveAndCreate(Func<DataFixture, User> userFactory)
        {
            var publicationRoleToCreate = PublicationRole.Approver;
            var publicationRoleToRemove = PublicationRole.Drafter;

            User user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRoleToRemove);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.Create(user.Id, publication.Id, publicationRoleToCreate, createdBy.Id);

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(publicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                // Should be  as the 'Drafter` role has been removed and replaced with the `Approver` role
                var createdPublicationRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, createdPublicationRole.Id);
                Assert.Equal(user.Id, createdPublicationRole.UserId);
                Assert.Equal(publication.Id, createdPublicationRole.PublicationId);
                Assert.Equal(publicationRoleToCreate, createdPublicationRole.Role);
                createdPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdPublicationRole.CreatedById);
                Assert.Null(createdPublicationRole.EmailSent);
            }
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task PublicationRoleToCreate(Func<DataFixture, User> userFactory)
        {
            var publicationRoleToCreate = PublicationRole.Drafter;

            User user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.Create(user.Id, publication.Id, publicationRoleToCreate, createdBy.Id);

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(publicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                var createdPublicationRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, createdPublicationRole.Id);
                Assert.Equal(user.Id, createdPublicationRole.UserId);
                Assert.Equal(publication.Id, createdPublicationRole.PublicationId);
                Assert.Equal(publicationRoleToCreate, createdPublicationRole.Role);
                createdPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdPublicationRole.CreatedById);
                Assert.Null(createdPublicationRole.EmailSent);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Allower)]
        [InlineData(PublicationRole.Owner)]
        public async Task OldRole_Throws(PublicationRole oldSystemPublicationRoleToCreate)
        {
            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await repository.Create(user.Id, publication.Id, oldSystemPublicationRoleToCreate, createdBy.Id)
                );
            }
        }
    }

    public class CreateManyIfNotExistsTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task ManyRoles_IgnoresRolesThatAlreadyExist()
        {
            var existingRoleCreatedDate = DateTime.UtcNow.AddDays(-2);
            var newRolesCreatedDate = DateTime.UtcNow;
            User createdBy = _fixture.DefaultUser();

            // Test across multiple User/Publication combinations
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();
            User user3 = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();
            Publication publication3 = _fixture.DefaultPublication();

            var existingUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreated(existingRoleCreatedDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(PublicationRole.Approver)
                // One 'Approver' role for some User/Publication combinations
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2))
                .GenerateList(4);

            var newUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreated(newRolesCreatedDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(PublicationRole.Drafter)
                // One 'Approver' role for different User/Publication combinations
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication3))
                .ForIndex(1, s => s.SetUser(user2).SetPublication(publication3))
                .ForIndex(2, s => s.SetUser(user3).SetPublication(publication1))
                .ForIndex(3, s => s.SetUser(user3).SetPublication(publication2))
                .ForIndex(4, s => s.SetUser(user3).SetPublication(publication3))
                .GenerateList(5);

            UserPublicationRole[] allUserPublicationRoles =
            [
                .. newUserPublicationRoles,
                .. existingUserPublicationRoles,
            ];
            var allUserPublicationRolesCreateDtos = allUserPublicationRoles
                .Select(upr => new UserPublicationRoleCreateDto(
                    UserId: upr.UserId,
                    PublicationId: upr.PublicationId,
                    Role: upr.Role,
                    CreatedById: upr.CreatedById!.Value,
                    CreatedDate: upr.Created
                ))
                .ToHashSet();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingUserPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Pass ALL roles (existing + new) to CreateManyIfNotExists. We want to test that
                // it only creates the new ones, and the existing ones are ignored/untouched.
                var results = await repository.CreateManyIfNotExists(allUserPublicationRolesCreateDtos);

                // Should only have created 5 new roles, as the others already existed
                Assert.Equal(5, results.Count);

                Assert.All(
                    results,
                    upr =>
                    {
                        Assert.Equal(PublicationRole.Drafter, upr.Role);
                        Assert.Equal(newRolesCreatedDate, upr.Created);
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                var resultsUserPublicationIdPairs = results.Select(upr => (upr.UserId, upr.PublicationId)).ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid)>
                    {
                        (user1.Id, publication3.Id),
                        (user2.Id, publication3.Id),
                        (user3.Id, publication1.Id),
                        (user3.Id, publication2.Id),
                        (user3.Id, publication3.Id),
                    },
                    resultsUserPublicationIdPairs
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                // Should have 8 roles in total: 4 existing + 4 new
                // The existing roles should be unchanged
                Assert.Equal(8, userPublicationRoles.Count);

                Assert.All(
                    userPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                var actual = userPublicationRoles
                    .Select(upr => (upr.UserId, upr.PublicationId, upr.Role, upr.Created))
                    .ToHashSet();

                var expected = new HashSet<(Guid UserId, Guid PublicationId, PublicationRole Role, DateTime Created)>
                {
                    // New roles
                    (user1.Id, publication3.Id, PublicationRole.Drafter, newRolesCreatedDate),
                    (user2.Id, publication3.Id, PublicationRole.Drafter, newRolesCreatedDate),
                    (user3.Id, publication1.Id, PublicationRole.Drafter, newRolesCreatedDate),
                    (user3.Id, publication2.Id, PublicationRole.Drafter, newRolesCreatedDate),
                    (user3.Id, publication3.Id, PublicationRole.Drafter, newRolesCreatedDate),
                    // Existing roles
                    (user1.Id, publication1.Id, PublicationRole.Approver, existingRoleCreatedDate),
                    (user2.Id, publication1.Id, PublicationRole.Approver, existingRoleCreatedDate),
                    (user1.Id, publication2.Id, PublicationRole.Approver, existingRoleCreatedDate),
                    (user2.Id, publication2.Id, PublicationRole.Approver, existingRoleCreatedDate),
                };

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task NoPublicationRoleChanges()
        {
            var publicationRoleToRemain = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRoleToRemain);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var results = await repository.CreateManyIfNotExists([
                    new(
                        UserId: user.Id,
                        PublicationId: publication.Id,
                        Role: publicationRoleToRemain,
                        CreatedById: createdBy.Id
                    ),
                ]);

                // Should not have created any new roles, as the same role already exists for the user/publication combination
                Assert.Empty(results);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                // Existing role should still exist, and no new roles should have been created
                Assert.Equal(2, userPublicationRoles.Count);

                var userPublicationRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, userPublicationRole.Id);
                Assert.Equal(user.Id, userPublicationRole.UserId);
                Assert.Equal(publication.Id, userPublicationRole.PublicationId);
                Assert.Equal(existingPublicationRole.Role, userPublicationRole.Role);
                userPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, userPublicationRole.CreatedById);
                Assert.Null(userPublicationRole.EmailSent);
            }
        }

        [Fact]
        public async Task PublicationRolesToRemoveAndCreate()
        {
            var publicationRoleToCreate = PublicationRole.Approver;
            var publicationRoleToRemove = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRoleToRemove);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var results = await repository.CreateManyIfNotExists([
                    new(
                        UserId: user.Id,
                        PublicationId: publication.Id,
                        Role: publicationRoleToCreate,
                        CreatedById: createdBy.Id
                    ),
                ]);

                var result = Assert.Single(results);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(publicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                // The 'Drafter' role should have been removed and replaced with the 'Approver' role,
                // so there should still only be 1 role for the user/publication combination.
                var createdPublicationRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, createdPublicationRole.Id);
                Assert.Equal(user.Id, createdPublicationRole.UserId);
                Assert.Equal(publication.Id, createdPublicationRole.PublicationId);
                Assert.Equal(publicationRoleToCreate, createdPublicationRole.Role);
                createdPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdPublicationRole.CreatedById);
                Assert.Null(createdPublicationRole.EmailSent);
            }
        }

        [Fact]
        public async Task PublicationRoleToCreate()
        {
            var publicationRoleToCreate = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var results = await repository.CreateManyIfNotExists([
                    new(
                        UserId: user.Id,
                        PublicationId: publication.Id,
                        Role: publicationRoleToCreate,
                        CreatedById: createdBy.Id
                    ),
                ]);

                var result = Assert.Single(results);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(publicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                var createdPublicationRole = Assert.Single(userPublicationRoles);

                Assert.NotEqual(Guid.Empty, createdPublicationRole.Id);
                Assert.Equal(user.Id, createdPublicationRole.UserId);
                Assert.Equal(publication.Id, createdPublicationRole.PublicationId);
                Assert.Equal(publicationRoleToCreate, createdPublicationRole.Role);
                createdPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdPublicationRole.CreatedById);
                Assert.Null(createdPublicationRole.EmailSent);
            }
        }

        [Fact]
        public async Task EmptySet_DoesNothing()
        {
            var repository = CreateRepository();

            var results = await repository.CreateManyIfNotExists([]);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(PublicationRole.Allower)]
        [InlineData(PublicationRole.Owner)]
        public async Task OldRole_Throws(PublicationRole oldSystemPublicationRoleToCreate)
        {
            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await repository.CreateManyIfNotExists([
                        // try creating a couple of NEW roles and one OLD role (in the middle of the list), to check that an error is thrown whenever
                        // the list contains a OLD role, even if it also contains NEW roles.
                        new(
                            UserId: user.Id,
                            PublicationId: publication.Id,
                            Role: PublicationRole.Approver,
                            CreatedById: createdBy.Id
                        ),
                        new(
                            UserId: user.Id,
                            PublicationId: publication.Id,
                            Role: oldSystemPublicationRoleToCreate,
                            CreatedById: createdBy.Id
                        ),
                        new(
                            UserId: user.Id,
                            PublicationId: publication.Id,
                            Role: PublicationRole.Drafter,
                            CreatedById: createdBy.Id
                        ),
                    ])
                );
            }
        }
    }

    public class GetByIdTests : UserPublicationRoleRepositoryTests
    {
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task Success(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetById(userPublicationRole.Id);

                Assert.NotNull(result);
                Assert.Equal(userPublicationRole.Id, result.Id);
                Assert.Equal(userPublicationRole.UserId, result.UserId);
                Assert.Equal(userPublicationRole.PublicationId, result.PublicationId);
                Assert.Equal(userPublicationRole.Role, result.Role);
                Assert.Equal(userPublicationRole.Created, result.Created);
                Assert.Equal(userPublicationRole.CreatedById, result.CreatedById);
                Assert.Equal(userPublicationRole.EmailSent, result.EmailSent);
            }
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNull()
        {
            var repository = CreateRepository();

            var result = await repository.GetById(Guid.NewGuid());

            Assert.Null(result);
        }
    }

    public class GetByCompositeKeyTests : UserPublicationRoleRepositoryTests
    {
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task Success(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetByCompositeKey(
                    userId: userPublicationRole.UserId,
                    publicationId: userPublicationRole.PublicationId,
                    role: userPublicationRole.Role
                );

                Assert.NotNull(result);
                Assert.Equal(userPublicationRole.Id, result.Id);
                Assert.Equal(userPublicationRole.UserId, result.UserId);
                Assert.Equal(userPublicationRole.PublicationId, result.PublicationId);
                Assert.Equal(userPublicationRole.Role, result.Role);
                Assert.Equal(userPublicationRole.Created, result.Created);
                Assert.Equal(userPublicationRole.CreatedById, result.CreatedById);
                Assert.Equal(userPublicationRole.EmailSent, result.EmailSent);
            }
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNull()
        {
            var repository = CreateRepository();

            var result = await repository.GetByCompositeKey(
                userId: Guid.NewGuid(),
                publicationId: Guid.NewGuid(),
                role: PublicationRole.Drafter
            );

            Assert.Null(result);
        }
    }

    public class QueryTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task ActiveOnlyFilter_ReturnsAllRolesForActiveUsers()
        {
            User activeUser1 = _fixture.DefaultUser();
            User activeUser2 = _fixture.DefaultUser();
            User userWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User userWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User softDeletedUser = _fixture.DefaultSoftDeletedUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should be returned
                .ForIndex(0, s => s.SetUser(activeUser1))
                .ForIndex(1, s => s.SetUser(activeUser1))
                .ForIndex(2, s => s.SetUser(activeUser2))
                // These should not
                .ForIndex(3, s => s.SetUser(userWithPendingInvite))
                .ForIndex(4, s => s.SetUser(userWithExpiredInvite))
                .ForIndex(5, s => s.SetUser(softDeletedUser))
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // ReSharper disable once RedundantArgumentDefaultValue
                var resultingQueryableWithExplicitFilter = repository.Query(ResourceRoleFilter.ActiveOnly);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var resultsWithExplicitFilter = await resultingQueryableWithExplicitFilter.ToListAsync();

                Assert.Equal(3, resultsWithExplicitFilter.Count);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userPublicationRoles[0].Id);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userPublicationRoles[1].Id);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userPublicationRoles[2].Id);

                // Also test that the default filter is ActiveOnly
                var resultingQueryableWithUnspecifiedFilter = repository.Query();

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var resultsWithUnspecifiedFilter = await resultingQueryableWithUnspecifiedFilter.ToListAsync();

                Assert.Equal(resultsWithExplicitFilter, resultsWithUnspecifiedFilter);
            }
        }

        [Fact]
        public async Task PendingOnlyFilter_ReturnsAllRolesForPendingUserInvites()
        {
            User userWithPendingInvite1 = _fixture.DefaultUserWithPendingInvite();
            User userWithPendingInvite2 = _fixture.DefaultUserWithPendingInvite();
            User activeUser = _fixture.DefaultUser();
            User userWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User softDeletedUser = _fixture.DefaultSoftDeletedUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should be returned
                .ForIndex(0, s => s.SetUser(userWithPendingInvite1))
                .ForIndex(1, s => s.SetUser(userWithPendingInvite1))
                .ForIndex(2, s => s.SetUser(userWithPendingInvite2))
                // These should not
                .ForIndex(3, s => s.SetUser(activeUser))
                .ForIndex(4, s => s.SetUser(userWithExpiredInvite))
                .ForIndex(5, s => s.SetUser(softDeletedUser))
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.PendingOnly);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(3, results.Count);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[2].Id);
            }
        }

        [Fact]
        public async Task AllButExpiredFilter_ReturnsAllRolesForActiveUsersOrPendingUserInvites()
        {
            User activeUser1 = _fixture.DefaultUser();
            User activeUser2 = _fixture.DefaultUser();
            User userWithPendingInvite1 = _fixture.DefaultUserWithPendingInvite();
            User userWithPendingInvite2 = _fixture.DefaultUserWithPendingInvite();
            User userWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User softDeletedUser = _fixture.DefaultSoftDeletedUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should be returned
                .ForIndex(0, s => s.SetUser(activeUser1))
                .ForIndex(1, s => s.SetUser(activeUser1))
                .ForIndex(2, s => s.SetUser(activeUser2))
                .ForIndex(3, s => s.SetUser(userWithPendingInvite1))
                .ForIndex(4, s => s.SetUser(userWithPendingInvite1))
                .ForIndex(5, s => s.SetUser(userWithPendingInvite2))
                // These should not
                .ForIndex(6, s => s.SetUser(userWithExpiredInvite))
                .ForIndex(7, s => s.SetUser(softDeletedUser))
                .GenerateList(8);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.AllButExpired);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(6, results.Count);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[2].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[3].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[4].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[5].Id);
            }
        }

        [Fact]
        public async Task AllFilter_ReturnsAllRoles()
        {
            User activeUser1 = _fixture.DefaultUser();
            User activeUser2 = _fixture.DefaultUser();
            User userWithPendingInvite1 = _fixture.DefaultUserWithPendingInvite();
            User userWithPendingInvite2 = _fixture.DefaultUserWithPendingInvite();
            User userWithExpiredInvite1 = _fixture.DefaultUserWithExpiredInvite();
            User userWithExpiredInvite2 = _fixture.DefaultUserWithExpiredInvite();
            User softDeletedUser1 = _fixture.DefaultSoftDeletedUser();
            User softDeletedUser2 = _fixture.DefaultSoftDeletedUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should ALL be returned
                .ForIndex(0, s => s.SetUser(activeUser1))
                .ForIndex(1, s => s.SetUser(activeUser1))
                .ForIndex(2, s => s.SetUser(activeUser2))
                .ForIndex(3, s => s.SetUser(userWithPendingInvite1))
                .ForIndex(4, s => s.SetUser(userWithPendingInvite1))
                .ForIndex(5, s => s.SetUser(userWithPendingInvite2))
                .ForIndex(6, s => s.SetUser(userWithExpiredInvite1))
                .ForIndex(7, s => s.SetUser(userWithExpiredInvite1))
                .ForIndex(8, s => s.SetUser(userWithExpiredInvite2))
                .ForIndex(9, s => s.SetUser(softDeletedUser1))
                .ForIndex(10, s => s.SetUser(softDeletedUser1))
                .ForIndex(11, s => s.SetUser(softDeletedUser2))
                .GenerateList(12);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.All);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(12, results.Count);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[2].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[3].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[4].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[5].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[6].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[7].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[8].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[9].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[10].Id);
                Assert.Single(results, upr => upr.Id == userPublicationRoles[11].Id);
            }
        }
    }

    public class RemoveByIdTests : UserPublicationRoleRepositoryTests
    {
        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task RoleExists_RemovesRoleAndReturnsTrue(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(PublicationRole.Approver)
                .WithUser(user)
                .WithPublication(publication);
            UserPublicationRole otherUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(PublicationRole.Drafter)
                .WithUser(_fixture.DefaultUser())
                .WithPublication(publication);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRole, otherUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.RemoveById(userPublicationRole.Id);

                Assert.True(result);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                // The existing 'Approver' role should have been deleted, but the other 'Drafter' role should remain,
                var remainingUserPublicationRole = Assert.Single(updatedPublicationRoles);

                Assert.Equal(otherUserPublicationRole.Id, remainingUserPublicationRole.Id);
            }
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsFalse()
        {
            var repository = CreateRepository();

            var result = await repository.RemoveById(Guid.NewGuid());

            Assert.False(result);
        }

        [Theory]
        [InlineData(PublicationRole.Owner)]
        [InlineData(PublicationRole.Allower)]
        public async Task OldRole_Throws(PublicationRole oldPublicationRoleToRemove)
        {
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(_fixture.DefaultUser())
                .WithPublication(_fixture.DefaultPublication());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(oldSystemUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await repository.RemoveById(oldSystemUserPublicationRole.Id)
                );
            }
        }
    }

    public class RemoveManyTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task ManyRoles()
        {
            // Test across multiple User/Publication combinations
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();
            User user3 = _fixture.DefaultUser();
            User user4 = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();
            Publication publication3 = _fixture.DefaultPublication();
            Publication publication4 = _fixture.DefaultPublication();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Drafter))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Drafter))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Drafter))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Drafter))
                .ForIndex(4, s => s.SetUser(user3).SetPublication(publication3).SetRole(PublicationRole.Approver))
                .ForIndex(5, s => s.SetUser(user3).SetPublication(publication4).SetRole(PublicationRole.Approver))
                .ForIndex(6, s => s.SetUser(user4).SetPublication(publication3).SetRole(PublicationRole.Approver))
                .ForIndex(7, s => s.SetUser(user4).SetPublication(publication4).SetRole(PublicationRole.Approver))
                .GenerateList(8);

            var userPublicationRolesToRemove = new List<UserPublicationRole>
            {
                userPublicationRoles[2],
                userPublicationRoles[6],
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany(userPublicationRolesToRemove);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have removed just the 2 specified roles, and left the other 6
                Assert.Equal(6, remainingUserPublicationRoles.Count);

                Assert.Equal(userPublicationRoles[0].Id, remainingUserPublicationRoles[0].Id);
                Assert.Equal(userPublicationRoles[1].Id, remainingUserPublicationRoles[1].Id);
                Assert.Equal(userPublicationRoles[3].Id, remainingUserPublicationRoles[3].Id);
                Assert.Equal(userPublicationRoles[4].Id, remainingUserPublicationRoles[4].Id);
                Assert.Equal(userPublicationRoles[5].Id, remainingUserPublicationRoles[5].Id);
                Assert.Equal(userPublicationRoles[7].Id, remainingUserPublicationRoles[7].Id);
            }
        }

        [Fact]
        public async Task EmptyList_DoesNothing()
        {
            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(_fixture.DefaultUser())
                .WithPublication(_fixture.DefaultPublication())
                .WithRole(PublicationRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany([]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserPublicationRole = await contentDbContext.UserPublicationRoles.SingleAsync();

                // Existing role should remain. Nothing should have happened.
                Assert.Equal(remainingUserPublicationRole.Id, userPublicationRole.Id);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Owner)]
        [InlineData(PublicationRole.Allower)]
        public async Task OldRole_Throws(PublicationRole oldPublicationRoleToRemove)
        {
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(_fixture.DefaultUser())
                .WithPublication(_fixture.DefaultPublication());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(oldSystemUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await repository.RemoveMany([oldSystemUserPublicationRole])
                );
            }
        }
    }

    public class RemoveForUserTests : UserPublicationRoleRepositoryTests
    {
        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task TargetUserHasRoles_RemovesTargetRoles(Func<DataFixture, User> userFactory)
        {
            User targetUser = userFactory(_fixture);
            User otherUser = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            // The OLD roles here will be removed in EES-6212
            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // These 4 roles should be removed (including the OLD permissions system roles)
                .ForIndex(0, s => s.SetPublication(publication1).SetUser(targetUser).SetRole(PublicationRole.Allower))
                .ForIndex(1, s => s.SetPublication(publication1).SetUser(targetUser).SetRole(PublicationRole.Owner))
                .ForIndex(2, s => s.SetPublication(publication2).SetUser(targetUser).SetRole(PublicationRole.Drafter))
                .ForIndex(3, s => s.SetPublication(publication2).SetUser(targetUser).SetRole(PublicationRole.Approver))
                // These roles are for a different email and should not be removed
                .ForIndex(4, s => s.SetPublication(publication1).SetUser(otherUser).SetRole(PublicationRole.Allower))
                .ForIndex(5, s => s.SetPublication(publication1).SetUser(otherUser).SetRole(PublicationRole.Owner))
                .ForIndex(6, s => s.SetPublication(publication2).SetUser(otherUser).SetRole(PublicationRole.Drafter))
                .ForIndex(7, s => s.SetPublication(publication2).SetUser(otherUser).SetRole(PublicationRole.Approver))
                .GenerateList(8);

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
                // Ignore query filters so that we can assess whether the correct NEW permissions system roles were deleted
                var remainingRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .Include(upr => upr.User)
                    .ToListAsync();

                Assert.Equal(4, remainingRoles.Count);

                Assert.Equal(publication1.Id, remainingRoles[0].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[0].User.Id);
                Assert.Equal(PublicationRole.Allower, remainingRoles[0].Role);
                Assert.Equal(publication1.Id, remainingRoles[1].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
                Assert.Equal(PublicationRole.Owner, remainingRoles[1].Role);
                Assert.Equal(publication2.Id, remainingRoles[2].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[2].User.Id);
                Assert.Equal(PublicationRole.Drafter, remainingRoles[2].Role);
                Assert.Equal(publication2.Id, remainingRoles[3].PublicationId);
                Assert.Equal(otherUser.Id, remainingRoles[3].User.Id);
                Assert.Equal(PublicationRole.Approver, remainingRoles[3].Role);
            }
        }

        [Fact]
        public async Task TargetUserHasNoRoles_DoesNothing()
        {
            User targetUser = _fixture.DefaultUser();
            User otherUser = _fixture.DefaultUser();
            const PublicationRole role1 = PublicationRole.Approver;
            const PublicationRole role2 = PublicationRole.Drafter;
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // These roles are for a different email and should not be removed
                .ForIndex(0, s => s.SetPublication(publication1).SetUser(otherUser).SetRole(role1))
                .ForIndex(1, s => s.SetPublication(publication2).SetUser(otherUser).SetRole(role2))
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

    public class UserHasRoleOnPublicationTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task ActiveOnlyFilter_RoleExists_TrueIfRoleIsForActiveUser()
        {
            User targetActiveUser = _fixture.DefaultUser();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetActiveUser)
                .WithPublication(targetPublication)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );

                // Also test that the default filter is ActiveOnly
                Assert.True(
                    await repository.UserHasRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleExists_FalseIfRoleIsNotForActiveUser()
        {
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();

            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // Target role is for a PENDING User Invite
                .ForIndex(
                    0,
                    s => s.SetUser(targetUserWithPendingInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s => s.SetUser(targetUserWithExpiredInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleDoesNotExist_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s => s.SetUser(targetActiveUser).SetPublication(_fixture.DefaultPublication()).SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
            }
        }

        [Fact]
        public async Task PendingOnlyFilter_RoleExists_TrueIfRoleIsForPendingUserInvite()
        {
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetUserWithPendingInvite)
                .WithPublication(targetPublication)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
            }
        }

        [Fact]
        public async Task PendingOnlyFilter_RoleExists_FalseIfRoleIsNotForPendingUserInvite()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();

            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // Target role is for a ACTIVE User
                .ForIndex(0, s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(targetRole))
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s => s.SetUser(targetUserWithExpiredInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for PENDING User Invites
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
            }
        }

        [Fact]
        public async Task PendingOnlyFilter_RoleDoesNotExist_False()
        {
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
            }
        }

        [Fact]
        public async Task AllButExpiredFilter_RoleExists_TrueIfRoleIsForActiveUserOrPendingUserInvite()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(targetPublication)
                .WithRole(targetRole)
                .ForIndex(0, s => s.SetUser(targetActiveUser))
                .ForIndex(1, s => s.SetUser(targetUserWithPendingInvite))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Both of these should return true as the roles are for an ACTIVE User and a PENDING User Invite
                Assert.True(
                    await repository.UserHasRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.True(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
            }
        }

        [Fact]
        public async Task AllButExpiredFilter_RoleExists_FalseIfRoleIsNotForActiveUserOrPendingUserInvite()
        {
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();

            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    0,
                    s => s.SetUser(targetUserWithExpiredInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    1,
                    s => s.SetUser(targetSoftDeletedUser).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users or PENDING User Invites
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
            }
        }

        [Fact]
        public async Task AllButExpiredFilter_RoleDoesNotExist_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s => s.SetUser(targetActiveUser).SetPublication(_fixture.DefaultPublication()).SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(targetPublication).SetRole(targetRole)
                )
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task AllFilter_RoleExists_TrueIrrelevantOfTypeOfUser(Func<DataFixture, User> userFactory)
        {
            var targetUser = userFactory(_fixture);
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetUser)
                .WithPublication(targetPublication)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }

        [Fact]
        public async Task AllFilter_RoleDoesNotExist_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s => s.SetUser(targetActiveUser).SetPublication(_fixture.DefaultPublication()).SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(targetPublication).SetRole(targetRole)
                )
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                // TARGET EXPIRED user invite + publication but DIFFERENT role
                .ForIndex(
                    6,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET EXPIRED user invite + role but DIFFERENT publication
                .ForIndex(
                    7,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT EXPIRED user invite
                .ForIndex(
                    8,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithExpiredInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                // TARGET SOFT DELETED user + publication but DIFFERENT role
                .ForIndex(
                    9,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET SOFT DELETED user + role but DIFFERENT publication
                .ForIndex(
                    10,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT SOFT DELETED user
                .ForIndex(
                    11,
                    s =>
                        s.SetUser(_fixture.DefaultSoftDeletedUser())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                .GenerateList(12);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }
    }

    public class UserHasAnyRoleOnPublicationTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task ExistingRoleNotInRolesToInclude_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRolesToInclude = PublicationRole.Drafter;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetActiveUser)
                .WithPublication(targetPublication)
                .WithRole(PublicationRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRolesToInclude
                    )
                );
            }
        }

        [Fact]
        public async Task EmptyRolesToInclude_TrueForAllRolesForMatchingUserAndPublication()
        {
            User targetActiveUser = _fixture.DefaultUser();
            Publication targetPublication = _fixture.DefaultPublication();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetActiveUser)
                .WithPublication(targetPublication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Drafter))
                .ForIndex(1, s => s.SetRole(PublicationRole.Approver))
                .GenerateList(2);

            var contentDbContextId1 = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId1))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRoles[0]);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId1))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id
                    )
                );
            }

            var contentDbContextId2 = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId2))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRoles[1]);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId2))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleExists_TrueIfRoleIsForActiveUser()
        {
            User targetActiveUser = _fixture.DefaultUser();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetActiveUser)
                .WithPublication(targetPublication)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );

                // Also test that the default filter is ActiveOnly
                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleExists_FalseIfRoleIsNotForActiveUser()
        {
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();

            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // Target role is for a PENDING User Invite
                .ForIndex(
                    0,
                    s => s.SetUser(targetUserWithPendingInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s => s.SetUser(targetUserWithExpiredInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleDoesNotExist_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s => s.SetUser(targetActiveUser).SetPublication(_fixture.DefaultPublication()).SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
            }
        }

        [Fact]
        public async Task PendingOnlyFilter_RoleExists_TrueIfRoleIsForPendingUserInvite()
        {
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetUserWithPendingInvite)
                .WithPublication(targetPublication)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
            }
        }

        [Fact]
        public async Task PendingOnlyFilter_RoleExists_FalseIfRoleIsNotForPendingUserInvite()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();

            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // Target role is for a ACTIVE User
                .ForIndex(0, s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(targetRole))
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s => s.SetUser(targetUserWithExpiredInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for PENDING User Invites
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
            }
        }

        [Fact]
        public async Task PendingOnlyFilter_RoleDoesNotExist_False()
        {
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
            }
        }

        [Fact]
        public async Task AllButExpiredFilter_RoleExists_TrueIfRoleIsForActiveUserOrPendingUserInvite()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(targetPublication)
                .WithRole(targetRole)
                .ForIndex(0, s => s.SetUser(targetActiveUser))
                .ForIndex(1, s => s.SetUser(targetUserWithPendingInvite))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Both of these should return true as the roles are for an ACTIVE User and a PENDING User Invite
                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
            }
        }

        [Fact]
        public async Task AllButExpiredFilter_RoleExists_FalseIfRoleIsNotForActiveUserOrPendingUserInvite()
        {
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();

            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    0,
                    s => s.SetUser(targetUserWithExpiredInvite).SetPublication(targetPublication).SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    1,
                    s => s.SetUser(targetSoftDeletedUser).SetPublication(targetPublication).SetRole(targetRole)
                )
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users or PENDING User Invites
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
            }
        }

        [Fact]
        public async Task AllButExpiredFilter_RoleDoesNotExist_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s => s.SetUser(targetActiveUser).SetPublication(_fixture.DefaultPublication()).SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(targetPublication).SetRole(targetRole)
                )
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task AllFilter_RoleExists_TrueIrrelevantOfTypeOfUser(Func<DataFixture, User> userFactory)
        {
            var targetUser = userFactory(_fixture);
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetUser)
                .WithPublication(targetPublication)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }

        [Fact]
        public async Task AllFilter_RoleDoesNotExist_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            User targetUserWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User targetUserWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User targetSoftDeletedUser = _fixture.DefaultSoftDeletedUser();
            Publication targetPublication = _fixture.DefaultPublication();
            const PublicationRole targetRole = PublicationRole.Drafter;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s => s.SetUser(targetActiveUser).SetPublication(_fixture.DefaultPublication()).SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(targetPublication).SetRole(targetRole)
                )
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                // TARGET EXPIRED user invite + publication but DIFFERENT role
                .ForIndex(
                    6,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET EXPIRED user invite + role but DIFFERENT publication
                .ForIndex(
                    7,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT EXPIRED user invite
                .ForIndex(
                    8,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithExpiredInvite())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                // TARGET SOFT DELETED user + publication but DIFFERENT role
                .ForIndex(
                    9,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Approver)
                )
                // TARGET SOFT DELETED user + role but DIFFERENT publication
                .ForIndex(
                    10,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetPublication(_fixture.DefaultPublication())
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT SOFT DELETED user
                .ForIndex(
                    11,
                    s =>
                        s.SetUser(_fixture.DefaultSoftDeletedUser())
                            .SetPublication(targetPublication)
                            .SetRole(targetRole)
                )
                .GenerateList(12);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetPublication.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }
    }

    public class MarkEmailAsSentTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Success_NoSuppliedDateSent()
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

                await repository.MarkEmailAsSent(userPublicationRole.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserPublicationRole = await contentDbContext.UserPublicationRoles.SingleAsync();

                Assert.Equal(userPublicationRole.Id, updatedUserPublicationRole.Id);
                updatedUserPublicationRole.EmailSent.AssertUtcNow();
            }
        }

        [Fact]
        public async Task Success_SuppliedDateSent()
        {
            UserPublicationRole userPublicationRole = _fixture.DefaultUserPublicationRole();

            var dateSent = DateTimeOffset.UtcNow.AddDays(-10);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(userPublicationRole.Id, dateSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserPublicationRole = await contentDbContext.UserPublicationRoles.SingleAsync();

                Assert.Equal(userPublicationRole.Id, updatedUserPublicationRole.Id);
                Assert.Equal(dateSent, updatedUserPublicationRole.EmailSent);
            }
        }

        [Fact]
        public async Task RoleDoesNotExist_Throws()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.MarkEmailAsSent(Guid.NewGuid())
            );
        }
    }

    private static UserPublicationRoleRepository CreateRepository(ContentDbContext? contentDbContext = null)
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(
            contentDbContext: contentDbContext,
            publicationRoleChangesHelper: new PublicationRoleChangesHelper()
        );
    }
}
