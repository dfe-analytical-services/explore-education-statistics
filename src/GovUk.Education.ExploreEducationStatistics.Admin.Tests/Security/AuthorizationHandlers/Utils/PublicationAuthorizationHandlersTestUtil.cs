#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils
{
    public static class PublicationAuthorizationHandlersTestUtil
    {
        public static async Task AssertPublicationHandlerSucceedsWithPublicationRoles<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params PublicationRole[] publicationRolesExpectedToPass)
            where TRequirement : IAuthorizationRequirement
        {
            var publication = new Publication();
            await AssertPublicationHandlerSucceedsWithPublicationRoles<TRequirement>(handlerSupplier, publication, publicationRolesExpectedToPass);
        }

        public static async Task AssertPublicationHandlerSucceedsWithPublicationRoles<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            Publication publication,
            params PublicationRole[] publicationRolesExpectedToPass)
            where TRequirement : IAuthorizationRequirement
        {
            var user = CreateClaimsPrincipal(Guid.NewGuid());

            await ForEachPublicationRoleAsync(async role =>
            {
                // Test the handler succeeds with the Owner role on the Publication for the User
                await AssertPublicationHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier,
                    new PublicationHandlerTestScenario
                    {
                        User = user,
                        Entity = publication,
                        // Setup a UserPublicationRole for this Publication and User
                        UserPublicationRoles = ListOf(
                            new UserPublicationRole
                            {
                                PublicationId = publication.Id,
                                UserId = user.GetUserId(),
                                Role = role
                            }),
                        ExpectedToPass = publicationRolesExpectedToPass.Contains(role),
                        UnexpectedFailMessage =
                            $"Expected having role {role} on the Publication to have made the handler succeed",
                    });

                await ForEachPublicationRoleAsync(async role =>
                {
                    // Test the handler fails without the role on the correct Publication or the correct User
                    await AssertPublicationHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier,
                        new PublicationHandlerTestScenario
                        {
                            User = user,
                            Entity = publication,
                            // Setup a UserPublicationRole for this Publication but a different User
                            UserPublicationRoles = ListOf(
                                new UserPublicationRole
                                {
                                    PublicationId = publication.Id,
                                    UserId = Guid.NewGuid(),
                                    Role = role
                                },
                                // Setup a UserPublicationRoles for this User but a different Publication
                                new UserPublicationRole
                                {
                                    PublicationId = Guid.NewGuid(),
                                    UserId = user.GetUserId(),
                                    Role = role
                                }),
                            ExpectedToPass = false,
                            UnexpectedPassMessage =
                                $"Expected not having {role} role on the Publication would have made the handler fail"
                        });
                });
            });
        }

        public static async Task AssertPublicationHandlerFailsRegardlessOfPublicationOwner<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            Publication publication)
            where TRequirement : IAuthorizationRequirement
        {
            var user = CreateClaimsPrincipal(Guid.NewGuid());

            // Test the handler fails with the Owner role on the Publication for the User
            await AssertPublicationHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier,
            new PublicationHandlerTestScenario
            {
                User = user,
                Entity = publication,
                UserPublicationRoles = new List<UserPublicationRole>
                {
                    // Setup a UserPublicationRole for this Publication and User
                    new UserPublicationRole
                    {
                        PublicationId = publication.Id,
                        UserId = user.GetUserId(),
                        Role = Owner
                    }
                },
                ExpectedToPass = false,
                UnexpectedPassMessage = "Expected handler to fail despite having Publication Owner role on the Publication"
            });

            // Test the handler fails without the Owner role on the Publication for the User
            await AssertPublicationHandlerHandlesScenarioSuccessfully<TRequirement>(handlerSupplier,
            new PublicationHandlerTestScenario
            {
                User = user,
                Entity = publication,
                UserPublicationRoles = new List<UserPublicationRole>(),
                ExpectedToPass = false,
                UnexpectedPassMessage = "Expected handler to fail with no roles on the Publication"
            });
        }
        
        public class PublicationHandlerTestScenario : HandlerTestScenario
        {
            public List<UserPublicationRole>? UserPublicationRoles { get; set; }
        }

        public static async Task AssertHandlerOnlySucceedsWithPublicationRoles<TRequirement, TEntity>(
            Guid publicationId,
            TEntity handleRequirementArgument,
            Action<ContentDbContext> addToDbHandler,
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            params PublicationRole[] rolesExpectedToSucceed)
            where TRequirement : IAuthorizationRequirement
        {
            var allPublicationRoles = GetEnumValues<PublicationRole>();
            var userId = Guid.NewGuid();

            await allPublicationRoles
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async role =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        addToDbHandler(contentDbContext);
                        await contentDbContext.AddAsync(new UserPublicationRole
                        {
                            UserId = userId,
                            Role = role,
                            PublicationId = publicationId,
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

            // NOTE: Permission should fail if user no publication role
            await using (var contentDbContext = InMemoryApplicationDbContext("no-publication-role"))
            {
                addToDbHandler(contentDbContext);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext("no-publication-role"))
            {
                var user = CreateClaimsPrincipal(userId);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()},
                    user, handleRequirementArgument);

                var handler = handlerSupplier(contentDbContext);
                await handler.HandleAsync(authContext);
                Assert.False(authContext.HasSucceeded, $"Should fail when user has no publication role");
            }
        }

        private static async Task AssertPublicationHandlerHandlesScenarioSuccessfully<TRequirement>(
            Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
            PublicationHandlerTestScenario scenario) where TRequirement : IAuthorizationRequirement
        {
            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                if (scenario.UserPublicationRoles != null)
                {
                    await context.AddRangeAsync(scenario.UserPublicationRoles);
                    await context.SaveChangesAsync();
                }
            }

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                var handler = handlerSupplier(context);
                await AssertHandlerHandlesScenarioSuccessfully<TRequirement>(handler, scenario);
            }
        }
    }
}
