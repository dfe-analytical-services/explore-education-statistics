using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;

public static class ReleaseVersionAuthorizationHandlersTestUtil
{
    private static readonly DataFixture DataFixture = new();

    /**
     * Assert that the given handler succeeds when a user has any of the "rolesExpectedToSucceed" on the supplied
     * Release, and fails otherwise
     */
    public static async Task AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        ReleaseVersion releaseVersion,
        params ReleaseRole[] rolesExpectedToSucceed)
        where TRequirement : IAuthorizationRequirement
    {
        var inTeamScenarios = CreateUserInProductionTeamScenarios(releaseVersion, rolesExpectedToSucceed);
        var notInTeamScenario = CreateUserNotInProductionTeamScenario(releaseVersion, rolesExpectedToSucceed);
        var allScenarios = new List<ReleaseVersionHandlerTestScenario>(inTeamScenarios) { notInTeamScenario };
        await allScenarios
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(scenario =>
                AssertReleaseVersionHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
    }

    /**
     * Assert that the given handler succeeds when a user has any of the "rolesExpectedToSucceed" on the supplied
     * Release, and fails otherwise
     */
    public static async Task AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<TRequirement>(
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        ReleaseVersion releaseVersion,
        params PublicationRole[] rolesExpectedToSucceed)
        where TRequirement : IAuthorizationRequirement
    {
        var inTeamScenarios = CreateUserInPublicationTeamScenarios(releaseVersion, rolesExpectedToSucceed);
        var notInTeamScenario = CreateUserNotInPublicationTeamScenario(releaseVersion, rolesExpectedToSucceed);
        var allScenarios = new List<ReleaseVersionHandlerTestScenario>(inTeamScenarios) { notInTeamScenario };
        await allScenarios
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(scenario =>
                AssertReleaseVersionHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier, scenario));
    }

    private static ReleaseVersionHandlerTestScenario CreateUserNotInProductionTeamScenario(ReleaseVersion releaseVersion,
        ReleaseRole[] rolesExpectedToSucceed)
    {
        var userId = Guid.NewGuid();

        var user = DataFixture.AuthenticatedUser(userId: userId);

        var rolesList = new List<UserReleaseRole>();

        // add some roles to unrelated Users to ensure that only the current User is being
        // taken into consideration
        rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
        {
            rolesList.Add(new UserReleaseRole
            {
                ReleaseVersionId = releaseVersion.Id,
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
                ReleaseVersionId = Guid.NewGuid(),
                UserId = userId,
                Role = roleExpectedToSucceed
            });
        });

        var notInTeamScenario = new ReleaseVersionHandlerTestScenario
        {
            User = user,
            Entity = releaseVersion,
            UserReleaseRoles = rolesList,
            ExpectedToPass = false,
            UnexpectedPassMessage = "Expected not having a role on the Release would have made the handler fail"
        };
        return notInTeamScenario;
    }

    private static List<ReleaseVersionHandlerTestScenario> CreateUserInProductionTeamScenarios(
        ReleaseVersion releaseVersion,
        ReleaseRole[] rolesExpectedToSucceed)
    {
        var scenarios = GetEnums<ReleaseRole>().Select(role =>
        {
            var userId = Guid.NewGuid();

            var user = DataFixture.AuthenticatedUser(userId: userId);

            // add a new UserReleaseRole for the current User and ReleaseRole under test
            var rolesList = new List<UserReleaseRole>
            {
                new UserReleaseRole
                {
                    ReleaseVersionId = releaseVersion.Id,
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
                    ReleaseVersionId = releaseVersion.Id,
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
                    ReleaseVersionId = Guid.NewGuid(),
                    UserId = userId,
                    Role = roleExpectedToSucceed
                });
            });

            return new ReleaseVersionHandlerTestScenario
            {
                User = user,
                Entity = releaseVersion,
                UserReleaseRoles = rolesList,
                ExpectedToPass = rolesExpectedToSucceed.Contains(role),
                UnexpectedFailMessage = "Expected role " + role + " to have made the handler succeed",
                UnexpectedPassMessage = "Expected role " + role + " to have made the handler fail",
            };
        }).ToList();
        return scenarios;
    }

    private static ReleaseVersionHandlerTestScenario CreateUserNotInPublicationTeamScenario(ReleaseVersion releaseVersion,
        PublicationRole[] rolesExpectedToSucceed)
    {
        var userId = Guid.NewGuid();

        var user = DataFixture.AuthenticatedUser(userId: userId);

        var rolesList = new List<UserPublicationRole>();

        // add some roles to unrelated Users to ensure that only the current User is being
        // taken into consideration
        rolesExpectedToSucceed.ToList().ForEach(roleExpectedToSucceed =>
        {
            rolesList.Add(new UserPublicationRole
            {
                PublicationId = releaseVersion.Publication.Id,
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

        var notInTeamScenario = new ReleaseVersionHandlerTestScenario
        {
            User = user,
            Entity = releaseVersion,
            UserPublicationRoles = rolesList,
            ExpectedToPass = false,
            UnexpectedPassMessage = "Expected not having a role on the Publication would have made the handler fail"
        };
        return notInTeamScenario;
    }

    private static List<ReleaseVersionHandlerTestScenario> CreateUserInPublicationTeamScenarios(
        ReleaseVersion releaseVersion,
        PublicationRole[] rolesExpectedToSucceed)
    {
        var scenarios = GetEnums<PublicationRole>().Select(role =>
        {
            var userId = Guid.NewGuid();

            var user = DataFixture.AuthenticatedUser(userId: userId);

            // add a new UserPublicationRole for the current User and PublicationRole under test
            var rolesList = new List<UserPublicationRole>
            {
                new UserPublicationRole
                {
                    PublicationId = releaseVersion.Publication.Id,
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
                    PublicationId = releaseVersion.Publication.Id,
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

            return new ReleaseVersionHandlerTestScenario
            {
                User = user,
                Entity = releaseVersion,
                UserPublicationRoles = rolesList,
                ExpectedToPass = rolesExpectedToSucceed.Contains(role),
                UnexpectedFailMessage = "Expected role " + role + " to have made the handler succeed",
                UnexpectedPassMessage = "Expected role " + role + " to have made the handler fail",
            };
        }).ToList();
        return scenarios;
    }

    public static async Task AssertReleaseVersionHandlerHandlesScenarioSuccessfully<TRequirement>(
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        ReleaseVersionHandlerTestScenario scenario) where TRequirement : IAuthorizationRequirement
    {
        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
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

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var handler = handlerSupplier(context);
            await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
        }
    }

    public static async Task AssertHandlerOnlySucceedsWithReleaseRoles<TRequirement, TEntity>(
        Guid releaseVersionId,
        TEntity handleRequirementArgument,
        Action<ContentDbContext> addToDbHandler,
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        params ReleaseRole[] rolesExpectedToSucceed)
        where TRequirement : IAuthorizationRequirement
    {
        var allReleaseRoles = GetEnums<ReleaseRole>();
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
                        ReleaseVersionId = releaseVersionId
                    });
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var user = DataFixture.AuthenticatedUser(userId: userId);
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
            var user = DataFixture.AuthenticatedUser(userId: userId);
            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { Activator.CreateInstance<TRequirement>() },
                user, handleRequirementArgument);

            var handler = handlerSupplier(contentDbContext);
            await handler.HandleAsync(authContext);
            Assert.False(authContext.HasSucceeded, "Should fail when user has no release role");
        }
    }

    public class ReleaseVersionHandlerTestScenario : HandlerTestScenario
    {
        public List<UserPublicationRole> UserPublicationRoles { get; set; }

        public List<UserReleaseRole> UserReleaseRoles { get; set; }
    }
}
