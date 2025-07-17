#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task CreateRelease()
    {
        Publication publication = _dataFixture.DefaultPublication();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(p => p.Id == publication.Id,
                CanCreateReleaseForSpecificPublication)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.Publications.Add(publication);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        context: contextDbContext,
                        userService: userService.Object);

                    return await service.CreateRelease(
                        new ReleaseCreateRequest
                        {
                            PublicationId = publication.Id,
                        }
                    );
                }
            );
    }

    [Fact]
    public async Task UpdateRelease()
    {
        Release release = _dataFixture.DefaultRelease()
            .WithPublication(_dataFixture.DefaultPublication());

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Release>(r => r.PublicationId == release.PublicationId,
                CanUpdateSpecificRelease)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.Releases.Add(release);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        context: contextDbContext,
                        userService: userService.Object);

                    return await service.UpdateRelease(
                        releaseId: release.Id,
                        new ReleaseUpdateRequest
                        {
                            Label = "initial",
                        }
                    );
                }
            );
    }

    private static ReleaseService BuildService(
        IUserService userService,
        ContentDbContext? context = null,
        IReleaseVersionService? releaseVersionService = null,
        IReleaseCacheService? releaseCacheService = null,
        IPublicationCacheService? publicationCacheService = null,
        IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
        IRedirectsCacheService? redirectsCacheService = null,
        IReleaseSlugValidator? releaseSlugValidator = null)
    {
        return new ReleaseService(
            context: context ?? Mock.Of<ContentDbContext>(),
            userService: userService,
            releaseVersionService: releaseVersionService ?? Mock.Of<IReleaseVersionService>(),
            releaseCacheService: releaseCacheService ?? Mock.Of<IReleaseCacheService>(),
            publicationCacheService: publicationCacheService ?? Mock.Of<IPublicationCacheService>(),
            releasePublishingStatusRepository: releasePublishingStatusRepository ?? Mock.Of<IReleasePublishingStatusRepository>(),
            redirectsCacheService: redirectsCacheService ?? Mock.Of<IRedirectsCacheService>(),
            adminEventRaiser: new AdminEventRaiserMockBuilder().Build(),
            guidGenerator: new SequentialGuidGenerator(),
            releaseSlugValidator: releaseSlugValidator ?? Mock.Of<IReleaseSlugValidator>()
        );
    }
}
