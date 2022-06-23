#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    PublicationAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateReleaseRoleAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanUpdateReleaseRolesAuthorizationHandler_SucceedsWithClaim()
        {
            await AssertHandlerSucceedsWithCorrectClaims
                <Tuple<Publication, ReleaseRole>, UpdateReleaseRoleRequirement>(
                    CreateHandler,
                    TupleOf(new Publication(), ReleaseRole.Lead),
                    ManageAnyUser
                );
        }

        [Fact]
        public async Task CanUpdateReleaseRolesAuthorizationHandler_Contributor_SucceedsWithPublicationOwner()
        {
            var publication = new Publication {Id = Guid.NewGuid()};
            var tuple = TupleOf(publication, ReleaseRole.Contributor);
            await AssertHandlerOnlySucceedsWithPublicationRoles
                <UpdateReleaseRoleRequirement, Tuple<Publication, ReleaseRole>>(
                    publication.Id,
                    tuple,
                    contentDbContext => contentDbContext.Add(publication),
                    CreateHandler,
                    PublicationRole.Owner);
        }

        [Fact]
        public async Task CanUpdateReleaseRolesAuthorizationHandler_NotContributor_FailsWithPublicationOwner()
        {
            var publication = new Publication {Id = Guid.NewGuid()};
            var tuple = TupleOf(publication, ReleaseRole.PrereleaseViewer);
            await AssertHandlerOnlySucceedsWithPublicationRoles
                <UpdateReleaseRoleRequirement, Tuple<Publication, ReleaseRole>>(
                    publication.Id,
                    tuple,
                    contentDbContext => contentDbContext.Add(publication),
                    CreateHandler);
        }

        private static UpdateReleaseRoleAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new UpdateReleaseRoleAuthorizationHandler(
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPublicationRepository>(Strict)));
        }
    }
}
