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
    public class UpdatePublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanUpdateAllPublicationsAuthorizationHandler_SucceedsWithClaim()
        {
            await AssertHandlerSucceedsWithCorrectClaims<Publication, UpdatePublicationRequirement>(
                contentDbContext =>
                    new UpdatePublicationAuthorizationHandler(new UserPublicationRoleRepository(contentDbContext)),
                new Publication(),
                UpdateAllPublications
            );
        }

        [Fact]
        public async Task CanUpdateAllPublicationsAuthorizationHandler_SucceedsWithPublicationOwner()
        {
            await AssertPublicationHandlerSucceedsWithPublicationOwnerRole<
                UpdatePublicationRequirement>(contentDbContext =>
                    new UpdatePublicationAuthorizationHandler(
                        new UserPublicationRoleRepository(contentDbContext)));
        }
    }
}
