#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using MockQueryable;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseInviteServicePermissionTest
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task InviteContributor_InvalidPermissions()
    {
        var releaseVersion = new ReleaseVersion();
        var publication = new Publication { Id = Guid.NewGuid(), ReleaseVersions = ListOf(releaseVersion) };

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == ReleaseRole.Contributor,
                CanUpdateSpecificReleaseRole
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupReleaseInviteService(
                        contentDbContext: contentDbContext,
                        userService: userService.Object
                    );
                    return await service.InviteContributor("test@test.com", publication.Id, SetOf(releaseVersion.Id));
                }
            });
    }

    [Fact]
    public async Task RemoveByPublication_InvalidPermissions()
    {
        User user = _fixture.DefaultUserWithPendingInvite();
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(m => m.FindPendingUserInviteByEmail(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == ReleaseRole.Contributor,
                CanUpdateSpecificReleaseRole
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupReleaseInviteService(
                        contentDbContext: contentDbContext,
                        userService: userService.Object,
                        userRepository: userRepository.Object
                    );
                    return await service.RemoveByPublication(user.Email, publication.Id, ReleaseRole.Contributor);
                }
            });
    }

    [Fact]
    public async Task RemoveByPublication()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        User user = _fixture.DefaultUserWithPendingInvite();

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(releaseVersion)
            .GenerateList(3);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(m => m.FindPendingUserInviteByEmail(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.PendingOnly))
            .Returns(userReleaseRoles.BuildMock());
        userReleaseRoleRepository
            .Setup(m => m.RemoveMany(userReleaseRoles, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object,
                userRepository: userRepository.Object
            );

            var result = await service.RemoveByPublication(
                user.Email,
                releaseVersion.Release.PublicationId,
                ReleaseRole.Contributor
            );

            result.AssertRight();
        }

        VerifyAllMocks(userReleaseRoleRepository, userRepository);
    }

    private static ReleaseInviteService SetupReleaseInviteService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserRoleService? userRoleService = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new ReleaseInviteService(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userService ?? AlwaysTrueUserService().Object,
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(Strict)
        );
    }
}
