#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    PublicationAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdatePublicationSummaryAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanUpdateAllPublicationsSummaryAuthorizationHandler_SucceedsWithClaim()
        {
            await AssertHandlerSucceedsWithCorrectClaims<Publication, UpdatePublicationSummaryRequirement>(
                CreateHandler,
                new Publication(),
                UpdateAllPublications
            );
        }

        [Fact]
        public async Task CanUpdateAllPublicationsSummaryAuthorizationHandler_SucceedsWithPublicationOwner()
        {
            await AssertPublicationHandlerSucceedsWithPublicationRoles<UpdatePublicationSummaryRequirement>(
                CreateHandler, Owner);
        }

        private static UpdatePublicationSummaryAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new UpdatePublicationSummaryAuthorizationHandler(
                new AuthorizationHandlerService(
                    new ReleaseVersionRepository(contentDbContext),
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPreReleaseService>(Strict)));
        }
    }
}
