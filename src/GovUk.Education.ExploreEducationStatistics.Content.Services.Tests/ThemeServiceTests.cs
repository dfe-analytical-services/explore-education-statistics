using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class ThemeServiceTests
{
    [Fact]
    public async Task ListThemes()
    {
        var themes = new List<Theme>
        {
            new()
            {
                Slug = "theme-c",
                Title = "Theme C",
                Summary = "Theme C summary",
                Publications = [new() { LatestPublishedReleaseVersionId = Guid.NewGuid() }],
            },
            new()
            {
                Slug = "theme-a",
                Title = "Theme A",
                Summary = "Theme A summary",
                Publications = [new() { LatestPublishedReleaseVersionId = Guid.NewGuid() }],
            },
            new()
            {
                Slug = "theme-b",
                Title = "Theme B",
                Summary = "Theme B summary",
                Publications = [new() { LatestPublishedReleaseVersionId = Guid.NewGuid() }],
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Themes.AddRangeAsync(themes);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildThemeService(context);

            var result = await service.ListThemes();

            Assert.Equal(3, result.Count);

            Assert.Equal(themes[1].Id, result[0].Id);
            Assert.Equal("theme-a", result[0].Slug);
            Assert.Equal("Theme A", result[0].Title);
            Assert.Equal("Theme A summary", result[0].Summary);

            Assert.Equal(themes[2].Id, result[1].Id);
            Assert.Equal("theme-b", result[1].Slug);
            Assert.Equal("Theme B", result[1].Title);
            Assert.Equal("Theme B summary", result[1].Summary);

            Assert.Equal(themes[0].Id, result[2].Id);
            Assert.Equal("theme-c", result[2].Slug);
            Assert.Equal("Theme C", result[2].Title);
            Assert.Equal("Theme C summary", result[2].Summary);
        }
    }

    [Fact]
    public async Task ListThemes_ExcludesThemesWithNoPublishedReleases()
    {
        var themeWithoutPublication = new Theme { Title = "Theme A", Publications = [] };

        var themeWithoutPublishedPublication = new Theme
        {
            Title = "Theme B",
            Publications = [new() { LatestPublishedReleaseVersionId = null }],
        };

        var themeWithPublishedReleases = new Theme
        {
            Title = "Theme C",
            Publications = [new() { LatestPublishedReleaseVersionId = Guid.NewGuid() }],
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Themes.AddRangeAsync(
                themeWithoutPublication,
                themeWithoutPublishedPublication,
                themeWithPublishedReleases
            );
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildThemeService(context);

            var result = await service.ListThemes();

            var theme = Assert.Single(result);
            Assert.Equal(themeWithPublishedReleases.Id, theme.Id);
            Assert.Equal("Theme C", theme.Title);
        }
    }

    [Fact]
    public async Task ListThemes_ExcludesThemesWithOnlySupersededPublications()
    {
        var themeWithPublishedSupersededPublication = new Theme
        {
            Title = "Theme A",
            Publications =
            [
                new()
                {
                    LatestPublishedReleaseVersionId = Guid.NewGuid(),
                    SupersededBy = new Publication
                    {
                        LatestPublishedReleaseVersionId = Guid.NewGuid(),
                    },
                },
            ],
        };

        var themeWithUnpublishedSupersededPublication = new Theme
        {
            Title = "Theme B",
            Publications =
            [
                new()
                {
                    LatestPublishedReleaseVersionId = Guid.NewGuid(),
                    SupersededBy = new Publication { LatestPublishedReleaseVersionId = null },
                },
            ],
        };

        var themeWithNonSupersededPublication = new Theme
        {
            Title = "Theme C",
            Publications = [new() { LatestPublishedReleaseVersionId = Guid.NewGuid() }],
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Themes.AddRangeAsync(
                themeWithPublishedSupersededPublication,
                themeWithUnpublishedSupersededPublication,
                themeWithNonSupersededPublication
            );
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildThemeService(context);

            var result = await service.ListThemes();

            Assert.Equal(2, result.Count);

            Assert.Equal(themeWithUnpublishedSupersededPublication.Id, result[0].Id);
            Assert.Equal("Theme B", result[0].Title);

            Assert.Equal(themeWithNonSupersededPublication.Id, result[1].Id);
            Assert.Equal("Theme C", result[1].Title);
        }
    }

    private static ThemeService BuildThemeService(ContentDbContext contentDbContext)
    {
        return new ThemeService(contentDbContext: contentDbContext);
    }
}
