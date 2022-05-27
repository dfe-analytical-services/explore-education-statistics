using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.PublicationAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdatePublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanUpdateAllPublicationsAuthorizationHandler_SucceedsWithClaim()
        {
            await AssertHandlerSucceedsWithCorrectClaims<Publication, UpdatePublicationRequirement>(
                CreateHandler,
                new Publication(),
                UpdateAllPublications
            );
        }

        [Fact]
        public async Task CanUpdateAllPublicationsAuthorizationHandler_SucceedsWithPublicationOwner()
        {
            await AssertPublicationHandlerSucceedsWithPublicationOwnerRole<UpdatePublicationRequirement>(
                CreateHandler);
        }

        private static UpdatePublicationAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new UpdatePublicationAuthorizationHandler(
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPublicationRepository>(Strict)));
        }
    }
}
