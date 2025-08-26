#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Security.AuthorizationHandlers;

public class ViewSubjectDataForPublishedReleasesAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ReleaseSubjectIsNotPublished()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 1, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 1 });

        ReleaseSubject releaseSubject = _dataFixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(_dataFixture
                .DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id));

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            var handler = BuildHandler(contentDbContext);

            var authContext = new AuthorizationHandlerContext(
                [Activator.CreateInstance<ViewSubjectDataRequirement>()],
                _dataFixture.Generator<ClaimsPrincipal>(),
                releaseSubject);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }

    [Fact]
    public async Task ReleaseSubjectIsLatestPublishedVersion()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 2, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: not null, Version: 1 });

        ReleaseSubject releaseSubject = _dataFixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(_dataFixture
                .DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id));

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            var handler = BuildHandler(contentDbContext);

            var authContext = new AuthorizationHandlerContext(
                [Activator.CreateInstance<ViewSubjectDataRequirement>()],
                _dataFixture.Generator<ClaimsPrincipal>(),
                releaseSubject);

            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }
    }

    private static ViewSubjectDataForPublishedReleasesAuthorizationHandler BuildHandler(
        ContentDbContext contentDbContext)
    {
        return new ViewSubjectDataForPublishedReleasesAuthorizationHandler(
            new ReleaseVersionRepository(contentDbContext)
        );
    }
}
