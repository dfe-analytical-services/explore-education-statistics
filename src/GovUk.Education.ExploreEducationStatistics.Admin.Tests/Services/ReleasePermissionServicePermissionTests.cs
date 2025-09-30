#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleasePermissionServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListReleaseRoles()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(
                _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
            );

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(
                publication => publication.Id == releaseVersion.Release.PublicationId,
                CanViewReleaseTeamAccess
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    var service = BuildService(contentDbContext, userService.Object);
                    return await service.ListReleaseRoles(
                        releaseVersion.Id,
                        rolesToInclude: [Contributor]
                    );
                }
            });
    }

    [Fact]
    public async Task ListReleaseInvites()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(
                _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
            );

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(
                publication => publication.Id == releaseVersion.Release.PublicationId,
                CanViewReleaseTeamAccess
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    var service = BuildService(contentDbContext, userService.Object);
                    return await service.ListReleaseInvites(releaseVersion.Id);
                }
            });
    }

    [Fact]
    public async Task ListPublicationContributors()
    {
        Publication publication = _dataFixture.DefaultPublication();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    var service = BuildService(contentDbContext, userService.Object);
                    return await service.ListPublicationContributors(publication.Id);
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseContributors()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(
                _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
            );

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple =>
                    tuple.Item1.Id == releaseVersion.Release.PublicationId
                    && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    var service = BuildService(contentDbContext, userService.Object);
                    return await service.UpdateReleaseContributors(releaseVersion.Id, userIds: []);
                }
            });
    }

    [Fact]
    public async Task RemoveAllUserContributorPermissionsForPublication()
    {
        Publication publication = _dataFixture.DefaultPublication();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (
                    var contentDbContext = InMemoryApplicationDbContext(contentDbContextId)
                )
                {
                    var service = BuildService(contentDbContext, userService.Object);
                    return await service.RemoveAllUserContributorPermissionsForPublication(
                        publication.Id,
                        userId: Guid.NewGuid()
                    );
                }
            });
    }

    private static ReleasePermissionService BuildService(
        ContentDbContext contentDbContext,
        IUserService userService
    )
    {
        var userReleaseRoleRepository = new UserReleaseRoleRepository(
            contentDbContext,
            logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()
        );

        return new(
            contentDbContext: contentDbContext,
            persistenceHelper: new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
            userReleaseRoleRepository: userReleaseRoleRepository,
            userService: userService
        );
    }
}
