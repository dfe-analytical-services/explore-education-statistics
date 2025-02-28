#nullable enable
using System;
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
    ReleaseVersionAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
{
    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithAccessAllReleasesClaim()
    {
        // Assert that any users with the "AccessAllReleases" claim can view an arbitrary PreRelease Summary
        // (and no other claim allows this)
        await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            new ReleaseVersion
            {
                Id = Guid.NewGuid()
            },
            AccessAllReleases);
    }

    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithReleaseRoles()
    {
        // Assert that a User who has any unrestricted viewer role on a Release can view the PreRelease Summary
        await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            new ReleaseVersion
            {
                Id = Guid.NewGuid()
            },
            ReleaseRole.Viewer, ReleaseRole.Lead, ReleaseRole.Contributor, ReleaseRole.Approver, ReleaseRole.PrereleaseViewer);
    }

    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithPublicationRoles()
    {
        var publication = new Publication
        {
            Id = Guid.NewGuid()
        };
        await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            new ReleaseVersion
            {
                PublicationId = publication.Id,
                Publication = publication
            },
            PublicationRole.Owner, PublicationRole.Approver);
    }

    private static ViewSpecificPreReleaseSummaryAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        return new ViewSpecificPreReleaseSummaryAuthorizationHandler(
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(contentDbContext),
                new UserReleaseRoleRepository(contentDbContext),
                new UserPublicationRoleRepository(contentDbContext),
                Mock.Of<IPreReleaseService>(Strict)));
    }
}
