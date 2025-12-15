#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;
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
        SupersededById = Guid.NewGuid(),
    };

    private static readonly DataFixture DataFixture = new();

    public class ClaimsTests
    {
        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_SucceedsWithClaim()
        {
            await AuthorizationHandlersTestUtil.ForEachSecurityClaimAsync(async claim =>
            {
                var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies();

                ClaimsPrincipal user = DataFixture.AuthenticatedUser(userId: UserId).WithClaim(claim.ToString());

                var authContext = CreateAuthContext(user, Publication);

                var expectedToPassByClaimAlone = claim == SecurityClaimTypes.CreateAnyRelease;

                if (!expectedToPassByClaimAlone)
                {
                    userPublicationRoleRepository
                        .Setup(rvr =>
                            rvr.UserHasAnyRoleOnPublication(
                                UserId,
                                Publication.Id,
                                ResourceRoleFilter.ActiveOnly,
                                It.IsAny<CancellationToken>(),
                                PublicationRole.Owner
                            )
                        )
                        .ReturnsAsync(false);
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
                var (handler, _) = CreateHandlerAndDependencies();

                var user = DataFixture.AuthenticatedUser(userId: UserId).WithClaim(claim.ToString());

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
                var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies();

                ClaimsPrincipal user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthContext(user, Publication);

                var isOwnerRole = publicationRole == PublicationRole.Owner;

                userPublicationRoleRepository
                    .Setup(rvr =>
                        rvr.UserHasAnyRoleOnPublication(
                            UserId,
                            Publication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            PublicationRole.Owner
                        )
                    )
                    .ReturnsAsync(isOwnerRole);

                await handler.HandleAsync(authContext);
                MockUtils.VerifyAllMocks(userPublicationRoleRepository);

                // A user with the publication owner role is allowed to create a new release
                Assert.Equal(isOwnerRole, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task CreateReleaseForSpecificPublicationAuthorizationHandler_FailsWhenArchived()
        {
            var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies();

            var user = DataFixture.AuthenticatedUser(userId: UserId);

            var authContext = CreateAuthContext(user, PublicationArchived);

            await handler.HandleAsync(authContext);

            // There should be no interactions to check the users' publication roles to determine
            // whether or not they can create a release
            MockUtils.VerifyAllMocks(userPublicationRoleRepository);

            // Verify that the user cannot create a new release of an archived publication
            Assert.False(authContext.HasSucceeded);
        }
    }

    private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user, Publication publication)
    {
        return AuthorizationHandlersTestUtil.CreateAuthorizationHandlerContext<
            CreateReleaseForSpecificPublicationRequirement,
            Publication
        >(user, publication);
    }

    private static (
        CreateReleaseForSpecificPublicationAuthorizationHandler,
        Mock<IUserPublicationRoleRepository>
    ) CreateHandlerAndDependencies()
    {
        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

        var handler = new CreateReleaseForSpecificPublicationAuthorizationHandler(
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                userReleaseRoleRepository.Object,
                userPublicationRoleRepository.Object,
                Mock.Of<IPreReleaseService>(Strict)
            )
        );

        return (handler, userPublicationRoleRepository);
    }
}
