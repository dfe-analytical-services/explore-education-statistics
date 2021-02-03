using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyAuthorizationHandlersTests
    {
        [Fact]
        public void UpdateAllSpecificMethodologiesAuthorizationHandler()
        {
            AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificMethodologyRequirement>(
                new UpdateAllSpecificMethodologiesAuthorizationHandler(),
                UpdateAllMethodologies
            );
        }

        [Fact]
        public async void HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler_Succeed()
        {
            var userId = Guid.NewGuid();
            var methodology = new Methodology();
            var publication = new Publication
            {
                Methodology = methodology
            };
            var release = new Release
            {
                Publication = publication
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.Lead
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                var handler = new UpdateSpecificMethodologyAuthorizationHandler(methodologyRepository);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new UpdateSpecificMethodologyRequirement()},
                    CreateClaimsPrincipal(userId),
                    methodology);
                await handler.HandleAsync(authContext);
                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async void HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler_PrereleaseRole_NoSucceed()
        {
            var userId = Guid.NewGuid();
            var methodology = new Methodology();
            var publication = new Publication
            {
                Methodology = methodology
            };
            var release = new Release
            {
                Publication = publication
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.PrereleaseViewer
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                var handler = new UpdateSpecificMethodologyAuthorizationHandler(methodologyRepository);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new UpdateSpecificMethodologyRequirement()},
                    CreateClaimsPrincipal(userId),
                    methodology);
                await handler.HandleAsync(authContext);
                Assert.False(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async void HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler_NoRole_NoSucceed()
        {
            var userId = Guid.NewGuid();
            var methodology = new Methodology();
            var publicationAttached = new Publication
            {
                Methodology = methodology
            };
            var publicationUnattached = new Publication();
            var release = new Release
            {
                Publication = publicationUnattached
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.Lead
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publicationAttached);
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                var handler = new UpdateSpecificMethodologyAuthorizationHandler(methodologyRepository);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new UpdateSpecificMethodologyRequirement()},
                    CreateClaimsPrincipal(userId),
                    methodology);
                await handler.HandleAsync(authContext);
                Assert.False(authContext.HasSucceeded);
            }
        }
    }
}
