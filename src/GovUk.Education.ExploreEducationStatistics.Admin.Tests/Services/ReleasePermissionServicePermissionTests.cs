#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleasePermissionServicePermissionTests
{
    private static readonly ReleaseVersion ReleaseVersion = new()
    {
        Id = Guid.NewGuid()
    };
    
    private static readonly Publication Publication = new()
    {
        Id = Guid.NewGuid(),
        ReleaseVersions = new List<ReleaseVersion>
        {
            ReleaseVersion
        }
    };
    
    [Fact]
    public async Task ListReleaseRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(publication => 
                publication.Id == Publication.Id , CanViewReleaseTeamAccess)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.AddRangeAsync(Publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupReleasePermissionService(contentDbContext, userService.Object);
                    return await service.ListReleaseRoles(ReleaseVersion.Id, new [] { Contributor });
                }
            });
    }
    
    [Fact]
    public async Task ListReleaseInvites()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(publication => 
                publication.Id == Publication.Id , CanViewReleaseTeamAccess)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.AddRangeAsync(Publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupReleasePermissionService(contentDbContext, userService.Object);
                    return await service.ListReleaseInvites(ReleaseVersion.Id);
                }
            });
    }

    [Fact]
    public async Task ListPublicationContributors()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == Publication.Id && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.AddRangeAsync(Publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupReleasePermissionService(contentDbContext, userService.Object);
                    return await service.ListPublicationContributors(Publication.Id);
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseContributors()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == Publication.Id && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.AddRangeAsync(Publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupReleasePermissionService(contentDbContext, userService.Object);
                    return await service.UpdateReleaseContributors(ReleaseVersion.Id, new List<Guid>());
                }
            });
    }

    [Fact]
    public async Task RemoveAllUserContributorPermissionsForPublication()
    {
        var publication = new Publication();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    await contentDbContext.AddRangeAsync(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupReleasePermissionService(contentDbContext, userService.Object);
                    return await service.RemoveAllUserContributorPermissionsForPublication(publication.Id, Guid.NewGuid());
                }
            });
    }

    private ReleasePermissionService SetupReleasePermissionService(
        ContentDbContext contentDbContext,
        IUserService userService)
    {
        return new(
            contentDbContext,
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            new ReleaseVersionRepository(contentDbContext),
            new UserReleaseRoleRepository(contentDbContext),
            new UserReleaseInviteRepository(contentDbContext),
            userService
        );
    }
}
