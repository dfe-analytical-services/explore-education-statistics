#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPrereleaseRoleRepository;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserPrereleaseRoleRepositoryTests
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

    public class CreateTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task NoSuppliedCreatedDate_SetsDateToUtcNow()
        {
            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                var result = await repository.Create(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    createdById: createdBy.Id
                );

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPreReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the pre-release role
                var createdPreReleaseRole = Assert.Single(userPreReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdPreReleaseRole.Id);
                Assert.Equal(user.Id, createdPreReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdPreReleaseRole.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, createdPreReleaseRole.Role);
                createdPreReleaseRole.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, createdPreReleaseRole.CreatedById);
                Assert.Null(createdPreReleaseRole.EmailSent);
            }
        }

        [Fact]
        public async Task SuppliedCreatedDate_SetsDateToSuppliedDate()
        {
            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            var createdDate = DateTime.UtcNow.AddDays(-2);
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                var result = await repository.Create(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    createdById: createdBy.Id,
                    createdDate: createdDate
                );

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, result.Role);
                Assert.Equal(createdDate, result.Created);
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPreReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the pre-release role
                var createdPreReleaseRole = Assert.Single(userPreReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdPreReleaseRole.Id);
                Assert.Equal(user.Id, createdPreReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdPreReleaseRole.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, createdPreReleaseRole.Role);
                Assert.Equal(createdDate, createdPreReleaseRole.Created);
                Assert.Equal(createdBy.Id, createdPreReleaseRole.CreatedById);
                Assert.Null(createdPreReleaseRole.EmailSent);
            }
        }
    }

    public class CreateManyIfNotExistsTests : UserPrereleaseRoleRepositoryTests
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
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion3 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var existingUserPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                .WithCreated(existingRoleCreatedDate)
                .WithCreatedById(createdBy.Id)
                // One role for each User/Publication combination for user1/user2/releaseVersion1/releaseVersion2
                // These should be ignored
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2))
                .GenerateList(4);

            var newUserPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                .WithCreated(newRolesCreatedDate)
                .WithCreatedById(createdBy.Id)
                // One role for each remaining User/Publication combination
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion3))
                .ForIndex(1, s => s.SetUser(user2).SetReleaseVersion(releaseVersion3))
                .ForIndex(2, s => s.SetUser(user3).SetReleaseVersion(releaseVersion1))
                .ForIndex(3, s => s.SetUser(user3).SetReleaseVersion(releaseVersion2))
                .ForIndex(4, s => s.SetUser(user3).SetReleaseVersion(releaseVersion3))
                .GenerateList(5);

            UserReleaseRole[] allUserPreReleaseRoles = [.. existingUserPreReleaseRoles, .. newUserPreReleaseRoles];
            var allUserPreReleaseRolesCreateDtos = allUserPreReleaseRoles
                .Select(uprr => new UserPrereleaseRoleCreateDto(
                    UserId: uprr.UserId,
                    ReleaseVersionId: uprr.ReleaseVersionId,
                    CreatedById: uprr.CreatedById!.Value,
                    CreatedDate: uprr.Created
                ))
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(existingUserPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                // Pass ALL roles (existing + new) to CreateManyIfNotExists. We want to test that
                // it only creates the new ones, and the existing ones are ignored/untouched.
                var results = await repository.CreateManyIfNotExists(allUserPreReleaseRolesCreateDtos);

                // Should only have created 5 new roles, as the others already existed
                Assert.Equal(5, results.Count);

                Assert.All(
                    results,
                    uprr =>
                    {
                        Assert.Equal(ReleaseRole.PrereleaseViewer, uprr.Role);
                        Assert.Equal(newRolesCreatedDate, uprr.Created);
                        Assert.Equal(createdBy.Id, uprr.CreatedById);
                        Assert.Null(uprr.EmailSent);
                    }
                );

                var resultsUserIdReleaseVersionIdPairs = results
                    .Select(uprr => (uprr.UserId, uprr.ReleaseVersionId))
                    .ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid)>
                    {
                        (user1.Id, releaseVersion3.Id),
                        (user2.Id, releaseVersion3.Id),
                        (user3.Id, releaseVersion1.Id),
                        (user3.Id, releaseVersion2.Id),
                        (user3.Id, releaseVersion3.Id),
                    },
                    resultsUserIdReleaseVersionIdPairs
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPreReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have 9 roles in total: 4 existing pre-release roles + 5 new pre-release roles
                // The existing roles should be unchanged
                Assert.Equal(9, userPreReleaseRoles.Count);

                Assert.All(
                    userPreReleaseRoles,
                    uprr =>
                    {
                        Assert.Equal(createdBy.Id, uprr.CreatedById);
                        Assert.Null(uprr.EmailSent);
                    }
                );

                var actualPreReleaseRoles = userPreReleaseRoles
                    .Select(uprr => (uprr.UserId, uprr.ReleaseVersionId, uprr.Role, uprr.Created))
                    .ToHashSet();

                var expectedPreReleaseRoles = new HashSet<(
                    Guid UserId,
                    Guid PublicationId,
                    ReleaseRole Role,
                    DateTime Created
                )>
                {
                    // Existing roles
                    (user1.Id, releaseVersion1.Id, ReleaseRole.PrereleaseViewer, existingRoleCreatedDate),
                    (user1.Id, releaseVersion2.Id, ReleaseRole.PrereleaseViewer, existingRoleCreatedDate),
                    (user2.Id, releaseVersion1.Id, ReleaseRole.PrereleaseViewer, existingRoleCreatedDate),
                    (user2.Id, releaseVersion2.Id, ReleaseRole.PrereleaseViewer, existingRoleCreatedDate),
                    // New roles
                    (user1.Id, releaseVersion3.Id, ReleaseRole.PrereleaseViewer, newRolesCreatedDate),
                    (user2.Id, releaseVersion3.Id, ReleaseRole.PrereleaseViewer, newRolesCreatedDate),
                    (user3.Id, releaseVersion1.Id, ReleaseRole.PrereleaseViewer, newRolesCreatedDate),
                    (user3.Id, releaseVersion2.Id, ReleaseRole.PrereleaseViewer, newRolesCreatedDate),
                    (user3.Id, releaseVersion3.Id, ReleaseRole.PrereleaseViewer, newRolesCreatedDate),
                };

                Assert.Equal(expectedPreReleaseRoles, actualPreReleaseRoles);
            }
        }

        [Fact]
        public async Task EmptySet_DoesNothing()
        {
            var repository = CreateRepository();

            var results = await repository.CreateManyIfNotExists([]);

            Assert.Empty(results);
        }
    }

    public class GetByIdTests : UserPrereleaseRoleRepositoryTests
    {
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task Success(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetById(userPreReleaseRole.Id);

                Assert.NotNull(result);
                Assert.Equal(userPreReleaseRole.Id, result.Id);
                Assert.Equal(userPreReleaseRole.UserId, result.UserId);
                Assert.Equal(userPreReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
                Assert.Equal(userPreReleaseRole.Created, result.Created);
                Assert.Equal(userPreReleaseRole.CreatedById, result.CreatedById);
                Assert.Equal(userPreReleaseRole.EmailSent, result.EmailSent);
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

    public class GetByCompositeKeyTests : UserPrereleaseRoleRepositoryTests
    {
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task Success(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithCreated(DateTime.UtcNow.AddDays(-2))
                .WithCreatedById(createdBy.Id)
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(createdBy);
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var result = await repository.GetByCompositeKey(
                    userId: userPreReleaseRole.UserId,
                    releaseVersionId: userPreReleaseRole.ReleaseVersionId
                );

                Assert.NotNull(result);
                Assert.Equal(userPreReleaseRole.Id, result.Id);
                Assert.Equal(userPreReleaseRole.UserId, result.UserId);
                Assert.Equal(userPreReleaseRole.ReleaseVersionId, result.ReleaseVersionId);
                Assert.Equal(userPreReleaseRole.Created, result.Created);
                Assert.Equal(userPreReleaseRole.CreatedById, result.CreatedById);
                Assert.Equal(userPreReleaseRole.EmailSent, result.EmailSent);
            }
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNull()
        {
            var repository = CreateRepository();

            var result = await repository.GetByCompositeKey(userId: Guid.NewGuid(), releaseVersionId: Guid.NewGuid());

            Assert.Null(result);
        }
    }

    public class QueryTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ActiveOnlyFilter_ReturnsAllRolesForActiveUsers()
        {
            User activeUser1 = _fixture.DefaultUser();
            User activeUser2 = _fixture.DefaultUser();
            User userWithPendingInvite = _fixture.DefaultUserWithPendingInvite();
            User userWithExpiredInvite = _fixture.DefaultUserWithExpiredInvite();
            User softDeletedUser = _fixture.DefaultSoftDeletedUser();

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
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
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryableWithExplicitFilter = repository.Query(ResourceRoleFilter.ActiveOnly);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var resultsWithExplicitFilter = await resultingQueryableWithExplicitFilter.ToListAsync();

                Assert.Equal(3, resultsWithExplicitFilter.Count);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userPreReleaseRoles[0].Id);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userPreReleaseRoles[1].Id);
                Assert.Single(resultsWithExplicitFilter, upr => upr.Id == userPreReleaseRoles[2].Id);

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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
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
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.PendingOnly);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(3, results.Count);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[2].Id);
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
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
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.AllButExpired);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(6, results.Count);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[2].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[3].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[4].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[5].Id);
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
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
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                var resultingQueryable = repository.Query(ResourceRoleFilter.All);

                // Don't apply any further filtering to the queryable, and just execute it to get all results
                var results = await resultingQueryable.ToListAsync();

                Assert.Equal(12, results.Count);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[0].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[1].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[2].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[3].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[4].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[5].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[6].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[7].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[8].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[9].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[10].Id);
                Assert.Single(results, upr => upr.Id == userPreReleaseRoles[11].Id);
            }
        }
    }

    public class RemoveByIdTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task RoleExists_RemovesAndReturnsTrue()
        {
            Publication publication = _fixture.DefaultPublication();
            UserReleaseRole userPreReleaseRoleToRemove = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                );
            UserReleaseRole otherPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(
                    _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease().WithPublication(publication))
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRoleToRemove);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext: contentDbContext);

                var result = await repository.RemoveById(userPreReleaseRoleToRemove.Id);

                Assert.True(result);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedPreReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // The existing target role should have been deleted, but the other role should remain
                var remainingUserPreReleaseRole = Assert.Single(updatedPreReleaseRoles);
                Assert.Equal(otherPreReleaseRole.Role, remainingUserPreReleaseRole.Role);
                Assert.Equal(otherPreReleaseRole.Id, remainingUserPreReleaseRole.Id);
            }
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsFalse()
        {
            var repository = CreateRepository();

            var result = await repository.RemoveById(Guid.NewGuid());

            Assert.False(result);
        }
    }

    public class RemoveManyTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ManyRoles()
        {
            // Test across multiple User/Publication combinations
            User user1 = _fixture.DefaultUser();
            User user2 = _fixture.DefaultUser();
            Publication publication1 = _fixture.DefaultPublication();
            Publication publication2 = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication1));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication2));

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // One role for each User/Publication combination.
                // We expect these to be removed.
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2))
                // Adding an additional role that should be left untouched by the removals
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(_fixture.DefaultUser())
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                .GenerateList(5);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany(userPreReleaseRoles);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserPreReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have removed ALL RELEASE roles except 1
                var remainingUserPreReleaseRole = Assert.Single(remainingUserPreReleaseRoles);
                Assert.Equal(userPreReleaseRoles[4].Id, remainingUserPreReleaseRole.Id);
            }
        }

        [Fact]
        public async Task EmptyList_DoesNothing()
        {
            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany([]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserPreReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                // Existing role should remain. Nothing should have happened.
                Assert.Equal(remainingUserPreReleaseRole.Id, userPreReleaseRole.Id);
            }
        }
    }

    public class RemoveForUserTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task TargetUserHasRoles_RemovesTargetRoles()
        {
            User targetUser = _fixture.DefaultUser().WithEmail("test1@test.com");
            User otherUser = _fixture.DefaultUser().WithEmail("test2@test.com");

            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // These 2 roles should be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetUser(targetUser))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetUser(targetUser))
                // These roles are for a different email and should not be removed
                .ForIndex(2, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(2, s => s.SetUser(otherUser))
                .ForIndex(3, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(3, s => s.SetUser(otherUser))
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
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

                Assert.Equal(releaseVersion2.Id, remainingRoles[1].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
            }
        }

        [Fact]
        public async Task TargetUserHasNoRoles_DoesNothing()
        {
            User targetUser = _fixture.DefaultUser().WithEmail("test1@test.com");
            User otherUser = _fixture.DefaultUser().WithEmail("test2@test.com");

            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // These roles are for a different email and should not be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetUser(otherUser))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetUser(otherUser))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(targetUser, otherUser);
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
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

                Assert.Equal(releaseVersion2.Id, remainingRoles[1].ReleaseVersionId);
                Assert.Equal(otherUser.Id, remainingRoles[1].User.Id);
            }
        }
    }

    public class UserHasPrereleaseRoleOnReleaseVersionTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ExistingRoleNotForTargetUser_False()
        {
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: Guid.NewGuid(),
                        releaseVersionId: targetReleaseVersion.Id
                    )
                );
            }
        }

        [Fact]
        public async Task ExistingRoleNotForTargetReleaseVersion_False()
        {
            User targetActiveUser = _fixture.DefaultUser();

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: Guid.NewGuid()
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

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );

                // Also test that the default filter is ActiveOnly
                Assert.True(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // Target role is for a PENDING User Invite
                .ForIndex(0, s => s.SetUser(targetUserWithPendingInvite).SetReleaseVersion(targetReleaseVersion))
                // Target role is for an EXPIRED User Invite
                .ForIndex(1, s => s.SetUser(targetUserWithExpiredInvite).SetReleaseVersion(targetReleaseVersion))
                // Target role is for a SOFT DELETED User
                .ForIndex(2, s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET ACTIVE user but DIFFERENT release version
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT ACTIVE user
                .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetUserWithPendingInvite)
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // role is for a ACTIVE User
                .ForIndex(0, s => s.SetUser(targetActiveUser).SetReleaseVersion(targetReleaseVersion))
                // role is for an EXPIRED User Invite
                .ForIndex(1, s => s.SetUser(targetUserWithExpiredInvite).SetReleaseVersion(targetReleaseVersion))
                // role is for a SOFT DELETED User
                .ForIndex(2, s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for PENDING User Invites
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET PENDING user invite + role but DIFFERENT release version
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication + role but DIFFERENT PENDING user invite
                .ForIndex(
                    1,
                    s => s.SetUser(_fixture.DefaultUserWithPendingInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(targetReleaseVersion)
                .ForIndex(0, s => s.SetUser(targetActiveUser))
                .ForIndex(1, s => s.SetUser(targetUserWithPendingInvite))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Both of these should return true as the roles are for an ACTIVE User and a PENDING User Invite
                Assert.True(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.True(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // role is for an EXPIRED User Invite
                .ForIndex(0, s => s.SetUser(targetUserWithExpiredInvite).SetReleaseVersion(targetReleaseVersion))
                // role is for a SOFT DELETED User
                .ForIndex(1, s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users or PENDING User Invites
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET ACTIVE user but DIFFERENT release version
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT ACTIVE user
                .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion))
                // TARGET PENDING user invite but DIFFERENT release version
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT PENDING user invite
                .ForIndex(
                    3,
                    s => s.SetUser(_fixture.DefaultUserWithPendingInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetUser)
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET ACTIVE user but DIFFERENT release version
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT ACTIVE user
                .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion))
                // TARGET PENDING user invite but DIFFERENT release version
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT PENDING user invite
                .ForIndex(
                    3,
                    s => s.SetUser(_fixture.DefaultUserWithPendingInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                // TARGET EXPIRED user invite but DIFFERENT release version
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT EXPIRED user invite
                .ForIndex(
                    5,
                    s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                // TARGET SOFT DELETED user but DIFFERENT release version
                .ForIndex(
                    6,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT SOFT DELETED user
                .ForIndex(7, s => s.SetUser(_fixture.DefaultSoftDeletedUser()).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(8);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetActiveUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithPendingInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetUserWithExpiredInvite.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnReleaseVersion(
                        userId: targetSoftDeletedUser.Id,
                        releaseVersionId: targetReleaseVersion.Id,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }
    }

    public class UserHasPrereleaseRoleOnPublicationTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ExistingRoleNotForTargetUser_False()
        {
            ReleaseVersion targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: Guid.NewGuid(),
                        publicationId: targetReleaseVersion.Release.PublicationId
                    )
                );
            }
        }

        [Fact]
        public async Task ExistingRoleNotForTargetPublication_False()
        {
            User targetActiveUser = _fixture.DefaultUser();

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: Guid.NewGuid()
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

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetActiveUser)
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );

                // Also test that the default filter is ActiveOnly
                Assert.True(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // role is for a PENDING User Invite
                .ForIndex(0, s => s.SetUser(targetUserWithPendingInvite).SetReleaseVersion(targetReleaseVersion))
                // role is for an EXPIRED User Invite
                .ForIndex(1, s => s.SetUser(targetUserWithExpiredInvite).SetReleaseVersion(targetReleaseVersion))
                // role is for a SOFT DELETED User
                .ForIndex(2, s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.ActiveOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET ACTIVE user but DIFFERENT publication
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT ACTIVE user
                .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetUserWithPendingInvite)
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // role is for a ACTIVE User
                .ForIndex(0, s => s.SetUser(targetActiveUser).SetReleaseVersion(targetReleaseVersion))
                // role is for an EXPIRED User Invite
                .ForIndex(1, s => s.SetUser(targetUserWithExpiredInvite).SetReleaseVersion(targetReleaseVersion))
                // role is for a SOFT DELETED User
                .ForIndex(2, s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for PENDING User Invites
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.PendingOnly
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET PENDING user invite but DIFFERENT publication
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT PENDING user invite
                .ForIndex(
                    1,
                    s => s.SetUser(_fixture.DefaultUserWithPendingInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(targetReleaseVersion)
                .ForIndex(0, s => s.SetUser(targetActiveUser))
                .ForIndex(1, s => s.SetUser(targetUserWithPendingInvite))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Both of these should return true as the roles are for an ACTIVE User and a PENDING User Invite
                Assert.True(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.True(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // role is for an EXPIRED User Invite
                .ForIndex(0, s => s.SetUser(targetUserWithExpiredInvite).SetReleaseVersion(targetReleaseVersion))
                // role is for a SOFT DELETED User
                .ForIndex(1, s => s.SetUser(targetSoftDeletedUser).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                // Each of these should return false as the roles are not for ACTIVE Users or PENDING User Invites
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET ACTIVE user but DIFFERENT publication
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT ACTIVE user
                .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion))
                // TARGET PENDING user invite but DIFFERENT publication
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT PENDING user invite
                .ForIndex(
                    3,
                    s => s.SetUser(_fixture.DefaultUserWithPendingInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.AllButExpired
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(targetUser)
                .WithReleaseVersion(targetReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.True(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
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

            var userPreReleaseRoles = _fixture
                .DefaultUserPrereleaseRole()
                // TARGET ACTIVE user but DIFFERENT publication
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(targetActiveUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT ACTIVE user
                .ForIndex(1, s => s.SetUser(_fixture.DefaultUser()).SetReleaseVersion(targetReleaseVersion))
                // TARGET PENDING user invite but DIFFERENT publication
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(targetUserWithPendingInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT PENDING user invite
                .ForIndex(
                    3,
                    s => s.SetUser(_fixture.DefaultUserWithPendingInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                // TARGET EXPIRED user invite but DIFFERENT publication
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(targetUserWithExpiredInvite)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT EXPIRED user invite
                .ForIndex(
                    5,
                    s => s.SetUser(_fixture.DefaultUserWithExpiredInvite()).SetReleaseVersion(targetReleaseVersion)
                )
                // TARGET SOFT DELETED user but DIFFERENT publication
                .ForIndex(
                    6,
                    s =>
                        s.SetUser(targetSoftDeletedUser)
                            .SetReleaseVersion(
                                _fixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())
                                    )
                            )
                )
                // TARGET publication but DIFFERENT SOFT DELETED user
                .ForIndex(7, s => s.SetUser(_fixture.DefaultSoftDeletedUser()).SetReleaseVersion(targetReleaseVersion))
                .GenerateList(8);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userPreReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetActiveUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithPendingInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetUserWithExpiredInvite.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
                Assert.False(
                    await repository.UserHasPrereleaseRoleOnPublication(
                        userId: targetSoftDeletedUser.Id,
                        publicationId: targetReleaseVersion.Release.PublicationId,
                        resourceRoleFilter: ResourceRoleFilter.All
                    )
                );
            }
        }
    }

    public class MarkEmailAsSentTests : UserPrereleaseRoleRepositoryTests
    {
        [Fact]
        public async Task Success_NoSuppliedDateSent()
        {
            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(userPreReleaseRole.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                Assert.Equal(userPreReleaseRole.Id, updatedUserReleaseRole.Id);
                updatedUserReleaseRole.EmailSent.AssertUtcNow();
            }
        }

        [Fact]
        public async Task Success_SuppliedDateSent()
        {
            UserReleaseRole userPreReleaseRole = _fixture
                .DefaultUserPrereleaseRole()
                .WithUser(_fixture.DefaultUser())
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                );

            var dateSent = DateTimeOffset.UtcNow.AddDays(-10);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.Add(userPreReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(userPreReleaseRole.Id, dateSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                Assert.Equal(userPreReleaseRole.Id, updatedUserReleaseRole.Id);
                Assert.Equal(dateSent, updatedUserReleaseRole.EmailSent);
            }
        }

        [Fact]
        public async Task UserReleaseRoleDoesNotExist_Throws()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.MarkEmailAsSent(Guid.NewGuid())
            );
        }
    }

    private static UserPrereleaseRoleRepository CreateRepository(ContentDbContext? contentDbContext = null)
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(contentDbContext);
    }
}
