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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateContactAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanUpdateAllContactAuthorizationHandler_SucceedsWithClaim()
        {
            await AssertHandlerSucceedsWithCorrectClaims<Publication, UpdateContactRequirement>(
                CreateHandler,
                new Publication(),
                UpdateAllPublications
            );
        }

        [Fact]
        public async Task CanUpdateAllContactAuthorizationHandler_SucceedsWithPublicationOwner()
        {
            await AssertPublicationHandlerSucceedsWithPublicationRoles<UpdateContactRequirement>(
                CreateHandler, Owner);
        }

        private static UpdateContactAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new UpdateContactAuthorizationHandler(
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPublicationRepository>(Strict)));
        }
    }
}
