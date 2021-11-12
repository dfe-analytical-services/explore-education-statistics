#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.PublicationAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateReleaseRoleAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanUpdateReleaseRolesAuthorizationHandler_SucceedsWithClaim()
        {
            await AssertHandlerSucceedsWithCorrectClaims<Tuple<Publication, ReleaseRole>, UpdateReleaseRoleRequirement>(
                contentDbContext =>
                    new UpdateReleaseRoleAuthorizationHandler(new UserPublicationRoleRepository(contentDbContext)),
                AsTuple(new Publication(), ReleaseRole.Lead),
                ManageAnyUser
            );
        }

        [Fact]
        public void CanUpdateReleaseRolesAuthorizationHandler_Contributor_SucceedsWithPublicationOwner()
        {
            var publication = new Publication {Id = Guid.NewGuid()};
            var tuple = AsTuple(publication, ReleaseRole.Contributor);
            AssertHandlerOnlySucceedsWithPublicationRole
                <UpdateReleaseRoleRequirement, Tuple<Publication, ReleaseRole>>(
                    publication.Id,
                    tuple,
                    contentDbContext => contentDbContext.Add(publication),
                    contentDbContext =>
                        new UpdateReleaseRoleAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    PublicationRole.Owner);
        }

        [Fact]
        public void CanUpdateReleaseRolesAuthorizationHandler_NotContributor_FailsWithPublicationOwner()
        {
            var publication = new Publication {Id = Guid.NewGuid()};
            var tuple = AsTuple(publication, ReleaseRole.PrereleaseViewer);
            AssertHandlerOnlySucceedsWithPublicationRole
                <UpdateReleaseRoleRequirement, Tuple<Publication, ReleaseRole>>(
                    publication.Id,
                    tuple,
                    contentDbContext => contentDbContext.Add(publication),
                    contentDbContext =>
                        new UpdateReleaseRoleAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)));
        }
    }
}
