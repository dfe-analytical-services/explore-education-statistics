#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using Moq;
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
        public async Task NoNewPermissionsSystemPublicationRoleChanges()
        {
            var releaseRoleToCreate = ReleaseRole.Contributor;
            var newPublicationRoleToRemain = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newPublicationRoleToRemain);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            // Should not have tried to remove any existing publication role
            // Should not have tried to create any new publication roles
            // So we have purposefully not set up any 'Create' or 'Remove' method calls on this mock
            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, existingPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                var result = await repository.Create(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    role: releaseRoleToCreate,
                    createdById: createdBy.Id
                );

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, result.Role);
                Assert.Equal(createdBy.Id, result.CreatedById);
                result.Created.AssertUtcNow();
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the `Contributor` release role
                var createdReleaseRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
                Assert.Equal(user.Id, createdReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
                Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);
                Assert.Null(createdReleaseRole.EmailSent);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRolesToRemoveAndCreate()
        {
            var releaseRoleToCreate = ReleaseRole.Approver;
            var newPublicationRoleToCreate = PublicationRole.Approver;
            var newPublicationRoleToRemove = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newPublicationRoleToRemove);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, existingPublicationRole);
            // Should have tried to remove the existing publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.RemoveRole(existingPublicationRole, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            // Should have tried to create the new 'Approver' publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr =>
                    rvr.CreateRole(
                        user.Id,
                        publication.Id,
                        newPublicationRoleToCreate,
                        createdBy.Id,
                        It.Is<DateTime>(createdDate =>
                            Math.Abs((createdDate - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(It.IsAny<UserPublicationRole>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                var result = await repository.Create(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    role: releaseRoleToCreate,
                    createdById: createdBy.Id
                );

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the `Approver` release role
                var createdReleaseRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
                Assert.Equal(user.Id, createdReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
                Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);
                Assert.Null(createdReleaseRole.EmailSent);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRoleToCreate()
        {
            var releaseRoleToCreate = ReleaseRole.Contributor;
            var newPublicationRoleToCreate = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, []);
            // Should not have tried to remove any existing publication role
            // Should have tried to create the new 'Approver' publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr =>
                    rvr.CreateRole(
                        user.Id,
                        publication.Id,
                        newPublicationRoleToCreate,
                        createdBy.Id,
                        It.Is<DateTime>(createdDate =>
                            Math.Abs((createdDate - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(It.IsAny<UserPublicationRole>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                var result = await repository.Create(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    role: releaseRoleToCreate,
                    createdById: createdBy.Id
                );

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, result.Role);
                result.Created.AssertUtcNow();
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the `Contributor` release role
                var createdReleaseRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
                Assert.Equal(user.Id, createdReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
                Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);
                Assert.Null(createdReleaseRole.EmailSent);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            const ReleaseRole releaseRoleToCreate = ReleaseRole.Contributor;
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
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
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Users.AddRange(user, createdBy);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, [.. existingPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                await repository.Create(
                    userId: user.Id,
                    releaseVersionId: releaseVersion.Id,
                    releaseRoleToCreate,
                    createdById: createdBy.Id
                );
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }
    }

    public class CreateManyIfNotExistsTests : UserReleaseRoleRepositoryTests
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
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication1));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication2));

            var newUserReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithCreated(createdDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(ReleaseRole.Contributor)
                // One 'Contributor' role for each User/Publication combination.
                // We expect a NEW permissions system 'Drafter' publication role to be created each of these,
                // in addition to these roles themselves.
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2))
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user1, user2);
                contentDbContext.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, true, []);
            foreach (var newUserReleaseRole in newUserReleaseRoles)
            {
                // Should create a 'Drafter' role for each User/Publication pairs.
                userPublicationRoleRepository
                    .Setup(m =>
                        m.CreateRole(
                            newUserReleaseRole.UserId,
                            newUserReleaseRole.ReleaseVersion.Release.PublicationId,
                            PublicationRole.Drafter,
                            createdBy.Id,
                            createdDate,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(It.IsAny<UserPublicationRole>());
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var results = await repository.CreateManyIfNotExists(newUserReleaseRoles);

                // Should have created 4 new release roles, and 4 NEW permissions roles.
                // But only expect the 4 release roles to be returned in the result. The publication roles are
                // created quietly in the background.
                Assert.Equal(4, results.Count);

                Assert.All(
                    results,
                    urr =>
                    {
                        Assert.Equal(ReleaseRole.Contributor, urr.Role);
                        Assert.Equal(createdDate, urr.Created);
                        Assert.Equal(createdBy.Id, urr.CreatedById);
                        Assert.Null(urr.EmailSent);
                    }
                );

                var resultsUserIdReleaseVersionIdPairs = results
                    .Select(urr => (urr.UserId, urr.ReleaseVersionId))
                    .ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid)>
                    {
                        (user1.Id, releaseVersion1.Id),
                        (user1.Id, releaseVersion2.Id),
                        (user2.Id, releaseVersion1.Id),
                        (user2.Id, releaseVersion2.Id),
                    },
                    resultsUserIdReleaseVersionIdPairs
                );
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);

            userPublicationRoleRepository.Verify(m => m.Query(ResourceRoleFilter.All, true), Times.Exactly(4));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                Assert.Equal(4, userReleaseRoles.Count);

                Assert.All(
                    userReleaseRoles,
                    urr =>
                    {
                        Assert.Equal(createdDate, urr.Created);
                        Assert.Equal(createdBy.Id, urr.CreatedById);
                        Assert.Null(urr.EmailSent);
                    }
                );

                var actualReleaseRoles = userReleaseRoles
                    .Select(urr => (urr.UserId, urr.ReleaseVersionId, urr.Role))
                    .ToHashSet();

                var expectedReleaseRoles = new HashSet<(Guid UserId, Guid PublicationId, ReleaseRole Role)>
                {
                    // New release roles
                    (user1.Id, releaseVersion1.Id, ReleaseRole.Contributor),
                    (user1.Id, releaseVersion2.Id, ReleaseRole.Contributor),
                    (user2.Id, releaseVersion1.Id, ReleaseRole.Contributor),
                    (user2.Id, releaseVersion2.Id, ReleaseRole.Contributor),
                };

                Assert.Equal(expectedReleaseRoles, actualReleaseRoles);
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
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication1));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication2));
            ReleaseVersion releaseVersion3 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication3));
            ReleaseVersion releaseVersion4 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication4));

            var newUserReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithCreated(createdDate)
                .WithCreatedById(createdBy.Id)
                // One 'Contributor' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                // We would normally expect a NEW permissions system 'Drafter' role to be created each of these,
                // in addition to these roles themselves. However, in this case, each User/Publication combination
                // also has a new RELEASE 'Approver' role which needs creating, meaning that the 'Drafter' PUBLICATION
                // role is upgraded to 'Approver'.
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
                // One 'Approver' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                .ForIndex(4, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Approver))
                .ForIndex(5, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                .ForIndex(6, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Approver))
                .ForIndex(7, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                // Here, we switch the order of creation of the 'Contributor' and 'Approver' roles, to check that the order of these
                // doesn't affect the outcome. In this case, the 'Contributor' role is created after the RELEASE 'Approver' role, but we
                // should still see the same upgrade of PUBLICATION 'Drafter' to 'Approver'.
                // One 'Approver' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(8, s => s.SetUser(user3).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Approver))
                .ForIndex(9, s => s.SetUser(user3).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Approver))
                .ForIndex(10, s => s.SetUser(user4).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Approver))
                .ForIndex(11, s => s.SetUser(user4).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Approver))
                // One 'Contributor' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(12, s => s.SetUser(user3).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Contributor))
                .ForIndex(13, s => s.SetUser(user3).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Contributor))
                .ForIndex(14, s => s.SetUser(user4).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Contributor))
                .ForIndex(15, s => s.SetUser(user4).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Contributor))
                .GenerateList(16);

            var userReleaseRolesResultingInDrafterCreation = newUserReleaseRoles[..4];
            var userReleaseRolesResultingInApproverCreation = newUserReleaseRoles[4..12];

            var userPublicationDraftersToCreate = userReleaseRolesResultingInDrafterCreation
                .Select(urr =>
                    _fixture
                        .DefaultUserPublicationRole()
                        .WithUserId(urr.UserId)
                        .WithPublicationId(urr.ReleaseVersion.Release.PublicationId)
                        .WithRole(PublicationRole.Drafter)
                        .WithCreatedById(urr.CreatedById!.Value)
                        .WithCreated(createdDate)
                        .Generate()
                )
                .ToList();

            var userPublicationApproversToCreate = userReleaseRolesResultingInApproverCreation
                .Select(urr =>
                    _fixture
                        .DefaultUserPublicationRole()
                        .WithUserId(urr.UserId)
                        .WithPublicationId(urr.ReleaseVersion.Release.PublicationId)
                        .WithRole(PublicationRole.Approver)
                        .WithCreatedById(urr.CreatedById!.Value)
                        .WithCreated(createdDate)
                        .Generate()
                )
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user1, user2, user3, user4);
                contentDbContext.ReleaseVersions.AddRange(
                    releaseVersion1,
                    releaseVersion2,
                    releaseVersion3,
                    releaseVersion4
                );
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);

            int queryCallCount = 0;
            userPublicationRoleRepository
                .Setup(m => m.Query(ResourceRoleFilter.All, true))
                .Returns(() =>
                {
                    queryCallCount++;

                    if (queryCallCount <= 4)
                        return Array.Empty<UserPublicationRole>().BuildMock();

                    if (queryCallCount <= 8)
                        return new[] { userPublicationDraftersToCreate[queryCallCount - 5] }.BuildMock();

                    if (queryCallCount <= 12)
                        return Array.Empty<UserPublicationRole>().BuildMock();

                    if (queryCallCount <= 16)
                        // The first 4 of 'userPublicationApproversToCreate' are for Users 1/2 & Publications 1/2.
                        // Here, we're interested in the ones created for Users 3/4 & Publications 3/4, as these are the
                        // ones that are created before the 'Contributor' role - which correspond to the last 4 query calls.
                        return new[] { userPublicationApproversToCreate[queryCallCount - 13 + 4] }.BuildMock();

                    throw new InvalidOperationException("Unexpected call count");
                });

            foreach (var userPublicationDrafterToCreate in userPublicationDraftersToCreate)
            {
                // Should create a 'Drafter' role for each User/Publication pairs where the 'Contributor' role is being created first
                // This is the case for Users 1/2 & Publications 1/2 pairs.
                userPublicationRoleRepository
                    .Setup(m =>
                        m.CreateRole(
                            userPublicationDrafterToCreate.UserId,
                            userPublicationDrafterToCreate.PublicationId,
                            userPublicationDrafterToCreate.Role,
                            userPublicationDrafterToCreate.CreatedById!.Value,
                            userPublicationDrafterToCreate.Created,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(userPublicationDrafterToCreate);

                // Should remove the created 'Drafter' role for each User/Publication pair, once the second 'Approver' role is created.
                userPublicationRoleRepository
                    .Setup(m =>
                        m.RemoveRole(
                            It.Is<UserPublicationRole>(upr => upr.Id == userPublicationDrafterToCreate.Id),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);
            }

            foreach (var userPublicationApproverToCreate in userPublicationApproversToCreate)
            {
                // Should create a 'Approver' role for each User/Publication pairs where the 'Approver' role is being created
                userPublicationRoleRepository
                    .Setup(m =>
                        m.CreateRole(
                            userPublicationApproverToCreate.UserId,
                            userPublicationApproverToCreate.PublicationId,
                            userPublicationApproverToCreate.Role,
                            userPublicationApproverToCreate.CreatedById!.Value,
                            userPublicationApproverToCreate.Created,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(userPublicationApproverToCreate);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var results = await repository.CreateManyIfNotExists(newUserReleaseRoles);

                // Should have created 16 new RELEASE roles, and 8 new PUBLICATION roles.
                // But only expect the 16 RELEASE roles to be returned in the result. The PUBLICATION roles are
                // created quietly in the background.
                Assert.Equal(16, results.Count);

                Assert.All(
                    results,
                    urr =>
                    {
                        Assert.Equal(createdDate, urr.Created);
                        Assert.Equal(createdBy.Id, urr.CreatedById);
                        Assert.Null(urr.EmailSent);
                    }
                );

                var resultsUserIdReleaseVersionIdPairs = results
                    .Select(urr => (urr.UserId, urr.ReleaseVersionId, urr.Role))
                    .ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid, ReleaseRole)>
                    {
                        (user1.Id, releaseVersion1.Id, ReleaseRole.Contributor),
                        (user1.Id, releaseVersion2.Id, ReleaseRole.Contributor),
                        (user2.Id, releaseVersion1.Id, ReleaseRole.Contributor),
                        (user2.Id, releaseVersion2.Id, ReleaseRole.Contributor),
                        (user1.Id, releaseVersion1.Id, ReleaseRole.Approver),
                        (user1.Id, releaseVersion2.Id, ReleaseRole.Approver),
                        (user2.Id, releaseVersion1.Id, ReleaseRole.Approver),
                        (user2.Id, releaseVersion2.Id, ReleaseRole.Approver),
                        (user3.Id, releaseVersion3.Id, ReleaseRole.Contributor),
                        (user3.Id, releaseVersion4.Id, ReleaseRole.Contributor),
                        (user4.Id, releaseVersion3.Id, ReleaseRole.Contributor),
                        (user4.Id, releaseVersion4.Id, ReleaseRole.Contributor),
                        (user3.Id, releaseVersion3.Id, ReleaseRole.Approver),
                        (user3.Id, releaseVersion4.Id, ReleaseRole.Approver),
                        (user4.Id, releaseVersion3.Id, ReleaseRole.Approver),
                        (user4.Id, releaseVersion4.Id, ReleaseRole.Approver),
                    },
                    resultsUserIdReleaseVersionIdPairs
                );
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);

            userPublicationRoleRepository.Verify(m => m.Query(ResourceRoleFilter.All, true), Times.Exactly(16));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                Assert.Equal(16, userReleaseRoles.Count);

                Assert.All(
                    userReleaseRoles,
                    urr =>
                    {
                        Assert.Equal(createdDate, urr.Created);
                        Assert.Equal(createdBy.Id, urr.CreatedById);
                        Assert.Null(urr.EmailSent);
                    }
                );

                var actualReleaseRoles = userReleaseRoles
                    .Select(urr => (urr.UserId, urr.ReleaseVersionId, urr.Role))
                    .ToHashSet();

                var expectedReleaseRoles = new HashSet<(Guid UserId, Guid PublicationId, ReleaseRole Role)>
                {
                    // New RELEASE roles
                    (user1.Id, releaseVersion1.Id, ReleaseRole.Contributor),
                    (user1.Id, releaseVersion2.Id, ReleaseRole.Contributor),
                    (user2.Id, releaseVersion1.Id, ReleaseRole.Contributor),
                    (user2.Id, releaseVersion2.Id, ReleaseRole.Contributor),
                    (user1.Id, releaseVersion1.Id, ReleaseRole.Approver),
                    (user1.Id, releaseVersion2.Id, ReleaseRole.Approver),
                    (user2.Id, releaseVersion1.Id, ReleaseRole.Approver),
                    (user2.Id, releaseVersion2.Id, ReleaseRole.Approver),
                    (user3.Id, releaseVersion3.Id, ReleaseRole.Contributor),
                    (user3.Id, releaseVersion4.Id, ReleaseRole.Contributor),
                    (user4.Id, releaseVersion3.Id, ReleaseRole.Contributor),
                    (user4.Id, releaseVersion4.Id, ReleaseRole.Contributor),
                    (user3.Id, releaseVersion3.Id, ReleaseRole.Approver),
                    (user3.Id, releaseVersion4.Id, ReleaseRole.Approver),
                    (user4.Id, releaseVersion3.Id, ReleaseRole.Approver),
                    (user4.Id, releaseVersion4.Id, ReleaseRole.Approver),
                };

                Assert.Equal(expectedReleaseRoles, actualReleaseRoles);
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
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication1));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication2));

            var existingUserPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreated(existingRoleCreatedDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(PublicationRole.Drafter)
                // One 'Drafter' role for each User/Publication combination
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2))
                .GenerateList(4);

            var existingUserReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithCreated(existingRoleCreatedDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(ReleaseRole.PrereleaseViewer)
                // One 'PrereleaseViewer' role for each User/Publication combination
                // These should be ignored
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2))
                .GenerateList(4);

            var newUserReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithCreated(newRolesCreatedDate)
                .WithCreatedById(createdBy.Id)
                .WithRole(ReleaseRole.Contributor)
                // One 'Contributor' role for each User/Publication combination
                // Don't expect a NEW permissions system role to be created for any of these, as they already have a
                // 'Drafter' role.
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2))
                .GenerateList(4);

            UserReleaseRole[] allUserReleaseRoles = [.. existingUserReleaseRoles, .. newUserReleaseRoles];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingUserPublicationRoles);
                contentDbContext.UserReleaseRoles.AddRange(existingUserReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(
                ResourceRoleFilter.All,
                true,
                [.. existingUserPublicationRoles]
            );

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                // Pass ALL release roles (existing + new) to CreateManyIfNotExists. We want to test that
                // it only creates the new ones, and the existing ones are ignored/untouched.
                var results = await repository.CreateManyIfNotExists(allUserReleaseRoles);

                // Should only have created 4 new roles, as the others already existed
                Assert.Equal(4, results.Count);

                Assert.All(
                    results,
                    urr =>
                    {
                        Assert.Equal(ReleaseRole.Contributor, urr.Role);
                        Assert.Equal(newRolesCreatedDate, urr.Created);
                        Assert.Equal(createdBy.Id, urr.CreatedById);
                        Assert.Null(urr.EmailSent);
                    }
                );

                var resultsUserIdReleaseVersionIdPairs = results
                    .Select(urr => (urr.UserId, urr.ReleaseVersionId))
                    .ToHashSet();

                Assert.Equal(
                    new HashSet<(Guid, Guid)>
                    {
                        (user1.Id, releaseVersion1.Id),
                        (user1.Id, releaseVersion2.Id),
                        (user2.Id, releaseVersion1.Id),
                        (user2.Id, releaseVersion2.Id),
                    },
                    resultsUserIdReleaseVersionIdPairs
                );
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userPublicationRoles = await contentDbContext
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have 12 roles in total: 4 existing publication roles + 4 existing release roles + 4 new release roles
                // The existing roles should be unchanged
                Assert.Equal(4, userPublicationRoles.Count);
                Assert.Equal(8, userReleaseRoles.Count);

                Assert.All(
                    userPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(createdBy.Id, upr.CreatedById);
                        Assert.Null(upr.EmailSent);
                    }
                );

                Assert.All(
                    userReleaseRoles,
                    urr =>
                    {
                        Assert.Equal(createdBy.Id, urr.CreatedById);
                        Assert.Null(urr.EmailSent);
                    }
                );

                var actualPublicationRoles = userPublicationRoles
                    .Select(upr => (upr.UserId, upr.PublicationId, upr.Role, upr.Created))
                    .ToHashSet();

                var actualReleaseRoles = userReleaseRoles
                    .Select(urr => (urr.UserId, urr.ReleaseVersionId, urr.Role, urr.Created))
                    .ToHashSet();

                var expectedPublicationRoles = new HashSet<(
                    Guid UserId,
                    Guid PublicationId,
                    PublicationRole Role,
                    DateTime Created
                )>
                {
                    // Existing roles
                    (user1.Id, publication1.Id, PublicationRole.Drafter, existingRoleCreatedDate),
                    (user2.Id, publication1.Id, PublicationRole.Drafter, existingRoleCreatedDate),
                    (user1.Id, publication2.Id, PublicationRole.Drafter, existingRoleCreatedDate),
                    (user2.Id, publication2.Id, PublicationRole.Drafter, existingRoleCreatedDate),
                };

                var expectedReleaseRoles = new HashSet<(
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
                    (user1.Id, releaseVersion1.Id, ReleaseRole.Contributor, newRolesCreatedDate),
                    (user1.Id, releaseVersion2.Id, ReleaseRole.Contributor, newRolesCreatedDate),
                    (user2.Id, releaseVersion1.Id, ReleaseRole.Contributor, newRolesCreatedDate),
                    (user2.Id, releaseVersion2.Id, ReleaseRole.Contributor, newRolesCreatedDate),
                };

                Assert.Equal(expectedPublicationRoles, actualPublicationRoles);
                Assert.Equal(expectedReleaseRoles, actualReleaseRoles);
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
        public async Task NoNewPermissionsSystemPublicationRoleChanges()
        {
            var releaseRoleToCreate = ReleaseRole.Contributor;
            var newPublicationRoleToRemain = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            var created = DateTime.UtcNow.AddDays(-1);
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));

            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newPublicationRoleToRemain);

            UserReleaseRole userReleaseRoleToCreate = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRoleToCreate)
                .WithCreatedById(createdBy.Id)
                .WithCreated(created);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            // Should not have tried to remove any existing publication role
            // Should not have tried to create any new publication roles
            // So we have purposefully not set up any 'Create' or 'Remove' method calls on this mock
            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, existingPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                var results = await repository.CreateManyIfNotExists([userReleaseRoleToCreate]);

                var result = Assert.Single(results);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, result.Role);
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Equal(created, result.Created);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the `Contributor` release role
                var createdReleaseRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
                Assert.Equal(user.Id, createdReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
                Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);
                Assert.Null(createdReleaseRole.EmailSent);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRolesToRemoveAndCreate()
        {
            var releaseRoleToCreate = ReleaseRole.Approver;
            var newPublicationRoleToCreate = PublicationRole.Approver;
            var newPublicationRoleToRemove = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            var created = DateTime.UtcNow.AddDays(-1);
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));

            UserPublicationRole existingPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(newPublicationRoleToRemove);

            UserReleaseRole userReleaseRoleToCreate = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRoleToCreate)
                .WithCreatedById(createdBy.Id)
                .WithCreated(created);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, existingPublicationRole);
            // Should have tried to remove the existing publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.RemoveRole(existingPublicationRole, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            // Should have tried to create the new 'Approver' publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr =>
                    rvr.CreateRole(
                        user.Id,
                        publication.Id,
                        newPublicationRoleToCreate,
                        createdBy.Id,
                        It.Is<DateTime>(createdDate =>
                            Math.Abs((createdDate - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(It.IsAny<UserPublicationRole>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                var results = await repository.CreateManyIfNotExists([userReleaseRoleToCreate]);

                var result = Assert.Single(results);

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, result.Role);
                Assert.Equal(created, result.Created);
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the `Approver` release role
                var createdReleaseRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
                Assert.Equal(user.Id, createdReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
                Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);
                Assert.Null(createdReleaseRole.EmailSent);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRoleToCreate()
        {
            var releaseRoleToCreate = ReleaseRole.Contributor;
            var newPublicationRoleToCreate = PublicationRole.Drafter;

            User user = _fixture.DefaultUser();
            User createdBy = _fixture.DefaultUser();
            var created = DateTime.UtcNow.AddDays(-1);
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));

            UserReleaseRole userReleaseRoleToCreate = _fixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRoleToCreate)
                .WithCreatedById(createdBy.Id)
                .WithCreated(created);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(user, createdBy);
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, []);
            // Should not have tried to remove any existing publication role
            // Should have tried to create the new 'Approver' publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr =>
                    rvr.CreateRole(
                        user.Id,
                        publication.Id,
                        newPublicationRoleToCreate,
                        createdBy.Id,
                        It.Is<DateTime>(createdDate =>
                            Math.Abs((createdDate - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(It.IsAny<UserPublicationRole>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                var results = await repository.CreateManyIfNotExists([userReleaseRoleToCreate]);

                var result = Assert.Single(results);

                Assert.NotNull(result);

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(user.Id, result.UserId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, result.Role);
                Assert.Equal(created, result.Created);
                Assert.Equal(createdBy.Id, result.CreatedById);
                Assert.Null(result.EmailSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have created the `Contributor` release role
                var createdReleaseRole = Assert.Single(userReleaseRoles);

                Assert.NotEqual(Guid.Empty, createdReleaseRole.Id);
                Assert.Equal(user.Id, createdReleaseRole.UserId);
                Assert.Equal(releaseVersion.Id, createdReleaseRole.ReleaseVersionId);
                Assert.Equal(releaseRoleToCreate, createdReleaseRole.Role);
                Assert.Equal(createdBy.Id, createdReleaseRole.CreatedById);
                Assert.Null(createdReleaseRole.EmailSent);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            const ReleaseRole releaseRoleToCreate = ReleaseRole.Contributor;
            var user = userFactory(_fixture);
            User createdBy = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
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
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Users.AddRange(user, createdBy);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, [.. existingPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                UserReleaseRole userReleaseRoleToCreate = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(user)
                    .WithReleaseVersion(releaseVersion)
                    .WithRole(releaseRoleToCreate)
                    .WithCreatedById(createdBy.Id);

                await repository.CreateManyIfNotExists([userReleaseRoleToCreate]);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
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
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1));

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
                .WithEmailSent(DateTimeOffset.UtcNow.AddDays(-1));

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
        public async Task NoNewPermissionsSystemPublicationRoleChanges()
        {
            var releaseRoleToRemove = ReleaseRole.Approver;

            User user = _fixture.DefaultUser();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            UserReleaseRole userReleaseRoleToRemove = _fixture
                .DefaultUserReleaseRole()
                .WithRole(releaseRoleToRemove)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserReleaseRole otherUserReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithRole(ReleaseRole.PrereleaseViewer)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleToRemove, otherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, []);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                await repository.RemoveById(userReleaseRoleToRemove.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // The existing 'Approver' role should have been deleted, but the other role should remain
                var remainingUserReleaseRole = Assert.Single(updatedUserReleaseRoles);
                Assert.Equal(otherUserReleaseRole.Role, remainingUserReleaseRole.Role);
                Assert.Equal(otherUserReleaseRole.Id, remainingUserReleaseRole.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRoleToRemoveAndCreate()
        {
            User deletedUserPlaceholder = _fixture.DefaultDeletedUserPlaceholder().WithId(Guid.NewGuid());

            var releaseRoleToRemove = ReleaseRole.Approver;
            var newPublicationRoleToRemove = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            UserReleaseRole userReleaseRoleToRemove = _fixture
                .DefaultUserReleaseRole()
                .WithRole(releaseRoleToRemove)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserReleaseRole otherUserReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithRole(ReleaseRole.Contributor)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleToRemove, otherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepositoryMock
                .Setup(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedUserPlaceholder);

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, userPublicationRole);
            // Should have tried to remove the existing publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.RemoveRole(userPublicationRole, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            // Should have tried to create the new 'Drafter' publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr =>
                    rvr.CreateRole(
                        userReleaseRoleToRemove.UserId,
                        userReleaseRoleToRemove.ReleaseVersion.Release.PublicationId,
                        PublicationRole.Drafter,
                        deletedUserPlaceholder.Id,
                        It.Is<DateTime>(dt =>
                            Math.Abs((dt - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(It.IsAny<UserPublicationRole>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object,
                    userRepository: userRepositoryMock.Object
                );

                await repository.RemoveById(userReleaseRoleToRemove.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // The existing release 'Approver' role should have been deleted, but the other role should remain
                var remainingUserReleaseRole = Assert.Single(updatedReleaseRoles);
                Assert.Equal(otherUserReleaseRole.Role, remainingUserReleaseRole.Role);
                Assert.Equal(otherUserReleaseRole.Id, remainingUserReleaseRole.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock, userRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRoleToRemove()
        {
            var releaseRoleToRemove = ReleaseRole.Approver;
            var newPublicationRoleToRemove = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            UserReleaseRole userReleaseRoleToRemove = _fixture
                .DefaultUserReleaseRole()
                .WithRole(releaseRoleToRemove)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserReleaseRole otherUserReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithRole(ReleaseRole.PrereleaseViewer)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleToRemove, otherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, userPublicationRole);
            // Should have tried to remove the existing publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.RemoveRole(userPublicationRole, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                await repository.RemoveById(userReleaseRoleToRemove.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // The existing release 'Approver' role should have been deleted, but the other role should remain
                var remainingUserReleaseRole = Assert.Single(updatedReleaseRoles);
                Assert.Equal(otherUserReleaseRole.Role, remainingUserReleaseRole.Role);
                Assert.Equal(otherUserReleaseRole.Id, remainingUserReleaseRole.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            var releaseRoleToRemove = ReleaseRole.Contributor;

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
                .ForIndex(0, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Owner)) // OLD system role
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
                .ForIndex(0, s => s.SetUser(user).SetReleaseVersion(releaseVersion1).SetRole(releaseRoleToRemove))
                // Different release version, but same publication - so should still be considered
                .ForIndex(1, s => s.SetUser(user).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUser())
                            .SetReleaseVersion(releaseVersion1)
                            .SetRole(ReleaseRole.PrereleaseViewer)
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
                            .SetRole(ReleaseRole.PrereleaseViewer)
                )
                .GenerateList(4);

            var userReleaseRoleToRemove = existingReleaseRoles[0];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingPublicationRoles);
                contentDbContext.UserReleaseRoles.AddRange(existingReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, [.. existingPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                await repository.RemoveById(userReleaseRoleToRemove.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }
    }

    public class RemoveManyTests : UserReleaseRoleRepositoryTests
    {
        [Fact]
        public async Task ManyRoles_RemovesExpectedOldAndNewPermissionsSystemRoles_WhenSingleNewPermissionsSystemRoleDowngradeRequired()
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

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                // One 'Drafter' role for each User/Publication combination.
                // We expect these to be removed too.
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Drafter))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Drafter))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Drafter))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Drafter))
                // Adding an additional role that should be left untouched by the removals
                .ForIndex(4, s => s.SetUser(_fixture.DefaultUser()).SetPublication(_fixture.DefaultPublication()))
                .GenerateList(5);

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                // One 'Contributor' role for each User/Publication combination.
                // We expect these to be removed.
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
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

            var userPublicationRoleIdsToRemove = new HashSet<Guid>
            {
                userPublicationRoles[0].Id,
                userPublicationRoles[1].Id,
                userPublicationRoles[2].Id,
                userPublicationRoles[3].Id,
            };
            var userReleaseRoleIdsToRemove = new HashSet<Guid>
            {
                userReleaseRoles[0].Id,
                userReleaseRoles[1].Id,
                userReleaseRoles[2].Id,
                userReleaseRoles[3].Id,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, true, [.. userPublicationRoles]);
            // Should remove the existing 'Drafter' publication role for all User 1/2 & Publication 1/2 pairs
            foreach (var userPublicationRoleIdToRemove in userPublicationRoleIdsToRemove)
            {
                userPublicationRoleRepository
                    .Setup(m =>
                        m.RemoveRole(
                            It.Is<UserPublicationRole>(upr => upr.Id == userPublicationRoleIdToRemove),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                await repository.RemoveMany(userReleaseRoleIdsToRemove);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have removed ALL RELEASE roles except 1
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(userReleaseRoles[4].Id, remainingUserReleaseRole.Id);
                Assert.Equal(userReleaseRoles[4].Role, remainingUserReleaseRole.Role);
            }
        }

        [Fact]
        public async Task ManyRoles_RemovesAndCreatesExpectedOldAndNewPermissionsSystemRoles_WhenMultipleNewPermissionsSystemRoleDowngradesRequired()
        {
            User deletedUserPlaceholder = _fixture.DefaultDeletedUserPlaceholder();

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
            ReleaseVersion releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication1));
            ReleaseVersion releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication2));
            ReleaseVersion releaseVersion3 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication3));
            ReleaseVersion releaseVersion4 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication4));

            var userPublicationRoles = _fixture
                .DefaultUserPublicationRole()
                .WithCreatedBy(createdBy)
                // One 'Approver' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                .ForIndex(0, s => s.SetUser(user1).SetPublication(publication1).SetRole(PublicationRole.Approver))
                .ForIndex(1, s => s.SetUser(user1).SetPublication(publication2).SetRole(PublicationRole.Approver))
                .ForIndex(2, s => s.SetUser(user2).SetPublication(publication1).SetRole(PublicationRole.Approver))
                .ForIndex(3, s => s.SetUser(user2).SetPublication(publication2).SetRole(PublicationRole.Approver))
                // One 'Approver' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(4, s => s.SetUser(user3).SetPublication(publication3).SetRole(PublicationRole.Approver))
                .ForIndex(5, s => s.SetUser(user3).SetPublication(publication4).SetRole(PublicationRole.Approver))
                .ForIndex(6, s => s.SetUser(user4).SetPublication(publication3).SetRole(PublicationRole.Approver))
                .ForIndex(7, s => s.SetUser(user4).SetPublication(publication4).SetRole(PublicationRole.Approver))
                // Adding an additional role that should be left untouched by the removals
                .ForIndex(8, s => s.SetUser(_fixture.DefaultUser()).SetPublication(_fixture.DefaultPublication()))
                .GenerateList(9);

            var userReleaseRoles = _fixture
                .DefaultUserReleaseRole()
                .WithCreatedBy(createdBy)
                // One 'Contributor' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                // We would normally expect the equivalent NEW permissions system 'Drafter' role to be removed each of these,
                // but it's the more powerful role 'Approver' that exists. So removing 'Contributor' alone won't remove this
                .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
                .ForIndex(2, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(3, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
                // One 'Approver' role for each User/Publication combination (Users 1/2 & Publications 1/2).
                // We expect removing these will cause the PUBLICATION 'Approver' role to be removed too.
                .ForIndex(4, s => s.SetUser(user1).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Approver))
                .ForIndex(5, s => s.SetUser(user1).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                .ForIndex(6, s => s.SetUser(user2).SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Approver))
                .ForIndex(7, s => s.SetUser(user2).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                // Here, we switch the order of the 'Contributor' and 'Approver' roles, to check that the removal order of these
                // doesn't affect the outcome. In this case, the 'Contributor' role is removed AFTER the RELEASE 'Approver' role, so
                // we expect the 'Approver' role to be removed first, and DOWNGRADED to a 'Drafter' role. Then, once
                // the 'Contributor' role is removed we expect the PUBLICATION 'Drafter` role to removed.
                // One 'Approver' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(8, s => s.SetUser(user3).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Approver))
                .ForIndex(9, s => s.SetUser(user3).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Approver))
                .ForIndex(10, s => s.SetUser(user4).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Approver))
                .ForIndex(11, s => s.SetUser(user4).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Approver))
                // One 'Contributor' role for each User/Publication combination (Users 3/4 & Publications 3/4).
                .ForIndex(12, s => s.SetUser(user3).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Contributor))
                .ForIndex(13, s => s.SetUser(user3).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Contributor))
                .ForIndex(14, s => s.SetUser(user4).SetReleaseVersion(releaseVersion3).SetRole(ReleaseRole.Contributor))
                .ForIndex(15, s => s.SetUser(user4).SetReleaseVersion(releaseVersion4).SetRole(ReleaseRole.Contributor))
                // Adding an additional role that should be left untouched by the removals
                .ForIndex(
                    16,
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
                .GenerateList(17);

            // Don't remove the last one, to check that we are correctly only removing the specified ones
            var userReleaseRoleIdsToRemove = userReleaseRoles[..16].Select(urr => urr.Id).ToHashSet();
            var userPublicationRoleIdsToRemove = userPublicationRoles[..8].Select(urr => urr.Id).ToHashSet();

            var userReleaseRolesResultingInDrafterCreation = userReleaseRoles[8..12];

            var userPublicationDraftersToCreate = userReleaseRolesResultingInDrafterCreation
                .Select(urr =>
                    _fixture
                        .DefaultUserPublicationRole()
                        .WithUserId(urr.UserId)
                        .WithPublicationId(urr.ReleaseVersion.Release.PublicationId)
                        .WithRole(PublicationRole.Drafter)
                        .WithCreatedById(deletedUserPlaceholder.Id)
                        .WithCreated(DateTime.UtcNow)
                        .Generate()
                )
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);

            int queryCallCount = 0;
            userPublicationRoleRepository
                .Setup(m => m.Query(ResourceRoleFilter.All, true))
                .Returns(() =>
                {
                    queryCallCount++;

                    if (queryCallCount <= 4)
                        return new[] { userPublicationRoles[queryCallCount - 1] }.BuildMock();

                    if (queryCallCount <= 8)
                        // Should return the same roles as the first 4 calls, as the `Approver` role wouldn't have been removed
                        // for the User 1/2 & Publication 1/2 combinations until these runs
                        return new[] { userPublicationRoles[queryCallCount - 5] }.BuildMock();

                    if (queryCallCount <= 12)
                        // Add 4 to grab the correct roles for the User 3/4 & Publication 3/4 combinations
                        return new[] { userPublicationRoles[queryCallCount - 9 + 4] }.BuildMock();

                    if (queryCallCount <= 16)
                        // Should, at this stage, be returning the newly created Drafter roles from the DOWNGRADE
                        return new[] { userPublicationDraftersToCreate[queryCallCount - 13] }.BuildMock();

                    throw new InvalidOperationException("Unexpected call count");
                });

            // Should remove the existing 'Approver' publication role for all User 1/2/3/4 & Publication 1/2/3/4 pairs
            foreach (var userPublicationRoleIdToRemove in userPublicationRoleIdsToRemove)
            {
                userPublicationRoleRepository
                    .Setup(m =>
                        m.RemoveRole(
                            It.Is<UserPublicationRole>(upr => upr.Id == userPublicationRoleIdToRemove),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);
            }

            foreach (var userPublicationDrafterToCreate in userPublicationDraftersToCreate)
            {
                // Should create a 'Drafter' role for each User/Publication pairs where the 'Approver' role is being removed and downgraded to 'Drafter'.
                // This is the case for Users 3/4 & Publications 3/4 pairs.
                userPublicationRoleRepository
                    .Setup(m =>
                        m.CreateRole(
                            userPublicationDrafterToCreate.UserId,
                            userPublicationDrafterToCreate.PublicationId,
                            userPublicationDrafterToCreate.Role,
                            userPublicationDrafterToCreate.CreatedById!.Value,
                            It.Is<DateTime>(createdDate =>
                                Math.Abs((createdDate - DateTime.UtcNow).Milliseconds)
                                <= AssertExtensions.TimeWithinMillis
                            ),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(userPublicationDrafterToCreate);

                // Should remove the created 'Drafter' role for each User/Publication pair, once the second 'Contributor' role is removed.
                userPublicationRoleRepository
                    .Setup(m =>
                        m.RemoveRole(
                            It.Is<UserPublicationRole>(upr => upr.Id == userPublicationDrafterToCreate.Id),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedUserPlaceholder);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                await repository.RemoveMany(userReleaseRoleIdsToRemove);
            }

            MockUtils.VerifyAllMocks(userRepository, userPublicationRoleRepository);

            userRepository.Verify(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()), Times.Exactly(4));

            userPublicationRoleRepository.Verify(m => m.Query(ResourceRoleFilter.All, true), Times.Exactly(16));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // Should have removed ALL RELEASE roles except 1
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(userReleaseRoles[16].Id, remainingUserReleaseRole.Id);
                Assert.Equal(userReleaseRoles[16].Role, remainingUserReleaseRole.Role);
            }
        }

        [Fact]
        public async Task EmptyList_DoesNothing()
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

                await repository.RemoveMany([]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingUserReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                // Existing role should remain. Nothing should have happened.
                Assert.Equal(remainingUserReleaseRole.Id, userReleaseRole.Id);
            }
        }

        [Fact]
        public async Task NoNewPermissionsSystemPublicationRoleChanges()
        {
            var releaseRoleToRemove = ReleaseRole.Approver;

            User user = _fixture.DefaultUser();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));
            UserReleaseRole userReleaseRoleToRemove = _fixture
                .DefaultUserReleaseRole()
                .WithRole(releaseRoleToRemove)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserReleaseRole otherUserReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithRole(ReleaseRole.PrereleaseViewer)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleToRemove, otherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, []);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                await repository.RemoveMany([userReleaseRoleToRemove.Id]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // The existing 'Approver' role should have been deleted, but the other role should remain
                var remainingUserReleaseRole = Assert.Single(updatedUserReleaseRoles);
                Assert.Equal(otherUserReleaseRole.Role, remainingUserReleaseRole.Role);
                Assert.Equal(otherUserReleaseRole.Id, remainingUserReleaseRole.Id);
            }

            // Should not have tried to remove any publication roles
            userPublicationRoleRepositoryMock.Verify(
                mock => mock.RemoveById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRoleToRemoveAndCreate()
        {
            User deletedUserPlaceholder = _fixture.DefaultDeletedUserPlaceholder().WithId(Guid.NewGuid());

            var releaseRoleToRemove = ReleaseRole.Approver;
            var newPublicationRoleToRemove = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            UserReleaseRole userReleaseRoleToRemove = _fixture
                .DefaultUserReleaseRole()
                .WithRole(releaseRoleToRemove)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserReleaseRole otherUserReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithRole(ReleaseRole.Contributor)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleToRemove, otherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepositoryMock
                .Setup(m => m.FindDeletedUserPlaceholder(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedUserPlaceholder);

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, userPublicationRole);
            // Should have tried to remove the existing publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.RemoveRole(userPublicationRole, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            // Should have tried to create the new 'Drafter' publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr =>
                    rvr.CreateRole(
                        userReleaseRoleToRemove.UserId,
                        userReleaseRoleToRemove.ReleaseVersion.Release.PublicationId,
                        PublicationRole.Drafter,
                        deletedUserPlaceholder.Id,
                        It.Is<DateTime>(dt =>
                            Math.Abs((dt - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(It.IsAny<UserPublicationRole>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object,
                    userRepository: userRepositoryMock.Object
                );

                await repository.RemoveMany([userReleaseRoleToRemove.Id]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // The existing release 'Approver' role should have been deleted, but the other role should remain
                var remainingUserReleaseRole = Assert.Single(updatedReleaseRoles);
                Assert.Equal(otherUserReleaseRole.Role, remainingUserReleaseRole.Role);
                Assert.Equal(otherUserReleaseRole.Id, remainingUserReleaseRole.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock, userRepositoryMock);
        }

        [Fact]
        public async Task NewPermissionsSystemPublicationRoleToRemove()
        {
            var releaseRoleToRemove = ReleaseRole.Approver;
            var newPublicationRoleToRemove = PublicationRole.Approver;

            User user = _fixture.DefaultUser();
            Publication publication = _fixture.DefaultPublication();
            ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(publication));
            UserReleaseRole userReleaseRoleToRemove = _fixture
                .DefaultUserReleaseRole()
                .WithRole(releaseRoleToRemove)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserReleaseRole otherUserReleaseRole = _fixture
                .DefaultUserReleaseRole()
                .WithRole(ReleaseRole.PrereleaseViewer)
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);
            UserPublicationRole userPublicationRole = _fixture
                .DefaultUserPublicationRole()
                .WithRole(newPublicationRoleToRemove)
                .WithUser(user)
                .WithPublication(publication);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleToRemove, otherUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, userPublicationRole);
            // Should have tried to remove the existing publication role
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.RemoveRole(userPublicationRole, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                await repository.RemoveMany([userReleaseRoleToRemove.Id]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

                // The existing release 'Approver' role should have been deleted, but the other role should remain
                var remainingUserReleaseRole = Assert.Single(updatedReleaseRoles);
                Assert.Equal(otherUserReleaseRole.Role, remainingUserReleaseRole.Role);
                Assert.Equal(otherUserReleaseRole.Id, remainingUserReleaseRole.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
        }

        // Checking all types of user ensures that we are correctly considering roles for any user state
        // (e.g. active, pending invite, expired invite, soft deleted)
        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task OnlyRolesForSpecifiedUserAndPublicationAreCheckedWhenDeterminingChanges(
            Func<DataFixture, User> userFactory
        )
        {
            var releaseRoleToRemove = ReleaseRole.Contributor;

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
                .ForIndex(0, s => s.SetUser(user).SetPublication(publication).SetRole(PublicationRole.Owner)) // OLD system role
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
                .ForIndex(0, s => s.SetUser(user).SetReleaseVersion(releaseVersion1).SetRole(releaseRoleToRemove))
                // Different release version, but same publication - so should still be considered
                .ForIndex(1, s => s.SetUser(user).SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                // Should not be considered when determining changes, as different user
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_fixture.DefaultUser())
                            .SetReleaseVersion(releaseVersion1)
                            .SetRole(ReleaseRole.PrereleaseViewer)
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
                            .SetRole(ReleaseRole.PrereleaseViewer)
                )
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(existingPublicationRoles);
                contentDbContext.UserReleaseRoles.AddRange(existingReleaseRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepositoryMock.SetupQuery(ResourceRoleFilter.All, true, [.. existingPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
                );

                var userReleaseRoleToRemove = existingReleaseRoles.Single(urr => urr.Role == releaseRoleToRemove);

                await repository.RemoveMany([userReleaseRoleToRemove.Id]);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepositoryMock);
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
        public async Task Success_NoSuppliedDateSent()
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

                await repository.MarkEmailAsSent(userReleaseRole.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                Assert.Equal(userReleaseRole.Id, updatedUserReleaseRole.Id);
                updatedUserReleaseRole.EmailSent.AssertUtcNow();
            }
        }

        [Fact]
        public async Task Success_SuppliedDateSent()
        {
            UserReleaseRole userReleaseRole = _fixture
                .DefaultUserReleaseRole()
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
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.MarkEmailAsSent(userReleaseRole.Id, dateSent);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUserReleaseRole = await contentDbContext.UserReleaseRoles.SingleAsync();

                Assert.Equal(userReleaseRole.Id, updatedUserReleaseRole.Id);
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

    private static UserReleaseRoleRepository CreateRepository(
        ContentDbContext? contentDbContext = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserRepository? userRepository = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(
            contentDbContext: contentDbContext,
            newPermissionsSystemHelper: new NewPermissionsSystemHelper(),
            userPublicationRoleRepository: userPublicationRoleRepository
                ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            userReleaseRoleQueryRepository: new UserReleaseRoleQueryRepository(contentDbContext),
            userRepository: userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict)
        );
    }
}
