#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.PublicationAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class ViewSpecificPublicationAuthorizationHandlersTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task SucceedsWithAccessAllPublicationsClaim()
    {
        // Assert that any users with the "AccessAllPublications" claim can view an arbitrary Publication
        // (and no other claim allows this)
        await AssertHandlerSucceedsWithCorrectClaims<Publication, ViewSpecificPublicationRequirement>(
            context => CreateHandler(context),
            _fixture.DefaultPublication(),
            AccessAllPublications
        );
    }

    [Fact]
    public async Task HasOwnerOrApproverRoleOnPublicationAuthorizationHandler_Succeeds()
    {
        await AssertPublicationHandlerSucceedsWithPublicationRoles<ViewSpecificPublicationRequirement>(
            contentDbContext => CreateHandler(contentDbContext),
            [PublicationRole.Owner, PublicationRole.Allower]
        );
    }

    [Fact]
    public async Task HasRoleOnAnyChildReleaseAuthorizationHandler_NoReleasesOnThisPublicationForThisUser()
    {
        Publication thisPublication = _fixture.DefaultPublication();
        User thisUser = _fixture.DefaultUser();

        ReleaseVersion releaseVersionOnAnotherPublication = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        ReleaseVersion releaseVersionOnThisPublication = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(thisPublication));

        UserReleaseRole releaseRoleForDifferentPublication = _fixture
            .DefaultUserReleaseRole()
            .WithUser(thisUser)
            .WithReleaseVersion(releaseVersionOnAnotherPublication);

        UserReleaseRole releaseRoleForDifferentUser = _fixture
            .DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersionOnThisPublication);

        await AssertHasRoleOnAnyChildReleaseHandlesOk(
            expectedToSucceed: false,
            user: thisUser,
            publication: thisPublication,
            releaseRoleForDifferentPublication,
            releaseRoleForDifferentUser
        );
    }

    [Fact]
    public async Task HasRoleOnAnyChildReleaseAuthorizationHandler_HasRoleOnAReleaseOfThisPublication()
    {
        Publication thisPublication = _fixture.DefaultPublication();
        User thisUser = _fixture.DefaultUser();

        ReleaseVersion releaseVersionOnThisPublication = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(thisPublication));

        UserReleaseRole roleOnThisPublication = _fixture
            .DefaultUserReleaseRole()
            .WithUser(thisUser)
            .WithReleaseVersion(releaseVersionOnThisPublication);

        await AssertHasRoleOnAnyChildReleaseHandlesOk(
            expectedToSucceed: true,
            user: thisUser,
            publication: thisPublication,
            roleOnThisPublication
        );
    }

    private async Task AssertHasRoleOnAnyChildReleaseHandlesOk(
        bool expectedToSucceed,
        User user,
        Publication publication,
        params UserReleaseRole[] releaseRoles
    )
    {
        await using var context = DbUtils.InMemoryApplicationDbContext();

        context.UserReleaseRoles.AddRange(releaseRoles);
        await context.SaveChangesAsync();

        var handler = CreateHandler(context: context);

        var authContext = new AuthorizationHandlerContext(
            [new ViewSpecificPublicationRequirement()],
            _fixture.AuthenticatedUser(userId: user.Id),
            publication
        );

        await handler.HandleAsync(authContext);

        Assert.Equal(expectedToSucceed, authContext.HasSucceeded);
    }

    private static ViewSpecificPublicationAuthorizationHandler CreateHandler(
        ContentDbContext context,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IPreReleaseService? preReleaseService = null
    )
    {
        userReleaseRoleRepository ??= new UserReleaseRoleRepository(contentDbContext: context);

        return new ViewSpecificPublicationAuthorizationHandler(
            userReleaseRoleRepository,
            new AuthorizationHandlerService(
                releaseVersionRepository ?? new ReleaseVersionRepository(context),
                userReleaseRoleRepository,
                userPublicationRoleRepository ?? new UserPublicationRoleRepository(contentDbContext: context),
                preReleaseService ?? Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
