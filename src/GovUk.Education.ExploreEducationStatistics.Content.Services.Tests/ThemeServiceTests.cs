#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = Guid.NewGuid()
                            }
                        }
                    }
                }
            },
            new()
            {
                Slug = "theme-a",
                Title = "Theme A",
                Summary = "Theme A summary",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = Guid.NewGuid()
                            }
                        }
                    }
                }
            },
            new()
            {
                Slug = "theme-b",
                Title = "Theme B",
                Summary = "Theme B summary",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
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
        var themes = new List<Theme>
        {
            // Theme has no topics and should be excluded
            new()
            {
                Title = "Theme A",
                Topics = new List<Topic>()
            },
            // Theme has topic with no publications and should be excluded
            new()
            {
                Title = "Theme B",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>()
                    }
                }
            },
            // Theme has topic with publication that's not published and should be excluded
            new()
            {
                Title = "Theme C",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = null
                            }
                        }
                    }
                }
            },
            // Theme has published releases
            new()
            {
                Title = "Theme D",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = Guid.NewGuid()
                            }
                        }
                    },
                    new()
                    {
                        Publications = new List<Publication>()
                    }
                }
            }
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

            var theme = Assert.Single(result);
            Assert.Equal(themes[3].Id, theme.Id);
            Assert.Equal("Theme D", theme.Title);
        }
    }

    [Fact]
    public async Task ListThemes_ExcludesThemesWithOnlySupersededPublications()
    {
        var themes = new List<Theme>
        {
            // Theme has superseded publication and should be excluded
            new()
            {
                Title = "Theme A",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = Guid.NewGuid(),
                                SupersededBy = new Publication
                                {
                                    // Superseding publication is published
                                    LatestPublishedReleaseId = Guid.NewGuid()
                                }
                            }
                        }
                    },
                    new()
                    {
                        Publications = new List<Publication>()
                    }
                }
            },
            // Theme has superseded publication but the superseding publication is not published yet
            new()
            {
                Title = "Theme B",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = Guid.NewGuid(),
                                SupersededBy = new Publication
                                {
                                    // Superseding publication is not published
                                    LatestPublishedReleaseId = null
                                }
                            }
                        }
                    },
                    new()
                    {
                        Publications = new List<Publication>()
                    }
                }
            },
            // Theme has publication which is not superseded
            new()
            {
                Title = "Theme C",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                LatestPublishedReleaseId = Guid.NewGuid()
                            }
                        }
                    },
                }
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

            Assert.Equal(2, result.Count);

            Assert.Equal(themes[1].Id, result[0].Id);
            Assert.Equal("Theme B", result[0].Title);

            Assert.Equal(themes[2].Id, result[1].Id);
            Assert.Equal("Theme C", result[1].Title);
        }
    }

    private static ThemeService BuildThemeService(
        ContentDbContext contentDbContext)
    {
        return new ThemeService(contentDbContext: contentDbContext);
    }
}
