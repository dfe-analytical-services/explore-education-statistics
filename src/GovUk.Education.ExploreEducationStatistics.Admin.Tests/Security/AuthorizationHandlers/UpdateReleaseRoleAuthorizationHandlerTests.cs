#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.PublicationAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class UpdateReleaseRoleAuthorizationHandlerTests
{
    [Fact]
    public async Task CanUpdateReleaseRolesAuthorizationHandler_SucceedsWithClaim()
    {
        await AssertHandlerSucceedsWithCorrectClaims<Tuple<Publication, ReleaseRole>, UpdateReleaseRoleRequirement>(
            CreateHandler,
            TupleOf(new Publication(), ReleaseRole.PrereleaseViewer),
            ManageAnyUser
        );
    }

    [Fact]
    public async Task CanUpdateReleaseRolesAuthorizationHandler_Contributor_SucceedsWithPublicationOwner()
    {
        var publication = new Publication { Id = Guid.NewGuid() };
        var tuple = TupleOf(publication, ReleaseRole.Contributor);
        await AssertHandlerOnlySucceedsWithPublicationRoles<
            UpdateReleaseRoleRequirement,
            Tuple<Publication, ReleaseRole>
        >(
            publication.Id,
            tuple,
            contentDbContext => contentDbContext.Add(publication),
            CreateHandler,
            PublicationRole.Owner
        );
    }

    [Fact]
    public async Task CanUpdateReleaseRolesAuthorizationHandler_NotContributor_FailsWithPublicationOwner()
    {
        var publication = new Publication { Id = Guid.NewGuid() };
        var tuple = TupleOf(publication, ReleaseRole.PrereleaseViewer);
        await AssertHandlerOnlySucceedsWithPublicationRoles<
            UpdateReleaseRoleRequirement,
            Tuple<Publication, ReleaseRole>
        >(publication.Id, tuple, contentDbContext => contentDbContext.Add(publication), CreateHandler);
    }

    private static UpdateReleaseRoleAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        return new UpdateReleaseRoleAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: Mock.Of<IUserReleaseRoleRepository>(Strict),
                userPublicationRoleRepository: new UserPublicationRoleRepository(contentDbContext: contentDbContext),
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
