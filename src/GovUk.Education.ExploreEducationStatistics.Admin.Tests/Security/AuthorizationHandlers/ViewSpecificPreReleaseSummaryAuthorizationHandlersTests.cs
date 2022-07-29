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
    ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
{
    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithAccessAllReleasesClaim()
    {
        // Assert that any users with the "AccessAllReleases" claim can view an arbitrary PreRelease Summary
        // (and no other claim allows this)
        await AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            AccessAllReleases);
    }

    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithReleaseRoles()
    {
        // Assert that a User who has any unrestricted viewer role on a Release can view the PreRelease Summary
        await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            Viewer, Lead, Contributor, Approver, PrereleaseViewer);
    }

    [Fact]
    public async Task ViewSpecificPreReleaseSummary_SucceedsWithPublicationRoles()
    {
        var publication = new Publication
        {
            Id = Guid.NewGuid()
        };
        await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<ViewSpecificPreReleaseSummaryRequirement>(
            CreateHandler,
            new Release
            {
                PublicationId = publication.Id,
                Publication = publication
            },
            Owner, ReleaseApprover);
    }

    private static ViewSpecificPreReleaseSummaryAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        return new ViewSpecificPreReleaseSummaryAuthorizationHandler(
            new AuthorizationHandlerResourceRoleService(
                new UserReleaseRoleRepository(contentDbContext),
                new UserPublicationRoleRepository(contentDbContext),
                Mock.Of<IPublicationRepository>(Strict)));
    }
}
