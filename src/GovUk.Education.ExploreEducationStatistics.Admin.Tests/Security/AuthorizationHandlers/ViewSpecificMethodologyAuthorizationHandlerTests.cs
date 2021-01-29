using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public void CanViewAllMethodologiesAuthorizationHandler()
        {
            AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificMethodologyRequirement>(
                new ViewSpecificMethodologyAuthorizationHandler.CanViewAllMethodologiesAuthorizationHandler(),
                AccessAllMethodologies);
        }

        [Fact]
        public async void HasRoleOnAnyAssociatedReleaseAuthorizationHandler_Succeed()
        {
            var userId = Guid.NewGuid();
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
            };
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.Lead
            };
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Publications = new List<Publication> { publication }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var handler = new ViewSpecificMethodologyAuthorizationHandler(contentDbContext);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ViewSpecificMethodologyRequirement()},
                    CreateClaimsPrincipal(userId),
                    methodology);
                await handler.HandleAsync(authContext);
                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async void HasRoleOnAnyAssociatedReleaseAuthorizationHandler_PrereleaseRole_NoSucceed()
        {
            var userId = Guid.NewGuid();
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
            };
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.PrereleaseViewer
            };
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Publications = new List<Publication> { publication }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var handler = new ViewSpecificMethodologyAuthorizationHandler(contentDbContext);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ViewSpecificMethodologyRequirement()},
                    CreateClaimsPrincipal(userId),
                    methodology);
                await handler.HandleAsync(authContext);
                Assert.False(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async void HasRoleOnAnyAssociatedReleaseAuthorizationHandler_NoRole_NoSucceed()
        {
            var userId = Guid.NewGuid();
            var publicationAttached = new Publication
            {
                Id = Guid.NewGuid(),
            };
            var publicationUnattached = new Publication
            {
                Id = Guid.NewGuid(),
            };
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publicationUnattached.Id
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.Lead
            };
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Publications = new List<Publication> { publicationAttached }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publicationAttached);
                await contentDbContext.AddAsync(publicationUnattached);
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var handler = new ViewSpecificMethodologyAuthorizationHandler(contentDbContext);
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ViewSpecificMethodologyRequirement()},
                    CreateClaimsPrincipal(userId),
                    methodology);
                await handler.HandleAsync(authContext);
                Assert.False(authContext.HasSucceeded);
            }
        }
    }
}
