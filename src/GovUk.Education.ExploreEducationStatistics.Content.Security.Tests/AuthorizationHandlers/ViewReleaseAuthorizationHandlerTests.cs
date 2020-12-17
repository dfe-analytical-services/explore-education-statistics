using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.Tests.AuthorizationHandlers
{
    public class ViewReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task ReleaseIsLiveAndOnlyVersion()
        {
            var publication = new Publication();

            var release = new Release
            {
                Published = DateTime.Parse("2019-10-10T12:00:00"),
                Publication = publication
            };

            publication.Releases.Add(release);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
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
            var publication = new Publication();

            var olderRelease = new Release
            {
                Published = DateTime.Parse("2019-10-08T12:00:00"),
                Publication = publication
            };

            var newerRelease = new Release
            {
                Published = DateTime.Parse("2019-10-10T12:00:00"),
                Publication = publication,
                PreviousVersion = olderRelease,
            };

            publication.Releases.AddRange(
                new List<Release>
                {
                    olderRelease,
                    newerRelease,
                }
            );

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var handler = new ViewReleaseAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewReleaseRequirement>() },
                    null,
                    newerRelease
                );

                await handler.HandleAsync(authContext);

                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task ReleaseIsNotLive()
        {
            var publication = new Publication();

            var release = new Release
            {
                Publication = publication
            };

            publication.Releases.Add(release);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
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
            var publication = new Publication();

            var olderRelease = new Release
            {
                Published = DateTime.Parse("2019-10-08T12:00:00"),
                Publication = publication
            };

            var newerRelease = new Release
            {
                Published = DateTime.Parse("2019-10-10T12:00:00"),
                Publication = publication,
                PreviousVersion = olderRelease,
            };

            publication.Releases.AddRange(
                new List<Release>
                {
                    olderRelease,
                    newerRelease,
                }
            );

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var handler = new ViewReleaseAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewReleaseRequirement>() },
                    null,
                    olderRelease
                );

                await handler.HandleAsync(authContext);

                Assert.False(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task ReleaseNotFound()
        {
            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext())
            {
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
}