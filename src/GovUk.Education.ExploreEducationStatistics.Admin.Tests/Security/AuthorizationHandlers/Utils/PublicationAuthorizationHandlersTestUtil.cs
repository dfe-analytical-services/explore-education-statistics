#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;

public static class PublicationAuthorizationHandlersTestUtil
{
    private static readonly DataFixture _fixture = new();

    public static async Task AssertPublicationHandlerSucceedsWithPublicationRoles<TRequirement>(
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        params PublicationRole[] publicationRolesExpectedToPass
    )
        where TRequirement : IAuthorizationRequirement
    {
        Publication publication = _fixture.DefaultPublication();

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
        Publication publication,
        TEntity handleRequirementArgument,
        Action<ContentDbContext> addToDbHandler,
        Func<ContentDbContext, IAuthorizationHandler> handlerSupplier,
        params PublicationRole[] rolesExpectedToSucceed
    )
        where TRequirement : IAuthorizationRequirement
    {
        var allPublicationRoles = GetEnums<PublicationRole>();
        User internalUser = _fixture.DefaultUser();
        ClaimsPrincipal identityUser = _fixture.AuthenticatedUser(userId: internalUser.Id);

        await allPublicationRoles
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async role =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    addToDbHandler(contentDbContext);
                    contentDbContext.Add(
                        _fixture
                            .DefaultUserPublicationRole()
                            .WithUser(internalUser)
                            .WithPublication(publication)
                            .WithRole(role)
                            .Generate()
                    );
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var authContext = new AuthorizationHandlerContext(
                        [Activator.CreateInstance<TRequirement>()],
                        identityUser,
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
            var authContext = new AuthorizationHandlerContext(
                [Activator.CreateInstance<TRequirement>()],
                identityUser,
                handleRequirementArgument
            );

            var handler = handlerSupplier(contentDbContext);
            await handler.HandleAsync(authContext);
            Assert.False(authContext.HasSucceeded, "Should fail when user has no publication role");
        }
    }
}
