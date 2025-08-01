#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    PublicationAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class ViewSpecificPublicationAuthorizationHandlersTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Publication _publication = new() { Id = Guid.NewGuid() };

    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task SucceedsWithAccessAllPublicationsClaim()
    {
        // Assert that any users with the "AccessAllPublications" claim can view an arbitrary Publication
        // (and no other claim allows this)
        await AssertHandlerSucceedsWithCorrectClaims<Publication, ViewSpecificPublicationRequirement>(
            context => CreateHandler(context),
            _publication,
            AccessAllPublications);
    }

    [Fact]
    public async Task HasOwnerOrApproverRoleOnPublicationAuthorizationHandler_Succeeds()
    {
        await AssertPublicationHandlerSucceedsWithPublicationRoles<ViewSpecificPublicationRequirement>(
            contentDbContext => CreateHandler(
                contentDbContext,
                userPublicationRoleRepository: new UserPublicationRoleRepository(contentDbContext)),
            [PublicationRole.Owner, PublicationRole.Allower]);
    }

    [Fact]
    public async Task HasRoleOnAnyChildReleaseAuthorizationHandler_NoReleasesOnThisPublicationForThisUser()
    {
        var releaseVersionOnAnotherPublication = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = Guid.NewGuid()
        };

        var releaseVersionOnThisPublication = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = _publication.Id
        };

        var releaseRoleForDifferentPublication = new UserReleaseRole
        {
            UserId = _userId,
            ReleaseVersion = releaseVersionOnAnotherPublication
        };

        var releaseRoleForDifferentUser = new UserReleaseRole
        {
            UserId = Guid.NewGuid(),
            ReleaseVersion = releaseVersionOnThisPublication
        };

        await AssertHasRoleOnAnyChildReleaseHandlesOk(
            false,
            releaseRoleForDifferentPublication,
            releaseRoleForDifferentUser);
    }

    [Fact]
    public async Task HasRoleOnAnyChildReleaseAuthorizationHandler_HasRoleOnAReleaseOfThisPublication()
    {
        var releaseVersionOnThisPublication = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = _publication.Id
        };

        var roleOnThisPublication = new UserReleaseRole
        {
            UserId = _userId,
            ReleaseVersion = releaseVersionOnThisPublication
        };

        await AssertHasRoleOnAnyChildReleaseHandlesOk(true, roleOnThisPublication);
    }

    private async Task AssertHasRoleOnAnyChildReleaseHandlesOk(bool expectedToSucceed,
        params UserReleaseRole[] releaseRoles)
    {
        await using (var context = DbUtils.InMemoryApplicationDbContext())
        {
            context.UserReleaseRoles.AddRange(releaseRoles);
            await context.SaveChangesAsync();

            var handler = new ViewSpecificPublicationAuthorizationHandler(
                context,
                new AuthorizationHandlerService(
                    new ReleaseVersionRepository(context),
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    new UserPublicationRoleRepository(context),
                    Mock.Of<IPreReleaseService>(Strict)));

            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new ViewSpecificPublicationRequirement() },
                _fixture.AuthenticatedUser(userId: _userId), _publication);

            await handler.HandleAsync(authContext);

            Assert.Equal(expectedToSucceed, authContext.HasSucceeded);
        }
    }

    private ViewSpecificPublicationAuthorizationHandler CreateHandler(
        ContentDbContext context,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IPreReleaseService? preReleaseService = null)
    {
        return new ViewSpecificPublicationAuthorizationHandler(
            context,
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(context),
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(context),
                userPublicationRoleRepository ?? new UserPublicationRoleRepository(context),
                preReleaseService ?? Mock.Of<IPreReleaseService>(Strict)));
    }
}
