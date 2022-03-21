using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils
{
    public static class ReleaseAuthorizationHandlersTestUtil
    {
        /**
         * Assert that the given handler succeeds when a user has any of the "claimsExpectedToSucceed" and is tested
         * against an arbitrary Release, and fails otherwise
         */
        public static async Task AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
            IAuthorizationHandler handler,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            await AssertHandlerSucceedsWithCorrectClaims<Release, TRequirement>(handler, release,
                claimsExpectedToSucceed);
        }

        public static async Task AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            await AssertHandlerSucceedsWithCorrectClaims<Release, TRequirement>(handlerSupplier, release,
                claimsExpectedToSucceed);
        }

        public static async Task AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            Release release,
            params SecurityClaimTypes[] claimsExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            await AssertHandlerSucceedsWithCorrectClaims<Release, TRequirement>(handlerSupplier, release,
                claimsExpectedToSucceed);
        }

        /**
         * Assert that the given handler succeeds when a user has any of the "rolesExpectedToSucceed" on an arbitrary
         * Release, and fails otherwise
         */
        public static async Task AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params ReleaseRole[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(handlerSupplier, release,
                rolesExpectedToSucceed);
        }

        /**
         * Assert that the given handler succeeds when a user has any of the "rolesExpectedToSucceed" on the supplied
         * Release, and fails otherwise
         */
        public static async Task AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            Release release,
            params ReleaseRole[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var inTeamScenarios = CreateUserInProductionTeamScenarios(release, rolesExpectedToSucceed);
            var notInTeamScenario = CreateUserNotInProductionTeamScenario(release, rolesExpectedToSucceed);
            var allScenarios = new List<ReleaseHandlerTestScenario>(inTeamScenarios) { notInTeamScenario };
            await allScenarios
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(scenario =>
                    AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
        }

        /**
         * Assert that the given handler succeeds when a user has any of the "rolesExpectedToSucceed" on the supplied
         * Release, and fails otherwise
         */
        public static async Task AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            Release release,
            params PublicationRole[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var inTeamScenarios = CreateUserInPublicationTeamScenarios(release, rolesExpectedToSucceed);
            var notInTeamScenario = CreateUserNotInPublicationTeamScenario(release, rolesExpectedToSucceed);
            var allScenarios = new List<ReleaseHandlerTestScenario>(inTeamScenarios) { notInTeamScenario };
            await allScenarios
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(scenario =>
                    AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
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
                Entity = release,
                UserReleaseRoles = rolesList,
                ExpectedToPass = false,
                UnexpectedPassMessage = "Expected not having a role on the Release would have made the handler fail"
            };
            return notInTeamScenario;
        }

        private static List<ReleaseHandlerTestScenario> CreateUserInProductionTeamScenarios(Release release,
            ReleaseRole[] rolesExpectedToSucceed)
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
                    Entity = release,
                    UserReleaseRoles = rolesList,
                    ExpectedToPass = rolesExpectedToSucceed.Contains(role),
                    UnexpectedFailMessage = "Expected role " + role + " to have made the handler succeed",
                    UnexpectedPassMessage = "Expected role " + role + " to have made the handler fail",
                };
            }).ToList();
            return scenarios;
        }

        private static ReleaseHandlerTestScenario CreateUserNotInPublicationTeamScenario(Release release,
            PublicationRole[] rolesExpectedToSucceed)
        {
            var userId = Guid.NewGuid();

            var user = CreateClaimsPrincipal(userId);

            var rolesList = new List<UserPublicationRole>();

            // add some roles to unrelated Users to ensure that only the current User is being
            // taken into consideration
            rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
            {
                rolesList.Add(new UserPublicationRole
                {
                    PublicationId = release.Publication.Id,
                    UserId = Guid.NewGuid(),
                    Role = roleExpectedToSucceed
                });
            });

            // add some roles to unrelated Publications to ensure that only the Publication under test is being
            // taken into consideration
            rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
            {
                rolesList.Add(new UserPublicationRole
                {
                    PublicationId = Guid.NewGuid(),
                    UserId = userId,
                    Role = roleExpectedToSucceed
                });
            });

            var notInTeamScenario = new ReleaseHandlerTestScenario
            {
                User = user,
                Entity = release,
                UserPublicationRoles = rolesList,
                ExpectedToPass = false,
                UnexpectedPassMessage = "Expected not having a role on the Publication would have made the handler fail"
            };
            return notInTeamScenario;
        }

        private static List<ReleaseHandlerTestScenario> CreateUserInPublicationTeamScenarios(Release release,
            PublicationRole[] rolesExpectedToSucceed)
        {
            var scenarios = GetEnumValues<PublicationRole>().Select(role =>
            {
                var userId = Guid.NewGuid();

                var user = CreateClaimsPrincipal(userId);

                // add a new UserPublicationRole for the current User and PublicationRole under test
                var rolesList = new List<UserPublicationRole>
                {
                    new UserPublicationRole
                    {
                        PublicationId = release.Publication.Id,
                        UserId = userId,
                        Role = role
                    }
                };

                // add some roles to unrelated Users to ensure that only the current User is being
                // taken into consideration
                rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
                {
                    rolesList.Add(new UserPublicationRole
                    {
                        PublicationId = release.Publication.Id,
                        UserId = Guid.NewGuid(),
                        Role = roleExpectedToSucceed
                    });
                });

                // add some roles to unrelated Publications to ensure that only the Publication under test is being
                // taken into consideration
                rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
                {
                    rolesList.Add(new UserPublicationRole
                    {
                        PublicationId = Guid.NewGuid(),
                        UserId = userId,
                        Role = roleExpectedToSucceed
                    });
                });

                return new ReleaseHandlerTestScenario
                {
                    User = user,
                    Entity = release,
                    UserPublicationRoles = rolesList,
                    ExpectedToPass = rolesExpectedToSucceed.Contains(role),
                    UnexpectedFailMessage = "Expected role " + role + " to have made the handler succeed",
                    UnexpectedPassMessage = "Expected role " + role + " to have made the handler fail",
                };
            }).ToList();
            return scenarios;
        }

        public static async Task AssertReleaseHandlerHandlesScenarioSuccessfully<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            ReleaseHandlerTestScenario scenario) where TRequirement : IAuthorizationRequirement
        {
            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                if (scenario.UserPublicationRoles != null)
                {
                    await context.AddRangeAsync(scenario.UserPublicationRoles);
                }

                if (scenario.UserReleaseRoles != null)
                {
                    await context.AddRangeAsync(scenario.UserReleaseRoles);
                }

                await context.SaveChangesAsync();
            }

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                var handler = handlerSupplier(context);
                await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
            }
        }

        public static async Task AssertHandlerOnlySucceedsWithReleaseRoles<TRequirement, TEntity>(
            Guid releaseId,
            TEntity handleRequirementArgument,
            Action<ContentDbContext> addToDbHandler,
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params ReleaseRole[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var allReleaseRoles = GetEnumValues<ReleaseRole>();
            var userId = Guid.NewGuid();

            await allReleaseRoles
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async role =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        addToDbHandler(contentDbContext);
                        await contentDbContext.AddAsync(new UserReleaseRole
                        {
                            UserId = userId,
                            Role = role,
                            ReleaseId = releaseId,
                        });
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var user = CreateClaimsPrincipal(userId);
                        var authContext = new AuthorizationHandlerContext(
                            new IAuthorizationRequirement[] { Activator.CreateInstance<TRequirement>() },
                            user, handleRequirementArgument);

                        var handler = handlerSupplier(contentDbContext);
                        await handler.HandleAsync(authContext);
                        if (rolesExpectedToSucceed.Contains(role))
                        {
                            Assert.True(authContext.HasSucceeded, $"Should succeed with role {role.ToString()}");
                        }
                        else
                        {
                            Assert.False(authContext.HasSucceeded, $"Should fail with role {role.ToString()}");
                        }
                    }
                });

            // NOTE: Permission should fail if user no release role
            await using (var contentDbContext = InMemoryApplicationDbContext("no-release-role"))
            {
                addToDbHandler(contentDbContext);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext("no-release-role"))
            {
                var user = CreateClaimsPrincipal(userId);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<TRequirement>() },
                    user, handleRequirementArgument);

                var handler = handlerSupplier(contentDbContext);
                await handler.HandleAsync(authContext);
                Assert.False(authContext.HasSucceeded, $"Should fail when user has no release role");
            }
        }

        public class ReleaseHandlerTestScenario : HandlerTestScenario
        {
            public List<UserPublicationRole> UserPublicationRoles { get; set; }

            public List<UserReleaseRole> UserReleaseRoles { get; set; }
        }
    }
}
