#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        [Fact]
        public async Task OldRole_NoNewPermissionsSystemPublicationRoleChanges()
        {
            var oldSystemPublicationRoleToCreate = PublicationRole.Allower;
            var newSystemPublicationRoleToRemain = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newSystemPublicationRoleToRemain);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.Create(
                    user.Id,
                    publication.Id,
                    oldSystemPublicationRoleToCreate,
                    createdBy.Id
                );

                // Should be the OLD `Allower` role which has been created that is returned
                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should be 2 as the 'Approver` role should be untouched,
                // and the `Owner` role has been created.
                Assert.Equal(2, userPublicationRoles.Count);

                var createdOldPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == oldSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
                Assert.Equal(user.Id, createdOldPublicationRole.UserId);
                Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, createdOldPublicationRole.Role);
                createdOldPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);
                Assert.Null(createdOldPublicationRole.EmailSent);

                Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == newSystemPublicationRoleToRemain && upr.Id == existingPublicationRole.Id
                );
            }
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRolesToRemoveAndCreate()
        {
            var oldSystemPublicationRoleToCreate = PublicationRole.Allower;
            var newSystemPublicationRoleToCreate = PublicationRole.Approver;
            var newSystemPublicationRoleToRemove = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newSystemPublicationRoleToRemove);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(existingPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.Create(
                    user.Id,
                    publication.Id,
                    oldSystemPublicationRoleToCreate,
                    createdBy.Id
                );

                // Should be the OLD `Allower` role which has been created that is returned
                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should be 2 as the 'Drafter` role has been removed and replaced with the `Approver` role,
                // and the `Owner` role has been created.
                Assert.Equal(2, userPublicationRoles.Count);

                var createdOldPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == oldSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
                Assert.Equal(user.Id, createdOldPublicationRole.UserId);
                Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, createdOldPublicationRole.Role);
                createdOldPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);
                Assert.Null(createdOldPublicationRole.EmailSent);

                var createdNewPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == newSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
                Assert.Equal(user.Id, createdNewPublicationRole.UserId);
                Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
                Assert.Equal(newSystemPublicationRoleToCreate, createdNewPublicationRole.Role);
                createdNewPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
                Assert.Null(createdNewPublicationRole.EmailSent);
            }
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRoleToCreate()
        {
            var oldSystemPublicationRoleToCreate = PublicationRole.Owner;
            var newSystemPublicationRoleToCreate = PublicationRole.Drafter;

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

                var result = await repository.Create(
                    user.Id,
                    publication.Id,
                    oldSystemPublicationRoleToCreate,
                    createdBy.Id
                );

                // Should be the OLD `Owner` role which has been created that is returned
                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should be 2 as the 'Drafter` role has been created,
                // and the `Owner` role has been created.
                Assert.Equal(2, userPublicationRoles.Count);

                var createdOldPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == oldSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
                Assert.Equal(user.Id, createdOldPublicationRole.UserId);
                Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, createdOldPublicationRole.Role);
                createdOldPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);
                Assert.Null(createdOldPublicationRole.EmailSent);

                var createdNewPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == newSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
                Assert.Equal(user.Id, createdNewPublicationRole.UserId);
                Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
                Assert.Equal(newSystemPublicationRoleToCreate, createdNewPublicationRole.Role);
                createdNewPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
                Assert.Null(createdNewPublicationRole.EmailSent);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task NewRole_Throws(PublicationRole newSystemPublicationRoleToCreate)
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
                    await repository.Create(user.Id, publication.Id, newSystemPublicationRoleToCreate, createdBy.Id)
                );
            }
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            const PublicationRole publicationRoleToCreate = PublicationRole.Owner;
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            // Checking both NEW and OLD permissions system roles ensures we are correctly considering both types when determining changes
            var existingPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .ForIndex(0, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Allower)) // OLD system role
                .ForIndex(1, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Approver)) // NEW system role
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(publication).SetRole(PublicationRole.Owner)
                ) // different OLD system role
                // Should not be considered when determining changes, as different publication
                .ForIndex(
                    3,
                    s => s.SetUser(user).SetPublication(_fixture.DefaultPublication()).SetRole(PublicationRole.Drafter)
                ) // different NEW system role
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.Create(
                    userId: user.Id,
                    publicationId: publication.Id,
                    role: publicationRoleToCreate,
                    createdById: createdBy.Id
                );
            }
        }
    }

    public class CreateManyIfNotExistsTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task ManyRoles_CreatesExpectedOldAndNewPermissionsSystemRoles_WhenNewPermissionsSystemRoleUpgradeNotRequired()
        {
            var createdDate = DateTime.UtcNow;
            User createdBy = _fixture.DefaultUser();

            // Test across multiple User/Publication combinations
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var newUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreated(createdDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(PublicationRole.Owner)
                // One 'Owner' role for each User/Publication combination.
                // We expect a NEW permissions system 'Drafter' role to be created each of these,
                // in addition to these roles themselves.
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2))
                .GenerateList(4);

            var userPublicationRolesCreateDtos = newUserPublicationRoles
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
                contentDbContext.Users.AddRange(user1, user2);
                contentDbContext.Publications.AddRange(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var results = await repository.CreateManyIfNotExists(userPublicationRolesCreateDtos);

                // Should have created 4 new OLD roles, and 4 new NEW roles.
                // But only expect the 4 OLD roles to be returned in the result. The NEW roles are
                // created quietly in the background.
                Assert.Equal(4, results.Count);

                Assert.All(
                    results,
                    upr =>
                    {
                        Assert.Equal(PublicationRole.Owner, upr.Role);
                        Assert.Equal(createdDate, upr.Created);
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                var resultsUserPublicationIdPairs = results.Select(upr => (upr.UserId, upr.PublicationId)).ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid)>
                    {
                        (user1.Id, publication1.Id),
                        (user1.Id, publication2.Id),
                        (user2.Id, publication1.Id),
                        (user2.Id, publication2.Id),
                    },
                    resultsUserPublicationIdPairs
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have 8 roles in total: 4 'Owner' + 4 newly created 'Drafter'
                Assert.Equal(8, userPublicationRoles.Count);

                Assert.All(
                    userPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(createdDate, upr.Created);
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                var actual = userPublicationRoles.Select(upr => (upr.UserId, upr.PublicationId, upr.Role)).ToHashSet();

                var expected = new HashSet<(Guid UserId, Guid PublicationId, PublicationRole Role)>
                {
                    // New OLD system roles
                    (user1.Id, publication1.Id, PublicationRole.Owner),
                    (user1.Id, publication2.Id, PublicationRole.Owner),
                    (user2.Id, publication1.Id, PublicationRole.Owner),
                    (user2.Id, publication2.Id, PublicationRole.Owner),
                    // New NEW system roles
                    (user1.Id, publication1.Id, PublicationRole.Drafter),
                    (user1.Id, publication2.Id, PublicationRole.Drafter),
                    (user2.Id, publication1.Id, PublicationRole.Drafter),
                    (user2.Id, publication2.Id, PublicationRole.Drafter),
                };

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task ManyRoles_CreatesExpectedOldAndNewPermissionsSystemRoles_WhenNewPermissionsSystemRoleUpgradeRequired()
        {
            var createdDate = DateTime.UtcNow;
            User createdBy = _fixture.DefaultUser();

            // Test across multiple User/Publication combinations
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();
            User user3 = _fixture.DefaultUser();
            User user4 = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();
            Publication publication3 = _fixture.DefaultPublication();
            Publication publication4 = _fixture.DefaultPublication();

            var newUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreated(createdDate)
                .WithCreatedById(createdBy.Id)
                // One 'Owner' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                // We would normally expect a NEW permissions system 'Drafter' role to be created each of these,
                // in addition to these roles themselves. However, in this case, each User/Publication combination
                // also has a new 'Allower' role which needs creating, meaning that the 'Drafter' role is upgraded to 'Approver'.
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Owner))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Owner))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Owner))
                // One 'Allower' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                .ForIndex(4, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Allower))
                .ForIndex(5, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Allower))
                .ForIndex(6, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Allower))
                .ForIndex(7, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Allower))
                // Here, we switch the order of creation of the 'Owner' and 'Allower' roles, to check that the order of these
                // doesn't affect the outcome. In this case, the 'Owner' role is created after the 'Allower' role, nut we
                // should still see the same upgrade of 'Drafter' to 'Approver'.
                // One 'Allower' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(8, s => s.SetUser(user3).SetPublication(publication3).SetRole(PublicationRole.Allower))
                .ForIndex(9, s => s.SetUser(user3).SetPublication(publication4).SetRole(PublicationRole.Allower))
                .ForIndex(10, s => s.SetUser(user4).SetPublication(publication3).SetRole(PublicationRole.Allower))
                .ForIndex(11, s => s.SetUser(user4).SetPublication(publication4).SetRole(PublicationRole.Allower))
                // One 'Owner' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(12, s => s.SetUser(user3).SetPublication(publication3).SetRole(PublicationRole.Owner))
                .ForIndex(13, s => s.SetUser(user3).SetPublication(publication4).SetRole(PublicationRole.Owner))
                .ForIndex(14, s => s.SetUser(user4).SetPublication(publication3).SetRole(PublicationRole.Owner))
                .ForIndex(15, s => s.SetUser(user4).SetPublication(publication4).SetRole(PublicationRole.Owner))
                .GenerateList(16);

            var userPublicationRolesCreateDtos = newUserPublicationRoles
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
                contentDbContext.Users.AddRange(user1, user2, user3, user4);
                contentDbContext.Publications.AddRange(publication1, publication2, publication3, publication4);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var results = await repository.CreateManyIfNotExists(userPublicationRolesCreateDtos);

                // Should have created 16 new OLD roles, and 8 new NEW roles.
                // But only expect the 16 OLD roles to be returned in the result. The NEW roles are
                // created quietly in the background.
                Assert.Equal(16, results.Count);

                Assert.All(
                    results,
                    upr =>
                    {
                        Assert.Equal(createdDate, upr.Created);
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                var resultsUserPublicationIdPairs = results
                    .Select(upr => (upr.UserId, upr.PublicationId, upr.Role))
                    .ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid, PublicationRole)>
                    {
                        (user1.Id, publication1.Id, PublicationRole.Owner),
                        (user1.Id, publication2.Id, PublicationRole.Owner),
                        (user2.Id, publication1.Id, PublicationRole.Owner),
                        (user2.Id, publication2.Id, PublicationRole.Owner),
                        (user1.Id, publication1.Id, PublicationRole.Allower),
                        (user1.Id, publication2.Id, PublicationRole.Allower),
                        (user2.Id, publication1.Id, PublicationRole.Allower),
                        (user2.Id, publication2.Id, PublicationRole.Allower),
                        (user3.Id, publication3.Id, PublicationRole.Owner),
                        (user3.Id, publication4.Id, PublicationRole.Owner),
                        (user4.Id, publication3.Id, PublicationRole.Owner),
                        (user4.Id, publication4.Id, PublicationRole.Owner),
                        (user3.Id, publication3.Id, PublicationRole.Allower),
                        (user3.Id, publication4.Id, PublicationRole.Allower),
                        (user4.Id, publication3.Id, PublicationRole.Allower),
                        (user4.Id, publication4.Id, PublicationRole.Allower),
                    },
                    resultsUserPublicationIdPairs
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have 24 roles in total: The 16 OLD roles + the 8 NEW ones
                Assert.Equal(24, userPublicationRoles.Count);

                Assert.All(
                    userPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(createdDate, upr.Created);
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                var actual = userPublicationRoles.Select(upr => (upr.UserId, upr.PublicationId, upr.Role)).ToHashSet();

                var expected = new HashSet<(Guid UserId, Guid PublicationId, PublicationRole Role)>
                {
                    // New OLD system roles
                    (user1.Id, publication1.Id, PublicationRole.Owner),
                    (user1.Id, publication2.Id, PublicationRole.Owner),
                    (user2.Id, publication1.Id, PublicationRole.Owner),
                    (user2.Id, publication2.Id, PublicationRole.Owner),
                    (user1.Id, publication1.Id, PublicationRole.Allower),
                    (user1.Id, publication2.Id, PublicationRole.Allower),
                    (user2.Id, publication1.Id, PublicationRole.Allower),
                    (user2.Id, publication2.Id, PublicationRole.Allower),
                    (user3.Id, publication3.Id, PublicationRole.Owner),
                    (user3.Id, publication4.Id, PublicationRole.Owner),
                    (user4.Id, publication3.Id, PublicationRole.Owner),
                    (user4.Id, publication4.Id, PublicationRole.Owner),
                    (user3.Id, publication3.Id, PublicationRole.Allower),
                    (user3.Id, publication4.Id, PublicationRole.Allower),
                    (user4.Id, publication3.Id, PublicationRole.Allower),
                    (user4.Id, publication4.Id, PublicationRole.Allower),
                    // New NEW system roles
                    (user1.Id, publication1.Id, PublicationRole.Approver),
                    (user1.Id, publication2.Id, PublicationRole.Approver),
                    (user2.Id, publication1.Id, PublicationRole.Approver),
                    (user2.Id, publication2.Id, PublicationRole.Approver),
                    (user3.Id, publication3.Id, PublicationRole.Approver),
                    (user3.Id, publication4.Id, PublicationRole.Approver),
                    (user4.Id, publication3.Id, PublicationRole.Approver),
                    (user4.Id, publication4.Id, PublicationRole.Approver),
                };

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task ManyRoles_IgnoresRolesThatAlreadyExist()
        {
            var existingRoleCreatedDate = DateTime.UtcNow.AddDays(-2);
            var newRolesCreatedDate = DateTime.UtcNow;
            User createdBy = _fixture.DefaultUser();

            // Test across multiple User/Publication combinations
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var existingUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreated(existingRoleCreatedDate)
                .WithCreatedById(createdBy.Id)
                // One 'Allower' role for each User/Publication combination
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Allower))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Allower))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Allower))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Allower))
                // One 'Approver' role for each User/Publication combination
                .ForIndex(4, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Approver))
                .ForIndex(5, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Approver))
                .ForIndex(6, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Approver))
                .ForIndex(7, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Approver))
                .GenerateList(8);

            var newUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreated(newRolesCreatedDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(PublicationRole.Owner)
                // One 'Owner' role for each User/Publication combination
                // Don't expect a NEW permissions system role to be created for any of these, as they already have a
                // more powerful 'Approver' role.
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2))
                .GenerateList(4);

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

                // Should only have created 4 new roles, as the others already existed
                Assert.Equal(4, results.Count);

                Assert.All(
                    results,
                    upr =>
                    {
                        Assert.Equal(PublicationRole.Owner, upr.Role);
                        Assert.Equal(newRolesCreatedDate, upr.Created);
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                var resultsUserPublicationIdPairs = results.Select(upr => (upr.UserId, upr.PublicationId)).ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid)>
                    {
                        (user1.Id, publication1.Id),
                        (user1.Id, publication2.Id),
                        (user2.Id, publication1.Id),
                        (user2.Id, publication2.Id),
                    },
                    resultsUserPublicationIdPairs
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have 12 roles in total: 8 existing + 4 new
                // The existing roles should be unchanged
                Assert.Equal(12, userPublicationRoles.Count);

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
                    (user1.Id, publication1.Id, PublicationRole.Owner, newRolesCreatedDate),
                    (user1.Id, publication2.Id, PublicationRole.Owner, newRolesCreatedDate),
                    (user2.Id, publication1.Id, PublicationRole.Owner, newRolesCreatedDate),
                    (user2.Id, publication2.Id, PublicationRole.Owner, newRolesCreatedDate),
                    // Existing roles
                    (user1.Id, publication1.Id, PublicationRole.Allower, existingRoleCreatedDate),
                    (user2.Id, publication1.Id, PublicationRole.Allower, existingRoleCreatedDate),
                    (user1.Id, publication2.Id, PublicationRole.Allower, existingRoleCreatedDate),
                    (user2.Id, publication2.Id, PublicationRole.Allower, existingRoleCreatedDate),
                    (user1.Id, publication1.Id, PublicationRole.Approver, existingRoleCreatedDate),
                    (user2.Id, publication1.Id, PublicationRole.Approver, existingRoleCreatedDate),
                    (user1.Id, publication2.Id, PublicationRole.Approver, existingRoleCreatedDate),
                    (user2.Id, publication2.Id, PublicationRole.Approver, existingRoleCreatedDate),
                };

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task EmptySet_DoesNothing()
        {
            var repository = CreateRepository();

            var results = await repository.CreateManyIfNotExists([]);

            Assert.Empty(results);
        }

        [Fact]
        public async Task OldRole_NoNewPermissionsSystemPublicationRoleChanges()
        {
            var oldSystemPublicationRoleToCreate = PublicationRole.Allower;
            var newSystemPublicationRoleToRemain = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newSystemPublicationRoleToRemain);

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
                        Role: oldSystemPublicationRoleToCreate,
                        CreatedById: createdBy.Id
                    ),
                ]);

                // Should be the OLD `Allower` role which has been created that is returned
                var result = Assert.Single(results);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should be 2 as the 'Approver` role should be untouched,
                // and the `Allower` role has been created.
                Assert.Equal(2, userPublicationRoles.Count);

                var createdOldPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == oldSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
                Assert.Equal(user.Id, createdOldPublicationRole.UserId);
                Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, createdOldPublicationRole.Role);
                createdOldPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);
                Assert.Null(createdOldPublicationRole.EmailSent);

                var existingNewPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == newSystemPublicationRoleToRemain && upr.Id == existingPublicationRole.Id
                );
            }
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRolesToRemoveAndCreate()
        {
            var oldSystemPublicationRoleToCreate = PublicationRole.Allower;
            var newSystemPublicationRoleToCreate = PublicationRole.Approver;
            var newSystemPublicationRoleToRemove = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newSystemPublicationRoleToRemove);

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
                        Role: oldSystemPublicationRoleToCreate,
                        CreatedById: createdBy.Id
                    ),
                ]);

                // Should be the OLD `Owner` role which has been created that is returned
                var result = Assert.Single(results);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should be 2 as the 'Drafter` role has been removed and replaced with the `Approver` role,
                // and the `Owner` role has been created.
                Assert.Equal(2, userPublicationRoles.Count);

                var createdOldPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == oldSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
                Assert.Equal(user.Id, createdOldPublicationRole.UserId);
                Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, createdOldPublicationRole.Role);
                createdOldPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);
                Assert.Null(createdOldPublicationRole.EmailSent);

                var createdNewPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == newSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
                Assert.Equal(user.Id, createdNewPublicationRole.UserId);
                Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
                Assert.Equal(newSystemPublicationRoleToCreate, createdNewPublicationRole.Role);
                createdNewPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
                Assert.Null(createdNewPublicationRole.EmailSent);
            }
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRoleToCreate()
        {
            var oldSystemPublicationRoleToCreate = PublicationRole.Owner;
            var newSystemPublicationRoleToCreate = PublicationRole.Drafter;

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
                        Role: oldSystemPublicationRoleToCreate,
                        CreatedById: createdBy.Id
                    ),
                ]);

                // Should be the OLD `Owner` role which has been created that is returned
                var result = Assert.Single(results);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should be 2 as the 'Drafter` role has been created,
                // and the `Owner` role has been created.
                Assert.Equal(2, userPublicationRoles.Count);

                var createdOldPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == oldSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdOldPublicationRole.Id);
                Assert.Equal(user.Id, createdOldPublicationRole.UserId);
                Assert.Equal(publication.Id, createdOldPublicationRole.PublicationId);
                Assert.Equal(oldSystemPublicationRoleToCreate, createdOldPublicationRole.Role);
                createdOldPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdOldPublicationRole.CreatedById);
                Assert.Null(createdOldPublicationRole.EmailSent);

                var createdNewPublicationRole = Assert.Single(
                    userPublicationRoles,
                    upr => upr.Role == newSystemPublicationRoleToCreate
                );

                Assert.NotEqual(Guid.Empty, createdNewPublicationRole.Id);
                Assert.Equal(user.Id, createdNewPublicationRole.UserId);
                Assert.Equal(publication.Id, createdNewPublicationRole.PublicationId);
                Assert.Equal(newSystemPublicationRoleToCreate, createdNewPublicationRole.Role);
                createdNewPublicationRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdNewPublicationRole.CreatedById);
                Assert.Null(createdNewPublicationRole.EmailSent);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task NewRole_Throws(PublicationRole newSystemPublicationRoleToCreate)
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
                        // try creating a couple of OLD roles and one NEW role (in the middle of the list), to check that an error is thrown whenever
                        // the list contains a NEW role, even if it also contains OLD roles.
                        new(
                            UserId: user.Id,
                            PublicationId: publication.Id,
                            Role: PublicationRole.Allower,
                            CreatedById: createdBy.Id
                        ),
                        new(
                            UserId: user.Id,
                            PublicationId: publication.Id,
                            Role: newSystemPublicationRoleToCreate,
                            CreatedById: createdBy.Id
                        ),
                        new(
                            UserId: user.Id,
                            PublicationId: publication.Id,
                            Role: PublicationRole.Owner,
                            CreatedById: createdBy.Id
                        ),
                    ])
                );
            }
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            const PublicationRole publicationRoleToCreate = PublicationRole.Owner;
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            // Checking both NEW and OLD permissions system roles ensures we are correctly considering both types when determining changes
            var existingPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .ForIndex(0, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Allower)) // OLD system role
                .ForIndex(1, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Approver)) // NEW system role
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(publication).SetRole(PublicationRole.Owner)
                ) // different OLD system role
                // Should not be considered when determining changes, as different publication
                .ForIndex(
                    3,
                    s => s.SetUser(user).SetPublication(_fixture.DefaultPublication()).SetRole(PublicationRole.Drafter)
                ) // different NEW system role
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.CreateManyIfNotExists([
                    new(
                        UserId: user.Id,
                        PublicationId: publication.Id,
                        Role: publicationRoleToCreate,
                        CreatedById: createdBy.Id
                    ),
                ]);
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

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task DefaultValueForIncludeNewPermissionsSystemRoles_IgnoresNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetRole(PublicationRole.Drafter))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result1 = await repository.GetById(userPublicationRoles[0].Id);
                var result2 = await repository.GetById(userPublicationRoles[1].Id);

                Assert.Null(result1);
                Assert.Null(result2);
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IncludeNewPermissionsSystemRolesIsFalse_IgnoresNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetRole(PublicationRole.Drafter))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result1 = await repository.GetById(
                    userPublicationRoles[0].Id,
                    includeNewPermissionsSystemRoles: false
                );
                var result2 = await repository.GetById(
                    userPublicationRoles[1].Id,
                    includeNewPermissionsSystemRoles: false
                );

                Assert.Null(result1);
                Assert.Null(result2);
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IncludeNewPermissionsSystemRolesIsTrue_IncludesNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetRole(PublicationRole.Drafter))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result1 = await repository.GetById(
                    userPublicationRoles[0].Id,
                    includeNewPermissionsSystemRoles: true
                );
                var result2 = await repository.GetById(
                    userPublicationRoles[1].Id,
                    includeNewPermissionsSystemRoles: true
                );

                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.Equal(userPublicationRoles[0].Id, result1.Id);
                Assert.Equal(userPublicationRoles[1].Id, result2.Id);
            }
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
                role: PublicationRole.Owner
            );

            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task DefaultValueForIncludeNewPermissionsSystemRoles_IgnoresNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetRole(PublicationRole.Drafter))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result1 = await repository.GetByCompositeKey(
                    userId: userPublicationRoles[0].UserId,
                    publicationId: userPublicationRoles[0].PublicationId,
                    role: userPublicationRoles[0].Role
                );
                var result2 = await repository.GetByCompositeKey(
                    userId: userPublicationRoles[1].UserId,
                    publicationId: userPublicationRoles[1].PublicationId,
                    role: userPublicationRoles[1].Role
                );

                Assert.Null(result1);
                Assert.Null(result2);
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IncludeNewPermissionsSystemRolesIsFalse_IgnoresNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetRole(PublicationRole.Drafter))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result1 = await repository.GetByCompositeKey(
                    userId: userPublicationRoles[0].UserId,
                    publicationId: userPublicationRoles[0].PublicationId,
                    role: userPublicationRoles[0].Role,
                    includeNewPermissionsSystemRoles: false
                );
                var result2 = await repository.GetByCompositeKey(
                    userId: userPublicationRoles[1].UserId,
                    publicationId: userPublicationRoles[1].PublicationId,
                    role: userPublicationRoles[1].Role,
                    includeNewPermissionsSystemRoles: false
                );

                Assert.Null(result1);
                Assert.Null(result2);
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IncludeNewPermissionsSystemRolesIsTrue_IncludesNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetRole(PublicationRole.Drafter))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result1 = await repository.GetByCompositeKey(
                    userId: userPublicationRoles[0].UserId,
                    publicationId: userPublicationRoles[0].PublicationId,
                    role: userPublicationRoles[0].Role,
                    includeNewPermissionsSystemRoles: true
                );
                var result2 = await repository.GetByCompositeKey(
                    userId: userPublicationRoles[1].UserId,
                    publicationId: userPublicationRoles[1].PublicationId,
                    role: userPublicationRoles[1].Role,
                    includeNewPermissionsSystemRoles: true
                );

                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.Equal(userPublicationRoles[0].Id, result1.Id);
                Assert.Equal(userPublicationRoles[1].Id, result2.Id);
            }
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

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task DefaultValueForIncludeNewPermissionsSystemRoles_IgnoresNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should ALL be filtered out
                .ForIndex(0, s => s.SetUser(user).SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetUser(user).SetRole(PublicationRole.Drafter))
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

                var resultingQueryable = repository.Query(ResourceRoleFilter.All);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Empty(results);
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IncludeNewPermissionsSystemRolesIsFalse_IgnoresNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should ALL be filtered out
                .ForIndex(0, s => s.SetUser(user).SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetUser(user).SetRole(PublicationRole.Drafter))
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

                var resultingQueryable = repository.Query(
                    ResourceRoleFilter.All,
                    includeNewPermissionsSystemRoles: false
                );

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Empty(results);
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IncludeNewPermissionsSystemRolesIsTrue_ReturnsNewPermissionsSystemRoles(
            Func<DataFixture, User> userFactory
        )
        {
            var user = userFactory(_fixture);

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should ALL be included
                .ForIndex(0, s => s.SetUser(user).SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetUser(user).SetRole(PublicationRole.Drafter))
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

                var resultingQueryable = repository.Query(
                    ResourceRoleFilter.All,
                    includeNewPermissionsSystemRoles: true
                );

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(2, results.Count);
            }
        }
    }

    public class RemoveByIdTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task OldRole_NoNewPermissionsSystemPublicationRoleChanges()
        {
            var oldPublicationRoleToRemove = PublicationRole.Owner;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            var otherUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithRole(PublicationRole.Allower)
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Allower))
                .ForIndex(1, s => s.SetRole(PublicationRole.Approver))
                .GenerateList(2);
            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                )
                .ForIndex(0, s => s.SetRole(ReleaseRole.Approver))
                .ForIndex(1, s => s.SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange([
                    oldSystemUserPublicationRole,
                    .. otherUserPublicationRoles,
                ]);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                await repository.RemoveById(oldSystemUserPublicationRole.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var updatedPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // The existing 'Owner' role should have been deleted, but the other roles should remain
                Assert.Equal(2, updatedPublicationRoles.Count);
                Assert.Contains(
                    updatedPublicationRoles,
                    upr => upr.Role == otherUserPublicationRoles[0].Role && upr.Id == otherUserPublicationRoles[0].Id
                );
                Assert.Contains(
                    updatedPublicationRoles,
                    upr => upr.Role == otherUserPublicationRoles[1].Role && upr.Id == otherUserPublicationRoles[1].Id
                );
            }
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRoleToRemoveAndCreate()
        {
            User deletedUserPlaceholder = _fixture.DefaultDeletedUserPlaceholder().WithId(Guid.NewGuid());

            var oldPublicationRoleToRemove = PublicationRole.Allower;
            var newPublicationRoleToRemove = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserPublicationRole newSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserPublicationRole otherUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(PublicationRole.Owner)
                .WithUser(user)
                .WithPublication(publication);
            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                )
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(
                    oldSystemUserPublicationRole,
                    newSystemUserPublicationRole,
                    otherUserPublicationRole
                );
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepositoryMock
                .Setup(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedUserPlaceholder);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userRepository: userRepositoryMock.Object
                );

                await repository.RemoveById(oldSystemUserPublicationRole.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var updatedPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // The existing 'Allower' & 'Approver' roles should have been deleted, but the other 'Owner' role should remain,
                // and the 'Approver' role should have been DOWNGRADED to a new 'Drafter' role. So there should be 2 remaining roles.
                Assert.Equal(2, updatedPublicationRoles.Count);

                Assert.Contains(updatedPublicationRoles, upr => upr.Role == otherUserPublicationRole.Role);

                // The new Drafter role should have been created with the correct values.
                var newlyCreatedDrafterRole = Assert.Single(
                    updatedPublicationRoles,
                    upr => upr.Role == PublicationRole.Drafter
                );
                Assert.Equal(user.Id, newlyCreatedDrafterRole.UserId);
                Assert.Equal(publication.Id, newlyCreatedDrafterRole.PublicationId);
                Assert.Equal(deletedUserPlaceholder.Id, newlyCreatedDrafterRole.CreatedById);
                newlyCreatedDrafterRole.Created.AssertUtcNow();
            }

            MockUtils.VerifyAllMocks(userRepositoryMock);
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRoleToRemove()
        {
            var oldPublicationRoleToRemove = PublicationRole.Owner;
            var newPublicationRoleToRemove = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserPublicationRole newSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                )
                .WithRole(ReleaseRole.PrereleaseViewer);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(
                    oldSystemUserPublicationRole,
                    newSystemUserPublicationRole
                );
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                await repository.RemoveById(oldSystemUserPublicationRole.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var updatedPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // The existing 'Owner' & 'Drafter' roles should have been deleted
                Assert.Empty(updatedPublicationRoles);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NewRole_Throws(PublicationRole newPublicationRoleToRemove)
        {
            UserPublicationRole newSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(_fixture.DefaultUser())
                .WithPublication(_fixture.DefaultPublication());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(newSystemUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await repository.RemoveById(newSystemUserPublicationRole.Id)
                );
            }
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            var oldPublicationRoleToRemove = PublicationRole.Owner;

            var user = userFactory(_fixture);
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            // Checking both NEW and OLD permissions system roles ensures we are correctly considering both types when determining changes
            var existingPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .ForIndex(0, s => s.SetUser(user).SetPublication(publication).SetRole(oldPublicationRoleToRemove)) // OLD system role
                .ForIndex(1, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Approver)) // NEW system role
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(publication).SetRole(PublicationRole.Allower)
                ) // different OLD system role
                // Should not be considered when determining changes, as different publication
                .ForIndex(
                    3,
                    s => s.SetUser(user).SetPublication(_fixture.DefaultPublication()).SetRole(PublicationRole.Drafter)
                ) // different NEW system role
                .GenerateList(4);
            var existingReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .ForIndex(
                    0,
                    s => s.SetUser(user).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.PrereleaseViewer)
                )
                // Different release version, but same publication - so should still be considered
                .ForIndex(1, s => s.SetUser(user).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUser())
                            .SetReleaseVersion(releaseVersion1)
                            .SetRole(ReleaseRole.Contributor)
                )
                // Should not be considered when determining changes, as different publication
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(user)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(ReleaseRole.Contributor)
                )
                .GenerateList(4);

            var userPublicationRoleToRemove = existingPublicationRoles[0];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingPublicationRoles);
                contentDbContext.UserReleaseRoles.AddRange(existingReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                await repository.RemoveById(userPublicationRoleToRemove.Id);
            }
        }
    }

    public class RemoveManyTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task ManyRoles_CreatesExpectedOldAndNewPermissionsSystemRoles_WhenSingleNewPermissionsSystemRoleDowngradeRequired()
        {
            // Test across multiple User/Publication combinations
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // One 'Owner' role for each User/Publication combination.
                // We exoect these to be removed.
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Owner))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Owner))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Owner))
                // One 'Drafter' role for each User/Publication combination.
                // We expect these to be removed too.
                .ForIndex(4, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Drafter))
                .ForIndex(5, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Drafter))
                .ForIndex(6, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Drafter))
                .ForIndex(7, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Drafter))
                // Adding an additional role that should be left untouched by the removals
                .ForIndex(8, s => s.SetUser(_fixture.DefaultUser()).SetPublication(_fixture.DefaultPublication()))
                .GenerateList(9);

            var userPublicationRoleIdsToRemove = userPublicationRoles[..4].Select(upr => upr.Id).ToHashSet();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany(userPublicationRoleIdsToRemove);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have removed ALL roles except 1
                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(userPublicationRoles[8].Id, remainingUserPublicationRole.Id);
                Assert.Equal(userPublicationRoles[8].Role, remainingUserPublicationRole.Role);
            }
        }

        [Fact]
        public async Task ManyRoles_RemovesAndCreatesExpectedOldAndNewPermissionsSystemRoles_WhenMultipleNewPermissionsSystemRoleDowngradesRequired()
        {
            User deletedUserPlaceholder = _fixture.DefaultDeletedUserPlaceholder();

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
                // One 'Owner' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                // We would normally expect the equivalent NEW permissions system 'Drafter' role to be removed each of these,
                // but it's the more powerful role 'Approver' that exists. So removing 'Owner' alone won't remove this
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Owner))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Owner))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Owner))
                // One 'Allower' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                // We expect removing these will cause the 'Approver' role to be removed too.
                .ForIndex(4, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Allower))
                .ForIndex(5, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Allower))
                .ForIndex(6, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Allower))
                .ForIndex(7, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Allower))
                // One 'Approver' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                .ForIndex(8, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Approver))
                .ForIndex(9, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Approver))
                .ForIndex(10, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Approver))
                .ForIndex(11, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Approver))
                // Here, we switch the order of the 'Owner' and 'Allower' roles, to check that the removal order of these
                // doesn't affect the outcome. In this case, the 'Owner' role is removed AFTER the 'Allower' role, so
                // we expect the 'Approver' role to be removed first, and DOWNGRADED to a 'Drafter' role. Then, once
                // the 'Owner' role is removed we expect the 'Drafter` role to removed.
                // One 'Allower' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(12, s => s.SetUser(user3).SetPublication(publication3).SetRole(PublicationRole.Allower))
                .ForIndex(13, s => s.SetUser(user3).SetPublication(publication4).SetRole(PublicationRole.Allower))
                .ForIndex(14, s => s.SetUser(user4).SetPublication(publication3).SetRole(PublicationRole.Allower))
                .ForIndex(15, s => s.SetUser(user4).SetPublication(publication4).SetRole(PublicationRole.Allower))
                // One 'Owner' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(16, s => s.SetUser(user3).SetPublication(publication3).SetRole(PublicationRole.Owner))
                .ForIndex(17, s => s.SetUser(user3).SetPublication(publication4).SetRole(PublicationRole.Owner))
                .ForIndex(18, s => s.SetUser(user4).SetPublication(publication3).SetRole(PublicationRole.Owner))
                .ForIndex(19, s => s.SetUser(user4).SetPublication(publication4).SetRole(PublicationRole.Owner))
                // One 'Approver' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(20, s => s.SetUser(user3).SetPublication(publication3).SetRole(PublicationRole.Approver))
                .ForIndex(21, s => s.SetUser(user3).SetPublication(publication4).SetRole(PublicationRole.Approver))
                .ForIndex(22, s => s.SetUser(user4).SetPublication(publication3).SetRole(PublicationRole.Approver))
                .ForIndex(23, s => s.SetUser(user4).SetPublication(publication4).SetRole(PublicationRole.Approver))
                // Adding an additional role that should be left untouched by the removals
                .ForIndex(24, s => s.SetUser(_fixture.DefaultUser()).SetPublication(_fixture.DefaultPublication()))
                .GenerateList(25);

            // Try removing the 'Owner' and THEN the 'Allower' roles for the User 1/2 & Publication 1/2 pairs.
            // Also try removing the 'Allower' and THEN the 'Owner' roles for the User 3/4 & Publication 3/4 pairs,
            // to check that the order of removal doesn't affect the expected outcome.
            var userPublicationRoleIdsToRemove = userPublicationRoles[..8]
                .Concat(userPublicationRoles[12..20])
                .Select(upr => upr.Id)
                .ToHashSet();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedUserPlaceholder);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext, userRepository: userRepository.Object);

                await repository.RemoveMany(userPublicationRoleIdsToRemove);
            }

            MockUtils.VerifyAllMocks(userRepository);

            userRepository.Verify(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()), Times.Exactly(4));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have removed ALL roles except 1
                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(userPublicationRoles[24].Id, remainingUserPublicationRole.Id);
                Assert.Equal(userPublicationRoles[24].Role, remainingUserPublicationRole.Role);
            }
        }

        [Fact]
        public async Task EmptyList_DoesNothing()
        {
            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(_fixture.DefaultUser())
                .WithPublication(_fixture.DefaultPublication())
                .WithRole(PublicationRole.Allower);

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

        [Fact]
        public async Task OldRole_NoNewPermissionsSystemPublicationRoleChanges()
        {
            var oldPublicationRoleToRemove = PublicationRole.Owner;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            var otherUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Allower))
                .ForIndex(1, s => s.SetRole(PublicationRole.Approver))
                .GenerateList(2);
            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                )
                .ForIndex(0, s => s.SetRole(ReleaseRole.Approver))
                .ForIndex(1, s => s.SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange([
                    oldSystemUserPublicationRole,
                    .. otherUserPublicationRoles,
                ]);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                var userPublicationRoleToRemove = await contentDbContext.UserPublicationRoles.SingleAsync(upr =>
                    upr.Id == oldSystemUserPublicationRole.Id
                );

                await repository.RemoveMany([userPublicationRoleToRemove.Id]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var updatedPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // The existing 'Owner' role should have been deleted, but the other roles should remain
                Assert.Equal(2, updatedPublicationRoles.Count);
                Assert.Contains(
                    updatedPublicationRoles,
                    upr => upr.Role == otherUserPublicationRoles[0].Role && upr.Id == otherUserPublicationRoles[0].Id
                );
                Assert.Contains(
                    updatedPublicationRoles,
                    upr => upr.Role == otherUserPublicationRoles[1].Role && upr.Id == otherUserPublicationRoles[1].Id
                );
            }
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRoleToRemoveAndCreate()
        {
            User deletedUserPlaceholder = _fixture.DefaultDeletedUserPlaceholder();

            var oldPublicationRoleToRemove = PublicationRole.Allower;
            var newPublicationRoleToRemove = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserPublicationRole newSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserPublicationRole otherUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(PublicationRole.Owner)
                .WithUser(user)
                .WithPublication(publication);
            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                )
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.PrereleaseViewer))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(
                    oldSystemUserPublicationRole,
                    newSystemUserPublicationRole,
                    otherUserPublicationRole
                );
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepositoryMock
                .Setup(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedUserPlaceholder);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userRepository: userRepositoryMock.Object
                );

                var userPublicationRoleToRemove = await contentDbContext.UserPublicationRoles.SingleAsync(upr =>
                    upr.Id == oldSystemUserPublicationRole.Id
                );

                await repository.RemoveMany([userPublicationRoleToRemove.Id]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var updatedPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // The existing 'Allower' & 'Approver' roles should have been deleted, but the other 'Owner' role should remain,
                // and the 'Approver' role should have been DOWNGRADED to a new 'Drafter' role. So there should be 2 remaining roles.
                Assert.Equal(2, updatedPublicationRoles.Count);

                Assert.Contains(updatedPublicationRoles, upr => upr.Role == otherUserPublicationRole.Role);

                // The new Drafter role should have been created with the correct values.
                var newlyCreatedDrafterRole = Assert.Single(
                    updatedPublicationRoles,
                    upr => upr.Role == PublicationRole.Drafter
                );
                Assert.Equal(user.Id, newlyCreatedDrafterRole.UserId);
                Assert.Equal(publication.Id, newlyCreatedDrafterRole.PublicationId);
                Assert.Equal(deletedUserPlaceholder.Id, newlyCreatedDrafterRole.CreatedById);
                newlyCreatedDrafterRole.Created.AssertUtcNow();
            }

            MockUtils.VerifyAllMocks(userRepositoryMock);
        }

        [Fact]
        public async Task OldRole_NewPermissionsSystemPublicationRoleToRemove()
        {
            var oldPublicationRoleToRemove = PublicationRole.Owner;
            var newPublicationRoleToRemove = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            UserPublicationRole oldSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(oldPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserPublicationRole newSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);
            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                )
                .WithRole(ReleaseRole.PrereleaseViewer);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(
                    oldSystemUserPublicationRole,
                    newSystemUserPublicationRole
                );
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                var userPublicationRoleToRemove = await contentDbContext.UserPublicationRoles.SingleAsync(upr =>
                    upr.Id == oldSystemUserPublicationRole.Id
                );

                await repository.RemoveMany([userPublicationRoleToRemove.Id]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Checking ALL UserPublicationRoles, which includes the NEW and OLD permissions system
                // publication roles. The 'IgnoreQueryFilters' will likely be removed in EES-6196
                // once we stop using the OLD roles.
                var updatedPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // The existing 'Owner' & 'Drafter' roles should have been deleted
                Assert.Empty(updatedPublicationRoles);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NewRole_Throws(PublicationRole newPublicationRoleToRemove)
        {
            UserPublicationRole newSystemUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(_fixture.DefaultUser())
                .WithPublication(_fixture.DefaultPublication());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.Add(newSystemUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await repository.RemoveMany([newSystemUserPublicationRole.Id])
                );
            }
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            var oldPublicationRoleToRemove = PublicationRole.Owner;

            var user = userFactory(_fixture);
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            // Checking both NEW and OLD permissions system roles ensures we are correctly considering both types when determining changes
            var existingPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .ForIndex(0, s => s.SetUser(user).SetPublication(publication).SetRole(oldPublicationRoleToRemove)) // OLD system role
                .ForIndex(1, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Approver)) // NEW system role
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetPublication(publication).SetRole(PublicationRole.Allower)
                ) // different OLD system role
                // Should not be considered when determining changes, as different publication
                .ForIndex(
                    3,
                    s => s.SetUser(user).SetPublication(_fixture.DefaultPublication()).SetRole(PublicationRole.Drafter)
                ) // different NEW system role
                .GenerateList(4);
            var existingReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .ForIndex(
                    0,
                    s => s.SetUser(user).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.PrereleaseViewer)
                )
                // Different release version, but same publication - so should still be considered
                .ForIndex(1, s => s.SetUser(user).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUser())
                            .SetReleaseVersion(releaseVersion1)
                            .SetRole(ReleaseRole.Contributor)
                )
                // Should not be considered when determining changes, as different publication
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(user)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(ReleaseRole.Contributor)
                )
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingPublicationRoles);
                contentDbContext.UserReleaseRoles.AddRange(existingReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                var userPublicationRoleToRemove = await contentDbContext.UserPublicationRoles.SingleAsync(upr =>
                    upr.Role == oldPublicationRoleToRemove
                );

                await repository.RemoveMany([userPublicationRoleToRemove.Id]);
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
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // These 4 roles should be removed (including the NEW permissions system roles)
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
            User targetUser = _fixture.DefaultUser().WithEmail("test1@test.com");
            User otherUser = _fixture.DefaultUser().WithEmail("test2@test.com");
            const PublicationRole role1 = PublicationRole.Allower;
            const PublicationRole role2 = PublicationRole.Owner;
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Allower)
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Allower)
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task IncludesNewPermissionsSystemRolesInCheck(PublicationRole targetRole)
        {
            User targetActiveUser = _fixture.DefaultUser();
            Publication targetPublication = _fixture.DefaultPublication();

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
            const PublicationRole targetRolesToInclude = PublicationRole.Owner;

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(targetActiveUser)
                .WithPublication(targetPublication)
                .WithRole(PublicationRole.Allower);

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
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Allower)
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetPublication(targetPublication)
                            .SetRole(PublicationRole.Allower)
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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
            const PublicationRole targetRole = PublicationRole.Owner;

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
            const PublicationRole targetRole = PublicationRole.Owner;

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetPublication(targetPublication).SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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
                            .SetRole(PublicationRole.Allower)
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
        public async Task UserPublicationRoleDoesNotExist_Throws()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.MarkEmailAsSent(Guid.NewGuid())
            );
        }
    }

    private static UserPublicationRoleRepository CreateRepository(
        ContentDbContext? contentDbContext = null,
        IUserRepository? userRepository = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(
            contentDbContext: contentDbContext,
            newPermissionsSystemHelper: new NewPermissionsSystemHelper(),
            userReleaseRoleQueryRepository: new UserReleaseRoleQueryRepository(contentDbContext),
            userRepository: userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict)
        );
    }
}
