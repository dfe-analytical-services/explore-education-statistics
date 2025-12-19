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

public abstract class UserReleaseRoleRepositoryTests
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

    public class CreateTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task Create()
        {
            User user = _fixture.DefaultUser();
            User createdByUser = _fixture.DefaultUser();

            var releaseVersion = new ReleaseVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Users.AddRange(user, createdByUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = CreateRepository(contentDbContext);

                var result = await service.Create(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    ReleaseRole.Contributor,
                    createdById: createdByUser.Id
                );

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, result.Role);
                Assert.Equal(createdByUser.Id, result.CreatedById);
                result.Created.AssertUtcNow();
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRole = await contentDbContext.UserReleaseRoles.AsQueryable().SingleAsync();

                Assert.NotEqual(Guid.Empty, userReleaseRole.Id);
                Assert.Equal(user.Id, userReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, userReleaseRole.ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, userReleaseRole.Role);
                Assert.Equal(createdByUser.Id, userReleaseRole.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRole.Created).Milliseconds, 0, 1500);
                Assert.Null(userReleaseRole.EmailSent);
            }
        }
    }

    public class CreateManyIfNotExistsTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task CreateManyIfNotExists_Users()
        {
            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();

            var existingRoleCreatedDate = DateTime.UtcNow.AddDays(-2);
            var newRolesCreatedDate = DateTime.UtcNow;

            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            UserReleaseRole existingUserReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUserId(user.Id)
                .WithReleaseVersionId(releaseVersion1.Id)
                .WithRole(ReleaseRole.Contributor)
                .WithCreated(existingRoleCreatedDate)
                .WithCreatedById(createdBy.Id);

            var newUserReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUserId(user.Id)
                .WithCreated(newRolesCreatedDate)
                .WithCreatedById(createdBy.Id)
                .ForIndex(0, s => s.SetReleaseVersionId(releaseVersion1.Id).SetRole(ReleaseRole.Approver))
                .ForIndex(1, s => s.SetReleaseVersionId(releaseVersion2.Id).SetRole(ReleaseRole.Contributor))
                .GenerateList(2);

            UserReleaseRole[] allUserPublicationRoles = [.. newUserReleaseRoles, existingUserReleaseRole];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2);
                contentDbContext.UserReleaseRoles.Add(existingUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.CreateManyIfNotExists(allUserPublicationRoles);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should only have created 2 new roles, as one already existed
                Assert.Equal(3, userReleaseRoles.Count);

                // Existing role should be unchanged
                Assert.Contains(
                    userReleaseRoles,
                    upr =>
                        upr.Id == existingUserReleaseRole.Id
                        && upr.UserId == user.Id
                        && upr.ReleaseVersionId == releaseVersion1.Id
                        && upr.Role == ReleaseRole.Contributor
                        && upr.Created == existingRoleCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
                );

                Assert.Contains(
                    userReleaseRoles,
                    upr =>
                        upr.UserId == user.Id
                        && upr.ReleaseVersionId == releaseVersion1.Id
                        && upr.Role == ReleaseRole.Approver
                        && upr.Created == newRolesCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
                );

                Assert.Contains(
                    userReleaseRoles,
                    upr =>
                        upr.UserId == user.Id
                        && upr.ReleaseVersionId == releaseVersion2.Id
                        && upr.Role == ReleaseRole.Contributor
                        && upr.Created == newRolesCreatedDate
                        && upr.CreatedById == createdBy.Id
                        && upr.EmailSent == null
                );
            }
        }
    }

    public class GetByIdTests : UserReleaseRoleRepositoryTests
    {
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task Success(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTime.UtcNow.AddDays(-1));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetById(userReleaseRole.Id);

                Assert.NotNull(result);
                Assert.Equal(userReleaseRole.Id, result.Id);
                Assert.Equal(userReleaseRole.UserId, result.UserId);
                Assert.Equal(userReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
                Assert.Equal(userReleaseRole.Role, result.Role);
                Assert.Equal(userReleaseRole.Created, result.Created);
                Assert.Equal(userReleaseRole.CreatedById, result.CreatedById);
                Assert.Equal(userReleaseRole.EmailSent, result.EmailSent);
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

    public class GetByCompositeKeyTests : UserReleaseRoleRepositoryTests
    {
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task Success(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTime.UtcNow.AddDays(-1));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetByCompositeKey(
                    userId: userReleaseRole.UserId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    role: userReleaseRole.Role
                );

                Assert.NotNull(result);
                Assert.Equal(userReleaseRole.Id, result.Id);
                Assert.Equal(userReleaseRole.UserId, result.UserId);
                Assert.Equal(userReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
                Assert.Equal(userReleaseRole.Role, result.Role);
                Assert.Equal(userReleaseRole.Created, result.Created);
                Assert.Equal(userReleaseRole.CreatedById, result.CreatedById);
                Assert.Equal(userReleaseRole.EmailSent, result.EmailSent);
            }
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNull()
        {
            var repository = CreateRepository();

            var result = await repository.GetByCompositeKey(
                userId: Guid.NewGuid(),
                releaseVersionId: Guid.NewGuid(),
                role: ReleaseRole.Contributor
            );

            Assert.Null(result);
        }
    }

    public class QueryTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ActiveOnlyFilter_ReturnsAllRolesForActiveUsers()
        {
            User activeUser1 = _fixture.DefaultUser();
            User activeUser2 = _fixture.DefaultUser();
            User userWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User userWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User softDeletedUser = _fixture.DefaultSoftDeletedUser();

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
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
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryableWithExplicitFilter = repository.Query(ResourceRoleFilter.ActiveOnly);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var resultsWithExplicitFilter = await resultingQueryableWithExplicitFilter.ToListAsync();

                Assert.Equal(3, resultsWithExplicitFilter.Count);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userReleaseRoles[0].Id);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userReleaseRoles[1].Id);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userReleaseRoles[2].Id);

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

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
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
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.PendingOnly);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(3, results.Count);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[2].Id);
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

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
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
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.AllButExpired);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(6, results.Count);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[2].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[3].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[4].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[5].Id);
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

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
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
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.All);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(12, results.Count);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[2].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[3].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[4].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[5].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[6].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[7].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[8].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[9].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[10].Id);
                Assert.Single(results, upr => upr.Id == userReleaseRoles[11].Id);
            }
        }
    }

    public class RemoveTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = CreateRepository(contentDbContext);

                var userReleaseRoleToRemove = await contentDbContext.UserReleaseRoles.SingleAsync(urr =>
                    urr.Id == userReleaseRole.Id
                );

                await service.Remove(userReleaseRoleToRemove);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedReleaseRole = await contentDbContext.UserReleaseRoles.SingleOrDefaultAsync(urr =>
                    urr.Id == userReleaseRole.Id
                );

                Assert.Null(updatedReleaseRole);
            }
        }
    }

    public class RemoveManyTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            User user1 = _fixture.DefaultUser().WithEmail("test1@test.com");
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            UserReleaseRole userReleaseRole1 = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user1)
                .WithReleaseVersion(releaseVersion1)
                .WithRole(ReleaseRole.Contributor);

            User user2 = _fixture.DefaultUser().WithEmail("test2@test.com");
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            UserReleaseRole userReleaseRole2 = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user2)
                .WithReleaseVersion(releaseVersion2)
                .WithRole(ReleaseRole.PrereleaseViewer);

            User user3 = _fixture.DefaultUser().WithEmail("test3@test.com");
            ReleaseVersion releaseVersion3 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            UserReleaseRole userReleaseRole3 = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user3)
                .WithReleaseVersion(releaseVersion3)
                .WithRole(ReleaseRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRole1, userReleaseRole2, userReleaseRole3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = CreateRepository(contentDbContext);

                await service.RemoveMany([userReleaseRole1, userReleaseRole2]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                Assert.Equal(userReleaseRole3.Id, userReleaseRole.Id);
            }
        }
    }

    public class RemoveForUserTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task TargetUserHasRoles_RemovesTargetRoles()
        {
            User targetUser = _fixture.DefaultUser().WithEmail("test1@test.com");
            User otherUser = _fixture.DefaultUser().WithEmail("test2@test.com");
            var role1 = ReleaseRole.Approver;
            var role2 = ReleaseRole.Contributor;
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // These 2 roles should be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetUser(targetUser))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetUser(targetUser))
                .ForIndex(1, s => s.SetRole(role2))
                // These roles are for a different email and should not be removed
                .ForIndex(2, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(2, s => s.SetUser(otherUser))
                .ForIndex(2, s => s.SetRole(role1))
                .ForIndex(3, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(3, s => s.SetUser(otherUser))
                .ForIndex(3, s => s.SetRole(role2))
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveForUser(targetUser.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles.Include(urr => urr.User).ToListAsync();

                Assert.Equal(2, remainingRoles.Count);

                Assert.Equal(releaseVersion1.Id, remainingRoles[0].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[0].User.Id);
                Assert.Equal(role1, remainingRoles[0].Role);

                Assert.Equal(releaseVersion2.Id, remainingRoles[1].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
                Assert.Equal(role2, remainingRoles[1].Role);
            }
        }

        [Fact]
        public async Task TargetUserHasNoRoles_DoesNothing()
        {
            User targetUser = _fixture.DefaultUser().WithEmail("test1@test.com");
            User otherUser = _fixture.DefaultUser().WithEmail("test2@test.com");
            var role1 = ReleaseRole.Approver;
            var role2 = ReleaseRole.Contributor;
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // These roles are for a different email and should not be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetUser(otherUser))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetUser(otherUser))
                .ForIndex(1, s => s.SetRole(role2))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(targetUser, otherUser);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveForUser(targetUser.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingRoles = await contentDbContext.UserReleaseRoles.Include(urr => urr.User).ToListAsync();

                Assert.Equal(2, remainingRoles.Count);

                Assert.Equal(releaseVersion1.Id, remainingRoles[0].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[0].User.Id);
                Assert.Equal(role1, remainingRoles[0].Role);

                Assert.Equal(releaseVersion2.Id, remainingRoles[1].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
                Assert.Equal(role2, remainingRoles[1].Role);
            }
        }
    }

    public class UserHasRoleOnReleaseVersionTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ExistingRoleNotForTargetUser_False()
        {
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRoles = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRoles);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: Guid.NewGuid(),
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRoles
                    )
                );
            }
        }

        [Fact]
        public async Task ExistingRoleNotForTargetReleaseVersion_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            var targetRoles = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithRole(targetRoles);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: Guid.NewGuid(),
                        role: targetRoles
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleExists_TrueIfRoleIsForActiveUser()
        {
            User targetActiveUser = _fixture.DefaultUser();
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );

                // Also test that the default filter is ActiveOnly
                Assert.True(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for a PENDING User Invite
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetUserWithPendingInvite)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for a ACTIVE User
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for PENDING User Invites
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET PENDING user invite + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole)
                .ForIndex(0, s => s.SetUser(targetActiveUser))
                .ForIndex(1, s => s.SetUser(targetUserWithPendingInvite))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Both of these should return true as the roles are for an ACTIVE User and a PENDING User Invite
                Assert.True(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.True(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    1,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users or PENDING User Invites
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // TARGET PENDING user invite + release version but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT release version
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // TARGET PENDING user invite + release version but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT release version
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // TARGET EXPIRED user invite + release version but DIFFERENT role
                .ForIndex(
                    6,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET EXPIRED user invite + role but DIFFERENT release version
                .ForIndex(
                    7,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT EXPIRED user invite
                .ForIndex(
                    8,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithExpiredInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // TARGET SOFT DELETED user + release version but DIFFERENT role
                .ForIndex(
                    9,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET SOFT DELETED user + role but DIFFERENT release version
                .ForIndex(
                    10,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT SOFT DELETED user
                .ForIndex(
                    11,
                    s =>
                        s.SetUser(_fixture.DefaultSoftDeletedUser())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(12);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        role: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }
    }

    public class UserHasAnyRoleOnReleaseVersionTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ExistingRoleNotForTargetUser_False()
        {
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRolesToInclude = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(ReleaseRole.Contributor);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: Guid.NewGuid(),
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRolesToInclude
                    )
                );
            }
        }

        [Fact]
        public async Task ExistingRoleNotForTargetReleaseVersion_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            var targetRolesToInclude = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.Contributor);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: Guid.NewGuid(),
                        rolesToInclude: targetRolesToInclude
                    )
                );
            }
        }

        [Fact]
        public async Task ExistingRoleNotInRolesToInclude_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRolesToInclude = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(ReleaseRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRolesToInclude
                    )
                );
            }
        }

        [Fact]
        public async Task EmptyRolesToInclude_TrueForAllRolesForMatchingUserAndReleaseVersion()
        {
            User targetActiveUser = _fixture.DefaultUser();
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contentDbContextId1 = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId1))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRoles[0]);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId1))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id
                    )
                );
            }

            var contentDbContextId2 = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId2))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRoles[1]);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId2))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleExists_TrueIfRoleIsForActiveUser()
        {
            User targetActiveUser = _fixture.DefaultUser();
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );

                // Also test that the default filter is ActiveOnly
                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for a PENDING User Invite
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetUserWithPendingInvite)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for a ACTIVE User
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for PENDING User Invites
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET PENDING user invite + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole)
                .ForIndex(0, s => s.SetUser(targetActiveUser))
                .ForIndex(1, s => s.SetUser(targetUserWithPendingInvite))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Both of these should return true as the roles are for an ACTIVE User and a PENDING User Invite
                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    1,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users or PENDING User Invites
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // TARGET PENDING user invite + release version but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT release version
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + release version but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT release version
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // TARGET PENDING user invite + release version but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT release version
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // TARGET EXPIRED user invite + release version but DIFFERENT role
                .ForIndex(
                    6,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET EXPIRED user invite + role but DIFFERENT release version
                .ForIndex(
                    7,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT EXPIRED user invite
                .ForIndex(
                    8,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithExpiredInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // TARGET SOFT DELETED user + release version but DIFFERENT role
                .ForIndex(
                    9,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET SOFT DELETED user + role but DIFFERENT release version
                .ForIndex(
                    10,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT SOFT DELETED user
                .ForIndex(
                    11,
                    s =>
                        s.SetUser(_fixture.DefaultSoftDeletedUser())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(12);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }
    }

    public class UserHasAnyRoleOnPublicationTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ExistingRoleNotForTargetUser_False()
        {
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRolesToInclude = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(ReleaseRole.Contributor);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: Guid.NewGuid(),
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRolesToInclude
                    )
                );
            }
        }

        [Fact]
        public async Task ExistingRoleNotForTargetPublication_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            var targetRolesToInclude = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.Contributor);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: Guid.NewGuid(),
                        rolesToInclude: targetRolesToInclude
                    )
                );
            }
        }

        [Fact]
        public async Task ExistingRoleNotInRolesToInclude_False()
        {
            User targetActiveUser = _fixture.DefaultUser();
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRolesToInclude = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(ReleaseRole.Approver);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRolesToInclude
                    )
                );
            }
        }

        [Fact]
        public async Task EmptyRolesToInclude_TrueForAllRolesForMatchingUserAndPublication()
        {
            User targetActiveUser = _fixture.DefaultUser();
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contentDbContextId1 = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId1))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRoles[0]);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId1))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId
                    )
                );
            }

            var contentDbContextId2 = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId2))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRoles[1]);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId2))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId
                    )
                );
            }
        }

        [Fact]
        public async Task ActiveOnlyFilter_RoleExists_TrueIfRoleIsForActiveUser()
        {
            User targetActiveUser = _fixture.DefaultUser();
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );

                // Also test that the default filter is ActiveOnly
                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for a PENDING User Invite
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetUserWithPendingInvite)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for a ACTIVE User
                .ForIndex(
                    0,
                    s => s.SetUser(targetActiveUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    2,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for PENDING User Invites
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole)
                .ForIndex(0, s => s.SetUser(targetActiveUser))
                .ForIndex(1, s => s.SetUser(targetUserWithPendingInvite))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Both of these should return true as the roles are for an ACTIVE User and a PENDING User Invite
                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // Target role is for an EXPIRED User Invite
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // Target role is for a SOFT DELETED User
                .ForIndex(
                    1,
                    s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users or PENDING User Invites
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(targetUser)
                .WithReleaseVersion(targetReleaseVersion)
                .WithRole(targetRole);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            var targetRole = ReleaseRole.Contributor;

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // TARGET ACTIVE user + publication but DIFFERENT role
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET ACTIVE user + role but DIFFERENT publication
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT ACTIVE user
                .ForIndex(
                    2,
                    s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion).SetRole(targetRole)
                )
                // TARGET PENDING user invite + publication but DIFFERENT role
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET PENDING user invite + role but DIFFERENT publication
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // TARGET EXPIRED user invite + publication but DIFFERENT role
                .ForIndex(
                    6,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET EXPIRED user invite + role but DIFFERENT publication
                .ForIndex(
                    7,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT EXPIRED user invite
                .ForIndex(
                    8,
                    s =>
                        s.SetUser(_fixture.DefaultUserWithExpiredInvite())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                // TARGET SOFT DELETED user + publication but DIFFERENT role
                .ForIndex(
                    9,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                // TARGET SOFT DELETED user + role but DIFFERENT publication
                .ForIndex(
                    10,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                            .SetRole(targetRole)
                )
                // TARGET publication + role but DIFFERENT SOFT DELETED user
                .ForIndex(
                    11,
                    s =>
                        s.SetUser(_fixture.DefaultSoftDeletedUser())
                            .SetReleaseVersion(targetReleaseVersion)
                            .SetRole(targetRole)
                )
                .GenerateList(12);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasAnyRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        rolesToInclude: targetRole,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }
    }

    public class MarkEmailAsSentTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(
                    userId: userReleaseRole.UserId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    role: userReleaseRole.Role
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                Assert.Equal(userReleaseRole.Id, updatedUserReleaseRole.Id);
                updatedUserReleaseRole.EmailSent.AssertUtcNow();
            }
        }

        [Fact]
        public async Task UserReleaseRoleDoesNotExist_Throws()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.MarkEmailAsSent(
                    userId: Guid.NewGuid(),
                    releaseVersionId: Guid.NewGuid(),
                    role: ReleaseRole.PrereleaseViewer
                )
            );
        }
    }

    private static UserReleaseRoleRepository CreateRepository(ContentDbContext? contentDbContext = null)
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(contentDbContext);
    }
}
