using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

/// <summary>
/// TODO EES-6432 EES-6433: This service is due to be removed by EES-6432/EES-6433 once the release page redesign is live.
/// </summary>
public class ReleaseServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task List()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(2));

        var release1Version1 = publication.Releases[0].Versions[0];
        var release2Version1 = publication.Releases[1].Versions[0];

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List(publication.Slug);

            var releases = result.AssertRight();

            Assert.Equal(2, releases.Count);

            // Ordered from newest to oldest
            Assert.Equal(release2Version1.Id, releases[0].Id);
            Assert.Equal(release2Version1.Release.Title, releases[0].Title);
            Assert.Equal(release2Version1.Release.Slug, releases[0].Slug);
            Assert.Equal(release2Version1.Release.TimePeriodCoverage.GetEnumLabel(), releases[0].CoverageTitle);
            Assert.Equal(release2Version1.Release.YearTitle, releases[0].YearTitle);
            Assert.Equal(release2Version1.Published, releases[0].Published);
            Assert.Equal(release2Version1.NextReleaseDate, releases[0].NextReleaseDate);
            Assert.Equal(release2Version1.Type, releases[0].Type);
            Assert.True(releases[0].LatestRelease);

            Assert.Equal(release1Version1.Id, releases[1].Id);
            Assert.Equal(release1Version1.Release.Title, releases[1].Title);
            Assert.Equal(release1Version1.Release.Slug, releases[1].Slug);
            Assert.Equal(release1Version1.Release.TimePeriodCoverage.GetEnumLabel(), releases[1].CoverageTitle);
            Assert.Equal(release1Version1.Release.YearTitle, releases[1].YearTitle);
            Assert.Equal(release1Version1.Published, releases[1].Published);
            Assert.Equal(release1Version1.NextReleaseDate, releases[1].NextReleaseDate);
            Assert.Equal(release1Version1.Type, releases[1].Type);
            Assert.False(releases[1].LatestRelease);
        }
    }

    [Fact]
    public async Task List_FiltersPreviousReleasesForAmendments()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 3).Generate(1));

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List(publication.Slug);

            var releases = result.AssertRight();

            Assert.Equal(
                new[] { publication.Releases[0].Versions.Single(rv => rv is { Version: 2 }).Id },
                releases.Select(r => r.Id).ToArray()
            );
        }
    }

    [Fact]
    public async Task List_FiltersDraftReleases()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true).Generate(2));

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List(publication.Slug);

            var releases = result.AssertRight();

            Assert.Equal(
                new[]
                {
                    publication
                        .Releases.Single(r => r.Year == 2001)
                        .Versions.Single(rv => rv is { Published: not null })
                        .Id,
                    publication
                        .Releases.Single(r => r.Year == 2000)
                        .Versions.Single(rv => rv is { Published: not null })
                        .Id,
                },
                releases.Select(r => r.Id).ToArray()
            );
        }
    }

    [Fact]
    public async Task List_PublicationNotFound()
    {
        await using var contentDbContext = InMemoryContentDbContext();
        var service = SetupReleaseService(contentDbContext);

        var result = await service.List("random-slug");

        result.AssertNotFound();
    }

    private static ReleaseService SetupReleaseService(
        ContentDbContext contentDbContext,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserService? userService = null
    )
    {
        return new(
            contentDbContext,
            releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
            userService ?? AlwaysTrueUserService().Object
        );
    }
}
