#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.Tests.AuthorizationHandlers
{
    public class ViewPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task HasLiveRelease()
        {
            var release = new Release
            {
                Published = new DateTime(2021, 1, 1),
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear
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
                var handler = new ViewPublicationAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewPublicationRequirement>() },
                    null,
                    publication
                );

                await handler.HandleAsync(authContext);

                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task HasLiveReleaseWithDraftAmendment()
        {
            var originalRelease = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = new DateTime(2020, 1, 1),
            };
            var amendedRelease = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
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
                var handler = new ViewPublicationAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewPublicationRequirement>() },
                    null,
                    publication
                );

                await handler.HandleAsync(authContext);

                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task HasLiveReleaseAndNewerDraftRelease()
        {
            var olderRelease = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = new DateTime(2020, 1, 1),
            };
            var newerRelease = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear
            };

            var publication = new Publication
            {
                Releases = ListOf(olderRelease, newerRelease)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var handler = new ViewPublicationAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewPublicationRequirement>() },
                    null,
                    publication
                );

                await handler.HandleAsync(authContext);

                Assert.True(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task HasNoLiveReleases()
        {
            var release = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
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
                var handler = new ViewPublicationAuthorizationHandler(contentDbContext);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { Activator.CreateInstance<ViewPublicationRequirement>() },
                    null,
                    publication
                );

                await handler.HandleAsync(authContext);

                Assert.False(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task DoesNotExist()
        {
            var release = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
            };

            var publication = new Publication
            {
                Releases = ListOf(release)
            };

            await using var contentDbContext = InMemoryContentDbContext();

            var handler = new ViewPublicationAuthorizationHandler(contentDbContext);

            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { Activator.CreateInstance<ViewPublicationRequirement>() },
                null,
                publication
            );

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }
}