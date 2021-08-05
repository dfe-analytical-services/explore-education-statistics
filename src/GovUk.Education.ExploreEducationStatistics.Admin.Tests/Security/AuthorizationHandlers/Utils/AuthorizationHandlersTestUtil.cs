using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils
{
    public static class AuthorizationHandlersTestUtil
    {
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
            await scenarios.ForEachAsync(scenario => AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario));
        }

        public static async Task AssertHandlerSucceedsWithCorrectClaims<TRequirement>(
            IAuthorizationHandler handler,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(null, claimsExpectedToSucceed);
            await scenarios.ForEachAsync(scenario => AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario));
        }
        
        public static async Task AssertHandlerSucceedsWithCorrectClaims<TEntity, TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            TEntity entity,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(entity, claimsExpectedToSucceed);
            await scenarios.ForEachAsync(scenario => AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
        }

        public static async Task AssertHandlerSucceedsWithCorrectClaims<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(null, claimsExpectedToSucceed);
            await scenarios.ForEachAsync(scenario => AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
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
            return GetEnumValues<SecurityClaimTypes>()
                .Select(
                    claim =>
                    {
                        var user = CreateClaimsPrincipal(
                            Guid.NewGuid(),
                            new Claim(claim.ToString(), "")
                        );

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

        public static async Task AssertHandlerHandlesScenarioSuccessfully<TRequirement>(
            IAuthorizationHandler handler,
            HandlerTestScenario scenario)
            where TRequirement : IAuthorizationRequirement
        {
            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()},
                scenario.User, scenario.Entity);

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

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
        {
            return CreateClaimsPrincipal(userId, new Claim[] {});
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params Claim[] additionalClaims)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            identity.AddClaims(additionalClaims);
            var user = new ClaimsPrincipal(identity);
            return user;
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params SecurityClaimTypes[] additionalClaims)
        {
            return CreateClaimsPrincipal(userId, 
                additionalClaims.Select(c => new Claim(c.ToString(), "")).ToArray());
        }

        public static void ForEachSecurityClaim(Action<SecurityClaimTypes> action)
        {
            GetEnumValues<SecurityClaimTypes>().ForEach(action.Invoke);
        }
        
        public static Task ForEachSecurityClaimAsync(Func<SecurityClaimTypes, Task> action)
        {
            return GetEnumValues<SecurityClaimTypes>().ForEachAsync(action.Invoke);
        }

        public static Task ForEachReleaseRoleAsync(Func<ReleaseRole, Task> action)
        {
            return GetEnumValues<ReleaseRole>().ForEachAsync(action.Invoke);
        }
        
        public static Task ForEachPublicationRoleAsync(Func<PublicationRole, Task> action)
        {
            return GetEnumValues<PublicationRole>().ForEachAsync(action.Invoke);
        }

        public static AuthorizationHandlerContext CreateAuthorizationHandlerContext<TRequirement, TEntity>(
            ClaimsPrincipal user, TEntity entity) where TRequirement : IAuthorizationRequirement
        {
            return new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()},
                user, entity);

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
