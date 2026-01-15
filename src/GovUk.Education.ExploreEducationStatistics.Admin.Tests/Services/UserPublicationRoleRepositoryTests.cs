#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
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
        public async Task Success()
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

                var result = await repository.Create(
                    userId: user.Id,
                    publicationId: publication.Id,
                    role: PublicationRole.Owner,
                    createdById: createdBy.Id
                );

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

    public class CreateManyIfNotExistsTests : UserPublicationRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();

            var existingRoleCreatedDate = DateTime.UtcNow.AddDays(-2);
            var newRolesCreatedDate = DateTime.UtcNow;

            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();

            UserPublicationRole existingUserPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUserId(user.Id)
                .WithPublicationId(publication1.Id)
                .WithRole(PublicationRole.Owner)
                .WithCreated(existingRoleCreatedDate)
                .WithCreatedById(createdBy.Id);

            var newUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUserId(user.Id)
                .WithCreated(newRolesCreatedDate)
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetPublicationId(publication1.Id).SetRole(PublicationRole.Allower))
                .ForIndex(1, s => s.SetPublicationId(publication2.Id).SetRole(PublicationRole.Owner))
                .GenerateList(2);

            UserPublicationRole[] allUserPublicationRoles = [.. newUserPublicationRoles, existingUserPublicationRole];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.Publications.AddRange(publication1, publication2);
                contentDbContext.UserPublicationRoles.Add(existingUserPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.CreateManyIfNotExists(allUserPublicationRoles);

                // Should only have created 2 new roles, as one already existed
                Assert.Equal(2, result.Count);

                Assert.Contains(
                    result,
                    upr =>
                        upr.UserId == user.Id
                        && upr.PublicationId == publication1.Id
                        && upr.Role == PublicationRole.Allower
                        && upr.Created == newRolesCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
                );

                Assert.Contains(
                    result,
                    upr =>
                        upr.UserId == user.Id
                        && upr.PublicationId == publication2.Id
                        && upr.Role == PublicationRole.Owner
                        && upr.Created == newRolesCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext.UserPublicationRoles.ToListAsync();

                // Should only have created 2 new roles, as one already existed
                Assert.Equal(3, userPublicationRoles.Count);

                // Existing role should be unchanged
                Assert.Contains(
                    userPublicationRoles,
                    upr =>
                        upr.Id == existingUserPublicationRole.Id
                        && upr.UserId == user.Id
                        && upr.PublicationId == publication1.Id
                        && upr.Role == PublicationRole.Owner
                        && upr.Created == existingRoleCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
                );

                Assert.Contains(
                    userPublicationRoles,
                    upr =>
                        upr.UserId == user.Id
                        && upr.PublicationId == publication1.Id
                        && upr.Role == PublicationRole.Allower
                        && upr.Created == newRolesCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
                );

                Assert.Contains(
                    userPublicationRoles,
                    upr =>
                        upr.UserId == user.Id
                        && upr.PublicationId == publication2.Id
                        && upr.Role == PublicationRole.Owner
                        && upr.Created == newRolesCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
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

        // This is a temporary test to ensure that the new permissions system roles are excluded in all results
        // via the global query filter
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IgnoresNewPermissionsSystemRoles(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1))
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

        // This is a temporary test to ensure that the new permissions system roles are excluded in all results
        // via the global query filter
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task IgnoresNewPermissionsSystemRoles(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_fixture.DefaultPublication())
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1))
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

        // This is a temporary test to ensure that the new permissions system roles are excluded in all results
        // via the global query filter
        [Fact]
        public async Task IgnoresNewPermissionsSystemRoles()
        {
            User activeUser = _fixture.DefaultUser();
            User userWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User userWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User softDeletedUser = _fixture.DefaultSoftDeletedUser();

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithPublication(_fixture.DefaultPublication())
                // These should ALL be filtered out
                .ForIndex(0, s => s.SetUser(activeUser).SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetUser(activeUser).SetRole(PublicationRole.Drafter))
                .ForIndex(2, s => s.SetUser(userWithPendingInvite).SetRole(PublicationRole.Approver))
                .ForIndex(3, s => s.SetUser(userWithPendingInvite).SetRole(PublicationRole.Drafter))
                .ForIndex(4, s => s.SetUser(userWithExpiredInvite).SetRole(PublicationRole.Approver))
                .ForIndex(5, s => s.SetUser(userWithExpiredInvite).SetRole(PublicationRole.Drafter))
                .ForIndex(6, s => s.SetUser(softDeletedUser).SetRole(PublicationRole.Approver))
                .ForIndex(7, s => s.SetUser(softDeletedUser).SetRole(PublicationRole.Drafter))
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

                var resultingQueryable = repository.Query(ResourceRoleFilter.All);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Empty(results);
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
            const string email = "test@test.com";

            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(_fixture.DefaultUser().WithEmail(email).Generate())
                .WithPublication(_fixture.DefaultPublication())
                .WithRole(publicationRole);

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

            User user2 = _fixture.DefaultUser().WithEmail("test2@test.com");
            Publication publication2 = _fixture.DefaultPublication();
            UserPublicationRole userPublicationRole2 = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user2)
                .WithPublication(publication2)
                .WithRole(PublicationRole.Owner);

            User user3 = _fixture.DefaultUser().WithEmail("test3@test.com");
            Publication publication3 = _fixture.DefaultPublication();
            UserPublicationRole userPublicationRole3 = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user3)
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
            const PublicationRole role1 = PublicationRole.Allower;
            const PublicationRole role2 = PublicationRole.Owner;
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
            const PublicationRole role1 = PublicationRole.Allower;
            const PublicationRole role2 = PublicationRole.Owner;
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
        public async Task Success()
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
        public async Task UserPublicationRoleDoesNotExist_Throws()
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

        return new(contentDbContext);
    }
}
