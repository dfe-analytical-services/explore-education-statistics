#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateReleaseForSpecificPublicationAuthorizationHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly Publication Publication = new() { Id = Guid.NewGuid() };

    private static readonly Publication PublicationArchived = new()
    {
        Id = Guid.NewGuid(),
        SupersededById = Guid.NewGuid()
    };

    private static readonly DataFixture DataFixture = new();

    public class ClaimsTests
    {
        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_SucceedsWithClaim()
        {
            await AuthorizationHandlersTestUtil.ForEachSecurityClaimAsync(async claim =>
            {
                var (handler,
                    userPublicationRoleAndInviteManager) = CreateHandlerAndDependencies();

                var user = DataFixture
                    .AuthenticatedUser(userId: UserId)
                    .WithClaim(claim.ToString());

                var authContext = CreateAuthContext(user, Publication);

                var expectedToPassByClaimAlone = claim == SecurityClaimTypes.CreateAnyRelease;

                if (!expectedToPassByClaimAlone)
                {
                    userPublicationRoleAndInviteManager
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                        .ReturnsAsync(new List<PublicationRole>());
                }

                await handler.HandleAsync(authContext);
                MockUtils.VerifyAllMocks(userPublicationRoleAndInviteManager);

                // A user with the CreateAnyRelease claim is allowed to create a new release
                Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_FailsWhenArchived()
        {
            await AuthorizationHandlersTestUtil.ForEachSecurityClaimAsync(async claim =>
            {
                var (handler, _) = CreateHandlerAndDependencies();

                var user = DataFixture
                    .AuthenticatedUser(userId: UserId)
                    .WithClaim(claim.ToString());

                var authContext = CreateAuthContext(user, PublicationArchived);

                await handler.HandleAsync(authContext);

                // No claims should allow creating a new release of an archived publication
                Assert.False(authContext.HasSucceeded);
            });
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_SucceedsWithPublicationOwner()
        {
            await AuthorizationHandlersTestUtil.ForEachPublicationRoleAsync(async publicationRole =>
            {
                var (handler, userPublicationRoleAndInviteManager) = CreateHandlerAndDependencies();

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthContext(user, Publication);

                userPublicationRoleAndInviteManager
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                    .ReturnsAsync(CollectionUtils.ListOf(publicationRole));

                await handler.HandleAsync(authContext);
                MockUtils.VerifyAllMocks(userPublicationRoleAndInviteManager);

                // A user with the publication owner role is allowed to create a new release
                var expectedToPassByRole = publicationRole == PublicationRole.Owner;
                Assert.Equal(expectedToPassByRole, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_FailsWhenArchived()
        {
            var (handler, userPublicationRoleAndInviteManager) = CreateHandlerAndDependencies();

            var user = DataFixture.AuthenticatedUser(userId: UserId);

            var authContext = CreateAuthContext(user, PublicationArchived);

            await handler.HandleAsync(authContext);

            // There should be no interactions to check the users' publication roles to determine
            // whether or not they can create a release
            MockUtils.VerifyAllMocks(userPublicationRoleAndInviteManager);

            // Verify that the user cannot create a new release of an archived publication
            Assert.False(authContext.HasSucceeded);
        }
    }

    private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user,
        Publication publication)
    {
        return AuthorizationHandlersTestUtil.CreateAuthorizationHandlerContext<
                CreateReleaseForSpecificPublicationRequirement, Publication>
            (user, publication);
    }

    private static
        (CreateReleaseForSpecificPublicationAuthorizationHandler,
        Mock<IUserPublicationRoleAndInviteManager>)
        CreateHandlerAndDependencies()
    {
        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(Strict);
        var userPublicationRoleAndInviteManager = new Mock<IUserPublicationRoleAndInviteManager>(Strict);

        var handler = new CreateReleaseForSpecificPublicationAuthorizationHandler(
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                userReleaseRoleAndInviteManager.Object,
                userPublicationRoleAndInviteManager.Object,
                Mock.Of<IPreReleaseService>(Strict))
        );

        return (handler, userPublicationRoleAndInviteManager);
    }
}
