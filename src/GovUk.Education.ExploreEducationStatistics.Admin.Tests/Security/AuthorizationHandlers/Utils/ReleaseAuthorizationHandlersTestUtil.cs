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
    public static class ReleaseAuthorizationHandlersTestUtil
    {
        /**
         * Assert that the given handler succeeds when a user has any of the "claimsExpectedToSucceed" and is tested
         * against an arbitrary Release, and fails otherwise
         */
        public static void AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
            IAuthorizationHandler handler, 
            params SecurityClaimTypes[] claimsExpectedToSucceed) 
            where TRequirement : IAuthorizationRequirement
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };
            
            AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(handler, release, claimsExpectedToSucceed);
        }

        /**
         * Assert that the given handler succeeds when a user has any of the "claimsExpectedToSucceed", and is tested
         * against the supplied Release, and fails otherwise
         */
        public static void AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
            IAuthorizationHandler handler, 
            Release release,
            params SecurityClaimTypes[] claimsExpectedToSucceed) 
            where TRequirement : IAuthorizationRequirement
        {
            var scenarios = GetEnumValues<SecurityClaimTypes>().Select(claim =>
            {
                var user = CreateClaimsPrincipal(Guid.NewGuid(), new Claim(claim.ToString(), ""));

                return new ReleaseHandlerTestScenario
                {
                    User = user,
                    Release = release,
                    ExpectedToPass = claimsExpectedToSucceed.Contains(claim),
                    UnexpectedFailMessage = "Expected claim " + claim + " to have caused the handler to succeed",
                    UnexpectedPassMessage = "Expected claim " + claim + " to have caused the handler to fail"
                };
            }).ToList();
            
            scenarios.ForEach(scenario => AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario));
        }

        /**
         * Assert that the given handler succeeds when a user has any of the "rolesExpectedToSucceed" on an arbitrary
         * Release, and fails otherwise
         */
        public static void AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params ReleaseRole[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };
            
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(handlerSupplier, release, rolesExpectedToSucceed);
        }

        /**
         * Assert that the given handler succeeds when a user has any of the "rolesExpectedToSucceed" on the supplied
         * Release, and fails otherwise
         */
        public static void AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            Release release, 
            params ReleaseRole[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var inTeamScenarios = CreateUserInProductionTeamScenarios(release, rolesExpectedToSucceed);
            var notInTeamScenario = CreateUserNotInProductionTeamScenario(release, rolesExpectedToSucceed);
            var allScenarios = new List<ReleaseHandlerTestScenario>(inTeamScenarios) {notInTeamScenario};
            allScenarios.ForEach(scenario => AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
        }
        
        public static async void AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(
            IAuthorizationHandler handler, 
            ReleaseHandlerTestScenario scenario) 
            where TRequirement : IAuthorizationRequirement
        {
            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()},
                scenario.User, scenario.Release);

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

        private static ReleaseHandlerTestScenario CreateUserNotInProductionTeamScenario(Release release,
            ReleaseRole[] rolesExpectedToSucceed)
        {
            var userId = Guid.NewGuid();

            var user = CreateClaimsPrincipal(userId);

            var rolesList = new List<UserReleaseRole>();

            // add some roles to unrelated Users to ensure that only the current User is being
            // taken into consideration
            rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
            {
                rolesList.Add(new UserReleaseRole
                {
                    ReleaseId = release.Id,
                    UserId = Guid.NewGuid(),
                    Role = roleExpectedToSucceed
                });
            });

            // add some roles to unrelated Releases to ensure that only the Release under test is being
            // taken into consideration
            rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
            {
                rolesList.Add(new UserReleaseRole
                {
                    ReleaseId = Guid.NewGuid(),
                    UserId = userId,
                    Role = roleExpectedToSucceed
                });
            });

            var notInTeamScenario = new ReleaseHandlerTestScenario
            {
                User = user,
                Release = release,
                UserReleaseRoles = rolesList,
                ExpectedToPass = false,
                UnexpectedPassMessage = "Expected not having a role on the Release would have made the handler fail"
            };
            return notInTeamScenario;
        }

        private static List<ReleaseHandlerTestScenario> CreateUserInProductionTeamScenarios(Release release, ReleaseRole[] rolesExpectedToSucceed)
        {
            var scenarios = GetEnumValues<ReleaseRole>().Select(role =>
            {
                var userId = Guid.NewGuid();

                var user = CreateClaimsPrincipal(userId);

                // add a new UserReleaseRole for the current User and ReleaseRole under test
                var rolesList = new List<UserReleaseRole>
                {
                    new UserReleaseRole
                    {
                        ReleaseId = release.Id,
                        UserId = userId,
                        Role = role
                    }
                };

                // add some roles to unrelated Users to ensure that only the current User is being
                // taken into consideration
                rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
                {
                    rolesList.Add(new UserReleaseRole
                    {
                        ReleaseId = release.Id,
                        UserId = Guid.NewGuid(),
                        Role = roleExpectedToSucceed
                    });
                });

                // add some roles to unrelated Releases to ensure that only the Release under test is being
                // taken into consideration
                rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
                {
                    rolesList.Add(new UserReleaseRole
                    {
                        ReleaseId = Guid.NewGuid(),
                        UserId = userId,
                        Role = roleExpectedToSucceed
                    });
                });

                return new ReleaseHandlerTestScenario
                {
                    User = user,
                    Release = release,
                    UserReleaseRoles = rolesList,
                    ExpectedToPass = rolesExpectedToSucceed.Contains(role),
                    UnexpectedFailMessage = "Expected role " + role + " to have made the handler succeed",
                    UnexpectedPassMessage = "Expected role " + role + " to have made the handler fail",
                };
            }).ToList();
            return scenarios;
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params Claim[] additionalClaims)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            identity.AddClaims(additionalClaims);
            var user = new ClaimsPrincipal(identity);
            return user;
        }

        public static void AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier, 
            ReleaseHandlerTestScenario scenario) where TRequirement : IAuthorizationRequirement
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.AddRange(scenario.UserReleaseRoles);
                context.SaveChanges();
                
                var handler = handlerSupplier(context);
                AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
            }
        }

        public class ReleaseHandlerTestScenario
        {
            public Release Release { get; set; }
            
            public ClaimsPrincipal User { get; set; }

            public List<UserReleaseRole> UserReleaseRoles { get; set; }
            
            public bool ExpectedToPass { get; set; }
            
            public string UnexpectedPassMessage { get; set; }
            
            public string UnexpectedFailMessage { get; set; }
        }
    }
}