#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateReleaseForSpecificPublicationAuthorizationHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly Publication Publication = new()
    {
        Id = Guid.NewGuid()
    };

    private static readonly Publication PublicationArchived = new()
    {
        Id = Guid.NewGuid(),
        SupersededById = Guid.NewGuid()
    };

    public class ClaimsTests
    {
        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_SucceedsWithClaim()
        {
            await AuthorizationHandlersTestUtil.ForEachSecurityClaimAsync(async claim =>
            {
                var (handler,
                    _,
                    userPublicationRoleRepository,
                    _) = CreateHandlerAndDependencies();

                var user = ClaimsPrincipalUtils.CreateClaimsPrincipal(UserId, claim);
                var authContext = CreateAuthContext(user, Publication);

                var expectedToPassByClaimAlone = claim == SecurityClaimTypes.CreateAnyRelease;

                if (!expectedToPassByClaimAlone)
                {
                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                        .ReturnsAsync(new List<PublicationRole>());
                }

                await handler.HandleAsync(authContext);
                MockUtils.VerifyAllMocks(userPublicationRoleRepository);

                // A user with the CreateAnyRelease claim is allowed to create a new release
                Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_FailsWhenArchived()
        {
            await AuthorizationHandlersTestUtil.ForEachSecurityClaimAsync(async claim =>
            {
                var (handler,
                    _,
                    _,
                    _) = CreateHandlerAndDependencies();

                var user = ClaimsPrincipalUtils.CreateClaimsPrincipal(UserId, claim);
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
                var (handler,
                    _,
                    userPublicationRoleRepository,
                    _) = CreateHandlerAndDependencies();

                var user = ClaimsPrincipalUtils.CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, Publication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                    .ReturnsAsync(CollectionUtils.ListOf(publicationRole));

                await handler.HandleAsync(authContext);
                MockUtils.VerifyAllMocks(userPublicationRoleRepository);

                // A user with the publication owner role is allowed to create a new release
                var expectedToPassByRole = publicationRole == PublicationRole.Owner;
                Assert.Equal(expectedToPassByRole, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_FailsWhenArchived()
        {
            var (handler,
                _,
                userPublicationRoleRepository,
                _) = CreateHandlerAndDependencies();

            var user = ClaimsPrincipalUtils.CreateClaimsPrincipal(UserId);
            var authContext = CreateAuthContext(user, PublicationArchived);

            await handler.HandleAsync(authContext);

            // There should be no interactions to check the users' publication roles to determine
            // whether or not they can create a release
            MockUtils.VerifyAllMocks(userPublicationRoleRepository);

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
        Mock<IUserReleaseRoleRepository>,
        Mock<IUserPublicationRoleRepository>,
        Mock<IPublicationRepository>
        )
        CreateHandlerAndDependencies()
    {
        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
        var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

        var handler = new CreateReleaseForSpecificPublicationAuthorizationHandler(
            new AuthorizationHandlerResourceRoleService(
                userReleaseRoleRepository.Object,
                userPublicationRoleRepository.Object,
                publicationRepository.Object)
        );

        return (
            handler,
            userReleaseRoleRepository,
            userPublicationRoleRepository,
            publicationRepository
        );
    }
}
