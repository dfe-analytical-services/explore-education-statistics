#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ViewReleaseStatusHistoryAuthorizationHandlerTests
    {
        public class ClaimTests
        {
            [Fact]
            public async Task ViewReleaseStatusHistoryAuthorizationHandler_ReleaseRoles()
            {
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewReleaseStatusHistoryRequirement>(
                    contentDbContext => new ViewReleaseStatusHistoryAuthorizationHandler(
                        new UserPublicationRoleRepository(contentDbContext),
                        new UserReleaseRoleRepository(contentDbContext)),
                    new Release(),
                    ReleaseRole.Viewer,
                    ReleaseRole.Contributor,
                    ReleaseRole.Approver,
                    ReleaseRole.Lead
                );
            }

            [Fact]
            public async Task ViewReleaseStatusHistoryAuthorizationHandler_PublicationRoles()
            {
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<ViewReleaseStatusHistoryRequirement>(
                    contentDbContext => new ViewReleaseStatusHistoryAuthorizationHandler(
                        new UserPublicationRoleRepository(contentDbContext),
                        new UserReleaseRoleRepository(contentDbContext)),
                    new Release
                    {
                        Publication = new Publication()
                    },
                    PublicationRole.Owner
                );
            }
        }
    }
}
