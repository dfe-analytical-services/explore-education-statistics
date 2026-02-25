#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class UpdateSpecificReleaseAuthorizationHandlerTests
{
    private static readonly DataFixture DataFixture = new();

    public class ClaimTests
    {
        [Fact]
        public async Task UpdateAllReleases_ClaimSucceeds()
        {
            Release release = DataFixture.DefaultRelease();

            await AssertHandlerSucceedsWithCorrectClaims<Release, UpdateSpecificReleaseRequirement>(
                HandlerSupplier(release),
                release,
                SecurityClaimTypes.UpdateAllReleases
            );
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task PublicationOwnersCanUpdateRelease()
        {
            Release release = DataFixture.DefaultRelease();

            await AssertHandlerSucceedsWithPublicationRoles<Release, UpdateSpecificReleaseRequirement>(
                handlerSupplier: HandlerSupplier(release),
                entity: release,
                publicationId: release.PublicationId,
                publicationRolesExpectedToPass: [PublicationRole.Owner]
            );
        }
    }

    private static Func<ContentDbContext, UpdateSpecificReleaseAuthorizationHandler> HandlerSupplier(Release release)
    {
        return contentDbContext =>
        {
            contentDbContext.Releases.Add(release);
            contentDbContext.SaveChanges();

            var (userPublicationRoleRepository, userReleaseRoleRepository) = ServiceFactory.BuildRoleRepositories(
                contentDbContext
            );

            return new UpdateSpecificReleaseAuthorizationHandler(
                new AuthorizationHandlerService(
                    releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                    userReleaseRoleRepository: userReleaseRoleRepository,
                    userPublicationRoleRepository: userPublicationRoleRepository,
                    preReleaseService: Mock.Of<IPreReleaseService>(Strict)
                )
            );
        };
    }
}
