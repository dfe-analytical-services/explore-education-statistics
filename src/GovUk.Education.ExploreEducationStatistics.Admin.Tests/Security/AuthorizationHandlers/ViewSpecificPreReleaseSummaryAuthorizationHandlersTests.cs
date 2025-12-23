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

public class ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
{
    private static readonly DataFixture _fixture = new();
    private static readonly ReleaseVersion _releaseVersion = _fixture
        .DefaultReleaseVersion()
        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithAccessAllReleasesClaim()
    {
        // Assert that any users with the "AccessAllReleases" claim can view an arbitrary PreRelease Summary
        // (and no other claim allows this)
        await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            _releaseVersion,
            AccessAllReleases
        );
    }

    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithReleaseRoles()
    {
        // Assert that a User who has any unrestricted viewer role on a Release can view the PreRelease Summary
        await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            _releaseVersion,
            ReleaseRole.Contributor,
            ReleaseRole.Approver,
            ReleaseRole.PrereleaseViewer
        );
    }

    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithPublicationRoles()
    {
        await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            _releaseVersion,
            PublicationRole.Owner,
            PublicationRole.Allower
        );
    }

    private static ViewSpecificPreReleaseSummaryAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        return new ViewSpecificPreReleaseSummaryAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: new UserReleaseRoleRepository(contentDbContext: contentDbContext),
                userPublicationRoleRepository: new UserPublicationRoleRepository(contentDbContext: contentDbContext),
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
