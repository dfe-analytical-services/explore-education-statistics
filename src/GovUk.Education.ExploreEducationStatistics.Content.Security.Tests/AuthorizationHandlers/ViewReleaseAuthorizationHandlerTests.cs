#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.Tests.AuthorizationHandlers
{
    public class ViewReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task ReleaseIsLiveAndOnlyVersion()
        {
            var release = new Release
            {
                Published = new DateTime(2021, 1, 1),
            };

            var publication = new Publication
            {
                Releases = ListOf(release)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var handler = new ViewReleaseAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewReleaseRequirement>() },
                    null,
                    release
                );

                await handler.HandleAsync(authContext);

                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task ReleaseIsLiveAndLatestVersion()
        {
            var originalRelease = new Release
            {
                Published = new DateTime(2020, 1, 1),
            };
            var amendedRelease = new Release
            {
                Published = new DateTime(2021, 2, 1),
                PreviousVersion = originalRelease,
            };

            var publication = new Publication
            {
                Releases = ListOf(originalRelease, amendedRelease)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var handler = new ViewReleaseAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewReleaseRequirement>() },
                    null,
                    amendedRelease
                );

                await handler.HandleAsync(authContext);

                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task ReleaseIsNotLive()
        {
            var release = new Release();

            var publication = new Publication
            {
                Releases = ListOf(release)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var handler = new ViewReleaseAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewReleaseRequirement>() },
                    null,
                    release
                );

                await handler.HandleAsync(authContext);

                Assert.False(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task ReleaseIsLiveButNotLatestVersion()
        {
            var originalRelease = new Release
            {
                Published = new DateTime(2020, 1, 1),
            };

            var amendedRelease = new Release
            {
                Published = new DateTime(2020, 2, 1),
                PreviousVersion = originalRelease,
            };

            var publication = new Publication
            {
                Releases = ListOf(originalRelease, amendedRelease)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var handler = new ViewReleaseAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewReleaseRequirement>() },
                    null,
                    originalRelease
                );

                await handler.HandleAsync(authContext);

                Assert.False(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task ReleaseNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();

            var handler = new ViewReleaseAuthorizationHandler(contentDbContext);

            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { Activator.CreateInstance<ViewReleaseRequirement>() },
                null,
                new Release
                {
                    Id = Guid.NewGuid()
                }
            );

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }
}
