using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils
{
    public static class AuthorizationHandlersTestUtil
    {
        private static readonly DataFixture DataFixture = new();

        /**
         * Assert that the given handler succeeds when a user has any of the "claimsExpectedToSucceed", and is tested
         * against the supplied entity, and fails otherwise
         */
        public static async Task AssertHandlerSucceedsWithCorrectClaims<TEntity, TRequirement>(
            IAuthorizationHandler handler,
            TEntity entity,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(entity, claimsExpectedToSucceed);
            await scenarios
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    scenario => AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario));
        }

        public static async Task AssertHandlerSucceedsWithCorrectClaims<TEntity, TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            TEntity entity,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(entity, claimsExpectedToSucceed);
            await scenarios
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(scenario =>
                    AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
        }

        public static async Task AssertHandlerSucceedsWithCorrectGlobalRoles<TEntity, TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            TEntity entity,
            params GlobalRoles.Role[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetGlobalRoleTestScenarios(entity, rolesExpectedToSucceed);
            await scenarios
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(scenario =>
                    AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
        }

        public static async Task AssertHandlerSucceedsWithCorrectClaims<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(null, claimsExpectedToSucceed);
            await scenarios
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(scenario =>
                    AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
        }

        public static async Task AssertHandlerHandlesScenarioSuccessfully<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            HandlerTestScenario scenario) where TRequirement : IAuthorizationRequirement
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                var handler = handlerSupplier(context);
                await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
            }
        }

        private static List<HandlerTestScenario> GetClaimTestScenarios(
            object entity,
            SecurityClaimTypes[] claimsExpectedToSucceed)
        {
            return GetEnums<SecurityClaimTypes>()
                .Select(
                    claim =>
                    {
                        var user = DataFixture
                            .AuthenticatedUser()
                            .WithClaim(claim.ToString());

                        return new HandlerTestScenario
                        {
                            User = user,
                            Entity = entity,
                            ExpectedToPass = claimsExpectedToSucceed.Contains(claim),
                            UnexpectedFailMessage =
                                "Expected claim " + claim + " to have caused the handler to succeed",
                            UnexpectedPassMessage = "Expected claim " + claim + " to have caused the handler to fail"
                        };
                    }
                )
                .ToList();
        }

        private static List<HandlerTestScenario> GetGlobalRoleTestScenarios(
            object entity,
            GlobalRoles.Role[] rolesExpectedToSucceed)
        {
            return GetEnums<GlobalRoles.Role>()
                .Select(
                    role =>
                    {
                        var user = DataFixture
                            .AuthenticatedUser()
                            .WithRole(role.GetEnumLabel());

                        return new HandlerTestScenario
                        {
                            User = user,
                            Entity = entity,
                            ExpectedToPass = rolesExpectedToSucceed.Contains(role),
                            UnexpectedFailMessage =
                                "Expected role " + role + " to have caused the handler to succeed",
                            UnexpectedPassMessage = "Expected role " + role + " to have caused the handler to fail"
                        };
                    }
                )
                .ToList();
        }

        public static async Task AssertHandlerHandlesScenarioSuccessfully<TRequirement>(
            IAuthorizationHandler handler,
            HandlerTestScenario scenario)
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

        public static void ForEachSecurityClaim(Action<SecurityClaimTypes> action)
        {
            GetEnums<SecurityClaimTypes>().ForEach(action.Invoke);
        }

        public static Task ForEachSecurityClaimAsync(Func<SecurityClaimTypes, Task> action)
        {
            return GetEnums<SecurityClaimTypes>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(action.Invoke);
        }

        public static Task ForEachReleaseRoleAsync(Func<ReleaseRole, Task> action)
        {
            return GetEnums<ReleaseRole>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(action.Invoke);
        }

        public static Task ForEachPublicationRoleAsync(Func<PublicationRole, Task> action)
        {
            return GetEnums<PublicationRole>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(action.Invoke);
        }

        public static AuthorizationHandlerContext CreateAuthorizationHandlerContext<TRequirement, TEntity>(
            ClaimsPrincipal user, TEntity entity) where TRequirement : IAuthorizationRequirement
        {
            return CreateAuthContext<TRequirement, TEntity>(user, entity);
        }

        public class HandlerTestScenario
        {
            public object Entity { get; set; }

            public ClaimsPrincipal User { get; set; }

            public bool ExpectedToPass { get; set; }

            public string UnexpectedPassMessage { get; set; }

            public string UnexpectedFailMessage { get; set; }
        }
    }
}
