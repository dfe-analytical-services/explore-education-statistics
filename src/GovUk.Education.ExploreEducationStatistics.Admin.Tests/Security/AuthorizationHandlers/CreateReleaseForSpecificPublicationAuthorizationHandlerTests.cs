using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.PublicationAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class CreateReleaseForSpecificPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_SucceedsWithClaim()
        {
            await AssertHandlerSucceedsWithCorrectClaims<Publication, CreateReleaseForSpecificPublicationRequirement>(
                contentDbContext =>
                    new CreateReleaseForSpecificPublicationAuthorizationHandler(
                        new UserPublicationRoleRepository(contentDbContext)),
                new Publication(),
                CreateAnyRelease
            );
        }

        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_SucceedsWithPublicationOwner()
        {
            await AssertPublicationHandlerSucceedsWithPublicationOwnerRole<
                CreateReleaseForSpecificPublicationRequirement>(contentDbContext =>
                new CreateReleaseForSpecificPublicationAuthorizationHandler(
                    new UserPublicationRoleRepository(contentDbContext)));
        }
    }
}
