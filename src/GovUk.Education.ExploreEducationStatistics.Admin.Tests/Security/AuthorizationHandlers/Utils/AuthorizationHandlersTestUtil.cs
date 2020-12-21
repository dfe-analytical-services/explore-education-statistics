using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Moq;
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
        public static void AssertHandlerSucceedsWithCorrectClaims<TEntity, TRequirement>(
            IAuthorizationHandler handler,
            TEntity entity,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(entity, claimsExpectedToSucceed);
            scenarios.ForEach(scenario => AssertHandlerHandlesScenarioSuccessfully<TEntity, TRequirement>(handler, scenario));
        }

        public static void AssertHandlerSucceedsWithCorrectClaims<TEntity, TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            TEntity entity,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetClaimTestScenarios(entity, claimsExpectedToSucceed);
            scenarios.ForEach(scenario => AssertHandlerHandlesScenarioSuccessfully<TEntity, TRequirement>(handlerSupplier, scenario));
        }

        public static void AssertHandlerHandlesScenarioSuccessfully<TEntity, TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            HandlerTestScenario<TEntity> scenario) where TRequirement : IAuthorizationRequirement
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                var handler = handlerSupplier(context);
                AssertHandlerHandlesScenarioSuccessfully<TEntity, TRequirement>(handler, scenario);
            }
        }

        private static List<HandlerTestScenario<TEntity>> GetClaimTestScenarios<TEntity>(
            TEntity entity,
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

                        return new HandlerTestScenario<TEntity>
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

        public static async void AssertHandlerHandlesScenarioSuccessfully<TEntity, TRequirement>(
            IAuthorizationHandler handler,
            HandlerTestScenario<TEntity> scenario)
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

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params Claim[] additionalClaims)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            identity.AddClaims(additionalClaims);
            var user = new ClaimsPrincipal(identity);
            return user;
        }

        public class HandlerTestScenario<TEntity>
        {
            public TEntity Entity { get; set; }

            public ClaimsPrincipal User { get; set; }

            public bool ExpectedToPass { get; set; }

            public string UnexpectedPassMessage { get; set; }

            public string UnexpectedFailMessage { get; set; }
        }
    }
}