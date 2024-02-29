#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;

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
                await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, ViewReleaseStatusHistoryRequirement>(
                    CreateHandler,
                    new ReleaseVersion(),
                    AccessAllReleases
                );
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task ViewReleaseStatusHistoryAuthorizationHandler_ReleaseRoles()
            {
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewReleaseStatusHistoryRequirement>(
                    CreateHandler,
                    new ReleaseVersion(),
                    ReleaseRole.Viewer,
                    ReleaseRole.Contributor,
                    ReleaseRole.Approver,
                    ReleaseRole.Lead
                );
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task ViewReleaseStatusHistoryAuthorizationHandler_PublicationRoles()
            {
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<ViewReleaseStatusHistoryRequirement>(
                    CreateHandler,
                    new ReleaseVersion
                    {
                        Publication = new Publication()
                    },
                    PublicationRole.Owner,
                    PublicationRole.Approver
                );
            }
        }

        private static ViewReleaseStatusHistoryAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new ViewReleaseStatusHistoryAuthorizationHandler(
                new AuthorizationHandlerService(
                    new ReleaseVersionRepository(contentDbContext),
                    new UserReleaseRoleRepository(contentDbContext),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPreReleaseService>(Strict)));
        }
    }
}
