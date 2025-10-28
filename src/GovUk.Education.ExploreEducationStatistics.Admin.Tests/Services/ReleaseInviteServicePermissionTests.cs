#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

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
                    return await service.InviteContributor("test@test.com", publication.Id, ListOf(releaseVersion.Id));
                }
            });
    }

    [Fact]
    public async Task RemoveByPublication_InvalidPermissions()
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
                    return await service.RemoveByPublication("test@test.com", publication.Id, ReleaseRole.Contributor);
                }
            });
    }

    [Fact]
    public async Task RemoveByPublication()
    {
        var email = "test@test.com";

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>();
        userReleaseInviteRepository
            .Setup(m =>
                m.RemoveByPublicationAndEmail(
                    releaseVersion.Release.PublicationId,
                    email,
                    default,
                    ReleaseRole.Contributor
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                userReleaseInviteRepository: userReleaseInviteRepository.Object
            );

            var result = await service.RemoveByPublication(
                email,
                releaseVersion.Release.PublicationId,
                ReleaseRole.Contributor
            );

            result.AssertRight();
        }

        VerifyAllMocks(userReleaseInviteRepository);
    }

    private static ReleaseInviteService SetupReleaseInviteService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserRoleService? userRoleService = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IEmailService? emailService = null,
        IOptions<AppOptions>? appOptions = null,
        IOptions<NotifyOptions>? notifyOptions = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        userRepository ??= new UserRepository(contentDbContext);

        return new ReleaseInviteService(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
            userRepository,
            userService ?? AlwaysTrueUserService().Object,
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            emailService ?? Mock.Of<IEmailService>(Strict),
            appOptions ?? Mock.Of<IOptions<AppOptions>>(),
            notifyOptions ?? Mock.Of<IOptions<NotifyOptions>>()
        );
    }
}
