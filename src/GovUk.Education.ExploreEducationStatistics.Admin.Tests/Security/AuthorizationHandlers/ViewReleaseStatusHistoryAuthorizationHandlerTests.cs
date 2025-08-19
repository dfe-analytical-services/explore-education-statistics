#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

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
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<ViewReleaseStatusHistoryRequirement>(
                CreateHandler,
                new ReleaseVersion(),
                ReleaseRole.Contributor,
                ReleaseRole.Approver
            );
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task ViewReleaseStatusHistoryAuthorizationHandler_PublicationRoles()
        {
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<ViewReleaseStatusHistoryRequirement>(
                CreateHandler,
                new ReleaseVersion { Publication = new Publication() },
                PublicationRole.Owner,
                PublicationRole.Allower
            );
        }
    }

    private static ViewReleaseStatusHistoryAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        var userRepository = new UserRepository(contentDbContext);

        return new ViewReleaseStatusHistoryAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: new UserReleaseRoleRepository(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository,
                    logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()),
                userPublicationRoleRepository: new UserPublicationRoleRepository(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository,
                    logger: Mock.Of<ILogger<UserPublicationRoleRepository>>()),
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)));
    }
}
