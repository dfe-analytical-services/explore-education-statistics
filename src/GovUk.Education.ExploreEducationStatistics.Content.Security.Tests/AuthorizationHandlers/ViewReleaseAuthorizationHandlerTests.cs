#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.Tests.AuthorizationHandlers;

public class ViewReleaseAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task OnlyReleaseVersionIsLatestPublishedVersion()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 1)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: not null, Version: 0 });

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var handler = BuildHandler(contentDbContext);

            var authContext = CreateAnonymousAuthContext<ViewReleaseRequirement, ReleaseVersion>(releaseVersion);

            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }
    }

    [Fact]
    public async Task ReleaseVersionIsLatestPublishedVersion()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 2, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: not null, Version: 1 });

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var handler = BuildHandler(contentDbContext);

            var authContext = CreateAnonymousAuthContext<ViewReleaseRequirement, ReleaseVersion>(releaseVersion);

            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }
    }

    [Fact]
    public async Task ReleaseVersionIsNotPublished()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 1, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 1 });

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var handler = BuildHandler(contentDbContext);

            var authContext = CreateAnonymousAuthContext<ViewReleaseRequirement, ReleaseVersion>(releaseVersion);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }

    [Fact]
    public async Task ReleaseVersionIsPublishedButNotLatestPublishedVersion()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 2)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: not null, Version: 0 });

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var handler = BuildHandler(contentDbContext);

            var authContext = CreateAnonymousAuthContext<ViewReleaseRequirement, ReleaseVersion>(releaseVersion);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }

    [Fact]
    public async Task ReleaseVersionNotFound()
    {
        await using var contentDbContext = InMemoryContentDbContext();

        var handler = BuildHandler(contentDbContext);

        var authContext = CreateAnonymousAuthContext<ViewReleaseRequirement, ReleaseVersion>(new ReleaseVersion
        {
            Id = Guid.NewGuid()
        });

        await handler.HandleAsync(authContext);

        Assert.False(authContext.HasSucceeded);
    }

    private static ViewReleaseAuthorizationHandler BuildHandler(ContentDbContext contentDbContext)
    {
        return new ViewReleaseAuthorizationHandler(
            new ReleaseVersionRepository(contentDbContext)
        );
    }
}
