#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;

public static class PublicationAuthorizationHandlersTestUtil
{
    private static readonly DataFixture DataFixture = new();

    public static async Task AssertPublicationHandlerSucceedsWithPublicationRoles<TRequirement>(
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        params PublicationRole[] publicationRolesExpectedToPass
    )
        where TRequirement : IAuthorizationRequirement
    {
        var publication = new Publication();
        await AssertHandlerSucceedsWithPublicationRoles<Publication, TRequirement>(
            handlerSupplier: handlerSupplier,
            entity: publication,
            publicationId: publication.Id,
            publicationRolesExpectedToPass: publicationRolesExpectedToPass
        );
    }

    public class PublicationRoleTestScenario : HandlerTestScenario
    {
        public List<UserPublicationRole>? UserPublicationRoles { get; set; }
    }

    public static async Task AssertHandlerOnlySucceedsWithPublicationRoles<TRequirement, TEntity>(
        Guid publicationId,
        TEntity handleRequirementArgument,
        Action<ContentDbContext> addToDbHandler,
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        params PublicationRole[] rolesExpectedToSucceed
    )
        where TRequirement : IAuthorizationRequirement
    {
        var allPublicationRoles = GetEnums<PublicationRole>();
        var userId = Guid.NewGuid();

        await allPublicationRoles
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async role =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    addToDbHandler(contentDbContext);
                    await contentDbContext.AddAsync(
                        new UserPublicationRole
                        {
                            UserId = userId,
                            Role = role,
                            PublicationId = publicationId,
                        }
                    );
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var user = DataFixture.AuthenticatedUser(userId: userId);
                    var authContext = new AuthorizationHandlerContext(
                        new IAuthorizationRequirement[] { Activator.CreateInstance<TRequirement>() },
                        user,
                        handleRequirementArgument
                    );

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
            var user = DataFixture.AuthenticatedUser(userId: userId);
            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { Activator.CreateInstance<TRequirement>() },
                user,
                handleRequirementArgument
            );

            var handler = handlerSupplier(contentDbContext);
            await handler.HandleAsync(authContext);
            Assert.False(authContext.HasSucceeded, "Should fail when user has no publication role");
        }
    }
}
