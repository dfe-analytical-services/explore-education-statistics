#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;

public static class AuthorizationHandlersTestUtil
{
    private static readonly DataFixture _dataFixture = new();

    /**
     * Assert that the given handler succeeds when a user has any of the "claimsExpectedToSucceed", and is tested
     * against the supplied entity, and fails otherwise
     */
    public static async Task AssertHandlerSucceedsWithCorrectClaims<TRequirement, TEntity>(
        IAuthorizationHandler handler,
        TEntity entity,
        SecurityClaimTypes[] claimsExpectedToSucceed,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        var scenarios = GetClaimTestScenarios(entity, claimsExpectedToSucceed, userId);

        foreach (var scenario in scenarios)
        {
            await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
        }
    }

    public static async Task AssertHandlerFailsForAllClaims<TRequirement, TEntity>(
        IAuthorizationHandler handler,
        TEntity entity,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        var scenarios = GetClaimTestScenarios(entity, [], userId);

        foreach (var scenario in scenarios)
        {
            await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
        }
    }

    public static async Task AssertHandlerSucceedsWithCorrectGlobalRoles<TRequirement, TEntity>(
        IAuthorizationHandler handler,
        TEntity entity,
        GlobalRoles.Role[] rolesExpectedToSucceed,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        var scenarios = GetGlobalRoleTestScenarios(entity, rolesExpectedToSucceed, userId);

        foreach (var scenario in scenarios)
        {
            await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
        }
    }

    public static async Task AssertHandlerFailsForAllGlobalRoles<TRequirement, TEntity>(
        IAuthorizationHandler handler,
        TEntity entity,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        var scenarios = GetGlobalRoleTestScenarios(entity, [], userId);

        foreach (var scenario in scenarios)
        {
            await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
        }
    }

    public static async Task AssertHandlerSucceedsForAnyValidPublicationRole<TRequirement, TEntity>(
        Func<IAuthorizationHandlerService, IAuthorizationHandler> handlerSupplier,
        TEntity entity,
        Guid publicationId,
        HashSet<PublicationRole> publicationRolesExpectedToSucceed,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        if (!publicationRolesExpectedToSucceed.Any())
        {
            throw new ArgumentException(
                "At least one publication role must be expected to succeed for this assertion for it to be meaningful"
            );
        }

        await AssertScenario(true);
        await AssertScenario(false);

        async Task AssertScenario(bool handlerResult)
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(userId ?? Guid.NewGuid());

            var authContext = CreateAuthContext<TRequirement, TEntity>(user, entity);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        user.GetUserId(),
                        publicationId,
                        It.Is<HashSet<PublicationRole>>(roles => roles.SetEquals(publicationRolesExpectedToSucceed))
                    )
                )
                .ReturnsAsync(handlerResult);

            var handler = handlerSupplier(authorizationHandlerService.Object);

            await handler.HandleAsync(authContext);

            MockUtils.VerifyAllMocks(authorizationHandlerService);

            Assert.Equal(handlerResult, authContext.HasSucceeded);
        }
    }

    public static async Task AssertHandlerFailsWithoutCheckingRoles<TRequirement, TEntity>(
        Func<IAuthorizationHandlerService, IAuthorizationHandler> handlerSupplier,
        TEntity entity,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        ClaimsPrincipal user = _dataFixture.AuthenticatedUser(userId ?? Guid.NewGuid());

        var authContext = CreateAuthContext<TRequirement, TEntity>(user, entity);

        // Since we create the IAuthorizationHandlerService in Strict mode, and do not setup any behaviour,
        // then the Assert will fail if ANY method on the IAuthorizationHandlerService is called.
        // If the Assert check passes, that must mean we the handler did not bother to check the roles.
        // Which is what we expect here.
        var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);

        var handler = handlerSupplier(authorizationHandlerService.Object);

        await handler.HandleAsync(authContext);

        Assert.False(authContext.HasSucceeded);
    }

    public static async Task AssertHandlerSucceedsIfUserHasAnyRoleOnPublication<TRequirement, TEntity>(
        Func<IAuthorizationHandlerService, IAuthorizationHandler> handlerSupplier,
        TEntity entity,
        Guid publicationId,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        await AssertScenario(true);
        await AssertScenario(false);

        async Task AssertScenario(bool handlerResult)
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(userId ?? Guid.NewGuid());

            var authContext = CreateAuthContext<TRequirement, TEntity>(user, entity);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s => s.UserHasAnyRoleOnPublication(user.GetUserId(), publicationId))
                .ReturnsAsync(handlerResult);

            var handler = handlerSupplier(authorizationHandlerService.Object);

            await handler.HandleAsync(authContext);

            MockUtils.VerifyAllMocks(authorizationHandlerService);

            Assert.Equal(handlerResult, authContext.HasSucceeded);
        }
    }

    public static async Task AssertHandlerSucceedsIfReleaseVersionIsViewableByUser<TRequirement, TEntity>(
        Func<IAuthorizationHandlerService, IAuthorizationHandler> handlerSupplier,
        TEntity entity,
        ReleaseVersion releaseVersion,
        Guid? userId = null
    )
        where TRequirement : IAuthorizationRequirement
    {
        await AssertScenario(true);
        await AssertScenario(false);

        async Task AssertScenario(bool handlerResult)
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(userId ?? Guid.NewGuid());

            var authContext = CreateAuthContext<TRequirement, TEntity>(user, entity);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s => s.IsReleaseVersionViewableByUser(releaseVersion, user))
                .ReturnsAsync(handlerResult);

            var handler = handlerSupplier(authorizationHandlerService.Object);

            await handler.HandleAsync(authContext);

            MockUtils.VerifyAllMocks(authorizationHandlerService);

            Assert.Equal(handlerResult, authContext.HasSucceeded);
        }
    }

    private static List<HandlerTestScenario> GetClaimTestScenarios<TEntity>(
        TEntity entity,
        SecurityClaimTypes[] claimsExpectedToSucceed,
        Guid? userId = null
    ) =>
        [
            .. GetEnums<SecurityClaimTypes>()
                .Select(claim =>
                {
                    ClaimsPrincipal user = _dataFixture
                        .AuthenticatedUser(userId ?? Guid.NewGuid())
                        .WithClaim(claim.ToString());

                    return new HandlerTestScenario
                    {
                        User = user,
                        Entity = entity!,
                        ExpectedToPass = claimsExpectedToSucceed.Contains(claim),
                        UnexpectedFailMessage = "Expected claim " + claim + " to have caused the handler to succeed",
                        UnexpectedPassMessage = "Expected claim " + claim + " to have caused the handler to fail",
                    };
                }),
        ];

    private static List<HandlerTestScenario> GetGlobalRoleTestScenarios<TEntity>(
        TEntity entity,
        GlobalRoles.Role[] rolesExpectedToSucceed,
        Guid? userId = null
    ) =>
        [
            .. GetEnums<GlobalRoles.Role>()
                .Select(role =>
                {
                    ClaimsPrincipal user = _dataFixture
                        .AuthenticatedUser(userId ?? Guid.NewGuid())
                        .WithRole(role.GetEnumLabel());

                    return new HandlerTestScenario
                    {
                        User = user,
                        Entity = entity!,
                        ExpectedToPass = rolesExpectedToSucceed.Contains(role),
                        UnexpectedFailMessage = "Expected role " + role + " to have caused the handler to succeed",
                        UnexpectedPassMessage = "Expected role " + role + " to have caused the handler to fail",
                    };
                }),
        ];

    private static async Task AssertHandlerHandlesScenarioSuccessfully<TRequirement>(
        IAuthorizationHandler handler,
        HandlerTestScenario scenario
    )
        where TRequirement : IAuthorizationRequirement
    {
        var authContext = CreateAuthContext<TRequirement>(scenario.User, scenario.Entity);

        await handler.HandleAsync(authContext);

        if (scenario.ExpectedToPass)
        {
            Assert.True(authContext.HasSucceeded, scenario.UnexpectedFailMessage);
        }
        else
        {
            Assert.False(authContext.HasSucceeded, scenario.UnexpectedPassMessage);
        }
    }

    public record HandlerTestScenario
    {
        // Intentionally using 'object' instead of a generic type parameter (like <TEntity>)
        // to reduce boilerplate.
        public required object Entity { get; init; }

        public required ClaimsPrincipal User { get; init; }

        public required bool ExpectedToPass { get; init; }

        public string? UnexpectedPassMessage { get; init; }

        public string? UnexpectedFailMessage { get; init; }
    }
}
