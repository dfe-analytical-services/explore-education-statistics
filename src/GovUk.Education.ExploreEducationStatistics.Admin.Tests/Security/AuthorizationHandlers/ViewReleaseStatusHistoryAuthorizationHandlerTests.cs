#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ViewReleaseStatusHistoryAuthorizationHandlerTests
{
    private readonly DataFixture _fixture = new();

    public class ClaimTests : ViewReleaseStatusHistoryAuthorizationHandlerTests
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

    public class ReleaseRoleTests : ViewReleaseStatusHistoryAuthorizationHandlerTests
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

    public class PublicationRoleTests : ViewReleaseStatusHistoryAuthorizationHandlerTests
    {
        [Fact]
        public async Task ViewReleaseStatusHistoryAuthorizationHandler_PublicationRoles()
        {
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<ViewReleaseStatusHistoryRequirement>(
                CreateHandler,
                _fixture
                    .DefaultReleaseVersion()
                    .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication())),
                PublicationRole.Owner,
                PublicationRole.Allower
            );
        }
    }

    private static ViewReleaseStatusHistoryAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        return new ViewReleaseStatusHistoryAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: new UserReleaseRoleRepository(contentDbContext: contentDbContext),
                userPublicationRoleRepository: new UserPublicationRoleRepository(contentDbContext: contentDbContext),
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
